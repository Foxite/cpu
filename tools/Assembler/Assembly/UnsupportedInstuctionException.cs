namespace Assembler.Assembly;

public class InvalidProcAssemblyProgramException : Exception {
	public string ArchitectureName { get; }
	public IReadOnlyCollection<InvalidInstruction> Instructions { get; }
	
	public InvalidProcAssemblyProgramException(string architectureName, IReadOnlyCollection<InvalidInstruction> instructions) {
		ArchitectureName = architectureName;
		Instructions = instructions;
	}
}
