using Assembler.Parsing.Proc16a;

namespace Assembler.Parsing; 

public interface IProc16aAssemblyParser {
	ProgramAst Parse(string sourceCode);
}
