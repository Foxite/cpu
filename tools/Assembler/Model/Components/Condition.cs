namespace Assembler;

public record Condition(
	AluOperand Left,
	CompareOperationAst CompareOperation,
	AluOperand Right
) : IAssemblyAst {
	public Condition(AluOperand left, CompareOperation compareOperation, AluOperand right) : this(left, new CompareOperationAst(compareOperation), right) { }
	
	public override string ToString() => $"condition {Left} {CompareOperation} {Right}";
}
