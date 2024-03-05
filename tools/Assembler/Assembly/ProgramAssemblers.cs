namespace Assembler.Assembly;

public static class ProgramAssemblers {
	public static ProgramAssembler Proc16a { get; } = new Proc16aAssembler();
	public static ProgramAssembler Proc16b { get; } = new Proc16bAssembler();
}
