using Assembler.Ast;

namespace Assembler.Assembly;

public abstract class InstructionMapInstructionConverter : IInstructionConverter {
	public string Architecture { get; }
	protected Dictionary<string, Instruction> Instructions { get; } = new();
	
	protected InstructionMapInstructionConverter(string architecture) {
		Architecture = architecture;
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
