using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public interface IInstructionConverter {
	public string Architecture { get; }
	
	public InstructionSupport ValidateInstruction(InstructionAst instructionAst);
	public ushort ConvertInstruction(InstructionAst instructionAst);
}
