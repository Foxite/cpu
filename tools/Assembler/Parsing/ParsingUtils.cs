namespace Assembler.Parsing;

public static class ParsingUtils {
	public static long ParseNumericLiteral(bool negative, int @base, string basePrefix, string tokenValue, Func<char, int> getDigitValue) {
		if (!tokenValue.StartsWith(basePrefix)) {
			throw new FormatException($"Token does not start with expected base prefix {basePrefix}: {tokenValue}");
		}

		string digits = tokenValue[basePrefix.Length..];
		
		int result = 0;
		for (int i = 0; i < digits.Length; i++) {
			if (digits[i] == '_') {
				continue;
			} else {
				result = result * @base + getDigitValue(digits[i]);
			}
		}

		if (negative) {
			result = -result;
		}
		
		return result;
	}
}
