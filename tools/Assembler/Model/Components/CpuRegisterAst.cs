namespace Assembler;

public record CpuRegisterAst(
	CpuRegister Register
) : IAssemblyAst {
	public override string ToString() => $"reg {Register}";
}
