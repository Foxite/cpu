using System.Runtime.CompilerServices;

namespace Assembler.Parsing;

public static class ParsingUtils {
	public static long ParseNumericLiteral(bool negative, int @base, string basePrefix, string tokenValue, Func<char, int> getDigitValue) {
		if (!tokenValue.StartsWith(basePrefix)) {
			throw new FormatException($"Token does not start with expected base prefix {basePrefix}: {tokenValue}");
		}

		string digits = tokenValue[basePrefix.Length..];
		
		int result = 0;
		foreach (char digit in digits) {
			if (digit == '_') {
				continue;
			} else {
				int digitValue;
				try {
					digitValue = getDigitValue(digit);
				} catch (Exception e) when (e is SwitchExpressionException or ArgumentException or ArgumentOutOfRangeException) {
					throw new FormatException("Invalid numeric literal", e);
				}
				result = result * @base + digitValue;
			}
		}

		if (negative) {
			result = -result;
		}
		
		return result;
	}
}
