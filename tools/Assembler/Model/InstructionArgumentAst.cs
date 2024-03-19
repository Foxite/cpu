namespace Assembler.Ast;

public abstract record InstructionArgumentAst : IAssemblyAst {
	public abstract override string ToString();
	
	public static InstructionArgumentAst Constant(long value) => new ConstantAst(value);
	public static InstructionArgumentAst Register(string value) => new RegisterAst(value);
	public static InstructionArgumentAst Symbol(string value) => new SymbolAst(value);
	public static InstructionArgumentAst String(string value) => new StringAst(value);
}

public record ConstantAst(long Value) : InstructionArgumentAst {
	public override string ToString() => $"Constant {Value}";
}

public record RegisterAst(string Value) : InstructionArgumentAst {
	public override string ToString() => $"Register {Value}";
}

public record SymbolAst(string Value) : InstructionArgumentAst {
	public override string ToString() => $"Symbol {Value}";
}

public record StringAst(string Value) : InstructionArgumentAst {
	public override string ToString() => $"String \"{Value}\"";
}

