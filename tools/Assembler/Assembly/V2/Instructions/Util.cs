using Assembler.Ast;

namespace Assembler.Assembly.V2;

public static class Util {
	public static IReadOnlyList<InstructionArgumentAst> ReplaceSymbols(this AssemblyContext context, IReadOnlyList<InstructionArgumentAst> arguments, AssemblyInstruction assemblyInstruction) {
		return arguments.Select(arg => {
			if (arg is SymbolAst symbolArg) {
				return context.GetSymbolValue(symbolArg.Value, assemblyInstruction);
			} else {
				return arg;
			}
		}).ToList();
	}
}
