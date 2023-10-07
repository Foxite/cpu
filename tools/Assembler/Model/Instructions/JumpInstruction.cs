namespace Assembler;

public record JumpInstruction(
	bool IsCondition,
	Condition? Condition,
	BooleanAst? Value,
	CpuRegisterAst TargetRegister
) : IAssemblyInstruction {
	public override string ToString() => $"JumpInstruction {(IsCondition ? Condition : Value)} to {TargetRegister}";
}
