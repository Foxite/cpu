using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class MacroProcessor {
	private readonly ProgramAssemblerFactory m_AssemblerFactory;

	public MacroProcessor(ProgramAssemblerFactory assemblerFactory) {
		m_AssemblerFactory = assemblerFactory;
	}

	public IEnumerable<ushort> AssembleMacro(int instructionOffset, AssemblerProgram program, IReadOnlyList<InstructionArgumentAst> arguments) {
		var macroArguments = new Dictionary<string, InstructionArgumentAst>();

		for (int i = 0; i < arguments.Count; i++) {
			macroArguments[$"macro{i}"] = arguments[i];
		}
		
		ProgramAssembler assembler = m_AssemblerFactory.GetAssembler(program, instructionOffset, macroArguments);
		
		return assembler.Assemble();
	}
}
