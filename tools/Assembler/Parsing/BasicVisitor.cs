using Antlr4.Runtime.Tree;
using Assembler.Ast;

namespace Assembler.Parsing.Antlr;

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
		return new ProgramAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, context.programStatement().Select(Visit).Cast<ProgramStatementAst>().ToList());
	}

	public override IAssemblyAst VisitProgramStatement(ProcAssemblyV2Grammar.ProgramStatementContext context) {
		ITerminalNode? labelSymbol = context.SYMBOL();
		string? labelValue = labelSymbol?.GetText();

		return new ProgramStatementAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, labelValue, (InstructionAst) Visit(context.instruction()));
	}

	public override IAssemblyAst VisitInstruction(ProcAssemblyV2Grammar.InstructionContext context) {
		ProcAssemblyV2Grammar.InstructionMnemonicContext mnemonic = context.instructionMnemonic();
		return new InstructionAst(
			context.Start.TokenSource.SourceName,
			context.Start.Line,
			context.Start.Column,
			//(InstructionMnemonicAst) Visit(context.instructionMnemonic()),
			mnemonic.DOT()?.GetText() + mnemonic.ATSIGN()?.GetText() + mnemonic.SYMBOL().GetText(),
			context.instructionArgument().Select(Visit).Cast<InstructionArgumentAst>().ToList()
		);
	}

	public override IAssemblyAst VisitInstructionArgument(ProcAssemblyV2Grammar.InstructionArgumentContext context) {
		if (context.SYMBOL() != null) {
			return new SymbolAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, context.SYMBOL().GetText());
		} else if (context.IMMEDIATE() != null) {
			return new ConstantAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, ParseNumber(context.IMMEDIATE().GetText()[1..]));
		} else if (context.REGISTER() != null) {
			string registerName = context.REGISTER().GetText()[1..];
			return new RegisterAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, registerName);
		} else if (context.STRING() != null) {
			return new StringAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, context.STRING().GetText()[1..^1]);
		} else if (context.constantExpression() != null) {
			return Visit(context.constantExpression());
		} else {
			throw new ParserException("Unable to recognize rule " + context);
		}
	}

	public override IAssemblyAst VisitConstantExpression(ProcAssemblyV2Grammar.ConstantExpressionContext context) {
		if (context.nestedConstantExpression() != null) {
			return Visit(context.nestedConstantExpression().constantExpression());
		} else if (context.EXPRSYMBOL() != null) {
			return new SymbolAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, context.EXPRSYMBOL().GetText());
		} else if (context.EXPRCONST() != null) {
			return new ConstantAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, ParseNumber(context.EXPRCONST().GetText()));
		} else if (context.binaryExpression != null) {
			return new BinaryOpExpressionAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, (IExpressionElement) Visit(context.constantExpression(0)), context.binaryExpression.Text switch {
				"+"  => BinaryExpressionOp.Add,
				"-"  => BinaryExpressionOp.Subtract,
				"*"  => BinaryExpressionOp.Multiply,
				"/"  => BinaryExpressionOp.Divide,
				"<<" => BinaryExpressionOp.LeftShift,
				">>" => BinaryExpressionOp.RightShift,
				"&"  => BinaryExpressionOp.And,
				"|"  => BinaryExpressionOp.Or,
				"^"  => BinaryExpressionOp.Xor,
			}, (IExpressionElement) Visit(context.constantExpression(1)));
		} else if (context.unaryExpression != null) {
			return new UnaryOpExpressionAst(context.Start.TokenSource.SourceName, context.Start.Line, context.Start.Column, context.unaryExpression.Text switch {
				"~"  => UnaryExpressionOp.Not,
			}, (IExpressionElement) Visit(context.constantExpression(0)));
		} else {
			throw new ParserException("Unable to recognize rule " + context);
		}
	}
}
