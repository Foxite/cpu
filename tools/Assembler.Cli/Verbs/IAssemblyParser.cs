using Assembler;

public interface IAssemblyParser {
	ExitCode Parse(string sourceCode, out ProgramAst? program);
}
