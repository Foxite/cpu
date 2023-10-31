using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using Assembler;
using Assembler.Antlr;
using Assembler.Parsing.Antlr;

public class AntlrAssemblyParser : IAssemblyParser {
	public ExitCode Parse(string sourceCode, out ProgramAst? program) {
		var charStream = new AntlrInputStream(sourceCode);
		var lexer = new proc16a_lexer(charStream);
		var tokens = new CommonTokenStream(lexer);
		var parser = new proc16a_grammar(tokens);
		var errorListenerLexer = new ErrorListener<int>();
		var errorListenerParser = new ErrorListener<IToken>();

		lexer.AddErrorListener(errorListenerLexer);
		parser.AddErrorListener(errorListenerParser);
		
		var visitor = new BasicVisitor();

		visitor.Visit(parser.program());

		if (errorListenerLexer.HadError) {
			Console.WriteLine("Lexer error");
			program = null;
			return ExitCode.CompileParseError;
		}
		
		if (errorListenerParser.HadError) {
			Console.WriteLine("Parser error");
			program = null;
			return ExitCode.CompileParseError;
		}

		program = null!;
		
		return ExitCode.InternalError;
	}
}

public class BasicVisitor : proc16a_grammarBaseVisitor<IAssemblyAst?> {
	public override IAssemblyAst? VisitProgram(proc16a_grammar.ProgramContext context) {
		return null;
	}

	public override IAssemblyAst? VisitProgramStatement(proc16a_grammar.ProgramStatementContext context) {
		return null;
	}
}
