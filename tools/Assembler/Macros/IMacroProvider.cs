using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public interface IMacroProvider {
	public string GetMacroSource(string name);
	public AssemblerProgram GetMacro(string name);
}