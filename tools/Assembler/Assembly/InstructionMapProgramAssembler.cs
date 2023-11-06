using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class InstructionMapProgramAssembler : ProgramAssembler {
	public override string ArchitectureName { get; }
	protected Dictionary<string, Instruction> Instructions { get; } = new Dictionary<string, Instruction>();
	
	public InstructionMapProgramAssembler(string architectureName) {
		ArchitectureName = architectureName;
	}

	// TODO Unit test
	protected internal override bool ValidateInstruction(InstructionAst instructionAst) {
		if (!Instructions.TryGetValue(instructionAst.Instruction, out Instruction? instruction)) {
			return false;
		}
		
		return instruction.Validate(instructionAst.Arguments);
	}
	
	// TODO Unit test
	protected internal override ushort ConvertInstruction(InstructionAst instructionAst, Func<string, ushort> getSymbolDefinition) {
		Instructions.TryGetValue(instructionAst.Instruction, out Instruction? instruction);
		
		return instruction!.Convert(instructionAst.Arguments.Select(arg => {
			if (arg.Type == InstructionArgumentType.Symbol) {
				return new InstructionArgumentAst(InstructionArgumentType.Constant, getSymbolDefinition(arg.RslsValue!), null);
			} else {
				return arg;
			}
		}).ToList());
	}

	public bool AddInstruction(string term, Instruction instruction) {
		return Instructions.TryAdd(term, instruction);
	}

	public abstract record Instruction() {
		public abstract bool Validate(IReadOnlyList<InstructionArgumentAst> arguments);
		public abstract ushort Convert(IReadOnlyList<InstructionArgumentAst> arguments);

		// TODO Unit test
		protected bool ValidateArgumentTypes(IReadOnlyList<InstructionArgumentAst> arguments, params InstructionArgumentType[][] types) {
			return types.Any(overload => overload.SequenceEqual(arguments.Select(argument => argument.Type)));
		}
	}
}