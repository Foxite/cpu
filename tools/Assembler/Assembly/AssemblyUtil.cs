using Assembler.Ast;

namespace Assembler.Assembly;

public static class AssemblyUtil {
	public static ushort SetBit(ushort instruction, int bit, bool value) {
		if (value) {
			return (ushort) (instruction | (1 << bit));
		} else {
			return (ushort) (instruction & (~(1 << bit)));
		}
	}

	private static InstructionSupport GetArguments(this InstructionAst instruction, out InstructionArgumentAst[] result, params Type[] types) {
		result = new InstructionArgumentAst[types.Length];
		
		if (types.Length != instruction.Arguments.Count) {
			return InstructionSupport.ParameterType;
		}

		for (int i = 0; i < types.Length; i++) {
			if (instruction.Arguments[i].GetType() == types.GetType()) {
				result[i] = instruction.Arguments[i];
			} else {
				return InstructionSupport.ParameterType;
			}
		}

		return InstructionSupport.Supported;
	}

	public static InstructionSupport GetArguments<T1>(this InstructionAst instruction, out T1 arg1)
		where T1 : InstructionArgumentAst {
		var ret = GetArguments(instruction, out InstructionArgumentAst[] result, typeof(T1));
		arg1 = (T1) result[0];
		return ret;
	}

	public static InstructionSupport GetArguments<T1, T2>(this InstructionAst instruction, out T1 arg1, out T2 arg2)
        where T1 : InstructionArgumentAst
        where T2 : InstructionArgumentAst {
		var ret = GetArguments(instruction, out InstructionArgumentAst[] result, typeof(T1));
		arg1 = (T1) result[0];
		arg2 = (T2) result[1];
		return ret;
	}

	public static InstructionSupport GetArguments<T1, T2, T3>(this InstructionAst instruction, out T1 arg1, out T2 arg2, out T3 arg3)
        where T1 : InstructionArgumentAst
        where T2 : InstructionArgumentAst
		where T3 : InstructionArgumentAst {
		var ret = GetArguments(instruction, out InstructionArgumentAst[] result, typeof(T1));
		arg1 = (T1) result[0];
		arg2 = (T2) result[1];
		arg3 = (T3) result[2];
		return ret;
	}

	public static InstructionSupport GetArguments<T1, T2, T3, T4>(this InstructionAst instruction, out T1 arg1, out T2 arg2, out T3 arg3, out T4 arg4)
        where T1 : InstructionArgumentAst
        where T2 : InstructionArgumentAst
		where T3 : InstructionArgumentAst
		where T4 : InstructionArgumentAst {
		var ret = GetArguments(instruction, out InstructionArgumentAst[] result, typeof(T1));
		arg1 = (T1) result[0];
		arg2 = (T2) result[2];
		arg3 = (T3) result[2];
		arg4 = (T4) result[3];
		return ret;
	}

	public static long EvaluateExpression(IExpressionElement expression, Func<string, IExpressionElement> getSymbol) {
		return expression switch {
			ConstantAst           constantAst           => constantAst.Value,
			SymbolAst             symbolAst             => EvaluateExpression(getSymbol(symbolAst.Value), getSymbol),
			BinaryOpExpressionAst binaryOpExpressionAst => binaryOpExpressionAst.Operator switch {
				BinaryExpressionOp.Add        => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) +  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.Subtract   => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) -  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.Multiply   => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) *  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.Divide     => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) /  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.LeftShift  => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) << (int) EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.RightShift => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) >> (int) EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.And        => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) &  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.Or         => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) |  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
				BinaryExpressionOp.Xor        => EvaluateExpression(binaryOpExpressionAst.Left, getSymbol) ^  EvaluateExpression(binaryOpExpressionAst.Right, getSymbol),
			},
			UnaryOpExpressionAst  unaryOpExpressionAst => unaryOpExpressionAst.Operator switch {
				UnaryExpressionOp.Not => ~EvaluateExpression(unaryOpExpressionAst.Operand, getSymbol),
			},
			_ => throw new ArgumentOutOfRangeException(nameof(expression)),
		};
	}
}
