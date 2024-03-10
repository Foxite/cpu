using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

/// <summary>
/// Converts <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture.
/// </summary>
public abstract class ProgramAssembler {
	public abstract string ArchitectureName { get; }

	public IReadOnlyList<ushort> Assemble(ProgramAst program) {
		var symbolDefinitions = new Dictionary<string, InstructionArgumentAst>();

		List<ProgramStatementAst> statementList = program.Statements.ToList();

		// Define label symbols (they exist everywhere in the program)
		for (int statementI = 0, instructionI = 0; statementI < statementList.Count; statementI++) {
			ProgramStatementAst statement = statementList[statementI];
			
			if (statement.Label != null) {
				symbolDefinitions[statement.Label] = new InstructionArgumentAst(InstructionArgumentType.Constant, instructionI, null);
			}

			if (!statement.Instruction.Mnemonic.StartsWith(".")) {
				instructionI++;
			}
		}

		var invalidInstructions = new List<InvalidInstruction>();
		
		//ushort GetSymbolDefinition(string symbol) => symbolDefinitions.TryGetValue(symbol, out ushort value) ? value : throw new SymbolNotDefinedException(symbol);

		var executableInstructions = new List<InstructionAst>();

		for (int i = 0; i < statementList.Count; i++) {
			ProgramStatementAst statement = statementList[i];
			var instruction = statement.Instruction;
			
			InstructionSupport support;
			if (instruction.Mnemonic.StartsWith(".")) {
				switch (instruction.Mnemonic[1..]) {
					case "const":
						if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Constant })) {
							symbolDefinitions[instruction.Arguments[0].RslsValue!] = new InstructionArgumentAst(InstructionArgumentType.Constant, instruction.Arguments[1].ConstantValue!.Value, null);
							support = InstructionSupport.Supported;
						} else if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Symbol })) {
							if (symbolDefinitions[instruction.Arguments[1].RslsValue!].Type == InstructionArgumentType.Register) {
								symbolDefinitions[instruction.Arguments[0].RslsValue!] = symbolDefinitions[instruction.Arguments[1].RslsValue!];
								support = InstructionSupport.Supported;
							} else {
								support = InstructionSupport.OtherError;
							}
						} else {
							support = InstructionSupport.ParameterType;
						}
						break;
					case "reg":
						if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Register })) {
							symbolDefinitions[instruction.Arguments[0].RslsValue!] = new InstructionArgumentAst(InstructionArgumentType.Register, null, instruction.Arguments[1].RslsValue);
							support = InstructionSupport.Supported;
						} else if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Symbol })) {
							if (symbolDefinitions[instruction.Arguments[1].RslsValue!].Type == InstructionArgumentType.Register) {
								symbolDefinitions[instruction.Arguments[0].RslsValue!] = symbolDefinitions[instruction.Arguments[1].RslsValue!];
								support = InstructionSupport.Supported;
							} else {
								support = InstructionSupport.OtherError;
							}
						} else {
							support = InstructionSupport.ParameterType;
						}
						break;
					default:
						support = InstructionSupport.NotRecognized;
						break;
				}
			} else {
				instruction = instruction with {
					Arguments = instruction.Arguments.Select(arg => {
						if (arg.Type == InstructionArgumentType.Symbol) {
							return symbolDefinitions[arg.RslsValue!];
						} else {
							return arg;
						}
					}).ToList(),
				};

				statement = statement with {
					Instruction = instruction,
				};

				statementList[i] = statement;
				
				try {
					support = ValidateInstruction(instruction);

					executableInstructions.Add(instruction);
				} catch (SymbolNotDefinedException ex) {
					invalidInstructions.Add(new InvalidInstruction(instruction, i, InstructionSupport.OtherError, $"Symbol not defined: {ex.Symbol}"));
					continue;
				}
			}

			if (support != InstructionSupport.Supported) {
				invalidInstructions.Add(new InvalidInstruction(instruction, i, support, support.ToString()));
			}
		}

		if (invalidInstructions.Count > 0) {
			throw new InvalidProcAssemblyProgramException(ArchitectureName, invalidInstructions);
		}

		return executableInstructions.Select(ConvertInstruction).ToList();
	}
	
	protected internal abstract InstructionSupport ValidateInstruction(InstructionAst instructionAst);
	protected internal abstract ushort ConvertInstruction(InstructionAst instructionAst);
}
