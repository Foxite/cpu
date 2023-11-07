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
		
		var unsupportedInstructions = new List<(InstructionAst Instruction, int Index)>();
		Func<string, ushort> getSymbolDefinition = symbol => symbolDefinitions[symbol];

		for (int i = 0; i < program.Statements.Count; i++) {
			ProgramStatementAst statement = program.Statements[i];
			
			if (!ValidateInstruction(statement.Instruction, getSymbolDefinition)) {
				unsupportedInstructions.Add((statement.Instruction, i));
			}
		}

		if (unsupportedInstructions.Count > 0) {
			throw new UnsupportedInstuctionException(ArchitectureName, unsupportedInstructions);
		}

		return program.Statements.Select(statement => ConvertInstruction(statement.Instruction, getSymbolDefinition)).ToList();
	}
	
	protected internal abstract bool ValidateInstruction(InstructionAst instructionAst, Func<string, ushort> getSymbolDefinition);
	protected internal abstract ushort ConvertInstruction(InstructionAst instructionAst, Func<string, ushort> getSymbolDefinition);
}