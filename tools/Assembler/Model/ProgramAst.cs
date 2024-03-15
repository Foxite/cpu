using System.Text;

namespace Assembler.Ast;

public record ProgramAst(IReadOnlyList<ProgramStatementAst> Statements) : IAssemblyAst {
	public ProgramAst(params ProgramStatementAst[] statements) : this((IReadOnlyList<ProgramStatementAst>) statements) {}

	public override string ToString() {
		var ret = new StringBuilder();
		foreach (ProgramStatementAst statement in Statements) {
			ret.AppendLine(statement.ToString());
		}

		return ret.ToString();
	}
	
	public virtual bool Equals(ProgramAst? other) => other != null && other.Statements.SequenceEqual(Statements);
	public override int GetHashCode() => 0;
}
