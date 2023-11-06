using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public class UnsupportedInstuctionException : Exception {
	public string ArchitectureName { get; }
	public IReadOnlyCollection<(InstructionAst Instruction, int Index)> Instructions { get; }
	
	public UnsupportedInstuctionException(string architectureName, IReadOnlyCollection<(InstructionAst Instruction, int Index)> instructions) {
		ArchitectureName = architectureName;
		Instructions = instructions;
	}

	//protected UnsupportedStatementException(SerializationInfo info, StreamingContext context) : base(info, context) { }
}
