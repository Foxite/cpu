using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public interface IMacroProcessor {
	AssemblerProgram GetMacro(string name);
	IEnumerable<ushort> AssembleMacro(int instructionOffset, AssemblerProgram program, IReadOnlyList<InstructionArgumentAst> arguments);
}
