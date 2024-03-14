using System.Text;
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

public class AssemblyContext {
	private Dictionary<string, SymbolDefinition> m_Symbols = new();
	
	public IMacroProvider MacroProvider { get; }
	public IInstructionConverter InstructionConverter { get; }
	public ProgramAssemblerv2 Assembler { get; }
	
	public int OutputIndex { get; private set; }
	
	public AssemblyContext(IMacroProvider macroProvider, IInstructionConverter instructionConverter, ProgramAssemblerv2 assembler) {
		MacroProvider = macroProvider;
		InstructionConverter = instructionConverter;
		Assembler = assembler;
	}

	public AssemblyContext CreateScope() {
		var ret = new AssemblyContext(MacroProvider, InstructionConverter, Assembler);

		/*
		foreach ((string key, SymbolDefinition value) in m_Symbols) {
			if (value.Imported) {
				ret.m_Symbols[key] = value;
			}
		}*/

		return ret;
	}

	public void IncreaseOutputIndex(int wordCount) {
		OutputIndex += wordCount;
	}

	public InstructionArgumentAst GetSymbol(string name) {
		return m_Symbols[name].Value;
	}

	public void SetSymbol(string name, SymbolDefinition symbolDefinition) {
		m_Symbols[name] = symbolDefinition;
	}
}

public class AssemblyInstructionFactory {
	public AssemblyInstruction Create(AssemblyContext context, ProgramStatementAst statement) {
		if (statement.Instruction.Mnemonic.StartsWith("@")) {
			AssemblerProgram macroProgram = context.MacroProvider.GetMacro(statement.Instruction.Mnemonic[1..]);
			return new MacroInstruction(statement.Label, macroProgram.Name, macroProgram.Path, context.Assembler.CompileInstructionList(context, macroProgram), statement.Instruction.Arguments);
		} else if (statement.Instruction.Mnemonic.StartsWith(".")) {
			return CreateCommandInstruction(context, statement);
		} else {
			return new ExecutableInstruction(statement.Label, statement.Instruction);
		}
	}

	private CommandInstruction CreateCommandInstruction(AssemblyContext context, ProgramStatementAst statement) {
		return statement.Instruction.Mnemonic[1..] switch {
			"const"  => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"reg"    => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"define" => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"bytes"  => new OutputWordsCommandInstruction(statement.Label, statement.Instruction.Arguments),
			"ascii"  => new OutputAsciiInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!),
		};
	}
}

public abstract record AssemblyInstruction(string? Label) {
	public abstract int GetWordCount(AssemblyContext context);
	public abstract IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context);
	public abstract IEnumerable<ushort> Assemble(AssemblyContext outerContext);
}

public abstract record CommandInstruction(string? Label) : AssemblyInstruction(Label);

// TODO validation of the arguments on all these commands
public record DefineSymbolCommandInstruction(string? Label, string Name, InstructionArgumentAst Value) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 0;
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => Array.Empty<ushort>();
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) {
		return new Dictionary<string, InstructionArgumentAst>() {
			{ Name, Value }
		};
	}
}

public record OutputWordsCommandInstruction(string? Label, IReadOnlyList<InstructionArgumentAst> WordArguments) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => WordArguments.Count;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return WordArguments.Select(argument => (ushort) argument.ConstantValue!.Value);
	}
}

public record OutputAsciiInstruction(string? Label, string Ascii) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Ascii.Length;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		return Encoding.ASCII.GetBytes(Ascii).Select(word => (ushort) word); // TODO check for attempts at sign preservation, maybe the return type needs to be ushort. Or byte and we switch the whole thing over to 8 bit words and define the isa as big endian.
	}
}

public record ExecutableInstruction(string? Label, InstructionAst InstructionAst) : AssemblyInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 1;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) => new[] { outerContext.InstructionConverter.ConvertInstruction(InstructionAst) };
}

public record MacroInstruction(string? Label, string Name, string Path, IReadOnlyList<AssemblyInstruction> Instructions, IReadOnlyList<InstructionArgumentAst> Arguments) : AssemblyInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Instructions.Sum(instruction => instruction.GetWordCount(context));
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;

	public override IEnumerable<ushort> Assemble(AssemblyContext outerContext) {
		AssemblyContext innerContext = outerContext.CreateScope();
		
		// TODO set macro args as symbols
		innerContext.SetSymbol();
		
		return outerContext.Assembler.Assemble(innerContext, Instructions);
	}
}

public record SymbolDefinition(
	string Name,
	// Symbol is given to included macros.
	//bool Imported,
	InstructionArgumentAst Value
);
