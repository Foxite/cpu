namespace Assembler.Assembly; 

/// <summary>
/// Converts <see cref="ProgramAst"/> into a sequence of bytes for a particular architecture.
/// </summary>
public abstract class ProgramAssembler<TAst> {
	public abstract string ArchitectureName { get; }

	public abstract IEnumerable<ushort> Assemble(TAst program);
}
