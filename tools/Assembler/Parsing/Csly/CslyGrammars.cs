using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace Assembler.Parsing.Csly;

public class CslyGrammars {
	[Production("Number: Minus? DecimalInteger")]
	public ConstantAst DecimalNumber(Token<CslyTokens> minus, Token<CslyTokens> token) {
		return new ConstantAst(ParsingUtils.ParseNumericLiteral(!minus.IsEmpty, 10, "", token.Value, ch => ch - '0'));
	}
	
	[Production("Number: Minus? HexadecimalInteger")]
	public ConstantAst HexadecimalNumber(Token<CslyTokens> minus, Token<CslyTokens> token) {
		return new ConstantAst(ParsingUtils.ParseNumericLiteral(!minus.IsEmpty, 16, "0x", token.Value.ToLower(), ch => ch switch {
			>= '0' and <= '9' => ch - '0',
			>= 'a' and <= 'f' => ch - 'a' + 10,
		}));
	}
	
	[Production("Number: Minus? BinaryInteger")]
	public ConstantAst BinaryNumber(Token<CslyTokens> minus, Token<CslyTokens> token) {
		return new ConstantAst(ParsingUtils.ParseNumericLiteral(!minus.IsEmpty, 2, "0b", token.Value, ch => ch == '0' ? 0 : 1));
	}
	
	[Production("Boolean: True")]
	[Production("Boolean: False")]
	public BooleanAst Boolean(Token<CslyTokens> token) {
		return new BooleanAst(token.TokenID == CslyTokens.True);
	}

	[Production("Statement: Register Assign [d] Number")]
	public IStatement DataWord(CpuRegisterAst register, ConstantAst number) {
		return new DataWordInstruction(register, true, number, null);
	}

	[Production("Statement: Register Assign [d] Symbol")]
	public IStatement DataWord(CpuRegisterAst register, Token<CslyTokens> symbol) {
		return new DataWordInstruction(register, false, null, new SymbolAst(symbol.Value));
	}

	[Production("Statement: Register Assign [d] Boolean")]
	public IStatement DataWord(CpuRegisterAst register, BooleanAst value) {
		return new DataWordInstruction(register, true, new ConstantAst((short) (value.Value ? 0xFFFF : 0x0000)), null);
	}

	[Production("Statement: Register Assign [d] Register")]
	public IStatement Assign(CpuRegisterAst write, CpuRegisterAst read) {
		return new AssignInstruction(write, read);
	}
	
	[Production("Register: ARegister")]
	[Production("Register: BRegister")]
	[Production("Register: StarA")]
	[Production("Register: StarB")]
	public CpuRegisterAst Register(Token<CslyTokens> register) {
		return new CpuRegisterAst(register.TokenID switch {
			CslyTokens.ARegister => CpuRegister.A,
			CslyTokens.BRegister => CpuRegister.B,
			CslyTokens.StarA => CpuRegister.StarA,
			CslyTokens.StarB => CpuRegister.StarB,
		});
	}

	[Production("AluWriteTarget: (Register Comma [d])* Register")]
	public AluWriteTarget AluWriteTarget(List<Group<CslyTokens, IAssemblyAst>> many, CpuRegisterAst single) {
		var ret = new List<CpuRegisterAst>();
		ret.AddRange(many.Select(item => (CpuRegisterAst) item.Value(0)));
		ret.Add(single);
		return new AluWriteTarget(ret.ToArray());
	}

	[Production("AluOperand: Register")]
	public AluOperand AluOperand(CpuRegisterAst register) {
		return new AluOperand(true, register, null);
	}

	[Production("AluOperand: Number")]
	public AluOperand AluOperand(ConstantAst number) {
		return new AluOperand(false, null, number);
	}

	[Production("AluOperand: Boolean")]
	public AluOperand AluOperand(BooleanAst value) {
		return new AluOperand(false, null, new ConstantAst((short) (value.Value ? 0xFFFF : 0x0000)));
	}

