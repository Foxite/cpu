using Antlr4.Runtime;

namespace Assembler.Parsing.Proc16a.Antlr;

public record Proc16aSyntaxError(string Symbol, int Line, int Column, string Message, RecognitionException Exception);
