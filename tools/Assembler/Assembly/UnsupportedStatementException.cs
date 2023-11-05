using Assembler.Parsing.Proc16a;

namespace Assembler.Assembly.Proc16a;

public class UnsupportedStatementException : Exception {
	public string ArchitectureName { get; }
	public IReadOnlyCollection<(IStatement statement, int index)> Statements { get; }
	
	public UnsupportedStatementException(string architectureName, IReadOnlyCollection<(IStatement statement, int index)> statements) {
		ArchitectureName = architectureName;
		Statements = statements;
	}

	//protected UnsupportedStatementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
