namespace Assembler;

public record AluOperand(
	bool IsRegister,
	CpuRegisterAst? Register,
	ConstantAst? Value
) : IAssemblyAst {
	public AluOperand(CpuRegister register) : this(true, new CpuRegisterAst(register), null) {}
	public AluOperand(short constantValue) : this(false, null, new ConstantAst(constantValue)) {}
	
	public override string ToString() => IsRegister ? Register!.ToString() : Value!.ToString();
}
