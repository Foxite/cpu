using Assembler.Ast;

namespace Assembler.Assembly.V2;

public static class Util {
	public static IReadOnlyList<InstructionArgumentAst> ReplaceSymbols(this AssemblyContext context, IReadOnlyList<InstructionArgumentAst> arguments) {
		return arguments.Select(arg => {
			if (arg is SymbolAst symbolArg) {
				return context.GetSymbolValue(symbolArg.Value);
			} else {
				return arg;
			}
		}).ToList();
	}
}
