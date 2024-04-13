using Assembler.Ast;

namespace Assembler.Assembly.V2;

public class AssemblyContext {
	private Dictionary<string, SymbolDefinition> m_Symbols = new();
	
	public IMacroProvider MacroProvider { get; }
	public IInstructionConverter InstructionConverter { get; }
	public ProgramAssemblerv2 Assembler { get; }
	
	public int OutputOffset { get; }
	
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

	private SymbolDefinition GetSymbol(string name, AssemblyInstruction instruction) {
		if (m_Symbols.TryGetValue(name, out SymbolDefinition? ret)) {
			return ret;
		} else {
			throw new SymbolNotDefinedException(name, instruction);
		}
	}

	private SymbolDefinition? GetSymbolOptional(string name) {
		if (m_Symbols.TryGetValue(name, out SymbolDefinition? ret)) {
			return ret;
		} else {
			return null;
		}
	}

	public InstructionArgumentAst GetSymbolValue(string name, AssemblyInstruction instruction) {
		InstructionArgumentAst ret = GetSymbol(name, instruction).Value;

		while (ret is SymbolAst symbolAst) {
			ret = GetSymbol(symbolAst.Value, instruction).Value;
		}

		return ret;
	}

	public void SetSymbol(SymbolDefinition symbolDefinition) {
		m_Symbols[symbolDefinition.Name] = symbolDefinition;
	}
	
	public bool IsSymbolDefined(string name) {
		SymbolDefinition? ret = GetSymbolOptional(name);

		while (ret != null && ret.Value is SymbolAst symbolValue) {
			ret = GetSymbolOptional(symbolValue.Value);
		}

		return ret != null;
	}
}
