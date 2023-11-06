using Antlr4.Runtime;
using Assembler.Parsing.ProcAssemblyV2;
using Assembler.Parsing.ProcAssemblyV2.Antlr;

namespace Assembler.Parsing.Antlr;

public class ProcAssemblyV2Parser : BaseAntlrParser<ProgramAst, ProcAssemblyV2Grammar> {
	protected override Lexer GetLexer(ICharStream charStream) {
		return new ProcAssemblyV2Lexer(charStream);
	}
	protected override ProcAssemblyV2Grammar GetParser(ITokenStream tokenStream) {
		return new ProcAssemblyV2Grammar(tokenStream);
	}
	protected override ProgramAst Visit(ProcAssemblyV2Grammar parser) {
		return (ProgramAst) new ProcAssemblyV2BasicVisitor().Visit(parser.program());
	}
}
