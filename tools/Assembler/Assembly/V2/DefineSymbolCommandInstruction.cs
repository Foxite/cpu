using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record DefineSymbolCommandInstruction(string? Label, string Name, InstructionArgumentAst Value) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 0;
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => Array.Empty<ushort>();
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) {
		return new Dictionary<string, InstructionArgumentAst>() {
			{ Name, Value }
		};
	}
}
