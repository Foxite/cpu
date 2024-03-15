using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record SymbolDefinition(
	string Name,
	// Symbol is given to included macros.
	//bool Imported,
	InstructionArgumentAst Value
);
