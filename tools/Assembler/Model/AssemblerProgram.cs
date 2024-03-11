namespace Assembler.Parsing.ProcAssemblyV2;

public record AssemblerProgram(
	string Name,
	string Path,
	//string Architecture,
	ProgramAst ProgramAst
);
