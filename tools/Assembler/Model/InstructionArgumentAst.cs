namespace Assembler.Ast;

public abstract record InstructionArgumentAst(string File, int LineNumber, int Column) : IAssemblyAst {
	public abstract override string ToString();
	
	public static ConstantAst           Constant(long value) => new ConstantAst("TEST", 0, 0, value);
	public static RegisterAst           Register(string value) => new RegisterAst("TEST", 0, 0, value);
	public static SymbolAst             Symbol(string value) => new SymbolAst("TEST", 0, 0, value);
	public static StringAst             String(string value) => new StringAst("TEST", 0, 0, value);
	public static UnaryOpExpressionAst  Expression(UnaryExpressionOp op, IExpressionElement expressionAst) => new UnaryOpExpressionAst("TEST", 0, 0, op, expressionAst);
	public static BinaryOpExpressionAst Expression(IExpressionElement left, BinaryExpressionOp op, IExpressionElement right) => new BinaryOpExpressionAst("TEST", 0, 0, left, op, right);
}

public interface IExpressionElement : IAssemblyAst { }

public record ConstantAst(string File, int LineNumber, int Column, long Value) : InstructionArgumentAst(File, LineNumber, Column), IExpressionElement {
	public override string ToString() => $"Constant {Value}";

	public virtual bool Equals(ConstantAst? other) => other != null && Value == other.Value;
}

public record RegisterAst(string File, int LineNumber, int Column, string Value) : InstructionArgumentAst(File, LineNumber, Column) {
	public override string ToString() => $"Register {Value}";

	public virtual bool Equals(RegisterAst? other) => other != null && Value == other.Value;
}

public record SymbolAst(string File, int LineNumber, int Column, string Value) : InstructionArgumentAst(File, LineNumber, Column), IExpressionElement {
	public override string ToString() => $"Symbol {Value}";

	public virtual bool Equals(SymbolAst? other) => other != null && Value == other.Value;
}

public record StringAst(string File, int LineNumber, int Column, string Value) : InstructionArgumentAst(File, LineNumber, Column) {
	public override string ToString() => $"String \"{Value}\"";

	public virtual bool Equals(StringAst? other) => other != null && Value == other.Value;
}

public abstract record ExpressionAst(string File, int LineNumber, int Column) : InstructionArgumentAst(File, LineNumber, Column), IExpressionElement;
public record BinaryOpExpressionAst(string File, int LineNumber, int Column, IExpressionElement Left, BinaryExpressionOp Operator, IExpressionElement Right) : ExpressionAst(File, LineNumber, Column) {
	public override string ToString() => $"[{Left} {Operator} {Right}]";

	public virtual bool Equals(BinaryOpExpressionAst? other) => other != null && Left.Equals(other.Left) && Operator == other.Operator && Right.Equals(other.Right);
}

public record UnaryOpExpressionAst(string File, int LineNumber, int Column, UnaryExpressionOp Operator, IExpressionElement Operand) : ExpressionAst(File, LineNumber, Column) {
	public override string ToString() => $"[{Operator} {Operand}]";

	public virtual bool Equals(UnaryOpExpressionAst? other) => other != null && Operator == other.Operator && Operand.Equals(other.Operand);
}

public enum BinaryExpressionOp {
	Add,
	Subtract,
	Multiply,
	Divide,
	LeftShift,
	RightShift,
	And,
	Or,
	Xor,
}

public enum UnaryExpressionOp {
	Not,
}
