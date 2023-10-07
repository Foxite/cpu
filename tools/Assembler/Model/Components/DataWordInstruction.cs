namespace Assembler;

public record DataWordInstruction(CpuRegisterAst Register, ConstantAst Value) : IAssemblyInstruction {
	public override string ToString() => $"DataWordInstruction {Register} = {Value}";
}
