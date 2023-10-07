using System.Text;

namespace Assembler;

public record ProgramAst(params IStatement[] Instructions) : IAssemblyAst {
	public override string ToString() {
		var ret = new StringBuilder();
		foreach (IStatement statement in Instructions) {
			ret.AppendLine(statement.ToString());
		}

		return ret.ToString();
	}
	
	public virtual bool Equals(ProgramAst? other) => other != null && other.Instructions.SequenceEqual(Instructions);
	public override int GetHashCode() => 0;
}
