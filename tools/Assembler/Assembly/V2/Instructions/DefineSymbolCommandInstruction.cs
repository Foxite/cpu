using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record DefineSymbolCommandInstruction(string? Label, string Name, InstructionArgumentAst Value) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 0;
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => Array.Empty<ushort>();
	
	public override IEnumerable<AssemblyInstruction> Render(AssemblyContext context) {
		return new[] {
			this with {
				Value = Value.Type == InstructionArgumentType.Symbol ? context.GetSymbolValue(Value.RslsValue!) : Value
			}
		};
	}
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) {
		return new Dictionary<string, InstructionArgumentAst>() {
			{ Name, Value }
		};
	}
}
