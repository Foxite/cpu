namespace Assembler;

public record DataWordInstruction(
	CpuRegisterAst Register,
	bool IsConstant,
	ConstantAst? Value,
	SymbolAst? Symbol
) : IStatement {
	public DataWordInstruction(CpuRegister register, long constantValue) : this(new CpuRegisterAst(register), true, new ConstantAst(constantValue), null) { }
	public DataWordInstruction(CpuRegister register, string symbolName) : this(new CpuRegisterAst(register), false, null, new SymbolAst(symbolName)) { }
	
	public override string ToString() => $"DataWordInstruction {Register} = {(IAssemblyAst?) Value ?? Symbol}";
}
