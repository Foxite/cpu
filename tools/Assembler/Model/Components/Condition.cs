namespace Assembler;

public record Condition(
	AluOperand Left,
	CompareOperationAst CompareOperation,
	AluOperand Right
) : IAssemblyAst {
	public override string ToString() => $"condition {Left} {CompareOperation} {Right}";
}
