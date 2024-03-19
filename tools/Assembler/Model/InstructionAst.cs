namespace Assembler.Ast;

public record InstructionAst(string File, int LineNumber, int Column, string Mnemonic, IReadOnlyList<InstructionArgumentAst> Arguments) : IAssemblyAst {
	/// <summary>
	/// USE IN TESTS ONLY
	/// </summary>
	/// <param name="mnemonic"></param>
	/// <param name="arguments"></param>
	public InstructionAst(string mnemonic, params InstructionArgumentAst[] arguments) : this("TEST", 0, 0, mnemonic, (IReadOnlyList<InstructionArgumentAst>) arguments) { }

	public override string ToString() {
		string ret = Mnemonic;

		for (int i = 0; i < Arguments.Count; i++) {
			if (i > 0) {
				ret += ",";
			}

			ret += " " + Arguments[i];
		}

		return ret;
	}
	
	public virtual bool Equals(InstructionAst? other) => other != null && other.Arguments.SequenceEqual(Arguments);
}
