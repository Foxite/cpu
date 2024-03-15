namespace Assembler.Ast;

public record AssemblerProgram(
	string Name,
	string Path,
	//string Architecture,
	ProgramAst ProgramAst
);
