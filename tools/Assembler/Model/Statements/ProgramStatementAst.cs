namespace Assembler;

public record ProgramStatementAst(string? Label, IStatement Statement) : IAssemblyAst {
	public override string ToString() {
		string ret = Statement.ToString()!;

		if (Label != null) {
			ret = $"label {Label}: {ret}";
		}

		return ret;
	}
}
