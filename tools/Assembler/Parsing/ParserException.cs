using System.Runtime.Serialization;

namespace Assembler.Parsing;

// TODO structured exception data
public class ParserException : Exception {
	public ParserException() { }
	protected ParserException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	public ParserException(string? message) : base(message) { }
	public ParserException(string? message, Exception? innerException) : base(message, innerException) { }
}
