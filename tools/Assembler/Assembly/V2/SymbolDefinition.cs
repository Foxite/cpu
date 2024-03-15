using Assembler.Ast;

namespace Assembler.Assembly.V2;

public record SymbolDefinition(
	string Name,
	// Symbol is given to included macros.
	//bool Imported,
	InstructionArgumentAst Value
);
