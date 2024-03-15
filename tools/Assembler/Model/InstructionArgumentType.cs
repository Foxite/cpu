namespace Assembler.Ast;

public enum InstructionArgumentType {
	Constant,
	Register,
	StarRegister,
	
	// A symbol argument will never make it to an assembler's implementation because it gets substituted for its constant value.
	Symbol,
	String,
}
