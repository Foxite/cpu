using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class ProgramAssemblerFactory {
	private readonly IInstructionConverter m_InstructionConverter;
	private readonly IReadOnlyDictionary<string, InstructionArgumentAst> m_GlobalSymbols;
	private readonly MacroProcessor m_MacroProcessor;

	public string Architecture => m_InstructionConverter.Architecture;

	public static IReadOnlyCollection<string> SupportedArchitectures => new []{ "proc16a", "proc16b" };

	public ProgramAssemblerFactory(IInstructionConverter instructionConverter, IMacroProvider macroProvider, IReadOnlyDictionary<string, InstructionArgumentAst>? globalSymbols = null) {
		m_InstructionConverter = instructionConverter;
		m_GlobalSymbols = globalSymbols ?? new Dictionary<string, InstructionArgumentAst>();
		m_MacroProcessor = new MacroProcessor(this, macroProvider);
	}
	
	public ProgramAssembler GetAssembler(AssemblerProgram program, int instructionOffset = 0, IReadOnlyDictionary<string, InstructionArgumentAst>? symbols = null) {
		var mergedSymbols = new Dictionary<string, InstructionArgumentAst>(m_GlobalSymbols);

		if (symbols != null) {
			foreach ((string? key, InstructionArgumentAst? value) in symbols) {
				mergedSymbols[key] = value;
			}
		}

		return new ProgramAssembler(m_InstructionConverter, m_MacroProcessor, program, instructionOffset, mergedSymbols);
	}
	
	public static bool ArchitectureIsSupported(string architecture) {
		return SupportedArchitectures.Contains(architecture.ToLower());
	}

	public static ProgramAssemblerFactory CreateFactory(IMacroProvider macroProvider, string architecture, IReadOnlyDictionary<string, InstructionArgumentAst> globalSymbols) {
		return new ProgramAssemblerFactory(architecture switch {
			"proc16a" => new Proc16aInstructionConverter(),
			"proc16b" => new Proc16bInstructionConverter(),
		}, macroProvider, globalSymbols);
	}
}
