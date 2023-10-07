namespace Assembler;

public record ConstantAst(short Value) : IAssemblyAst {
	public override string ToString() => $"constant {Value}";
}
