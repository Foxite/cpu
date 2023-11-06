using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class UnsupportedStatementException : Exception {
	public string ArchitectureName { get; }
	public IReadOnlyCollection<(ProgramStatementAst statement, int index)> Statements { get; }
	
	public UnsupportedStatementException(string architectureName, IReadOnlyCollection<(ProgramStatementAst statement, int index)> statements) {
		ArchitectureName = architectureName;
		Statements = statements;
	}

	//protected UnsupportedStatementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
