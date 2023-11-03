using sly.lexer;

namespace Assembler.Parsing.Proc16a.Csly;

public enum Proc16aCslyTokens {
	[Lexeme(",")]
	Comma,
	
	[Lexeme(":")]
	Colon,
	
	[Lexeme("AND")]
	BitwiseAnd,
	[Lexeme("OR")]
	BitwiseOr,
	[Lexeme("NOT")]
	BitwiseNot,
	[Lexeme("XOR")]
	BitwiseXor,
	[Lexeme("XNOR")]
	BitwiseXnor,
	[Lexeme("NOR")]
	BitwiseNor,
	[Lexeme("NAND")]
	BitwiseNand,

	[Lexeme("A")]
	ARegister,
	[Lexeme("B")]
	BRegister,
	
	[Lexeme("\\+")]
	Plus,
	[Lexeme("-")]
	Minus,
	[Lexeme("\\*[^AB]")]
	Multiply,
	[Lexeme("/")]
	Divide,
	[Lexeme("<<")]
	LeftShift,
	[Lexeme(">>")]
	RightShift,
	
	[Lexeme("\\*A")]
	StarA,
	[Lexeme("\\*B")]
	StarB,
	
	[Lexeme(">")]
	GreaterThan,
	[Lexeme("==")]
	Equals,
	[Lexeme(">=")]
	GreaterThanOrEquals,
	[Lexeme("<")]
	LessThan,
	[Lexeme("!=")]
	NotEquals,
	[Lexeme("<=")]
	LessThanOrEquals,
	[Lexeme("true")]
	True,
	[Lexeme("false")]
	False,

	[Lexeme("=")]
	Assign,
	
	[Lexeme("JMP")]
	Jump,
	
	[Lexeme("[ \\t]+", isSkippable: true)]
	Whitespace,
	
	[Lexeme("(#.*)?\n", isLineEnding: true)]
	EndOfLine,
	
	[Lexeme("0b[01_]+")]
	BinaryInteger,
	[Lexeme("0x[0-9A-Fa-f_]+")]
	HexadecimalInteger,
	[Lexeme("[0-9_]+")]
	DecimalInteger,
	
	[Lexeme("[A-z_][A-z0-9_]*")]
	Symbol,
}
