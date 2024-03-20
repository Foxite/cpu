using Assembler.Ast;

namespace Assembler.Assembly.V2;

public class ProgramAssemblerv2 {
	public IReadOnlyList<ushort> AssembleAst(AssemblyContext context, AssemblerProgram assemblerProgram) {
		IReadOnlyList<AssemblyInstruction> instructions = CompileInstructionList(context, assemblerProgram);
		IReadOnlyList<AssemblyInstruction> symbolRenderedInstructions = RenderSymbols(context, instructions);
		IReadOnlyList<InvalidInstruction> invalidInstructions = ValidateInstructions(context, symbolRenderedInstructions);

		if (invalidInstructions.Count > 0) {
			throw new InvalidProcAssemblyProgramException(assemblerProgram, context.InstructionConverter.Architecture, invalidInstructions);
		}

		IReadOnlyList<AssemblyInstruction> renderedInstructions = RenderInstructions(context, symbolRenderedInstructions);
		IReadOnlyList<ushort> machineCode = AssembleMachineCode(context, renderedInstructions);
		return machineCode;
	}
	
	public IReadOnlyList<AssemblyInstruction> CompileInstructionList(AssemblyContext context, AssemblerProgram program) {
		var ret = new List<AssemblyInstruction>();

		var instructionFactory = new AssemblyInstructionFactory();

		foreach (ProgramStatementAst statement in program.ProgramAst.Statements) {
			ret.Add(instructionFactory.Create(context, statement));
		}

		return ret;
	}

	public IReadOnlyList<AssemblyInstruction> RenderSymbols(AssemblyContext context, IReadOnlyList<AssemblyInstruction> instructions) {
		int labelIndex = context.OutputIndex;
		foreach (AssemblyInstruction instruction in instructions) {
			if (instruction.Label != null) {
				context.SetSymbol(new SymbolDefinition(instruction.Label, false, new ConstantAst(instruction.File, instruction.Line, -1, labelIndex))); // TODO this kinda sucks
			}

			labelIndex += instruction.GetWordCount(context);
		}
		
		var symbolRenderedInstructions = new List<AssemblyInstruction>();
		foreach (AssemblyInstruction instruction in instructions) {
			var definedSymbols = instruction.GetDefinedSymbols(context);
			if (definedSymbols != null) {
				foreach ((string? key, InstructionArgumentAst? value) in definedSymbols) {
					context.SetSymbol(new SymbolDefinition(key, false, value));
				}
			}

			symbolRenderedInstructions.Add(instruction.RenderSymbols(context));
		}

		foreach (var renderedInstruction in symbolRenderedInstructions) {
			if (renderedInstruction.HasUnrenderedSymbols()) {
				throw new Exception($"Logic error: rendered instruction has unrendered symbols: {renderedInstruction}");
			}
		}

		return symbolRenderedInstructions;
	}

	public IReadOnlyList<InvalidInstruction> ValidateInstructions(AssemblyContext context, IReadOnlyList<AssemblyInstruction> symbolRenderedInstructions) {
		var invalidInstructions = new List<InvalidInstruction>();
		
		foreach (AssemblyInstruction instruction in symbolRenderedInstructions) {
			invalidInstructions.AddRange(instruction.Validate(context));
		}

		return invalidInstructions;
	}

	public IReadOnlyList<AssemblyInstruction> RenderInstructions(AssemblyContext context, IReadOnlyList<AssemblyInstruction> validatedInstructions) {
		var renderedInstructions = new List<AssemblyInstruction>();
		
		foreach (AssemblyInstruction instruction in validatedInstructions) {
			renderedInstructions.AddRange(instruction.RenderInstructions(context));
		}

		return renderedInstructions;
	}

	public IReadOnlyList<ushort> AssembleMachineCode(AssemblyContext context, IReadOnlyList<AssemblyInstruction> renderedInstructions) {
		var ret = new List<ushort>();
		foreach (var renderedInstruction in renderedInstructions) {
			ret.AddRange(renderedInstruction.Assemble(context));
		}

		return ret;
	}
}
