using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

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
			// TODO validation of the arguments on all these commands
			"const"  => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"reg"    => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"define" => new DefineSymbolCommandInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!, statement.Instruction.Arguments[1]),
			"bytes"  => new OutputWordsCommandInstruction(statement.Label, statement.Instruction.Arguments),
			"ascii"  => new OutputAsciiInstruction(statement.Label, statement.Instruction.Arguments[0].RslsValue!),
		};
	}
}
