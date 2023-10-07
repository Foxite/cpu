namespace Assembler;

public record AluOperationAst(
	AluOperation Operation
) : IAssemblyAst {
	public override string ToString() => Operation.ToString();
}
