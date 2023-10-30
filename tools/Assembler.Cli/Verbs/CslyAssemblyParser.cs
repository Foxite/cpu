using Assembler;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;

public class CslyAssemblyParser : IAssemblyParser {
	public ExitCode Parse(string sourceCode, out ProgramAst? program) {
		var parserDefinition = new AssemblyParser();
		var parserBuilder = new ParserBuilder<AssemblyToken, IAssemblyAst>();

		BuildResult<Parser<AssemblyToken, IAssemblyAst>> buildResult = parserBuilder.BuildParser(parserDefinition, ParserType.EBNF_LL_RECURSIVE_DESCENT, "Program");

		if (buildResult.IsError) {
			Console.Error.WriteLine("Unable to build parser!");
			program = null;
			return ExitCode.InternalError;
		}
		
		Parser<AssemblyToken, IAssemblyAst> parser = buildResult.Result;
		ParseResult<AssemblyToken, IAssemblyAst> parseResult;
		
		parseResult = parser.Parse(sourceCode);

		if (parseResult.IsError) {
			Console.Error.WriteLine("Syntax error in source file:");
			foreach (var error in parseResult.Errors) {
				Console.Error.WriteLine($"{error.Line}:{error.Column} [{error.ErrorType}] {error.ErrorMessage}");
			}
			program = null;
			return ExitCode.CompileParseError;
		}

		program = (ProgramAst) parseResult.Result;
		return ExitCode.Success;
	}
}
