using Assembler.Ast;

namespace Assembler.Assembly.V2;

public abstract record AssemblyInstruction(string File, int Line, string? Label, int Position) {
	public abstract int GetWordCount(AssemblyContext context);
	public abstract IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context);
	public abstract AssemblyInstruction RenderSymbols(AssemblyContext context);
	public abstract bool HasUnrenderedSymbols();
	public abstract IEnumerable<InvalidInstruction> Validate(AssemblyContext context);
	public virtual IEnumerable<AssemblyInstruction> RenderInstructions(AssemblyContext context) => new[] { this };
	public abstract IEnumerable<ushort> Assemble(AssemblyContext outerContext);
	public abstract string ToShortString();
}
