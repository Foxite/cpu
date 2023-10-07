namespace Assembler;

public record CompareOperationAst(
	CompareOperation Operation
) : IAssemblyAst {
	public override string ToString() => Operation.ToString();
}
