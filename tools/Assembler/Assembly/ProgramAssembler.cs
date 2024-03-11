using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

/// <summary>
/// Converts a <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture. Only works for one call of Assemble, then it should be discarded.
/// </summary>
public sealed class ProgramAssembler {
	private readonly IInstructionConverter m_InstructionConverter;
	private readonly MacroProcessor m_MacroProcessor;
	
	private readonly int m_InstructionOffset;
	private readonly AssemblerProgram m_Program;
	
	private readonly Dictionary<string, InstructionArgumentAst> m_SymbolDefinitions;
	private readonly List<ProgramStatementAst> m_StatementList;
	private readonly List<InstructionAst> m_ExecutableInstructions = new();
	private readonly List<InvalidInstruction> m_InvalidInstructions = new();

	public string Architecture => m_InstructionConverter.Architecture;

	public ProgramAssembler(IInstructionConverter instructionConverter, MacroProcessor macroProcessor, AssemblerProgram program, int instructionOffset = 0, IReadOnlyDictionary<string, InstructionArgumentAst>? symbols = null) {
		m_InstructionConverter = instructionConverter;
		m_MacroProcessor = macroProcessor;
		m_Program = program;
		m_InstructionOffset = instructionOffset;
		m_StatementList = program.ProgramAst.Statements.ToList();

		m_SymbolDefinitions = symbols != null ? new Dictionary<string, InstructionArgumentAst>(symbols) : new Dictionary<string, InstructionArgumentAst>();
	}

	public IReadOnlyList<ushort> Assemble() {
		// Define label symbols (they exist everywhere in the program)
		DefineLabelSymbols();
		
		for (int i = 0; i < m_StatementList.Count; i++) {
			ProcessStatement(i);
		}

		if (m_InvalidInstructions.Count > 0) {
			throw new InvalidProcAssemblyProgramException(m_Program, Architecture, m_InvalidInstructions);
		}

		var result = new List<ushort>();
		foreach (var instruction in m_ExecutableInstructions) {
			
			if (instruction.Mnemonic.StartsWith("#")) {
				ProcessInclude(statement.Instruction, i);
			} else {
				m_InstructionConverter.ConvertInstruction(instruction);
			}
		}

		return result;
	}

	private void DefineLabelSymbols() {
		for (int statementI = 0, instructionI = m_InstructionOffset; statementI < m_StatementList.Count; statementI++) {
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
		} else if (statement.Instruction.Mnemonic.StartsWith("#")) {
			// Will be processed during Convert stage
			support = InstructionSupport.Supported;
		} else {
			support = ProcessInstruction(statement, i);
		}

		if (support != InstructionSupport.Supported) {
			m_InvalidInstructions.Add(new InvalidInstruction(statement.Instruction, i, support, support.ToString()));
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

	private InstructionSupport ProcessInclude(InstructionAst instruction, int index) {
		var includeName = instruction.Mnemonic[1..];

		var macroProgramAst = m_MacroProcessor.GetMacro(includeName);
		var macroProgram = m_MacroProcessor.AssembleMacro(index, macroProgramAst, instruction.Arguments);
		
		
	}
	
	private InstructionSupport ProcessAssemblerCommand(InstructionAst instruction) {
		switch (instruction.Mnemonic[1..]) {
			case "const":
				return ProcessAssemblerCommand_Constant(
					instruction,
					InstructionArgumentType.Constant,
					new InstructionArgumentAst(InstructionArgumentType.Constant, instruction.Arguments[1].ConstantValue!.Value, null)
				);
			case "reg":
				return ProcessAssemblerCommand_Constant(
					instruction,
					InstructionArgumentType.Register,
					new InstructionArgumentAst(InstructionArgumentType.Register, null, instruction.Arguments[1].RslsValue)
				);
			default:
				return InstructionSupport.NotRecognized;
		}
	}

	private InstructionSupport ProcessAssemblerCommand_Constant(InstructionAst instruction, InstructionArgumentType argumentType, InstructionArgumentAst result) {
		if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, argumentType })) {
			m_SymbolDefinitions[instruction.Arguments[0].RslsValue!] = result;
			return InstructionSupport.Supported;
		} else if (instruction.Arguments.Select(arg => arg.Type).SequenceEqual(new[] { InstructionArgumentType.Symbol, InstructionArgumentType.Symbol })) {
			if (m_SymbolDefinitions[instruction.Arguments[1].RslsValue!].Type == argumentType) {
				m_SymbolDefinitions[instruction.Arguments[0].RslsValue!] = m_SymbolDefinitions[instruction.Arguments[1].RslsValue!];
				return InstructionSupport.Supported;
			} else {
				return InstructionSupport.OtherError;
			}
		} else {
			return InstructionSupport.ParameterType;
		}
	}
}

public class MacroProcessor {
	public ProgramAst GetMacro(string name) {
		
	}

	public IEnumerable<ushort> AssembleMacro(int instructionOffset, ProgramAst programAst, IReadOnlyList<InstructionArgumentAst> arguments) {
		
	}
}
