namespace Assembler.Ast;

public interface IAssemblyAst {
	public string File { get; }
	public int LineNumber { get; }
	public int Column { get; }
}
