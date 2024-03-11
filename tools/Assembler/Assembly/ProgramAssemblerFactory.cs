using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class ProgramAssemblerFactory {
	private readonly IInstructionConverter m_InstructionConverter;

	public string Architecture => m_InstructionConverter.Architecture;

	public static IReadOnlyCollection<string> SupportedArchitectures => new []{ "proc16a", "proc16b" };

	public ProgramAssemblerFactory(IInstructionConverter instructionConverter) {
		m_InstructionConverter = instructionConverter;
	}
	
	public ProgramAssembler GetAssembler(AssemblerProgram program, MacroProcessor macroProcessor, int instructionOffset = 0, IReadOnlyDictionary<string, InstructionArgumentAst>? symbols = null) {
		return new ProgramAssembler(m_InstructionConverter, macroProcessor, program, instructionOffset, symbols);
	}
	
	public static bool ArchitectureIsSupported(string architecture) {
		return SupportedArchitectures.Contains(architecture.ToLower());
	}

	public static ProgramAssemblerFactory CreateFactory(string architecture) {
		return new ProgramAssemblerFactory(architecture switch {
			"proc16a" => new Proc16aInstructionConverter(),
			"proc16b" => new Proc16bInstructionConverter(),
		});
	}
}
