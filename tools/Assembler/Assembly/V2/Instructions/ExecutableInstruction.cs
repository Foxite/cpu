using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record ExecutableInstruction(string? Label, InstructionAst InstructionAst) : AssemblyInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 1;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => new[] { outerContext.InstructionConverter.ConvertInstruction(InstructionAst) };
}
