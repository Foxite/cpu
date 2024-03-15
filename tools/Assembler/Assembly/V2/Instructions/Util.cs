using Assembler.Ast;

namespace Assembler.Assembly.V2;

public static class Util {
	public static IReadOnlyList<InstructionArgumentAst> ReplaceSymbols(this AssemblyContext context, IReadOnlyList<InstructionArgumentAst> arguments) {
		return arguments.Select(arg => {
			if (arg.Type == InstructionArgumentType.Symbol) {
				return context.GetSymbolValue(arg.RslsValue!);
			} else {
				return arg;
			}
		}).ToList();
	}
}
