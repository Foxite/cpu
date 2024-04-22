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
		var positionAssignedInstructions = new List<AssemblyInstruction>();
		int position = context.OutputOffset;
		foreach (AssemblyInstruction instruction in instructions) {
			if (instruction.Label != null) {
				// TODO this kinda sucks
				// Replace the AST currently used as the symbol/argument value with some kind of value object that tracks both the actual value and the symbol that defined it, with the full file/line/column for both
				context.SetSymbol(new SymbolDefinition(instruction.Label, false, new ConstantAst(instruction.File, instruction.Line, -1, position)));
			}

			positionAssignedInstructions.Add(instruction with {
				Position = position,
			});
			
			position += instruction.GetWordCount(context);
		}

		void LogIfNecessary(object obj) {
			if (context.TraceLogging) {
				Console.Error.WriteLine(new string('\t', context.NestLevel) + obj);
			}
		}
		
		LogIfNecessary("{");
		
		var symbolRenderedInstructions = new List<AssemblyInstruction>();
		foreach (AssemblyInstruction instruction in positionAssignedInstructions) {
			var definedSymbols = instruction.GetDefinedSymbols(context);
			if (definedSymbols != null) {
				foreach ((string? key, InstructionArgumentAst? value) in definedSymbols) {
					context.SetSymbol(new SymbolDefinition(key, false, value));
				}
			}

			LogIfNecessary(instruction);
			LogIfNecessary(instruction.GetWordCount(context));
			AssemblyInstruction rendered = instruction.RenderSymbols(context);
			LogIfNecessary(rendered);
			LogIfNecessary("");
			LogIfNecessary("");
			symbolRenderedInstructions.Add(rendered);
		}
		
		LogIfNecessary("}");
		
		foreach (var renderedInstruction in symbolRenderedInstructions) {
			if (renderedInstruction.HasUnrenderedSymbols()) {
				throw new Exception($"Logic error: rendered instruction has unrendered symbols: {renderedInstruction}");
			}

			if (renderedInstruction.Position == -1) {
				throw new Exception($"Logic error: rendered instruction has unset position: {renderedInstruction}");
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
			if (context.OutputLineMapping) { //  && context.NestLevel == 0
				Console.Write(new string('\t', context.NestLevel));
				Console.Write($"{instruction.File}:{instruction.Line} == {instruction.Position} <{instruction.ToShortString()}>");
				Console.WriteLine();
			}
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