	[Production("AluOperation: Plus")]
	[Production("AluOperation: Minus")]
	[Production("AluOperation: Multiply")]
	[Production("AluOperation: Divide")]
	[Production("AluOperation: LeftShift")]
	[Production("AluOperation: RightShift")]
	[Production("AluOperation: BitwiseAnd")]
	[Production("AluOperation: BitwiseOr")]
	[Production("AluOperation: BitwiseNot")]
	[Production("AluOperation: BitwiseXor")]
	[Production("AluOperation: BitwiseXnor")]
	[Production("AluOperation: BitwiseNor")]
	[Production("AluOperation: BitwiseNand")]
	[Production("AluOperation: GreaterThan")]
	[Production("AluOperation: Equals")]
	[Production("AluOperation: GreaterThanOrEquals")]
	[Production("AluOperation: LessThan")]
	[Production("AluOperation: NotEquals")]
	[Production("AluOperation: LessThanOrEquals")]
	[Production("AluOperation: True")]
	[Production("AluOperation: False")]
	public AluOperationAst AluOperation(Token<CslyTokens> token) {
		return new AluOperationAst(token.TokenID switch {
			CslyTokens.Plus					=> Assembler.AluOperation.Add,
			CslyTokens.Minus				=> Assembler.AluOperation.Subtract,
			CslyTokens.Multiply				=> Assembler.AluOperation.Multiply,
			CslyTokens.Divide				=> Assembler.AluOperation.Divide,
			CslyTokens.LeftShift			=> Assembler.AluOperation.ShiftLeft,
			CslyTokens.RightShift			=> Assembler.AluOperation.ShiftRight,
			CslyTokens.BitwiseAnd			=> Assembler.AluOperation.BitwiseAnd,
			CslyTokens.BitwiseOr			=> Assembler.AluOperation.BitwiseOr,
			CslyTokens.BitwiseNot			=> Assembler.AluOperation.BitwiseNot,
			CslyTokens.BitwiseXor			=> Assembler.AluOperation.BitwiseXor,
			CslyTokens.BitwiseXnor			=> Assembler.AluOperation.BitwiseXnor,
			CslyTokens.BitwiseNor			=> Assembler.AluOperation.BitwiseNor,
			CslyTokens.BitwiseNand			=> Assembler.AluOperation.BitwiseNand,
			CslyTokens.GreaterThan			=> Assembler.AluOperation.GreaterThan,
			CslyTokens.Equals				=> Assembler.AluOperation.Equals,
			CslyTokens.GreaterThanOrEquals	=> Assembler.AluOperation.GreaterThanOrEquals,
			CslyTokens.LessThan				=> Assembler.AluOperation.LessThan,
			CslyTokens.NotEquals			=> Assembler.AluOperation.NotEquals,
			CslyTokens.LessThanOrEquals		=> Assembler.AluOperation.LessThanOrEquals,
			CslyTokens.True					=> Assembler.AluOperation.True,
			CslyTokens.False				=> Assembler.AluOperation.False,
		});
	}
	
	[Production("Statement: AluWriteTarget Assign [d] AluOperand AluOperation AluOperand")]
	public IStatement AluInstruction(AluWriteTarget writeTarget, AluOperand x, AluOperationAst operation, AluOperand y) {
		return new AluInstruction(writeTarget, x, y, operation);
	}
	
	// TODO unit test this
	[Production("Statement: AluWriteTarget Assign [d] BitwiseNot [d] AluOperand")]
	public IStatement AluBitwiseNot(AluWriteTarget writeTarget, AluOperand x) {
		return new AluInstruction(writeTarget, x, null, Assembler.AluOperation.BitwiseNot);
	}

	[Production("CompareOperation: GreaterThan")]
	[Production("CompareOperation: Equals")]
	[Production("CompareOperation: GreaterThanOrEquals")]
	[Production("CompareOperation: LessThan")]
	[Production("CompareOperation: NotEquals")]
	[Production("CompareOperation: LessThanOrEquals")]
	public CompareOperationAst CompareOperation(Token<CslyTokens> token) {
		return new CompareOperationAst(token.TokenID switch {
			CslyTokens.GreaterThan 			=> Assembler.CompareOperation.GreaterThan,
			CslyTokens.Equals 				=> Assembler.CompareOperation.Equals,
			CslyTokens.GreaterThanOrEquals 	=> Assembler.CompareOperation.GreaterThanOrEquals,
			CslyTokens.LessThan 				=> Assembler.CompareOperation.LessThan,
			CslyTokens.NotEquals 			=> Assembler.CompareOperation.NotEquals,
			CslyTokens.LessThanOrEquals 		=> Assembler.CompareOperation.LessThanOrEquals,
		});
	}

	[Production("Condition: AluOperand CompareOperation AluOperand")]
	public Condition Condition(AluOperand left, CompareOperationAst compareOperation, AluOperand right) {
		return new Condition(left, compareOperation, right);
	}

	[Production("Statement: Condition Jump [d] Register")]
	public IStatement JumpInstruction(Condition condition, CpuRegisterAst register) {
		return new JumpInstruction(true, condition, null, register);
	}

	[Production("Statement: Boolean Jump [d] Register")]
	public IStatement JumpInstruction(BooleanAst value, CpuRegisterAst register) {
		return new JumpInstruction(false, null, value, register);
	}

	[Production("LineEnding: EndOfLine+")]
	public IAssemblyAst? LineEnding(List<Token<CslyTokens>> ignored) {
		return null;
	}

	[Production("OptionalLineEnding: LineEnding?")]
	public IAssemblyAst? OptionalLineEnding(ValueOption<IAssemblyAst?> ignored) {
		return null;
	}

	[Production("ProgramStatement: (Symbol Colon OptionalLineEnding)? Statement")]
	public ProgramStatementAst ProgramStatement(ValueOption<Group<CslyTokens, IAssemblyAst>> label, IStatement statement) {
		string? labelName = null;
		if (label.IsSome) {
			labelName = label.Match(g => g, () => throw new NotImplementedException()).Token(0).Value;
		}

		return new ProgramStatementAst(labelName, statement);
	}

	[Production("Program: LineEnding? (ProgramStatement LineEnding)* ProgramStatement? LineEnding?")]
	public ProgramAst Program(ValueOption<IAssemblyAst?> lineEnding1, List<Group<CslyTokens, IAssemblyAst>> statements, ValueOption<IAssemblyAst> last, ValueOption<IAssemblyAst?> lineEnding2) {
		var ret = statements.Select(group => group.Value(0)).Cast<ProgramStatementAst>().ToList();
		if (last.IsSome) {
			ret.Add((ProgramStatementAst) last.Match(i => i, () => throw new InvalidProgramException("OPIERFUAERDS987Y TGQH4WRT897 =MYB-6YH57 8B9N4343ER 890iumt")));
		}
		return new ProgramAst(ret.ToArray());
	}
}
