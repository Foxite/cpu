namespace Assembler.Ast;

public record ProgramStatementAst(string File, int LineNumber, int Column, string? Label, InstructionAst Instruction) : IAssemblyAst {
	public override string ToString() {
		string ret = Instruction.ToString()!;

		if (Label != null) {
			ret = $"label {Label}: {ret}";
		}

		return ret;
	}
}
