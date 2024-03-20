using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record OutputWordsCommandInstruction(string File, int Line, string? Label, IReadOnlyList<InstructionArgumentAst> Words) : CommandInstruction(File, Line, Label) {
	public override int GetWordCount(AssemblyContext context) => Words.Count;
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override AssemblyInstruction RenderSymbols(AssemblyContext context) {
		return this with {
			Words = context.ReplaceSymbols(Words)
		};
	}
	
	public override bool HasUnrenderedSymbols() => Words.Any(arg => arg is SymbolAst);
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return Words.Select(argument => (ushort) ((ConstantAst) argument).Value);
	}
}
