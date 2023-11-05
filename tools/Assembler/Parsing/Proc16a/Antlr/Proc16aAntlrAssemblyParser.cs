using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Assembler.Parsing.Antlr;

namespace Assembler.Parsing.Proc16a.Antlr;

public class Proc16aAntlrAssemblyParser : IProc16aAssemblyParser {
	public ProgramAst Parse(string sourceCode) {
		var charStream = new AntlrInputStream(sourceCode);
		var lexer = new Proc16aLexer(charStream);
		var tokens = new CommonTokenStream(lexer);
		var parser = new Proc16aGrammar(tokens);
		//parser.ErrorHandler = new RecoveringErrorStrategy();
		//parser.ErrorHandler = new DefaultErrorStrategy();
		//parser.ErrorHandler = new BailErrorStrategy();
		
		var errorListenerLexer = new AntlrErrorListener<int>();
		var errorListenerParser = new AntlrErrorListener<IToken>();

		lexer.AddErrorListener(errorListenerLexer);
		parser.AddErrorListener(errorListenerParser);
		
		var visitor = new Proc16aBasicVisitor();

		var program = (ProgramAst) visitor.Visit(parser.program());

		if (errorListenerLexer.HadError) {
			throw new ParserException("Lexer errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}
		
		if (errorListenerParser.HadError) {
			throw new ParserException("Parser errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}

		return program;
	}
}
