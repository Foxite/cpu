using Assembler.Ast;

namespace Assembler.Assembly.V2;

public class ProgramAssemblerv2 {
	public List<AssemblyInstruction> CompileInstructionList(AssemblyContext context, AssemblerProgram program) {
		var ret = new List<AssemblyInstruction>();

		var instructionFactory = new AssemblyInstructionFactory();

		foreach (ProgramStatementAst statement in program.ProgramAst.Statements) {
			ret.Add(instructionFactory.Create(context, statement));
		}

		return ret;
	}

	public IReadOnlyList<AssemblyInstruction> RenderInstructions(AssemblyContext context, IReadOnlyList<AssemblyInstruction> instructions) {
		int labelIndex = context.OutputIndex;
		foreach (AssemblyInstruction instruction in instructions) {
			if (instruction.Label != null) {
				context.SetSymbol(new SymbolDefinition(instruction.Label, false, new InstructionArgumentAst(InstructionArgumentType.Constant, labelIndex, null)));
			}

			labelIndex += instruction.GetWordCount(context);
		}
		
		var ret = new List<AssemblyInstruction>();
		foreach (AssemblyInstruction instruction in instructions) {
			var definedSymbols = instruction.GetDefinedSymbols(context);
			if (definedSymbols != null) {
				foreach ((string? key, InstructionArgumentAst? value) in definedSymbols) {
					context.SetSymbol(new SymbolDefinition(key, false, value));
				}
			}

			ret.AddRange(instruction.Render(context));
		}

		foreach (var renderedInstruction in ret) {
			if (renderedInstruction.HasUnrenderedSymbols()) {
				throw new Exception($"Logic error: rendered instruction has unrendered symbols: {renderedInstruction}");
			}
		}

		return ret;
	}

	public IReadOnlyList<ushort> AssembleMachineCode(AssemblyContext context, IReadOnlyList<AssemblyInstruction> renderedInstructions) {
		var ret = new List<ushort>();
		foreach (var renderedInstruction in renderedInstructions) {
			ret.AddRange(renderedInstruction.Assemble(context));
		}

		return ret;
	}
}
