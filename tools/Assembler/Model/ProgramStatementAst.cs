namespace Assembler.Ast;

public record ProgramStatementAst(string File, int LineNumber, int Column, string? Label, InstructionAst Instruction) : IAssemblyAst {
	/// <summary>
	/// USE IN TESTS ONLY
	/// </summary>
	/// <param name="mnemonic"></param>
	/// <param name="arguments"></param>
	public ProgramStatementAst(string? label, InstructionAst instruction) : this("TEST", 0, 0, label, instruction) { }
	
	public override string ToString() {
		string ret = Instruction.ToString()!;

		if (Label != null) {
			ret = $"label {Label}: {ret}";
		}

		return ret;
	}
	
	public virtual bool Equals(ProgramStatementAst? other) => other != null && Label == other.Label && Instruction.Equals(other.Instruction);
}
