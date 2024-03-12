using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class DictionaryMacroProvider : IMacroProvider {
	private readonly ProcAssemblyParser m_Parser;
	private readonly Dictionary<string, string> m_Macros = new();

	public DictionaryMacroProvider(ProcAssemblyParser parser) {
		m_Parser = parser;
	}

	public void AddMacro(string name, string source) {
		m_Macros[name] = source;
	}

	public string GetMacroSource(string name) {
		return m_Macros[name];
	}
	
	public AssemblerProgram GetMacro(string name) {
		return new AssemblerProgram(name, "dictionary", m_Parser.Parse(GetMacroSource(name)));
	}
}
