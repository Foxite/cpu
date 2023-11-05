namespace Assembler.Parsing.ProcAssemblyV2;

public record InstructionAst(string Instruction, IReadOnlyList<InstructionArgumentAst> Arguments) {
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
}
