namespace Assembler;

public record AluInstruction(
	AluWriteTarget Write,
	AluOperand XOperand,
	AluOperand YOperand,
	AluOperationAst Operation
) : IAssemblyInstruction {
	public override string ToString() => $"AluInstruction {Write} = {XOperand} {Operation} {YOperand}";
}
