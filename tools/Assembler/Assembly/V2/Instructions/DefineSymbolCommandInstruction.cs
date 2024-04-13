using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record DefineSymbolCommandInstruction(string File, int Line, string? Label, int Position, string Name, InstructionArgumentAst Value) : CommandInstruction(File, Line, Label, Position) {
	public override int GetWordCount(AssemblyContext context) => 0;
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst> GetDefinedSymbols(AssemblyContext context) {
		return new Dictionary<string, InstructionArgumentAst>() {
			{ Name, Value }
		};
	}

	public override AssemblyInstruction RenderSymbols(AssemblyContext context) {
		return this with {
			Value = Value is SymbolAst valueSymbol ? context.GetSymbolValue(valueSymbol.Value, this) : Value
		};
	}
	
	public override bool HasUnrenderedSymbols() => Value is SymbolAst;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => Array.Empty<ushort>();

	public override string ToString() => $"{File}:{Line} ({Position})  [{Label}] .define {Name}, {Value}";
}
