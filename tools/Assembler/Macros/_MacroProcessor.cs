using Assembler.Ast;

namespace Assembler.Assembly.V1;

public class _MacroProcessor {
	private readonly _ProgramAssemblerFactory m_AssemblerFactory;
	private readonly IMacroProvider m_MacroProvider;

	public _MacroProcessor(_ProgramAssemblerFactory assemblerFactory, IMacroProvider macroProvider) {
		m_AssemblerFactory = assemblerFactory;
		m_MacroProvider = macroProvider;
	}

	public IEnumerable<ushort> AssembleMacro(int instructionOffset, string includeName, IReadOnlyList<InstructionArgumentAst> arguments) {
		var macroArguments = new Dictionary<string, InstructionArgumentAst>();

		for (int i = 0; i < arguments.Count; i++) {
			macroArguments[$"macro{i}"] = arguments[i];
		}
		
		AssemblerProgram program = m_MacroProvider.GetMacro(includeName);
		_ProgramAssembler assembler = m_AssemblerFactory.GetAssembler(program, instructionOffset, macroArguments);
		
		return assembler.Assemble();
	}
	
	public int GetInstructionCount(string name) {
		var macroProgram = m_MacroProvider.GetMacro(name);
		int ret = 0;
		foreach (ProgramStatementAst statement in macroProgram.ProgramAst.Statements) {
			if (statement.Instruction.Mnemonic.StartsWith(".")) {
				// pass
			} else if (statement.Instruction.Mnemonic.StartsWith("@")) {
				ret += GetInstructionCount(statement.Instruction.Mnemonic[1..]);
			} else {
				ret += 1;
			}
		}

		return ret;
	}
}
