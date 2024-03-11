using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class MacroProcessor : IMacroProcessor {
	private readonly ProcAssemblyParser m_Parser;
	private readonly ProgramAssemblerFactory m_AssemblerFactory;
	private readonly IReadOnlyCollection<string> m_SearchPaths;

	public MacroProcessor(ProcAssemblyParser parser, ProgramAssemblerFactory assemblerFactory, IReadOnlyCollection<string> searchPaths) {
		m_Parser = parser;
		m_AssemblerFactory = assemblerFactory;
		m_SearchPaths = searchPaths;
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
	
	public AssemblerProgram GetMacro(string name) {
		string path = GetMacroPath(name);
		return new AssemblerProgram(name, path, m_Parser.Parse(File.ReadAllText(path)));
	}

	public IEnumerable<ushort> AssembleMacro(int instructionOffset, AssemblerProgram program, IReadOnlyList<InstructionArgumentAst> arguments) {
		return m_AssemblerFactory.GetAssembler(program, this, instructionOffset).Assemble();
	}
}
