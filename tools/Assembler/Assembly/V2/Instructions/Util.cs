using Assembler.Ast;

namespace Assembler.Assembly.V2;

public static class Util {
	public static IReadOnlyList<InstructionArgumentAst> ReplaceSymbols(this AssemblyContext context, IReadOnlyList<InstructionArgumentAst> arguments, AssemblyInstruction assemblyInstruction) {
		return arguments.Select(arg => {
			return arg switch {
				ExpressionAst expression => new ConstantAst(
					expression.File,
					expression.LineNumber,
					expression.Column,
					AssemblyUtil.EvaluateExpression(expression, symbol => (IExpressionElement) context.GetSymbolValue(symbol, assemblyInstruction))
				),
				SymbolAst symbolArg      => context.GetSymbolValue(symbolArg.Value, assemblyInstruction),
				_                        => arg
			};
		}).ToList();
	}
}
