using Antlr4.Runtime;
using Assembler.Parsing.ProcAssemblyV2;
using Assembler.Parsing.ProcAssemblyV2.Antlr;

namespace Assembler.Parsing.Antlr;

public class ProcAssemblyParser {
	public ProgramAst Parse(string sourceCode) {
		var charStream = new AntlrInputStream(sourceCode);
		Lexer lexer = new ProcAssemblyV2Lexer(charStream);
		var tokens = new CommonTokenStream(lexer);
		var parser = new ProcAssemblyV2Grammar(tokens);
		//parser.ErrorHandler = new RecoveringErrorStrategy();
		//parser.ErrorHandler = new DefaultErrorStrategy();
		//parser.ErrorHandler = new BailErrorStrategy();
		
		var errorListenerLexer = new AntlrErrorListener<int>();
		var errorListenerParser = new AntlrErrorListener<IToken>();

		lexer.AddErrorListener(errorListenerLexer);
		parser.AddErrorListener(errorListenerParser);

		var program = (ProgramAst) new BasicVisitor().Visit(parser.program());

		if (errorListenerLexer.HadError) {
			throw new ParserException("Lexer errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}
		
		if (errorListenerParser.HadError) {
			throw new ParserException("Parser errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}

		return program;
	}
}