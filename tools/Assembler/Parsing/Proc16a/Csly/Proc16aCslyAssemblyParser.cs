using sly.buildresult;
using sly.parser;
using sly.parser.generator;

namespace Assembler.Parsing.Proc16a.Csly;

public class Proc16aCslyAssemblyParser : IProc16aAssemblyParser {
	private readonly Parser<Proc16aCslyTokens, IAssemblyAst> m_Parser;
	
	public Proc16aCslyAssemblyParser() {
		var parserDefinition = new Proc16aCslyGrammars();
		var parserBuilder = new ParserBuilder<Proc16aCslyTokens, IAssemblyAst>();

		BuildResult<Parser<Proc16aCslyTokens, IAssemblyAst>> buildResult = parserBuilder.BuildParser(parserDefinition, ParserType.EBNF_LL_RECURSIVE_DESCENT, "Program");

		if (buildResult.IsError) {
			throw new ParserException("Unable to build parser!\n" + string.Join('\n', buildResult.Errors.Select(error => $"{error.Level} {error.Code} {error.Message}")));
		}
		
		m_Parser = buildResult.Result;
	}
	
	public ProgramAst Parse(string sourceCode) {
		ParseResult<Proc16aCslyTokens, IAssemblyAst> parseResult;
		
		parseResult = m_Parser.Parse(sourceCode);

		if (parseResult.IsError) {
			throw new ParserException("Syntax error in source file:\n" + string.Join('\n', parseResult.Errors.Select(error => $"{error.Line}:{error.Column} [{error.ErrorType}] {error.ErrorMessage}")));
		}
		
		return (ProgramAst) parseResult.Result;
	}
}
