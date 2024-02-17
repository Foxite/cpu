namespace Assembler.Assembly;

public class SymbolNotDefinedException : Exception {
	public string Symbol { get; }
	
	public SymbolNotDefinedException(string symbol) {
		Symbol = symbol;
	}
}
