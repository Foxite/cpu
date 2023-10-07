namespace Assembler.Tests;

public class AssemblyAstComparerTests {
	[Test]
	public void TestProgramEquality1() {
		var left = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);

		var right = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);
		
		Assert.That(left, Is.EqualTo(right).Using(new AssemblyAstComparer()));
	}
	
	[Test]
	public void TestProgramEquality2() {
		var left = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);

		var right = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(6))
		);
		
		Assert.That(left, Is.Not.EqualTo(right).Using(new AssemblyAstComparer()));
	}
	
	[Test]
	public void TestProgramEquality3() {
		var left = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);

		var right = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.B), new ConstantAst(5))
		);
		
		Assert.That(left, Is.Not.EqualTo(right).Using(new AssemblyAstComparer()));
	}
	
	[Test]
	public void TestProgramEquality4() {
		var left = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);

		var right = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5)),
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);
		
		Assert.That(left, Is.Not.EqualTo(right).Using(new AssemblyAstComparer()));
	}
	
	[Test]
	public void TestProgramEquality5() {
		var left = new ProgramAst(
			new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
		);

		var right = new ProgramAst(
			new JumpInstruction(
				true,
				new Condition(
					new AluOperand(true, new CpuRegisterAst(CpuRegister.A), null),
					new CompareOperationAst(CompareOperation.Equals),
					new AluOperand(false, null, new ConstantAst(0))
				),
				null,
				new CpuRegisterAst(CpuRegister.B)
			)
		);
		
		Assert.That(left, Is.Not.EqualTo(right).Using(new AssemblyAstComparer()));
	}
}
