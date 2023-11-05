namespace Assembler.Parsing.Proc16a;

public record BooleanAst(bool Value) : IAssemblyAst {
	public override string ToString() => $"boolean {Value}";
}
