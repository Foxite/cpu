using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace Assembler.Parsing.Proc16a.Csly;

public class Proc16aCslyGrammars {
	[Production("Number: Minus? DecimalInteger")]
	public ConstantAst DecimalNumber(Token<Proc16aCslyTokens> minus, Token<Proc16aCslyTokens> token) {
		return new ConstantAst(ParsingUtils.ParseNumericLiteral(!minus.IsEmpty, 10, "", token.Value, ch => ch - '0'));
	}
	
	[Production("Number: Minus? HexadecimalInteger")]
	public ConstantAst HexadecimalNumber(Token<Proc16aCslyTokens> minus, Token<Proc16aCslyTokens> token) {
		return new ConstantAst(ParsingUtils.ParseNumericLiteral(!minus.IsEmpty, 16, "0x", token.Value.ToLower(), ch => ch switch {
			>= '0' and <= '9' => ch - '0',
			>= 'a' and <= 'f' => ch - 'a' + 10,
		}));
	}
	
	[Production("Number: Minus? BinaryInteger")]
	public ConstantAst BinaryNumber(Token<Proc16aCslyTokens> minus, Token<Proc16aCslyTokens> token) {
		return new ConstantAst(ParsingUtils.ParseNumericLiteral(!minus.IsEmpty, 2, "0b", token.Value, ch => ch == '0' ? 0 : 1));
	}
	
	[Production("Boolean: True")]
	[Production("Boolean: False")]
	public BooleanAst Boolean(Token<Proc16aCslyTokens> token) {
		return new BooleanAst(token.TokenID == Proc16aCslyTokens.True);
	}

	[Production("Statement: Register Assign [d] Number")]
	public IStatement DataWord(CpuRegisterAst register, ConstantAst number) {
		return new DataWordInstruction(register, true, number, null);
	}

