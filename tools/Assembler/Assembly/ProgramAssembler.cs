using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

/// <summary>
/// Converts <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture.
/// </summary>
public abstract class ProgramAssembler {
	public abstract string ArchitectureName { get; }

	public IReadOnlyList<ushort> Assemble(ProgramAst program) {
		var symbolDefinitions = new Dictionary<string, ushort>();
		var unsupportedStatements = new Stack<(InstructionAst Instruction, int Index)>();

		for (int i = 0; i < program.Statements.Count; i++) {
			ProgramStatementAst statement = program.Statements[i];
			
			if (statement.Label != null) {
				symbolDefinitions[statement.Label] = (ushort) i;
			}

			if (!ValidateInstruction(statement.Instruction)) {
				unsupportedStatements.Push((statement.Instruction, i));
			}
		}

		if (unsupportedStatements.Count > 0) {
			throw new UnsupportedInstuctionException(ArchitectureName, unsupportedStatements);
		}

		return program.Statements.Select(statement => ConvertInstruction(statement.Instruction, symbol => symbolDefinitions[symbol])).ToList();
	}
	
	protected internal abstract ushort ConvertInstruction(InstructionAst instructionAst, Func<string, ushort> getSymbolDefinition);

	protected internal abstract bool ValidateInstruction(InstructionAst instructionAst);
}