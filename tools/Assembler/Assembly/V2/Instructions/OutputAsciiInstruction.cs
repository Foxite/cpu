using System.Text;
using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record OutputAsciiInstruction(string File, int Line, string? Label, int Position, string Ascii) : CommandInstruction(File, Line, Label, Position) {
	public override int GetWordCount(AssemblyContext context) => Ascii.Length;
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override AssemblyInstruction RenderSymbols(AssemblyContext context) => this;
	
	public override bool HasUnrenderedSymbols() => false;

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return Encoding.ASCII.GetBytes(Ascii).Select(word => (ushort) word);
	}

	public override string ToString() => $"{File}:{Line} ({Position})  [{Label}] .ascii \"{Ascii}\"";
}
