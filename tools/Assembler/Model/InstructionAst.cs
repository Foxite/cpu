namespace Assembler.Parsing.ProcAssemblyV2;

public record InstructionAst(string Mnemonic, IReadOnlyList<InstructionArgumentAst> Arguments) : IAssemblyAst {
	public InstructionAst(string mnemonic, params InstructionArgumentAst[] arguments) : this(mnemonic, (IReadOnlyList<InstructionArgumentAst>) arguments) { }

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
