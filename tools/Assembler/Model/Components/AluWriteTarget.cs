namespace Assembler;

public record AluWriteTarget(List<CpuRegisterAst> Registers) : IAssemblyAst {
	public override string ToString() => $"write to {string.Join(", ", Registers.Select(register => register.ToString()))}";
}
