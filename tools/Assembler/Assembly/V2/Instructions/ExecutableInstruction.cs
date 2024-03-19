using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record ExecutableInstruction(string? Label, InstructionAst InstructionAst) : AssemblyInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 1;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => new[] { outerContext.InstructionConverter.ConvertInstruction(InstructionAst) };
	public override bool HasUnrenderedSymbols() => InstructionAst.Arguments.Any(arg => arg is SymbolAst);
	
	public override IEnumerable<AssemblyInstruction> Render(AssemblyContext context) {
		return new[] {
			this with {
				InstructionAst = InstructionAst with {
					Arguments = context.ReplaceSymbols(InstructionAst.Arguments)
				}
			}
		};
	}
}
