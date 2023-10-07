using System.Globalization;
using sly.lexer;
using sly.parser.generator;
using sly.parser.parser;

namespace Assembler;

public class AssemblyParser {
	[Production("Statement: Label")]
	public LabelElement LabelAst(Token<AssemblyToken> label) {
		return new LabelElement(label.Value[..^1]);
	}
	
	[Production("Number: DecimalInteger")]
	public ConstantAst DecimalNumber(Token<AssemblyToken> token) {
		return new ConstantAst(short.Parse(token.Value.Replace("_", "")));
	}
	
	[Production("Number: HexadecimalInteger")]
	public ConstantAst HexadecimalNumber(Token<AssemblyToken> token) {
		return new ConstantAst(short.Parse(token.Value["0x".Length..].Replace("_", ""), NumberStyles.AllowHexSpecifier));
	}
	
	[Production("Number: BinaryInteger")]
	public ConstantAst BinaryNumber(Token<AssemblyToken> token) {
		string bits = token.Value["0b".Length..];
		short ret = 0;
		for (int i = bits.Length - 1; i >= 0; i--) {
			ret |= (short) (bits[i] == '1' ? 1 : 0);
			ret <<= 1;
		}

		return new ConstantAst(ret);
	}
	
	[Production("Boolean: True")]
	[Production("Boolean: False")]
	public BooleanAst Boolean(Token<AssemblyToken> token) {
		return new BooleanAst(token.TokenID == AssemblyToken.True);
	}

	[Production("Statement: Register Assign [d] Number")]
	public IStatement DataWord(CpuRegisterAst register, ConstantAst number) {
		return new DataWordInstruction(register, true, number, null);
	}

	[Production("Statement: Register Assign [d] Symbol")]
	public IStatement DataWord(CpuRegisterAst register, Token<AssemblyToken> symbol) {
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
	public CpuRegisterAst Register(Token<AssemblyToken> register) {
		return new CpuRegisterAst(register.TokenID switch {
			AssemblyToken.ARegister => CpuRegister.A,
			AssemblyToken.BRegister => CpuRegister.B,
			AssemblyToken.StarA => CpuRegister.StarA,
			AssemblyToken.StarB => CpuRegister.StarB,
		});
	}

	[Production("AluWriteTarget: (Register Comma [d])* Register")]
	public AluWriteTarget AluWriteTarget(List<Group<AssemblyToken, IAssemblyAst>> many, CpuRegisterAst single) {
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
	public AluOperationAst AluOperation(Token<AssemblyToken> token) {
		return new AluOperationAst(token.TokenID switch {
			AssemblyToken.Plus					=> Assembler.AluOperation.Add,
			AssemblyToken.Minus					=> Assembler.AluOperation.Subtract,
			AssemblyToken.Multiply				=> Assembler.AluOperation.Multiply,
			AssemblyToken.Divide				=> Assembler.AluOperation.Divide,
			AssemblyToken.LeftShift				=> Assembler.AluOperation.ShiftLeft,
			AssemblyToken.RightShift			=> Assembler.AluOperation.ShiftRight,
			AssemblyToken.BitwiseAnd			=> Assembler.AluOperation.BitwiseAnd,
			AssemblyToken.BitwiseOr				=> Assembler.AluOperation.BitwiseOr,
			AssemblyToken.BitwiseNot			=> Assembler.AluOperation.BitwiseNot,
			AssemblyToken.BitwiseXor			=> Assembler.AluOperation.BitwiseXor,
			AssemblyToken.BitwiseXnor			=> Assembler.AluOperation.BitwiseXnor,
			AssemblyToken.BitwiseNor			=> Assembler.AluOperation.BitwiseNor,
			AssemblyToken.BitwiseNand			=> Assembler.AluOperation.BitwiseNand,
			AssemblyToken.GreaterThan			=> Assembler.AluOperation.GreaterThan,
			AssemblyToken.Equals				=> Assembler.AluOperation.Equals,
			AssemblyToken.GreaterThanOrEquals	=> Assembler.AluOperation.GreaterThanOrEquals,
			AssemblyToken.LessThan				=> Assembler.AluOperation.LessThan,
			AssemblyToken.NotEquals				=> Assembler.AluOperation.NotEquals,
			AssemblyToken.LessThanOrEquals		=> Assembler.AluOperation.LessThanOrEquals,
			AssemblyToken.True					=> Assembler.AluOperation.True,
			AssemblyToken.False					=> Assembler.AluOperation.False,
		});
	}
	
	[Production("Statement: AluWriteTarget Assign [d] AluOperand AluOperation AluOperand")]
	public IStatement AluInstruction(AluWriteTarget writeTarget, AluOperand x, AluOperationAst operation, AluOperand y) {
		return new AluInstruction(writeTarget, x, y, operation);
	}

	[Production("CompareOperation: GreaterThan")]
	[Production("CompareOperation: Equals")]
	[Production("CompareOperation: GreaterThanOrEquals")]
	[Production("CompareOperation: LessThan")]
	[Production("CompareOperation: NotEquals")]
	[Production("CompareOperation: LessThanOrEquals")]
	public CompareOperationAst CompareOperation(Token<AssemblyToken> token) {
		return new CompareOperationAst(token.TokenID switch {
			AssemblyToken.GreaterThan 			=> Assembler.CompareOperation.GreaterThan,
			AssemblyToken.Equals 				=> Assembler.CompareOperation.Equals,
			AssemblyToken.GreaterThanOrEquals 	=> Assembler.CompareOperation.GreaterThanOrEquals,
			AssemblyToken.LessThan 				=> Assembler.CompareOperation.LessThan,
			AssemblyToken.NotEquals 			=> Assembler.CompareOperation.NotEquals,
			AssemblyToken.LessThanOrEquals 		=> Assembler.CompareOperation.LessThanOrEquals,
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
	public IAssemblyAst? LineEnding(List<Token<AssemblyToken>> ignored) {
		return null;
	}

	[Production("Program: LineEnding? (Statement LineEnding)* Statement? LineEnding?")]
	public ProgramAst Program(ValueOption<IAssemblyAst?> lineEnding1, List<Group<AssemblyToken, IAssemblyAst>> statements, ValueOption<IAssemblyAst> last, ValueOption<IAssemblyAst?> lineEnding2) {
		var ret = statements.Select(group => group.Value(0)).Cast<IStatement>().ToList();
		if (last.IsSome) {
			ret.Add((IStatement) last.Match(i => i, () => throw new InvalidProgramException("OPIERFUAERDS987Y TGQH4WRT897 =MYB-6YH57 8B9N4343ER 890iumt")));
		}
		return new ProgramAst(ret.ToArray());
	}
}
