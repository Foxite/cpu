using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public interface IInstructionConverter {
	public InstructionSupport ValidateInstruction(InstructionAst instructionAst);
	public ushort ConvertInstruction(InstructionAst instructionAst);
}

public abstract class InstructionMapInstructionConverter : IInstructionConverter {
	public string ArchitectureName { get; }
	protected Dictionary<string, Instruction> Instructions { get; } = new();
	
	protected InstructionMapInstructionConverter(string architectureName) {
		ArchitectureName = architectureName;
	}

	public InstructionSupport ValidateInstruction(InstructionAst instructionAst) {
		if (!Instructions.TryGetValue(instructionAst.Mnemonic, out Instruction? instruction)) {
			return InstructionSupport.NotRecognized;
		}
		
		return instruction.Validate(instructionAst.Arguments);
	}
	
	public ushort ConvertInstruction(InstructionAst instructionAst) {
		Instructions.TryGetValue(instructionAst.Mnemonic, out Instruction? instruction);
		
		return instruction!.Convert(instructionAst.Arguments);
	}
	
	protected bool AddInstruction(string term, Instruction instruction) {
		return Instructions.TryAdd(term, instruction);
	}

	protected abstract record Instruction() {
		public abstract InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> arguments);
		public abstract ushort Convert(IReadOnlyList<InstructionArgumentAst> arguments);
	}
}
