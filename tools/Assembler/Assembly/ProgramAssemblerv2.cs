using System.Text;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly; 

public class ProgramAssemblerv2 {
	private readonly IMacroProvider m_MacroProvider;
	private readonly IInstructionConverter m_InstructionConverter;
	
	public ProgramAssemblerv2(IMacroProvider macroProvider, IInstructionConverter instructionConverter) {
		m_MacroProvider = macroProvider;
		m_InstructionConverter = instructionConverter;

	}
	
	public List<AssemblyInstruction> CompileInstructionList(AssemblerProgram program) {
		var ret = new List<AssemblyInstruction>();

		var instructionFactory = new AssemblyInstructionFactory();

		var context = new AssemblyContext(m_MacroProvider, m_InstructionConverter, this);

		foreach (ProgramStatementAst statement in program.ProgramAst.Statements) {
			ret.Add(instructionFactory.Create(context, statement));
		}

		return ret;
	}
}

public class AssemblyContext {
	public IMacroProvider MacroProvider { get; }
	//public MacroProcessor MacroProcessor { get; }
	public IInstructionConverter InstructionConverter { get; }
	public ProgramAssemblerv2 Assembler { get; }
	
	//public int InstructionOffset { get; }
	//public AssemblerProgram Program { get; }

	public AssemblyContext(
		IMacroProvider macroProvider,
		//MacroProcessor macroProcessor,
		IInstructionConverter instructionConverter,
		ProgramAssemblerv2 assembler
		//int instructionOffset,
		//AssemblerProgram program
	) {
		MacroProvider = macroProvider;
		//MacroProcessor = macroProcessor;
		InstructionConverter = instructionConverter;
		Assembler = assembler;
		//InstructionOffset = instructionOffset;
		//Program = program;
	}
}

public class AssemblyInstructionFactory {
	public AssemblyInstruction Create(AssemblyContext context, ProgramStatementAst statement) {
		if (statement.Instruction.Mnemonic.StartsWith("@")) {
			AssemblerProgram macroProgram = context.MacroProvider.GetMacro(statement.Instruction.Mnemonic[1..]);
			return new MacroInstruction(statement.Label, macroProgram.Name, macroProgram.Path, context.Assembler.CompileInstructionList(macroProgram));
		} else if (statement.Instruction.Mnemonic.StartsWith(".")) {
			return CreateCommandInstruction(context, statement);
		} else {
			return new ExecutableInstruction(statement.Label, statement.Instruction);
		}
	}

	private CommandInstruction CreateCommandInstruction(AssemblyContext context, ProgramStatementAst statement) {
		return statement.Instruction.Mnemonic[1..] switch {
			"const" => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"reg"   => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"bytes" => new OutputWordsCommandInstruction(statement.Label, statement.Instruction.Arguments),
			"ascii" => new OutputAsciiInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!),
		};
	}
}

public abstract record AssemblyInstruction(string? Label) {
	public abstract int GetWordCount(AssemblyContext context);
	public abstract IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context);
	public abstract IEnumerable<ushort> Assemble(AssemblyContext context);
}

public abstract record CommandInstruction(string? Label) : AssemblyInstruction(Label);

// TODO validation of the arguments on all these commands
public record DefineSymbolCommandInstruction(string? Label, string Name, InstructionArgumentAst Value) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 0;
	public override IEnumerable<ushort> Assemble(AssemblyContext context) => Array.Empty<ushort>();
	
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) {
		return new Dictionary<string, InstructionArgumentAst>() {
			{ Name, Value }
		};
	}
}

public record OutputWordsCommandInstruction(string? Label, IReadOnlyList<InstructionArgumentAst> WordArguments) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => WordArguments.Count;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext context) {
		return WordArguments.Select(argument => (ushort) argument.ConstantValue!.Value);
	}
}

public record OutputAsciiInstruction(string? Label, string Ascii) : CommandInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => Ascii.Length;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	
	public override IEnumerable<ushort> Assemble(AssemblyContext context) {
		return Encoding.ASCII.GetBytes(Ascii).Select(word => (ushort) word); // TODO check for attempts at sign preservation, maybe the return type needs to be ushort. Or byte and we switch the whole thing over to 8 bit words and define the isa as big endian.
	}
}

public record ExecutableInstruction(string? Label, InstructionAst InstructionAst) : AssemblyInstruction(Label) {
	public override int GetWordCount(AssemblyContext context) => 1;
	public override IReadOnlyDictionary<string, InstructionArgumentAst>? GetDefinedSymbols(AssemblyContext context) => null;
	public override IEnumerable<ushort> Assemble(AssemblyContext context) => new[] { context.InstructionConverter.ConvertInstruction(InstructionAst) };
}

public record MacroInstruction(string? Label, string Name, string Path, IReadOnlyList<AssemblyInstruction> Instructions) : AssemblyInstruction(Label) {
}
