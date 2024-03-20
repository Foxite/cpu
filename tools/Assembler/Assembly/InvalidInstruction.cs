using Assembler.Ast;

namespace Assembler.Assembly;

public record InvalidInstruction(InstructionAst Instruction, InstructionSupport Support, string Message);
