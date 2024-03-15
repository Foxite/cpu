using Assembler.Ast;

namespace Assembler.Assembly.V2;

public class AssemblyContextFactory {
	private readonly IMacroProvider m_MacroProvider;
	private readonly IInstructionConverter m_InstructionConverter;
	
	public static IReadOnlyCollection<string> SupportedArchitectures => new []{ "proc16a", "proc16b" };

	public AssemblyContextFactory(IMacroProvider macroProvider, IInstructionConverter instructionConverter) {
		m_MacroProvider = macroProvider;
		m_InstructionConverter = instructionConverter;
	}
	
	public AssemblyContext CreateContext(IReadOnlyDictionary<string, InstructionArgumentAst>? symbols, ProgramAssemblerv2 assembler) {
		var ret = new AssemblyContext(m_MacroProvider, m_InstructionConverter, assembler);

		if (symbols != null) {
			foreach ((string key, InstructionArgumentAst value) in symbols) {
				ret.SetSymbol(new SymbolDefinition(key, value));
			}
		}

		return ret;
	}
	
	public static bool ArchitectureIsSupported(string architecture) {
		return SupportedArchitectures.Contains(architecture.ToLower());
	}

	public static AssemblyContextFactory CreateFactory(IMacroProvider macroProvider, string architecture) {
		return new AssemblyContextFactory(macroProvider, architecture switch {
			"proc16a" => new Proc16aInstructionConverter(),
			"proc16b" => new Proc16bInstructionConverter(),
		});
	}
}
