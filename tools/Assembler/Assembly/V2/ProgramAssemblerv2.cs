using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

public class ProgramAssemblerv2 {
	private readonly IMacroProvider m_MacroProvider;
	private readonly IInstructionConverter m_InstructionConverter;
	
	public ProgramAssemblerv2(IMacroProvider macroProvider, IInstructionConverter instructionConverter) {
		m_MacroProvider = macroProvider;
		m_InstructionConverter = instructionConverter;
		
		//var context = new AssemblyContext(m_MacroProvider, m_InstructionConverter, this);
	}
	
	public List<AssemblyInstruction> CompileInstructionList(AssemblyContext context, AssemblerProgram program) {
		var ret = new List<AssemblyInstruction>();

		var instructionFactory = new AssemblyInstructionFactory();

		foreach (ProgramStatementAst statement in program.ProgramAst.Statements) {
			ret.Add(instructionFactory.Create(context, statement));
		}

		return ret;
	}

	public IReadOnlyList<ushort> Assemble(AssemblyContext context, IReadOnlyList<AssemblyInstruction> instructions) {
		int labelIndex = context.OutputIndex;
		foreach (AssemblyInstruction instruction in instructions) {
			if (instruction.Label != null) {
				context.SetSymbol(instruction.Label, new SymbolDefinition(instruction.Label, new InstructionArgumentAst(InstructionArgumentType.Constant, labelIndex, null)));
			}

			labelIndex += instruction.GetWordCount(context);
		}
		
		var ret = new List<ushort>();
		foreach (AssemblyInstruction instruction in instructions) {
			var definedSymbols = instruction.GetDefinedSymbols(context);
			if (definedSymbols != null) {
				foreach ((string? key, InstructionArgumentAst? value) in definedSymbols) {
					context.SetSymbol(key, new SymbolDefinition(key, value));
				}
			}

			ret.AddRange(instruction.Assemble(context));
		}

		return ret;
	}
}
