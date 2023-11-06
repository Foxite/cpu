using Antlr4.Runtime;

namespace Assembler.Parsing.Antlr;

public abstract class BaseAntlrParser<TResult, TParser> where TParser : Parser {
	protected abstract Lexer GetLexer(ICharStream charStream);
	protected abstract TParser GetParser(ITokenStream tokenStream);
	protected abstract TResult Visit(TParser parser);
	
	public TResult Parse(string sourceCode) {
		var charStream = new AntlrInputStream(sourceCode);
		Lexer lexer = GetLexer(charStream);
		var tokens = new CommonTokenStream(lexer);
		TParser parser = GetParser(tokens);
		//parser.ErrorHandler = new RecoveringErrorStrategy();
		//parser.ErrorHandler = new DefaultErrorStrategy();
		//parser.ErrorHandler = new BailErrorStrategy();
		
		var errorListenerLexer = new AntlrErrorListener<int>();
		var errorListenerParser = new AntlrErrorListener<IToken>();

		lexer.AddErrorListener(errorListenerLexer);
		parser.AddErrorListener(errorListenerParser);
		
		TResult program = Visit(parser);

		if (errorListenerLexer.HadError) {
			throw new ParserException("Lexer errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}
		
		if (errorListenerParser.HadError) {
			throw new ParserException("Parser errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}

		return program;
	}
}