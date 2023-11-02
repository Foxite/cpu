using Antlr4.Runtime;

namespace Assembler.Parsing.Antlr;

public record SyntaxError(string Symbol, int Line, int Column, string Message, RecognitionException Exception);
