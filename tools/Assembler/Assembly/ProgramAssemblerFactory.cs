using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class ProgramAssemblerFactory {
	public IEnumerable<string> SupportedArchitectures => new []{ "proc16a", "proc16b" };
	
	public ProgramAssembler GetAssembler(string architecture, ProgramAst programAst) {
		return architecture.ToLower() switch {
			"proc16a" => new ProgramAssembler(new Proc16aInstructionConverter(), programAst),
			"proc16b" => new ProgramAssembler(new Proc16bInstructionConverter(), programAst),
		};
	}
	
	public bool CanGetAssembler(string architecture) {
		return architecture.ToLower() is "proc16a" or "proc16b";
	}
}
