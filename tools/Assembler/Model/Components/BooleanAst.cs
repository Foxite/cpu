namespace Assembler;

public record BooleanAst(bool Value) : IAssemblyAst {
	public override string ToString() => $"boolean {Value}";
}
