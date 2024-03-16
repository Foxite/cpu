using Assembler.Ast;

namespace Assembler.Assembly.V2;

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

		foreach ((string key, SymbolDefinition value) in m_Symbols) {
			if (value.Imported) {
				ret.m_Symbols[key] = value;
			}
		}

		return ret;
	}

	public void IncreaseOutputIndex(int wordCount) {
		OutputIndex += wordCount;
	}

	private SymbolDefinition? GetSymbol(string name, bool optional) {
		if (m_Symbols.TryGetValue(name, out SymbolDefinition? ret)) {
			return ret;
		} else if (optional) {
			return null;
		} else {
			throw new SymbolNotDefinedException(name);
		}
	}

	public InstructionArgumentAst GetSymbolValue(string name) {
		InstructionArgumentAst ret = GetSymbol(name, false)!.Value;

		while (ret.Type == InstructionArgumentType.Symbol) {
			ret = GetSymbol(ret.RslsValue!, false)!.Value;
		}

		return ret;
	}

	public void SetSymbol(SymbolDefinition symbolDefinition) {
		m_Symbols[symbolDefinition.Name] = symbolDefinition;
	}
	
	public bool IsSymbolDefined(string name) {
		var symbol = GetSymbol(name, true);

		while (symbol != null && symbol.Value.Type == InstructionArgumentType.Symbol) {
			symbol = GetSymbol(symbol.Value.RslsValue!, true);
		}

		return symbol != null;
	}
}
