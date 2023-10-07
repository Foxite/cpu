using sly.lexer;

namespace Assembler;

public enum AssemblyToken {
	[Lexeme("A")]
	ARegister,
	[Lexeme("B")]
	BRegister,
	[Lexeme("\\*A")]
	StarA,
	[Lexeme("\\*B")]
	StarB,
	
	[Lexeme(",")]
	Comma,
	
	[Lexeme("\\+")]
	Plus,
	[Lexeme("-")]
	Minus,
	[Lexeme("\\*")]
	Multiply,
	[Lexeme("/")]
	Divide,
	[Lexeme("<<")]
	LeftShift,
	[Lexeme(">>")]
	RightShift,
	
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
	
	[Lexeme("0b[01_]+")]
	BinaryInteger,
	[Lexeme("0x[0-9A-Fa-f_]+")]
	HexadecimalInteger,
	[Lexeme("[0-9_]+")]
	DecimalInteger,
	
	[Lexeme("[A-z_]+:")]
	Label,
	
	[Lexeme("JMP")]
	Jump,
	
	[Lexeme("[ \\t]+", isSkippable: true)]
	Whitespace,
	
	[Lexeme("(#.*)?\n", isLineEnding: true)]
	EndOfLine,
	
	[Lexeme("[A-z_][A-z0-9_]*")]
	Symbol,
}
