using Assembler.Assembly.V2;

namespace Assembler.Assembly;

public class SymbolNotDefinedException : Exception {
	public string Symbol { get; }
	public AssemblyInstruction? Instruction { get; }

	public SymbolNotDefinedException(string symbol, AssemblyInstruction? instruction) {
		Symbol = symbol;
		Instruction = instruction;
	}

	public override string ToString() => $"Symbol not defined: {Symbol} in instruction: {Instruction?.ToString() ?? "not specified"}";
}
