namespace Assembler.Parsing.Proc16a;

public record ConstantAst(long Value) : IAssemblyAst {
	public override string ToString() => $"constant {Value}";
}
