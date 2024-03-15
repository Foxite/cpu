using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record OutputWordsCommandInstruction(string? Label, IReadOnlyList<InstructionArgumentAst> WordArguments) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => WordArguments.Count;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return WordArguments.Select(argument => (ushort) argument.ConstantValue!.Value);
	}
}
