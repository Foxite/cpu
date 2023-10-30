using Antlr4.Runtime;
using Assembler.Antlr;

namespace Assembler.Parsing.Antlr; 

/*
public class Proc16aAntlrParser {
	public ProgramAst Parse(string sourceCode) {
		throw new NotImplementedException();
	}
}
*/

public class ErrorListener<T> : ConsoleErrorListener<T> {
	public bool HadError { get; private set; }
	
	public override void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int col, string msg, RecognitionException e) {
		HadError = true;
		base.SyntaxError(output, recognizer, offendingSymbol, line, col, msg, e);
	}
}
