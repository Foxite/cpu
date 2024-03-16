using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record MacroInstruction(string? Label, string Name, string Path, IReadOnlyList<AssemblyInstruction> Instructions, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction(Label) {
	private AssemblyContext ScopeContext(AssemblyContext outerContext, bool resolveSymbols) {
		AssemblyContext innerContext = outerContext.CreateScope();
		
		for (int i = 0; i < Arguments.Count; i++) {
			var argument = Arguments[i];
			if (resolveSymbols && argument.Type == InstructionArgumentType.Symbol) {
				argument = outerContext.GetSymbolValue(Arguments[i].RslsValue!);
			}
			
			innerContext.SetSymbol(new SymbolDefinition($"macro{i}", false, argument));
		}

		return innerContext;
	}
	
	public override int GetWordCount(AssemblyContext context) => Instructions.Sum(instruction => instruction.GetWordCount(ScopeContext(context, false)));
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<AssemblyInstruction> Render(AssemblyContext outerContext) {
		//AssemblyContext innerContext = ScopeContext(outerContext, true);
		//return Instructions.SelectMany(instruction => instruction.Render(innerContext));

		AssemblyContext innerContext = ScopeContext(outerContext, true);
		return outerContext.Assembler.RenderInstructions(innerContext, Instructions);
	}
	
	public override bool HasUnrenderedSymbols() => throw new Exception($"Logic error: {nameof(MacroInstruction)} should not be present in rendered instructions");

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return outerContext.Assembler.AssembleMachineCode(ScopeContext(outerContext, true), Instructions);
	}
}
