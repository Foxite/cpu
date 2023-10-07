namespace Assembler;

public record JumpInstruction(
	bool IsCondition,
	Condition? Condition,
	BooleanAst? Value,
	CpuRegisterAst TargetRegister
) : IStatement {
	public JumpInstruction(Condition condition, CpuRegister targetRegister) : this(true, condition, null, new CpuRegisterAst(targetRegister)) { }
	public JumpInstruction(bool value, CpuRegister targetRegister) : this(false, null, new BooleanAst(value), new CpuRegisterAst(targetRegister)) { }
	
	public override string ToString() => $"JumpInstruction {(IsCondition ? Condition : Value)} to {TargetRegister}";
}
