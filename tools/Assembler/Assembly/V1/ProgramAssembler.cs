using Assembler.Ast;

namespace Assembler.Assembly.V1; 

/// <summary>
/// Converts a <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture. Only works for one call of Assemble, then it should be discarded.
/// </summary>
public sealed class _ProgramAssembler {
	private readonly IInstructionConverter m_InstructionConverter;
	private readonly _MacroProcessor m_MacroProcessor;
	
	private readonly int m_InstructionOffset;
	private readonly AssemblerProgram m_Program;
	
	private readonly Dictionary<string, InstructionArgumentAst> m_SymbolDefinitions;
	private readonly List<ProgramStatementAst> m_StatementList;
	private readonly List<InstructionAst> m_ExecutableInstructions = new();
	private readonly List<InvalidInstruction> m_InvalidInstructions = new();

	public string Architecture => m_InstructionConverter.Architecture;

	public _ProgramAssembler(IInstructionConverter instructionConverter, _MacroProcessor macroProcessor, AssemblerProgram program, int instructionOffset = 0, IReadOnlyDictionary<string, InstructionArgumentAst>? symbols = null) {
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
		foreach (InstructionAst instruction in m_ExecutableInstructions) {
			if (instruction.Mnemonic.StartsWith("@")) {
				result.AddRange(ProcessInclude(instruction, result.Count));
			} else {
				result.Add(m_InstructionConverter.ConvertInstruction(instruction));
			}
		}

		return result;
	}

	private void DefineLabelSymbols() {
		for (int statementI = 0, instructionI = m_InstructionOffset; statementI < m_StatementList.Count; statementI++) {
			ProgramStatementAst statement = m_StatementList[statementI];
			
			if (statement.Label != null) {
				m_SymbolDefinitions[statement.Label] = new ConstantAst(instructionI);
			}

			if (statement.Instruction.Mnemonic.StartsWith(".")) {
				// pass
			} else if (statement.Instruction.Mnemonic.StartsWith("@")) {
				// TODO find a way to avoid reading the macro twice (should probably use caching)
				instructionI += m_MacroProcessor.GetInstructionCount(statement.Instruction.Mnemonic[1..]);
			} else {
				instructionI++;
			}
		}
	}

	private void ProcessStatement(int i) {
		ProgramStatementAst statement = m_StatementList[i];
		
		InstructionSupport support;
		if (statement.Instruction.Mnemonic.StartsWith(".")) {
			support = ProcessAssemblerCommand(ReplaceSymbolsInArguments(statement).Instruction);
		} else if (statement.Instruction.Mnemonic.StartsWith("@")) {
			m_ExecutableInstructions.Add(statement.Instruction);
			support = InstructionSupport.Supported;
		} else {
			support = ProcessInstruction(statement, i);
		}

		if (support != InstructionSupport.Supported) {
			m_InvalidInstructions.Add(new InvalidInstruction(statement.Instruction, i, support, support.ToString()));
		}
	}

	private IReadOnlyList<InstructionArgumentAst> ReplaceSymbols(IReadOnlyList<InstructionArgumentAst> arguments, bool optional = false) {
		return arguments.Select(arg => {
			if (arg is SymbolAst symbol && (optional || m_SymbolDefinitions.ContainsKey(symbol.Value))) {
				return m_SymbolDefinitions[symbol.Value];
			} else {
				return arg;
			}
		}).ToList();
	}

	private ProgramStatementAst ReplaceSymbolsInArguments(ProgramStatementAst statement) {
		return statement with {
			Instruction = statement.Instruction with {
				Arguments = ReplaceSymbols(statement.Instruction.Arguments)
			}
		};
	}
	
	private InstructionSupport ProcessInstruction(ProgramStatementAst statement, int index) {
		statement = ReplaceSymbolsInArguments(statement);

		m_StatementList[index] = statement;
		
		m_ExecutableInstructions.Add(statement.Instruction);
			
		try {
			return m_InstructionConverter.ValidateInstruction(statement.Instruction);
		} catch (SymbolNotDefinedException) {
			//m_InvalidInstructions.Add(new InvalidInstruction(instruction, i, InstructionSupport.OtherError, $"Symbol not defined: {ex.Symbol}"));
			return InstructionSupport.SymbolNotDefined;
		}
	}

	private IEnumerable<ushort> ProcessInclude(InstructionAst instruction, int index) {
		string includeName = instruction.Mnemonic[1..];

		return m_MacroProcessor.AssembleMacro(index, includeName, ReplaceSymbols(instruction.Arguments));
	}
	
	private InstructionSupport ProcessAssemblerCommand(InstructionAst instruction) {
		switch (instruction.Mnemonic[1..]) {
			case "const": {
				var constantResult = instruction.GetArguments(out symbolName, out ConstantAst valueConstant);
				if (constantResult != InstructionSupport.Supported) {
					return constantResult;
				}

				return ProcessAssemblerCommand_Constant_Constant(symbolName, valueConstant);
			}
			case "reg": {
				var argumentResult = instruction.GetArguments(out SymbolAst symbolName, out RegisterAst register);
				if (argumentResult != InstructionSupport.Supported) {
					
				}
			}
			default:
				return InstructionSupport.NotRecognized;
		}
	}

	private InstructionSupport ProcessAssemblerCommand_Define_Symbol(SymbolAst name, SymbolAst value) {
		m_SymbolDefinitions[name.Value] = m_SymbolDefinitions[value.Value];
		return InstructionSupport.Supported;
	}
	private InstructionSupport ProcessAssemblerCommand_Define_Constant(SymbolAst name, ConstantAst value) {
		m_SymbolDefinitions[name.Value] = value;
		return InstructionSupport.Supported;
	}
	private InstructionSupport ProcessAssemblerCommand_Define_Register(SymbolAst name, RegisterAst value) {
		m_SymbolDefinitions[name.Value] = value;
		return InstructionSupport.Supported;
	}
}
