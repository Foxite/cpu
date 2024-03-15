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

	public IReadOnlyList<ushort> AssembleMachineCode(AssemblyContext context, IReadOnlyList<AssemblyInstruction> instructions) {
		int labelIndex = context.OutputIndex;
		foreach (AssemblyInstruction instruction in instructions) {
			if (instruction.Label != null) {
				context.SetSymbol(new SymbolDefinition(instruction.Label, new InstructionArgumentAst(InstructionArgumentType.Constant, labelIndex, null)));
			}

			labelIndex += instruction.GetWordCount(context);
		}
		
		var ret = new List<ushort>();
		foreach (AssemblyInstruction instruction in instructions) {
			var definedSymbols = instruction.GetDefinedSymbols(context);
			if (definedSymbols != null) {
				foreach ((string? key, InstructionArgumentAst? value) in definedSymbols) {
					context.SetSymbol(new SymbolDefinition(key, value));
				}
			}

			ret.AddRange(instruction.Assemble(context));
		}

		return ret;
	}
}
