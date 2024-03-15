using Antlr4.Runtime.Tree;
using Assembler.Parsing;

namespace Assembler.Ast.Antlr;

public class BasicVisitor : ProcAssemblyV2GrammarBaseVisitor<IAssemblyAst> {
	private static long ParseNumber(string tokenValue) {
		bool isNegative = tokenValue.StartsWith('-');
		string numberText = isNegative ? tokenValue[1..] : tokenValue;

		if (numberText.StartsWith("0x")) {
			return ParsingUtils.ParseNumericLiteral(isNegative, 16, "0x", numberText.ToLower(), ch => ch switch {
				>= '0' and <= '9' => ch - '0',
				>= 'a' and <= 'f' => ch - 'a' + 10,
			});
		} else if (numberText.StartsWith("0b")) {
			return ParsingUtils.ParseNumericLiteral(isNegative, 2, "0b", numberText, ch => ch == '0' ? 0 : 1);
		} else {
			return ParsingUtils.ParseNumericLiteral(isNegative, 10, "", numberText, ch => ch - '0');
		}
	}

	public override IAssemblyAst VisitProgram(ProcAssemblyV2Grammar.ProgramContext context) {
		return new ProgramAst(context.programStatement().Select(Visit).Cast<ProgramStatementAst>().ToList());
	}

	public override IAssemblyAst VisitProgramStatement(ProcAssemblyV2Grammar.ProgramStatementContext context) {
		ITerminalNode? labelSymbol = context.SYMBOL();
		string? labelValue = labelSymbol?.GetText();

		return new ProgramStatementAst(labelValue, (InstructionAst) Visit(context.instruction()));
	}

	public override IAssemblyAst VisitInstruction(ProcAssemblyV2Grammar.InstructionContext context) {
		ProcAssemblyV2Grammar.InstructionMnemonicContext mnemonic = context.instructionMnemonic();
		return new InstructionAst(
			//(InstructionMnemonicAst) Visit(context.instructionMnemonic()),
			mnemonic.DOT()?.GetText() + mnemonic.ATSIGN()?.GetText() + mnemonic.SYMBOL().GetText(),
			context.instructionArgument().Select(Visit).Cast<InstructionArgumentAst>().ToList()
		);
	}

	public override IAssemblyAst VisitInstructionArgument(ProcAssemblyV2Grammar.InstructionArgumentContext context) {
		if (context.SYMBOL() != null) {
			return InstructionArgumentAst.Symbol(context.SYMBOL().GetText());
		} else if (context.IMMEDIATE() != null) {
			return InstructionArgumentAst.Constant(ParseNumber(context.IMMEDIATE().GetText()[1..]));
		} else if (context.REGISTER() != null) {
			string registerName = context.REGISTER().GetText()[1..];
			if (context.REGISTER().GetText()[0] == '*') {
				return InstructionArgumentAst.StarRegister(registerName);
			} else {
				return InstructionArgumentAst.Register(registerName);
			}
		} else if (context.STRING() != null) {
			return InstructionArgumentAst.String(context.STRING().GetText()[1..^1]);
		} else {
			throw new ParserException("Unable to recognize rule " + context);
		}
	}
}
