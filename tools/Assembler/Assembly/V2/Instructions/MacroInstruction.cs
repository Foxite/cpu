using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record MacroInstruction(string File, int Line, string? Label, string MacroPath, IReadOnlyList<AssemblyInstruction> Instructions, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction(File, Line, Label) {
	private AssemblyContext ScopeContext(AssemblyContext outerContext, bool resolveSymbols) {
		AssemblyContext innerContext = outerContext.CreateScope();
		
		for (int i = 0; i < Arguments.Count; i++) {
			var argument = Arguments[i];
			if (resolveSymbols && argument is SymbolAst symbolArgument) {
				argument = outerContext.GetSymbolValue(symbolArgument.Value, this);
			}
			
			innerContext.SetSymbol(new SymbolDefinition($"macro{i}", false, argument));
		}

		return innerContext;
	}
	
	public override int GetWordCount(AssemblyContext context) => Instructions.Sum(instruction => instruction.GetWordCount(ScopeContext(context, false)));

	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override AssemblyInstruction RenderSymbols(AssemblyContext outerContext) => this with {
		Arguments = outerContext.ReplaceSymbols(Arguments, this),
		Instructions = outerContext.Assembler.RenderSymbols(ScopeContext(outerContext, true), Instructions),
	};

	public override bool HasUnrenderedSymbols() => Instructions.Any(instruction => instruction.HasUnrenderedSymbols());
	
	public override IEnumerable<InvalidInstruction> Validate(AssemblyContext context) => context.Assembler.ValidateInstructions(ScopeContext(context, true), Instructions);

	public override IEnumerable<AssemblyInstruction> RenderInstructions(AssemblyContext outerContext) => outerContext.Assembler.RenderInstructions(ScopeContext(outerContext, true), Instructions);

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => outerContext.Assembler.AssembleMachineCode(ScopeContext(outerContext, true), Instructions);
}
