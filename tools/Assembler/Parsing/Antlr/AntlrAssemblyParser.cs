using Antlr4.Runtime;
using Assembler.Antlr;

namespace Assembler.Parsing.Antlr;

public class AntlrAssemblyParser : IAssemblyParser {
	public ProgramAst Parse(string sourceCode) {
		var charStream = new AntlrInputStream(sourceCode);
		var lexer = new proc16a_lexer(charStream);
		var tokens = new CommonTokenStream(lexer);
		var parser = new proc16a_grammar(tokens);
		var errorListenerLexer = new ErrorListener<int>();
		var errorListenerParser = new ErrorListener<IToken>();

		lexer.AddErrorListener(errorListenerLexer);
		parser.AddErrorListener(errorListenerParser);
		
		var visitor = new BasicVisitor();

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
