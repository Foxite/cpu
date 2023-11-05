namespace Assembler.Parsing.Proc16a;

public record AluOperationAst(
	AluOperation Operation
) : IAssemblyAst {
	public override string ToString() => Operation.ToString();
}
