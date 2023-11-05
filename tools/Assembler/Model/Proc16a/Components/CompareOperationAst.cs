namespace Assembler.Parsing.Proc16a;

public record CompareOperationAst(
	CompareOperation Operation
) : IAssemblyAst {
	public override string ToString() => Operation.ToString();
}
