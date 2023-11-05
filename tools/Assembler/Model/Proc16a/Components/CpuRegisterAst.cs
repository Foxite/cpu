namespace Assembler.Parsing.Proc16a;

public record CpuRegisterAst(
	CpuRegister Register
) : IAssemblyAst {
	public override string ToString() => $"reg {Register}";
}
