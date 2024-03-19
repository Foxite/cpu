using System.Text;

namespace Assembler.Ast;

public record ProgramAst(string File, int LineNumber, int Column, IReadOnlyList<ProgramStatementAst> Statements) : IAssemblyAst {
	public ProgramAst(params ProgramStatementAst[] statements) : this("TEST", 0, 0, statements) {}

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
