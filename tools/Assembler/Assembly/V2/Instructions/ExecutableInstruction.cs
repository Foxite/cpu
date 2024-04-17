using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record ExecutableInstruction(string File, int Line, string? Label, int Position, InstructionAst InstructionAst) : AssemblyInstruction(File, Line, Label, Position) {
	/// <summary>
	/// ONLY USE IN TESTS
	/// </summary>
	public ExecutableInstruction(string? label, int position, InstructionAst instructionAst) : this("TEST", -1, label, position, instructionAst) {}
	
	public override int GetWordCount(AssemblyContext context) => 1;
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;

	public override AssemblyInstruction RenderSymbols(AssemblyContext context) => this with {
		InstructionAst = InstructionAst with {
			Arguments = context.ReplaceSymbols(InstructionAst.Arguments, this),
		},
	};

	public override bool HasUnrenderedSymbols() => InstructionAst.Arguments.Any(arg => arg is SymbolAst);
	
	public override IEnumerable<InvalidInstruction> Validate(AssemblyContext context) {
		InstructionSupport result = context.InstructionConverter.ValidateInstruction(InstructionAst);
		if (result != InstructionSupport.Supported) {
			yield return new InvalidInstruction(InstructionAst, result, result.ToString());
		}
	}
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => new[] { outerContext.InstructionConverter.ConvertInstruction(InstructionAst) };

	public override string ToString() => $"{File}:{Line} ({Position})  [{Label}] {InstructionAst}";

	public virtual bool Equals(ExecutableInstruction? other) => other != null && Position == other.Position && InstructionAst.Equals(other.InstructionAst);
}
