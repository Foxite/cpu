namespace Assembler;

public record AluOperand(
	bool IsRegister,
	CpuRegisterAst? Register,
	ConstantAst? Value
) : IAssemblyAst {
	public override string ToString() => IsRegister ? Register!.ToString() : Value!.ToString();
}
