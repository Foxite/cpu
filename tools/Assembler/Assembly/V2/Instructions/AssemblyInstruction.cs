using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public abstract record AssemblyInstruction(string? Label) {
	public abstract int GetWordCount(AssemblyContext context);
	public abstract IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context);
	public abstract IEnumerable<ushort> Assemble(AssemblyContext outerContext);
}
