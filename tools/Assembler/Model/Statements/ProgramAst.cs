using System.Text;

namespace Assembler;

public record ProgramAst(params IStatement[] Statements) : IAssemblyAst {
	public override string ToString() {
		var ret = new StringBuilder();
		foreach (IStatement statement in Statements) {
			ret.AppendLine(statement.ToString());
		}

		return ret.ToString();
	}
	
	public virtual bool Equals(ProgramAst? other) => other != null && other.Statements.SequenceEqual(Statements);
	public override int GetHashCode() => 0;
}
