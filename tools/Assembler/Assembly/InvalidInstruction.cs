using Assembler.Ast;

namespace Assembler.Assembly;

public record InvalidInstruction(InstructionAst Instruction, int Index, InstructionSupport Support, string Message);
