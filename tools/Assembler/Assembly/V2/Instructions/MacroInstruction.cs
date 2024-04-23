using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record MacroInstruction(string File, int Line, string? Label, int Position, string MacroPath, IReadOnlyList<AssemblyInstruction> Instructions, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction(File, Line, Label, Position) {
	private AssemblyContext ScopeContext(AssemblyContext outerContext, bool resolveSymbols) {
		AssemblyContext innerContext = outerContext.CreateScope(Position);
		
		for (int i = 0; i < Arguments.Count; i++) {
			var argument = Arguments[i];
			if (resolveSymbols) {
				argument = outerContext.GetSymbolValue(argument, this);
			}
			
			innerContext.SetSymbol(new SymbolDefinition($"macro{i}", false, argument));
		}

		return innerContext;
	}
	
	public override int GetWordCount(AssemblyContext context) {
		int ret = 0;

		foreach (AssemblyInstruction instruction in Instructions) {
			ret += instruction.GetWordCount(ScopeContext(context, false));
		}

		return ret;
	}

	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override AssemblyInstruction RenderSymbols(AssemblyContext outerContext) => this with {
		Arguments = outerContext.ReplaceSymbols(Arguments, this),
		Instructions = outerContext.Assembler.RenderSymbols(ScopeContext(outerContext, true), Instructions),
	};

	public override bool HasUnrenderedSymbols() => Instructions.Any(instruction => instruction.HasUnrenderedSymbols());
	
	public override IEnumerable<InvalidInstruction> Validate(AssemblyContext context) => context.Assembler.ValidateInstructions(ScopeContext(context, true), Instructions);

	public override IEnumerable<AssemblyInstruction> RenderInstructions(AssemblyContext outerContext) => outerContext.Assembler.RenderInstructions(ScopeContext(outerContext, true), Instructions);

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => outerContext.Assembler.AssembleMachineCode(ScopeContext(outerContext, true), Instructions);

	public override string ToString() => $"{File}:{Line} ({Position})  [{Label}] @{MacroPath} {string.Join(", ", Arguments)}";

	public override string ToShortString() => $"@{Path.GetFileNameWithoutExtension(MacroPath)} {string.Join(", ", Arguments)}";
}
