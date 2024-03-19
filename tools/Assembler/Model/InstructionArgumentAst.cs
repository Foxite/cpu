namespace Assembler.Ast;

public abstract record InstructionArgumentAst(string File, int LineNumber, int Column) : IAssemblyAst {
	public abstract override string ToString();
	
	public static InstructionArgumentAst Constant(long value) => new ConstantAst("TEST", 0, 0, value);
	public static InstructionArgumentAst Register(string value) => new RegisterAst("TEST", 0, 0, value);
	public static InstructionArgumentAst Symbol(string value) => new SymbolAst("TEST", 0, 0, value);
	public static InstructionArgumentAst String(string value) => new StringAst("TEST", 0, 0, value);
}

public record ConstantAst(string File, int LineNumber, int Column, long Value) : InstructionArgumentAst(File, LineNumber, Column) {
	public override string ToString() => $"Constant {Value}";
}

public record RegisterAst(string File, int LineNumber, int Column, string Value) : InstructionArgumentAst(File, LineNumber, Column) {
	public override string ToString() => $"Register {Value}";
}

public record SymbolAst(string File, int LineNumber, int Column, string Value) : InstructionArgumentAst(File, LineNumber, Column) {
	public override string ToString() => $"Symbol {Value}";
}

public record StringAst(string File, int LineNumber, int Column, string Value) : InstructionArgumentAst(File, LineNumber, Column) {
	public override string ToString() => $"String \"{Value}\"";
}
