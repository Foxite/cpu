using Assembler.Ast;

namespace Assembler.Assembly;

public class InvalidProcAssemblyProgramException : Exception {
	public AssemblerProgram Program { get; }
	public string ArchitectureName { get; }
	public IReadOnlyCollection<InvalidInstruction> Instructions { get; }
	
	public InvalidProcAssemblyProgramException(AssemblerProgram program, string architectureName, IReadOnlyCollection<InvalidInstruction> instructions) {
		Program = program;
		ArchitectureName = architectureName;
		Instructions = instructions;
	}
}
