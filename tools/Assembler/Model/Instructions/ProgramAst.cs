using System.Text;

namespace Assembler;

public record ProgramAst(params IAssemblyInstruction[] Instructions) : IAssemblyAst {
	public override string ToString() {
		var ret = new StringBuilder();
		foreach (IAssemblyInstruction instruction in Instructions) {
			ret.AppendLine(instruction.ToString());
		}

		return ret.ToString();
	}
}
