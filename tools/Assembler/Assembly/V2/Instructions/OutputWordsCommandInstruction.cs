using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record OutputWordsCommandInstruction(string? Label, IReadOnlyList<InstructionArgumentAst> Words) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Words.Count;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<AssemblyInstruction> Render(AssemblyContext context) {
		return new[] {
			this with {
				Words = context.ReplaceSymbols(Words)
			}
		};
	}
	
	public override bool HasUnrenderedSymbols() => Words.Any(arg => arg.Type == InstructionArgumentType.Symbol);
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return Words.Select(argument => (ushort) argument.ConstantValue!.Value);
	}
}
