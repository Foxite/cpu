namespace Assembler;

public record AssignInstruction(
	CpuRegisterAst Write,
	CpuRegisterAst Read
) : IAssemblyInstruction;
