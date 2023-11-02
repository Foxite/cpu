using Antlr4.Runtime;

namespace Assembler.Parsing.Antlr;

public class ErrorListener<T> : IAntlrErrorListener<T> where T : notnull {
	public bool HadError { get; private set; }
	public List<SyntaxError> Errors { get; } = new();
	
	public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int col, string msg, RecognitionException e) {
		HadError = true;
		
		Errors.Add(new SyntaxError(offendingSymbol.ToString()!, line, col, msg, e));
	}
}
