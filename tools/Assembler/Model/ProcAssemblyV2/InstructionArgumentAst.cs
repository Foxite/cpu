namespace Assembler.Parsing.ProcAssemblyV2;

public record InstructionArgumentAst(InstructionArgumentType Type, long? ConstantValue, string? RslsValue) : IAssemblyAst {
	public override string ToString() {
		return Type switch {
			InstructionArgumentType.Constant => $"Constant {ConstantValue}",
			InstructionArgumentType.Register => $"Register {RslsValue}",
			InstructionArgumentType.StarRegister => $"StarRegister {RslsValue}",
			InstructionArgumentType.Symbol => $"Symbol {RslsValue}",
			InstructionArgumentType.String => $"String \"{RslsValue}\"",
		};
	}
	
	public static InstructionArgumentAst Constant(long value) => new InstructionArgumentAst(InstructionArgumentType.Constant, value, null);
	public static InstructionArgumentAst Register(string value) => new InstructionArgumentAst(InstructionArgumentType.Register, null, value);
	public static InstructionArgumentAst StarRegister(string value) => new InstructionArgumentAst(InstructionArgumentType.StarRegister, null, value);
	public static InstructionArgumentAst Symbol(string value) => new InstructionArgumentAst(InstructionArgumentType.Symbol, null, value);
	public static InstructionArgumentAst String(string value) => new InstructionArgumentAst(InstructionArgumentType.String, null, value);
}
