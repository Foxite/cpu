using sly.buildresult;
using sly.parser;
using sly.parser.generator;

namespace Assembler.Parsing.Csly;

public class CslyAssemblyParser : IAssemblyParser {
	private readonly Parser<CslyTokens, IAssemblyAst> m_Parser;
	
	public CslyAssemblyParser() {
		var parserDefinition = new CslyGrammars();
		var parserBuilder = new ParserBuilder<CslyTokens, IAssemblyAst>();

		BuildResult<Parser<CslyTokens, IAssemblyAst>> buildResult = parserBuilder.BuildParser(parserDefinition, ParserType.EBNF_LL_RECURSIVE_DESCENT, "Program");

		if (buildResult.IsError) {
			throw new ParserException("Unable to build parser!\n" + string.Join('\n', buildResult.Errors.Select(error => $"{error.Level} {error.Code} {error.Message}")));
		}
		
		m_Parser = buildResult.Result;
	}
	
	public ProgramAst Parse(string sourceCode) {
		ParseResult<CslyTokens, IAssemblyAst> parseResult;
		
		parseResult = m_Parser.Parse(sourceCode);

		if (parseResult.IsError) {
			throw new ParserException("Syntax error in source file:\n" + string.Join('\n', parseResult.Errors.Select(error => $"{error.Line}:{error.Column} [{error.ErrorType}] {error.ErrorMessage}")));
		}
		
		return (ProgramAst) parseResult.Result;
	}
}
