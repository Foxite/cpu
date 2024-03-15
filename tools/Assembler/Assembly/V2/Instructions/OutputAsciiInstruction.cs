using System.Text;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record OutputAsciiInstruction(string? Label, string Ascii) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Ascii.Length;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return Encoding.ASCII.GetBytes(Ascii).Select(word => (ushort) word); // TODO check for attempts at sign preservation, maybe the return type needs to be ushort. Or byte and we switch the whole thing over to 8 bit words and define the isa as big endian.
	}
}
