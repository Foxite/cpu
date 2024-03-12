using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class FileMacroProvider : IMacroProvider {
	private readonly ProcAssemblyParser m_Parser;
	private readonly IReadOnlyCollection<string> m_SearchPaths;

	public FileMacroProvider(ProcAssemblyParser parser, IReadOnlyCollection<string> searchPaths) {
		m_Parser = parser;
		m_SearchPaths = searchPaths;
	}
	
	public string GetMacroSource(string name) {
		string path = GetMacroPath(name);
		return File.ReadAllText(path);
	}
	
	public AssemblerProgram GetMacro(string name) {
		string path = GetMacroPath(name);
		return new AssemblerProgram(name, path, m_Parser.Parse(File.ReadAllText(path)));
	}
	
	private string GetMacroPath(string name) {
		foreach (string searchPath in m_SearchPaths) {
			var files = Directory.GetFiles(searchPath, "*.pa2");
			string? result = files.FirstOrDefault(path => Path.GetFileNameWithoutExtension(path) == name);
			if (result != null) {
				return result;
			}
		}

		throw new FileNotFoundException($"Could not find macro definition named {name} in any of the provided search paths");
	}
}