	[Production("Statement: Register Assign [d] Symbol")]
	public IStatement DataWord(CpuRegisterAst register, Token<Proc16aCslyTokens> symbol) {
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
	public CpuRegisterAst Register(Token<Proc16aCslyTokens> register) {
		return new CpuRegisterAst(register.TokenID switch {
			Proc16aCslyTokens.ARegister => CpuRegister.A,
			Proc16aCslyTokens.BRegister => CpuRegister.B,
			Proc16aCslyTokens.StarA => CpuRegister.StarA,
			Proc16aCslyTokens.StarB => CpuRegister.StarB,
		});
	}

	[Production("AluWriteTarget: (Register Comma [d])* Register")]
	public AluWriteTarget AluWriteTarget(List<Group<Proc16aCslyTokens, IAssemblyAst>> many, CpuRegisterAst single) {
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
	public AluOperationAst AluOperation(Token<Proc16aCslyTokens> token) {
		return new AluOperationAst(token.TokenID switch {
			Proc16aCslyTokens.Plus                => Proc16a.AluOperation.Add,
			Proc16aCslyTokens.Minus               => Proc16a.AluOperation.Subtract,
			Proc16aCslyTokens.Multiply            => Proc16a.AluOperation.Multiply,
			Proc16aCslyTokens.Divide              => Proc16a.AluOperation.Divide,
			Proc16aCslyTokens.LeftShift           => Proc16a.AluOperation.ShiftLeft,
			Proc16aCslyTokens.RightShift          => Proc16a.AluOperation.ShiftRight,
			Proc16aCslyTokens.BitwiseAnd          => Proc16a.AluOperation.BitwiseAnd,
			Proc16aCslyTokens.BitwiseOr           => Proc16a.AluOperation.BitwiseOr,
			Proc16aCslyTokens.BitwiseNot          => Proc16a.AluOperation.BitwiseNot,
			Proc16aCslyTokens.BitwiseXor          => Proc16a.AluOperation.BitwiseXor,
			Proc16aCslyTokens.BitwiseXnor         => Proc16a.AluOperation.BitwiseXnor,
			Proc16aCslyTokens.BitwiseNor          => Proc16a.AluOperation.BitwiseNor,
			Proc16aCslyTokens.BitwiseNand         => Proc16a.AluOperation.BitwiseNand,
			Proc16aCslyTokens.GreaterThan         => Proc16a.AluOperation.GreaterThan,
			Proc16aCslyTokens.Equals              => Proc16a.AluOperation.Equals,
			Proc16aCslyTokens.GreaterThanOrEquals => Proc16a.AluOperation.GreaterThanOrEquals,
			Proc16aCslyTokens.LessThan            => Proc16a.AluOperation.LessThan,
			Proc16aCslyTokens.NotEquals           => Proc16a.AluOperation.NotEquals,
			Proc16aCslyTokens.LessThanOrEquals    => Proc16a.AluOperation.LessThanOrEquals,
			Proc16aCslyTokens.True                => Proc16a.AluOperation.True,
			Proc16aCslyTokens.False               => Proc16a.AluOperation.False,
		});
	}
	
	[Production("Statement: AluWriteTarget Assign [d] AluOperand AluOperation AluOperand")]
	public IStatement AluInstruction(AluWriteTarget writeTarget, AluOperand x, AluOperationAst operation, AluOperand y) {
		return new AluInstruction(writeTarget, x, y, operation);
	}
	
	// TODO unit test this
	[Production("Statement: AluWriteTarget Assign [d] BitwiseNot [d] AluOperand")]
	public IStatement AluBitwiseNot(AluWriteTarget writeTarget, AluOperand x) {
		return new AluInstruction(writeTarget, x, null, Proc16a.AluOperation.BitwiseNot);
	}

	[Production("CompareOperation: GreaterThan")]
	[Production("CompareOperation: Equals")]
	[Production("CompareOperation: GreaterThanOrEquals")]
	[Production("CompareOperation: LessThan")]
	[Production("CompareOperation: NotEquals")]
	[Production("CompareOperation: LessThanOrEquals")]
	public CompareOperationAst CompareOperation(Token<Proc16aCslyTokens> token) {
		return new CompareOperationAst(token.TokenID switch {
			Proc16aCslyTokens.GreaterThan 			=> Proc16a.CompareOperation.GreaterThan,
			Proc16aCslyTokens.Equals 				=> Proc16a.CompareOperation.Equals,
			Proc16aCslyTokens.GreaterThanOrEquals 	=> Proc16a.CompareOperation.GreaterThanOrEquals,
			Proc16aCslyTokens.LessThan 				=> Proc16a.CompareOperation.LessThan,
			Proc16aCslyTokens.NotEquals 			=> Proc16a.CompareOperation.NotEquals,
			Proc16aCslyTokens.LessThanOrEquals 		=> Proc16a.CompareOperation.LessThanOrEquals,
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
	public IAssemblyAst? LineEnding(List<Token<Proc16aCslyTokens>> ignored) {
		return null;
	}

	[Production("OptionalLineEnding: LineEnding?")]
	public IAssemblyAst? OptionalLineEnding(ValueOption<IAssemblyAst?> ignored) {
		return null;
	}

	[Production("ProgramStatement: (Symbol Colon OptionalLineEnding)? Statement")]
	public ProgramStatementAst ProgramStatement(ValueOption<Group<Proc16aCslyTokens, IAssemblyAst>> label, IStatement statement) {
		string? labelName = null;
		if (label.IsSome) {
			labelName = label.Match(g => g, () => throw new NotImplementedException()).Token(0).Value;
		}

		return new ProgramStatementAst(labelName, statement);
	}

	[Production("Program: LineEnding? (ProgramStatement LineEnding)* ProgramStatement? LineEnding?")]
	public ProgramAst Program(ValueOption<IAssemblyAst?> lineEnding1, List<Group<Proc16aCslyTokens, IAssemblyAst>> statements, ValueOption<IAssemblyAst> last, ValueOption<IAssemblyAst?> lineEnding2) {
		var ret = statements.Select(group => group.Value(0)).Cast<ProgramStatementAst>().ToList();
		if (last.IsSome) {
			ret.Add((ProgramStatementAst) last.Match(i => i, () => throw new InvalidProgramException("OPIERFUAERDS987Y TGQH4WRT897 =MYB-6YH57 8B9N4343ER 890iumt")));
		}
		return new ProgramAst(ret.ToArray());
	}
}
