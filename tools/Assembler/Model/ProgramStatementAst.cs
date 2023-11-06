namespace Assembler.Parsing.ProcAssemblyV2;

public record ProgramStatementAst(string? Label, InstructionAst Instruction) : IAssemblyAst {
	public override string ToString() {
		string ret = Instruction.ToString()!;

		if (Label != null) {
			ret = $"label {Label}: {ret}";
		}

		return ret;
	}
}
