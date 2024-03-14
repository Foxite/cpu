using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

public class ProgramAssemblerv2 {
	public List<AssemblyInstruction> CompileInstructionList(AssemblerProgram program) {
		var ret = new List<AssemblyInstruction>();

		var context = new AssemblyContext();

		foreach (var statement in program.ProgramAst.Statements) {
			
		}
	}
}

public class AssemblyContext {
	public IMacroProvider MacroProvider { get; }
	public MacroProcessor MacroProcessor { get; }
	public IInstructionConverter InstructionConverter { get; }
	public ProgramAssemblerv2 Assembler { get; }
	
	public int InstructionOffset { get; }
	public AssemblerProgram Program { get; }

	public AssemblyContext(IMacroProvider macroProvider, MacroProcessor macroProcessor, IInstructionConverter instructionConverter, ProgramAssemblerv2 assembler, int instructionOffset, AssemblerProgram program) {
		MacroProvider = macroProvider;
		MacroProcessor = macroProcessor;
		InstructionConverter = instructionConverter;
		Assembler = assembler;
		InstructionOffset = instructionOffset;
		Program = program;
	}
}

public class AssemblyInstructionFactory {
	public AssemblyInstruction Create(AssemblyContext context, InstructionAst instruction) {
		if (instruction.Mnemonic.StartsWith("@")) {
			// TODO
			// Make sure macros are recursively resolved
			
		} else if (instruction.Mnemonic.StartsWith(".")) {
			// .const
			// .reg
			// .bytes
			// .ascii
			return CommandInstruction.Create(context, instruction);
		} else {
			return new ExecutableInstruction(instruction.Mnemonic, instruction.Arguments);
		}
	}
}

public abstract record AssemblyInstruction() {
	public abstract int GetWordCount(AssemblyContext context);
	public abstract IEnumerable<short> Assemble(AssemblyContext context);
}

public abstract record CommandInstruction : AssemblyInstruction;

public record ExecutableInstruction(string Mnemonic, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction {
}

public record MacroInstruction(string Name, IReadOnlyList<AssemblyInstruction> Instructions) : AssemblyInstruction {
}
