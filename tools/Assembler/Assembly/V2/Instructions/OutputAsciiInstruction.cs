using System.Text;
using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record OutputAsciiInstruction(string? Label, string Ascii) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Ascii.Length;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<AssemblyInstruction> Render(AssemblyContext context) => new[] { this };

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return Encoding.ASCII.GetBytes(Ascii).Select(word => (ushort) word);
	}
}
