using Assembler.Ast;

namespace Assembler.Assembly.V2;

public class AssemblyInstructionFactory {
	public AssemblyInstruction Create(AssemblyContext context, ProgramStatementAst statement) {
		if (statement.Instruction.Mnemonic.StartsWith("@")) {
			AssemblerProgram macroProgram = context.MacroProvider.GetMacro(statement.Instruction.Mnemonic[1..]);
			return new MacroInstruction(statement.File, statement.LineNumber, statement.Label, -1, macroProgram.Path, context.Assembler.CompileInstructionList(context, macroProgram), statement.Instruction.Arguments);
		} else if (statement.Instruction.Mnemonic.StartsWith(".")) {
			return CreateCommandInstruction(context, statement);
		} else {
			return new ExecutableInstruction(statement.File, statement.LineNumber, statement.Label, -1, statement.Instruction);
		}
	}

	private CommandInstruction CreateCommandInstruction(AssemblyContext context, ProgramStatementAst statement) {
		// TODO command map similar to InstructionConverter
		return statement.Instruction.Mnemonic[1..] switch {
			// TODO validation of the arguments on all these commands
			"const"  => new DefineSymbolCommandInstruction(statement.File, statement.LineNumber, statement.Label, -1, ((SymbolAst) statement.Instruction.Arguments[0]).Value, statement.Instruction.Arguments[1]),
			"reg"    => new DefineSymbolCommandInstruction(statement.File, statement.LineNumber, statement.Label, -1, ((SymbolAst) statement.Instruction.Arguments[0]).Value, statement.Instruction.Arguments[1]),
			"define" => new DefineSymbolCommandInstruction(statement.File, statement.LineNumber, statement.Label, -1, ((SymbolAst) statement.Instruction.Arguments[0]).Value, statement.Instruction.Arguments[1]),
			"words"  => new OutputWordsCommandInstruction(statement.File, statement.LineNumber, statement.Label, -1, statement.Instruction.Arguments),
			"ascii"  => new OutputAsciiInstruction(statement.File, statement.LineNumber, statement.Label, -1, ((StringAst) statement.Instruction.Arguments[0]).Value),
		};
	}
}
