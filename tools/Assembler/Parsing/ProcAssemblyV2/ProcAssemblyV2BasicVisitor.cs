using Antlr4.Runtime.Tree;

namespace Assembler.Parsing.ProcAssemblyV2.Antlr;

public class ProcAssemblyV2BasicVisitor : ProcAssemblyV2GrammarBaseVisitor<IAssemblyAst> {
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
	
	
}
