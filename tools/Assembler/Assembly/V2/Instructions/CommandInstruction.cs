namespace Assembler.Assembly.V2;

public abstract record CommandInstruction(string? Label) : AssemblyInstruction(Label);
