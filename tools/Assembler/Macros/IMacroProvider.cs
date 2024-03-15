using Assembler.Ast;

namespace Assembler.Assembly;

public interface IMacroProvider {
	public string GetMacroSource(string name);
	public AssemblerProgram GetMacro(string name);
}