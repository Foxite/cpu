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
		
		proc16a_grammar.ProgramContext programContext = parser.program();

		var visitor = new BasicVisitor();

		visitor.VisitProgram(programContext);

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
		
		var statements = new List<ProgramStatementAst>();

		foreach (proc16a_grammar.ProgramStatementContext statementContext in programContext.programStatement()) {
			Console.WriteLine(statementContext);
		}
		
		program = new ProgramAst(statements);
		return ExitCode.Success;
	}
}

public class BasicVisitor : proc16a_grammarBaseVisitor<IAssemblyAst?> {
	private readonly List<ProgramStatementAst> m_Statements;

	public override IAssemblyAst VisitProgram(proc16a_grammar.ProgramContext context) {
		base.VisitProgram(context);
		return new ProgramAst(m_Statements);
	}

	public override IAssemblyAst? VisitProgramStatement(proc16a_grammar.ProgramStatementContext context) {
		return base.VisitProgramStatement(context);
	}
}
