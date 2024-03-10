using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public abstract class InstructionMapProgramAssembler : ProgramAssembler {
	public override string ArchitectureName { get; }
	protected Dictionary<string, Instruction> Instructions { get; } = new();
	
	protected InstructionMapProgramAssembler(string architectureName) {
		ArchitectureName = architectureName;
	}

	protected internal override InstructionSupport ValidateInstruction(InstructionAst instructionAst) {
		if (!Instructions.TryGetValue(instructionAst.Mnemonic, out Instruction? instruction)) {
			return InstructionSupport.NotRecognized;
		}
		
		return instruction.Validate(instructionAst.Arguments);
	}
	
	protected internal override ushort ConvertInstruction(InstructionAst instructionAst) {
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

public enum InstructionSupport {
	Supported,
	NotRecognized,
	ParameterType,
	OtherError,
}
