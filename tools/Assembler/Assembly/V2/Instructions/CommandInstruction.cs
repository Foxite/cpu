namespace Assembler.Assembly.V2;

public abstract record CommandInstruction(string File, int Line, string? Label, int Position) : AssemblyInstruction(File, Line, Label, Position) {
	public override IEnumerable<InvalidInstruction> Validate(AssemblyContext context) => Array.Empty<InvalidInstruction>();
}
