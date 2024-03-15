using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record MacroInstruction(string? Label, string Name, string Path, IReadOnlyList<AssemblyInstruction> Instructions, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction(Label) {
	private AssemblyContext ScopeContext(AssemblyContext outerContext) {
		AssemblyContext innerContext = outerContext.CreateScope();
		
		for (int i = 0; i < Arguments.Count; i++) {
			innerContext.SetSymbol(new SymbolDefinition($"macro{i}", Arguments[i]));
		}

		return innerContext;
	}
	
	public override int GetWordCount(AssemblyContext context) => Instructions.Sum(instruction => instruction.GetWordCount(ScopeContext(context)));
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<AssemblyInstruction> Render(AssemblyContext outerContext) {
		AssemblyContext innerContext = ScopeContext(outerContext);
		return Instructions.SelectMany(instruction => instruction.Render(innerContext));
	}

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return outerContext.Assembler.AssembleMachineCode(ScopeContext(outerContext), Instructions);
	}
}
