namespace Assembler.Parsing.Proc16a;

public record AssignInstruction(
	CpuRegisterAst Write,
	CpuRegisterAst Read
) : IStatement {
	public AssignInstruction(CpuRegister write, CpuRegister read) : this(new CpuRegisterAst(write), new CpuRegisterAst(read)) { }
	
	public override string ToString() => $"AssignInstruction {Write} = {Read}";
}
