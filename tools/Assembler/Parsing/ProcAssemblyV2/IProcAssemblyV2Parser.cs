namespace Assembler.Parsing.ProcAssemblyV2.Antlr; 

public interface IProcAssemblyV2Parser {
	ProgramAst Parse(string sourceCode);
}
