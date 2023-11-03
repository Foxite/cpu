using Antlr4.Runtime;

namespace Assembler.Parsing.Proc16a.Antlr;

public class Proc16aErrorListener<T> : IAntlrErrorListener<T> where T : notnull {
	public bool HadError { get; private set; }
	public List<Proc16aSyntaxError> Errors { get; } = new();
	
	public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int col, string msg, RecognitionException e) {
		HadError = true;
		
		Errors.Add(new Proc16aSyntaxError(offendingSymbol.ToString()!, line, col, msg, e));
	}
}
