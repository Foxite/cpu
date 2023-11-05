using Antlr4.Runtime.Tree;

namespace Assembler.Parsing.Proc16a.Antlr;

public class Proc16aBasicVisitor : Proc16aGrammarBaseVisitor<IAssemblyAst> {
	private static CpuRegister ParseRegister(ITerminalNode registerToken) {
		return registerToken.GetText() switch {
			"A" => CpuRegister.A,
			"B" => CpuRegister.B,
			"*A" => CpuRegister.StarA,
			"*B" => CpuRegister.StarB,
		};
	}

	private static long ParseNumber(ITerminalNode numberToken) {
		string tokenValue = numberToken.GetText();
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

	public override IAssemblyAst VisitAluOperand(Proc16aGrammar.AluOperandContext context) {
		ITerminalNode? number = context.NUMBER();
		ITerminalNode? register = context.REGISTER();

		if (number != null) {
			return new AluOperand(ParseNumber(number));
		} else if (register != null) {
			return new AluOperand(ParseRegister(register));
		} else {
			// TODO throw typed exception
			throw new Exception($"Unrecognized grammar {context.GetText()}");
		}
	}
	
	private static AluOperation ParseBinaryAluOperation(ITerminalNode aluBinaryOperation) {
		return aluBinaryOperation.GetText() switch {
			"+" 	=> AluOperation.Add,
			"-" 	=> AluOperation.Subtract,
			"*" 	=> AluOperation.Multiply,
			"/" 	=> AluOperation.Divide,
			"<<" 	=> AluOperation.ShiftLeft,
			">>" 	=> AluOperation.ShiftRight,
			
			"AND" 	=> AluOperation.BitwiseAnd,
			"OR" 	=> AluOperation.BitwiseOr,
			"XOR" 	=> AluOperation.BitwiseXor,
			"XNOR" 	=> AluOperation.BitwiseXnor,
			"NOR" 	=> AluOperation.BitwiseNor,
			"NAND" 	=> AluOperation.BitwiseNand,
			
			">" 	=> AluOperation.GreaterThan,
			"==" 	=> AluOperation.Equals,
			">=" 	=> AluOperation.GreaterThanOrEquals,
			"<" 	=> AluOperation.LessThan,
			"!=" 	=> AluOperation.NotEquals,
			"<=" 	=> AluOperation.LessThanOrEquals,
			"true" 	=> AluOperation.True,
			"false" => AluOperation.False,
		};
	}

	public override IAssemblyAst VisitProgram(Proc16aGrammar.ProgramContext context) {
		return new ProgramAst(
			context
				.programStatement()
				.Select(Visit)
				.Cast<ProgramStatementAst>()
				.ToList()
		);
	}

	public override IAssemblyAst VisitProgramStatement(Proc16aGrammar.ProgramStatementContext context) {
		ITerminalNode? labelToken = context.SYMBOL();
		string? labelValue = null;
		if (labelToken != null) {
			labelValue = labelToken.GetText();
		}

		var instruction = (IStatement) Visit(context.instruction());

		return new ProgramStatementAst(labelValue, instruction);
	}

	public override IAssemblyAst VisitDatawordInstruction(Proc16aGrammar.DatawordInstructionContext context) {
		CpuRegister register = ParseRegister(context.REGISTER());

		ITerminalNode? valueToken = context.NUMBER();
		ITerminalNode? symbolToken = context.SYMBOL();
		ITerminalNode? booleanToken = context.BOOLEAN();

		if (symbolToken != null) {
			return new DataWordInstruction(register, symbolToken.GetText());
		} else if (valueToken != null) {
			return new DataWordInstruction(register, ParseNumber(valueToken));
		} else if (booleanToken != null) {
			return new DataWordInstruction(register, booleanToken.GetText() switch {
				"true" => 0xFFFF,
				"false" => 0x0000,
				// TODO throw proper parsing exception
			});
		} else {
			// TODO throw typed exception
			throw new Exception($"Unrecognized grammar {context.GetText()}");
		}
	}

	public override IAssemblyAst VisitAssignInstruction(Proc16aGrammar.AssignInstructionContext context) {
		return new AssignInstruction(ParseRegister(context.REGISTER(0)), ParseRegister(context.REGISTER(1)));
	}

	public override IAssemblyAst VisitAluInstruction(Proc16aGrammar.AluInstructionContext context) {
		bool isUnary = context.UNARY_OPERATION() != null;
		
		if (isUnary) {
			return new AluInstruction(
				new AluWriteTarget(ParseRegister(context.REGISTER())),
				(AluOperand) Visit(context.aluOperand(0)),
				null,
				context.UNARY_OPERATION().GetText() switch {
					"NOT" or "~" => AluOperation.BitwiseNot,
					// TODO throw proper exception
				}
			);
		} else {
			return new AluInstruction(
				new AluWriteTarget(ParseRegister(context.REGISTER())),
				(AluOperand) Visit(context.aluOperand(0)),
				(AluOperand) Visit(context.aluOperand(1)),
				ParseBinaryAluOperation(context.BINARY_OPERATION())
			);
		}
	}

	public override IAssemblyAst VisitJumpInstruction(Proc16aGrammar.JumpInstructionContext context) {
		Proc16aGrammar.ComparePhraseContext? comparePhrase = context.comparePhrase();

		if (comparePhrase != null) {
			return new JumpInstruction(
				new Condition(
					new AluOperand(ParseRegister(comparePhrase.REGISTER())),
					comparePhrase.BINARY_OPERATION().GetText() switch {
						">" 	=> CompareOperation.GreaterThan,
						"==" 	=> CompareOperation.Equals,
						">=" 	=> CompareOperation.GreaterThanOrEquals,
						"<" 	=> CompareOperation.LessThan,
						"!=" 	=> CompareOperation.NotEquals,
						"<=" 	=> CompareOperation.LessThanOrEquals,
					},
					new AluOperand(ParseNumber(comparePhrase.NUMBER()))
				),
				ParseRegister(context.REGISTER())
			);
		} else {
			return new JumpInstruction(context.BOOLEAN().GetText() == "true", ParseRegister(context.REGISTER()));
		}
	}
}
