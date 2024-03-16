using Assembler.Ast;

namespace Assembler.Assembly.V2;

public abstract record AssemblyInstruction(string? Label) {
	public abstract int GetWordCount(AssemblyContext context);
	public abstract IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context);
	public abstract IEnumerable<AssemblyInstruction> Render(AssemblyContext context);
	public abstract bool HasUnrenderedSymbols();
	public abstract IEnumerable<ushort> Assemble(AssemblyContext outerContext);
}
