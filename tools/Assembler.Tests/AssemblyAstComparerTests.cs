namespace Assembler.Tests;

public class AssemblyAstComparerTests {
	[Test]
	public void TestProgramEquality1() {
		var left = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5)
		);

		var right = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5)
		);
		
		Assert.That(left, Is.EqualTo(right));
	}
	
	[Test]
	public void TestProgramEquality2() {
		var left = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5)
		);

		var right = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 6)
		);
		
		Assert.That(left, Is.Not.EqualTo(right));
	}
	
	[Test]
	public void TestProgramEquality3() {
		var left = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5)
		);

		var right = new ProgramAst(
			new DataWordInstruction(CpuRegister.B, 5)
		);
		
		Assert.That(left, Is.Not.EqualTo(right));
	}
	
	[Test]
	public void TestProgramEquality4() {
		var left = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5)
		);

		var right = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5),
			new DataWordInstruction(CpuRegister.A, 5)
		);
		
		Assert.That(left, Is.Not.EqualTo(right));
	}
	
	[Test]
	public void TestProgramEquality5() {
		var left = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 5)
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
		
		Assert.That(left, Is.Not.EqualTo(right));
	}

	[Test]
	public void TestProgramEquality6() {
		var left = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 0),
			new DataWordInstruction(CpuRegister.B, 2),
			new AluInstruction(
				new AluWriteTarget(CpuRegister.A),
				new AluOperand(CpuRegister.A),
				new AluOperand(1),
				AluOperation.Add
			),
			new JumpInstruction(
				new Condition(new AluOperand(CpuRegister.A), CompareOperation.GreaterThan, new AluOperand((short) 0)),
				CpuRegister.B
			)
		);
		
		// the same
		var right = new ProgramAst(
			new DataWordInstruction(CpuRegister.A, 0),
			new DataWordInstruction(CpuRegister.B, 2),
			new AluInstruction(
				new AluWriteTarget(CpuRegister.A),
				new AluOperand(CpuRegister.A),
				new AluOperand(1),
				AluOperation.Add
			),
			new JumpInstruction(
				new Condition(new AluOperand(CpuRegister.A), CompareOperation.GreaterThan, new AluOperand((short) 0)),
				CpuRegister.B
			)
		);
		
		Assert.That(left, Is.EqualTo(right));
	}
}
