using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record DefineSymbolCommandInstruction(string File, int Line, string? Label, int Position, string Name, InstructionArgumentAst Value) : CommandInstruction(File, Line, Label, Position) {
	/// <summary>
	/// ONLY USE IN TESTS
	/// </summary>
	public DefineSymbolCommandInstruction(string? label, int position, string name, InstructionArgumentAst value) : this("TEST", -1, label, position, name, value) {}

	public override int GetWordCount(AssemblyContext context) => 0;
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst> GetDefinedSymbols(AssemblyContext context) {
		return new Dictionary<string, InstructionArgumentAst>() {
			{ Name, Value }
		};
	}

	public override AssemblyInstruction RenderSymbols(AssemblyContext context) {
		return this with {
			Value = Value switch {
				SymbolAst valueSymbol    => context.GetSymbolValue(valueSymbol.Value, this),
				ExpressionAst expression => new ConstantAst(Value.File, Value.LineNumber, Value.Column, AssemblyUtil.EvaluateExpression(expression, symbol => (IExpressionElement) context.GetSymbolValue(symbol, this))),
				_                        => Value,
			},
		};
	}
	
	public override bool HasUnrenderedSymbols() => Value is SymbolAst;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => Array.Empty<ushort>();

	public virtual bool Equals(DefineSymbolCommandInstruction? other) => other != null && Position == other.Position && Name == other.Name && Value.Equals(other.Value);

	public override string ToString() => $"{File}:{Line} ({Position})  [{Label}] .define {Name}, {Value}";
}
