namespace Assembler.Parsing.Proc16a;

public record AluInstruction(
	AluWriteTarget Write,
	AluOperand XOperand,
	AluOperand? YOperand,
	AluOperationAst Operation
) : IStatement {
	public AluInstruction(AluWriteTarget writeTarget, AluOperand xOperand, AluOperand? yOperand, AluOperation operation) : this(writeTarget, xOperand, yOperand, new AluOperationAst(operation)) { }

	public override string ToString() => $"AluInstruction {Write}: {XOperand} {Operation} {YOperand}";
}
