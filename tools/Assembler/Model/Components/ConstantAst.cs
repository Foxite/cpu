namespace Assembler;

public record ConstantAst(long Value) : IAssemblyAst {
	public override string ToString() => $"constant {Value}";
}
