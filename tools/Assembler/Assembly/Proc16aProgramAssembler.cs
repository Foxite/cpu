using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class Proc16aProgramAssembler : ProgramAssembler {
	public override string ArchitectureName => "proc16a";
	
	protected internal override bool ValidateInstruction(InstructionAst statementInstruction) {
		throw new NotImplementedException();
	}
	
	protected internal override ushort ConvertInstruction(InstructionAst statementInstruction, Func<string, ushort> getSymbolDefinitions) {
		throw new NotImplementedException();
	}
}
