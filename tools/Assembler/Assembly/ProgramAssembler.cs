using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

/// <summary>
/// Converts a <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture. Only works for one call of Assemble, then it should be discarded.
/// </summary>
public sealed class ProgramAssembler {
	private readonly IInstructionConverter m_InstructionConverter;
	private readonly Dictionary<string, InstructionArgumentAst> m_SymbolDefinitions = new();
	private readonly List<ProgramStatementAst> m_StatementList;
	private readonly List<InstructionAst> m_ExecutableInstructions = new();
	private readonly List<InvalidInstruction> m_InvalidInstructions = new();
	
	public string ArchitectureName { get; }


	public ProgramAssembler(IInstructionConverter instructionConverter, ProgramAst programAst) {
		m_InstructionConverter = instructionConverter;
		m_StatementList = programAst.Statements.ToList();
	}

	public IReadOnlyList<ushort> Assemble() {
		// Define label symbols (they exist everywhere in the program)
		DefineLabelSymbols();
		
		for (int i = 0; i < m_StatementList.Count; i++) {
			ProcessStatement(i);
		}

		if (m_InvalidInstructions.Count > 0) {
			throw new InvalidProcAssemblyProgramException(ArchitectureName, m_InvalidInstructions);
		}

		return m_ExecutableInstructions.Select(m_InstructionConverter.ConvertInstruction).ToList();
	}

	private void DefineLabelSymbols() {
		for (int statementI = 0, instructionI = 0; statementI < m_StatementList.Count; statementI++) {
			ProgramStatementAst statement = m_StatementList[statementI];
			
			if (statement.Label != null) {
				m_SymbolDefinitions[statement.Label] = new InstructionArgumentAst(InstructionArgumentType.Constant, instructionI, null);
			}

			if (!statement.Instruction.Mnemonic.StartsWith(".")) {
				instructionI++;
			}
		}
	}

	private void ProcessStatement(int i) {
		ProgramStatementAst statement = m_StatementList[i];
		
		InstructionSupport support;
		if (statement.Instruction.Mnemonic.StartsWith(".")) {
			support = ProcessAssemblerCommand(statement.Instruction);
		} else {
			support = ProcessInstruction(statement, i);
		}

		if (support != InstructionSupport.Supported) {
			m_InvalidInstructions.Add(new InvalidInstruction(statement.Instruction, i, support, support.ToString()));
		}
	}

	private InstructionSupport ProcessAssemblerCommand(InstructionAst instruction) {
		switch (instruction.Mnemonic[1..]) {
			case "const":
				if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Constant })) {
					m_SymbolDefinitions[instruction.Arguments[0].RslsValue!] = new InstructionArgumentAst(InstructionArgumentType.Constant, instruction.Arguments[1].ConstantValue!.Value, null);
					return InstructionSupport.Supported;
				} else if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Symbol })) {
					if (m_SymbolDefinitions[instruction.Arguments[1].RslsValue!].Type == InstructionArgumentType.Register) {
						m_SymbolDefinitions[instruction.Arguments[0].RslsValue!] = m_SymbolDefinitions[instruction.Arguments[1].RslsValue!];
						return InstructionSupport.Supported;
					} else {
						return InstructionSupport.OtherError;
					}
				} else {
					return InstructionSupport.ParameterType;
				}
			case "reg":
				if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Register })) {
					m_SymbolDefinitions[instruction.Arguments[0].RslsValue!] = new InstructionArgumentAst(InstructionArgumentType.Register, null, instruction.Arguments[1].RslsValue);
					return InstructionSupport.Supported;
				} else if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Symbol })) {
					if (m_SymbolDefinitions[instruction.Arguments[1].RslsValue!].Type == InstructionArgumentType.Register) {
						m_SymbolDefinitions[instruction.Arguments[0].RslsValue!] = m_SymbolDefinitions[instruction.Arguments[1].RslsValue!];
						return InstructionSupport.Supported;
					} else {
						return InstructionSupport.OtherError;
					}
				} else {
					return InstructionSupport.ParameterType;
				}
			default:
				return InstructionSupport.NotRecognized;
		}
	}

	private InstructionSupport ProcessInstruction(ProgramStatementAst statement, int index) {
		statement = statement with {
			Instruction = statement.Instruction with {
				Arguments = statement.Instruction.Arguments.Select(arg => {
					if (arg.Type == InstructionArgumentType.Symbol) {
						return m_SymbolDefinitions[arg.RslsValue!];
					} else {
						return arg;
					}
				}).ToList(),
			},
		};

		m_StatementList[index] = statement;
		
		m_ExecutableInstructions.Add(statement.Instruction);
			
		try {
			return m_InstructionConverter.ValidateInstruction(statement.Instruction);
		} catch (SymbolNotDefinedException) {
			//m_InvalidInstructions.Add(new InvalidInstruction(instruction, i, InstructionSupport.OtherError, $"Symbol not defined: {ex.Symbol}"));
			return InstructionSupport.SymbolNotDefined;
		}
	}
}
