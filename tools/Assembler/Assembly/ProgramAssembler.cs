namespace Assembler.Assembly; 

/// <summary>
/// Converts <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture.
/// </summary>
public abstract class ProgramAssembler {
	public abstract string ArchitectureName { get; }
	
	public IEnumerable<ushort> Assemble(ProgramAst program) {
		var symbolDefinitions = new Dictionary<string, short>();
		var unsupportedStatements = new Stack<(IStatement statement, int index)>();

		for (int i = 0; i < program.Statements.Length; i++) {
			IStatement statement = program.Statements[i];
			
			if (statement is LabelElement labelElement) {
				symbolDefinitions[labelElement.Name] = (short) i;
			}

			if (!ValidateStatement(statement)) {
				unsupportedStatements.Push((statement, i));
			}
		}

		if (unsupportedStatements.Count > 0) {
			throw new UnsupportedStatementException(ArchitectureName, unsupportedStatements);
		}

		foreach (IStatement statement in program.Statements) {
			yield return ConvertStatement(statement, name => symbolDefinitions[name]);
		}
	}

	/// <summary>
	/// Indicates if this architecture can support the given statement.
	/// </summary>
	protected internal abstract bool ValidateStatement(IStatement statement);
	
	/// <summary>
	/// Provides bytes for the given statement.
	/// </summary>
	protected internal abstract ushort ConvertStatement(IStatement statement, Func<string, short> getSymbolValue);
}
