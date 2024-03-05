using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

/// <summary>
/// Converts <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture.
/// </summary>
public abstract class ProgramAssembler {
	public abstract string ArchitectureName { get; }

	public IReadOnlyList<ushort> Assemble(ProgramAst program) {
		var symbolDefinitions = new Dictionary<string, ushort>();

		for (int i = 0; i < program.Statements.Count; i++) {
			ProgramStatementAst statement = program.Statements[i];
			
			if (statement.Label != null) {
				symbolDefinitions[statement.Label] = (ushort) i;
			}
		}
		
		var invalidInstructions = new List<InvalidInstruction>();
		
		ushort GetSymbolDefinition(string symbol) => symbolDefinitions.TryGetValue(symbol, out ushort value) ? value : throw new SymbolNotDefinedException(symbol);

		for (int i = 0; i < program.Statements.Count; i++) {
			ProgramStatementAst statement = program.Statements[i];

			InstructionSupport support;
			try {
				support = ValidateInstruction(statement.Instruction, GetSymbolDefinition);
			} catch (SymbolNotDefinedException ex) {
				invalidInstructions.Add(new InvalidInstruction(statement.Instruction, i, InstructionSupport.OtherError, $"Symbol not defined: {ex.Symbol}"));
				continue;
			}

			if (support != InstructionSupport.Supported) {
				invalidInstructions.Add(new InvalidInstruction(statement.Instruction, i, support, support.ToString()));
			}
		}

		if (invalidInstructions.Count > 0) {
			throw new InvalidProcAssemblyProgramException(ArchitectureName, invalidInstructions);
		}

		return program.Statements.Select(statement => ConvertInstruction(statement.Instruction, GetSymbolDefinition)).ToList();
	}
	
	protected internal abstract InstructionSupport ValidateInstruction(InstructionAst instructionAst, Func<string, ushort> getSymbolDefinition);
	protected internal abstract ushort ConvertInstruction(InstructionAst instructionAst, Func<string, ushort> getSymbolDefinition);
}
