namespace Assembler.Parsing.ProcAssemblyV2;

public record InstructionAst(string Instruction, IReadOnlyList<InstructionArgumentAst> Arguments) : IAssemblyAst {
	public InstructionAst(string instruction, params InstructionArgumentAst[] arguments) : this(instruction, (IReadOnlyList<InstructionArgumentAst>) arguments) { }

	public override string ToString() {
		string ret = Instruction;

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
