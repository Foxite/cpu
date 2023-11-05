namespace Assembler.Parsing.Proc16a;

public record SymbolAst(string Name) : IAssemblyAst {
	public override string ToString() => $"symbol {Name}";
}
