using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record MacroInstruction(string? Label, string Name, string Path, IReadOnlyList<AssemblyInstruction> Instructions, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Instructions.Sum(instruction => instruction.GetWordCount(context));
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		AssemblyContext innerContext = outerContext.CreateScope();
		
		// TODO set macro args as symbols
		innerContext.SetSymbol();
		
		return outerContext.Assembler.Assemble(innerContext, Instructions);
	}
}
