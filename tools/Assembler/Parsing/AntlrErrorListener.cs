using Antlr4.Runtime;

namespace Assembler.Parsing.Antlr;

public class AntlrErrorListener<T> : IAntlrErrorListener<T> where T : notnull {
	public bool HadError { get; private set; }
	public List<AntlrSyntaxError> Errors { get; } = new();
	
	public void SyntaxError(TextWriter output, IRecognizer recognizer, T offendingSymbol, int line, int col, string msg, RecognitionException e) {
		HadError = true;
		
		Errors.Add(new AntlrSyntaxError(offendingSymbol.ToString()!, line, col, msg, e));
	}
}
