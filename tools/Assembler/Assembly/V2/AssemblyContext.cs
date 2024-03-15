using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class AssemblyContext {
	private Dictionary<string, SymbolDefinition> m_Symbols = new();
	
	public IMacroProvider MacroProvider { get; }
	public IInstructionConverter InstructionConverter { get; }
	public ProgramAssemblerv2 Assembler { get; }
	
	public int OutputIndex { get; private set; }
	
	public AssemblyContext(IMacroProvider macroProvider, IInstructionConverter instructionConverter, ProgramAssemblerv2 assembler) {
		MacroProvider = macroProvider;
		InstructionConverter = instructionConverter;
		Assembler = assembler;
	}

	public AssemblyContext CreateScope() {
		var ret = new AssemblyContext(MacroProvider, InstructionConverter, Assembler);

		/*
		foreach ((string key, SymbolDefinition value) in m_Symbols) {
			if (value.Imported) {
				ret.m_Symbols[key] = value;
			}
		}*/

		return ret;
	}

	public void IncreaseOutputIndex(int wordCount) {
		OutputIndex += wordCount;
	}

	public InstructionArgumentAst GetSymbol(string name) {
		return m_Symbols[name].Value;
	}

	public void SetSymbol(string name, SymbolDefinition symbolDefinition) {
		m_Symbols[name] = symbolDefinition;
	}
}
