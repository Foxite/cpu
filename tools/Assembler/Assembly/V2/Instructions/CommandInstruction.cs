namespace Assembler.Assembly;

public abstract record CommandInstruction(string? Label) : AssemblyInstruction(Label);
