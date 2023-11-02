namespace Assembler.Parsing; 

public interface IAssemblyParser {
	ProgramAst Parse(string sourceCode);
}
