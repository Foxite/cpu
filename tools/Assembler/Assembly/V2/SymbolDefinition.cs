using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record SymbolDefinition(
	string Name,
	// Symbol is given to included macros. (Global symbol)
	bool Imported,
	InstructionArgumentAst Value
);
