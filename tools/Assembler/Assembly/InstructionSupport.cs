namespace Assembler.Assembly;

public enum InstructionSupport {
	Supported,
	NotRecognized,
	ParameterType,
	ParameterCount,
	OtherError,
	SymbolNotDefined,
}
