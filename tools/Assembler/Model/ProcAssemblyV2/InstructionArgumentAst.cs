namespace Assembler.Parsing.ProcAssemblyV2;

public record InstructionArgumentAst(InstructionArgumentType Type, long? ConstantValue, string? RslsValue) {
	public override string ToString() {
		return Type switch {
			InstructionArgumentType.Constant => $"Constant {ConstantValue}",
			InstructionArgumentType.Register => $"Register {RslsValue}",
			InstructionArgumentType.Symbol => $"Symbol {RslsValue}",
			InstructionArgumentType.Label => $"Label {RslsValue}",
			InstructionArgumentType.String => $"String \"{RslsValue}\"",
		};
	}
}
