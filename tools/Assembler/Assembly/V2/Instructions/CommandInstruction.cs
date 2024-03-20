namespace Assembler.Assembly.V2;

public abstract record CommandInstruction(string File, int Line, string? Label) : AssemblyInstruction(File, Line, Label) {
	public override IEnumerable<InvalidInstruction> Validate(AssemblyContext context) => Array.Empty<InvalidInstruction>();
}
