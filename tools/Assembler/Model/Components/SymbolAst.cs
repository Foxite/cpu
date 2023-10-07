namespace Assembler;

public record SymbolAst(string Name) : IAssemblyAst {
	public override string ToString() => $"symbol {Name}";
}
