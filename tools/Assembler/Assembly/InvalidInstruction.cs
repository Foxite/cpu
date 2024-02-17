using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public record InvalidInstruction(InstructionAst Instruction, int Index, string Message);
