using Antlr4.Runtime;
using Assembler.Ast;

namespace Assembler.Parsing.Antlr;

public class ProcAssemblyParser {
	private T UseParser<T>(string sourceName, string sourceCode, Func<ProcAssemblyV2Grammar, T> callback) {
		var charStream = new AntlrInputStream(sourceCode);
		Lexer lexer = new ProcAssemblyV2Lexer(charStream);
		var tokens = new CommonTokenStream(lexer);
		var parser = new ProcAssemblyV2Grammar(tokens);

		charStream.name = sourceName;
		
		//parser.ErrorHandler = new RecoveringErrorStrategy();
		//parser.ErrorHandler = new DefaultErrorStrategy();
		//parser.ErrorHandler = new BailErrorStrategy();
		
		var errorListenerLexer = new AntlrErrorListener<int>();
		var errorListenerParser = new AntlrErrorListener<IToken>();

		lexer.AddErrorListener(errorListenerLexer);
		parser.AddErrorListener(errorListenerParser);

		T ret = callback(parser);

		if (errorListenerLexer.HadError) {
			throw new ParserException("Lexer errors:\n" + string.Join('\n', errorListenerLexer.Errors.Select(error => error.ToString())));
		}
		
		if (errorListenerParser.HadError) {
			throw new ParserException("Parser errors:\n" + string.Join('\n', errorListenerParser.Errors.Select(error => error.ToString())));
		}

		return ret;
	}
	
	public ProgramAst Parse(string sourceName, string sourceCode) {
		return UseParser(sourceName, sourceCode, parser => (ProgramAst) new BasicVisitor().Visit(parser.program()));
	}
	
	public InstructionArgumentAst ParseSymbolValue(string sourceName, string value) {
		return UseParser(sourceName, value, parser => (InstructionArgumentAst) new BasicVisitor().VisitInstructionArgument(parser.instructionArgument()));
	}
}
