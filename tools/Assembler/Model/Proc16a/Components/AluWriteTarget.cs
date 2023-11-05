namespace Assembler.Parsing.Proc16a;

public record AluWriteTarget(params CpuRegisterAst[] Registers) : IAssemblyAst {
	public AluWriteTarget(params CpuRegister[] registers) : this(registers.Select(reg => new CpuRegisterAst(reg)).ToArray()) { }
	
	public override string ToString() => $"write to {string.Join(", ", Registers.Select(register => register.ToString()))}";

	public virtual bool Equals(AluWriteTarget? other) => other != null && other.Registers.SequenceEqual(Registers);
	public override int GetHashCode() => 0;
}
