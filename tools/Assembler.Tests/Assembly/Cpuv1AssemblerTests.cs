using Assembler.Assembly;

namespace Assembler.Tests; 

public class Cpuv1AssemblerTests {
	private Cpuv1ProgramAssembler m_Assembler;

	[SetUp]
	public void Setup() {
		m_Assembler = new Cpuv1ProgramAssembler();
	}
	
	public static object[][] ValidationTestCases() => new[] {
		// data word instruction
		new object[] { new DataWordInstruction(CpuRegister.A, 0), true }, // can assign any positive value to A register as well as -1 or -2 (using the ALU)
		new object[] { new DataWordInstruction(CpuRegister.A, 1), true },
		new object[] { new DataWordInstruction(CpuRegister.A, 2), true },
		new object[] { new DataWordInstruction(CpuRegister.A, -1), true },
		new object[] { new DataWordInstruction(CpuRegister.A, -2), true },
		new object[] { new DataWordInstruction(CpuRegister.A, -3), false },
		new object[] { new DataWordInstruction(CpuRegister.A, 3), true },
		new object[] { new DataWordInstruction(CpuRegister.A, 1234), true },
		
		new object[] { new DataWordInstruction(CpuRegister.B, 0), true }, // can assign values between -2 and 2 to any other register by using the ALU with only constants
		new object[] { new DataWordInstruction(CpuRegister.B, 1), true },
		new object[] { new DataWordInstruction(CpuRegister.B, 2), true },
		new object[] { new DataWordInstruction(CpuRegister.B, -1), true },
		new object[] { new DataWordInstruction(CpuRegister.B, -2), true },
		new object[] { new DataWordInstruction(CpuRegister.B, -3), false },
		new object[] { new DataWordInstruction(CpuRegister.B, 3), false },
		
		new object[] { new DataWordInstruction(CpuRegister.StarA, 0), true },
		new object[] { new DataWordInstruction(CpuRegister.StarA, 1), true },
		new object[] { new DataWordInstruction(CpuRegister.StarA, 2), true },
		new object[] { new DataWordInstruction(CpuRegister.StarA, -1), true },
		new object[] { new DataWordInstruction(CpuRegister.StarA, -2), true },
		new object[] { new DataWordInstruction(CpuRegister.StarA, -3), false },
		new object[] { new DataWordInstruction(CpuRegister.StarA, 3), false },
		
		new object[] { new DataWordInstruction(CpuRegister.StarB, 0), true },
		new object[] { new DataWordInstruction(CpuRegister.StarB, 1), true },
		new object[] { new DataWordInstruction(CpuRegister.StarB, 2), true },
		new object[] { new DataWordInstruction(CpuRegister.StarB, -1), true },
		new object[] { new DataWordInstruction(CpuRegister.StarB, -2), true },
		new object[] { new DataWordInstruction(CpuRegister.StarB, -3), false },
		new object[] { new DataWordInstruction(CpuRegister.StarB, 3), false },
		
		
		// alu instructions
		
		// cannot operate on constants other than 0 or 1
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.A), new AluOperand((short) 0), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.A), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), new AluOperand(2), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), new AluOperand(3), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), new AluOperand(-1), AluOperation.Add), false },
		
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand((short) 0), new AluOperand(CpuRegister.A), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(1), new AluOperand(CpuRegister.A), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(1), new AluOperand(CpuRegister.B), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(2), new AluOperand(CpuRegister.B), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(3), new AluOperand(CpuRegister.B), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(-1), new AluOperand(CpuRegister.B), AluOperation.Add), false },
		
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand((short) 0), new AluOperand((short) 0), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(1), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(1), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(2), new AluOperand(2), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(3), new AluOperand(3), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(-1), new AluOperand(-1), AluOperation.Add), false },
		
		// can operate on ram as long as all Star registers are the same one
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.StarA), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.StarA), new AluOperand(CpuRegister.A), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.StarB), new AluOperand(CpuRegister.A), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.StarB), new AluOperand(CpuRegister.StarB), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.StarA), new AluOperand(CpuRegister.StarA), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.StarB), new AluOperand(CpuRegister.StarA), new AluOperand(1), AluOperation.Add), false },
		
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.StarA), new AluOperand(CpuRegister.A), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.A), new AluOperand(CpuRegister.StarA), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.A), new AluOperand(CpuRegister.StarB), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.StarB), new AluOperand(CpuRegister.StarB), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.StarA), new AluOperand(CpuRegister.StarA), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.StarA), new AluOperand(CpuRegister.StarB), AluOperation.Add), false },
		
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A, CpuRegister.StarA), new AluOperand(CpuRegister.StarA), new AluOperand(1), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A, CpuRegister.StarB), new AluOperand(CpuRegister.StarA), new AluOperand(1), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A, CpuRegister.StarA, CpuRegister.StarB), new AluOperand(CpuRegister.StarA), new AluOperand(1), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.A, CpuRegister.StarA), new AluOperand(CpuRegister.StarA), new AluOperand(CpuRegister.StarB), AluOperation.Add), false },
		
		// if both operands are cpu registers then they cannot be the same one
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.A), new AluOperand(CpuRegister.B), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.B), new AluOperand(CpuRegister.A), AluOperation.Add), true },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.A), new AluOperand(CpuRegister.A), AluOperation.Add), false },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.B), new AluOperand(CpuRegister.B), AluOperation.Add), false },
		
		
		// assign instruction
		// can assign registers to each other but not one star register to a different star register
		new object[] { new AssignInstruction(CpuRegister.A, CpuRegister.A), true },
		new object[] { new AssignInstruction(CpuRegister.A, CpuRegister.B), true },
		new object[] { new AssignInstruction(CpuRegister.A, CpuRegister.StarA), true },
		new object[] { new AssignInstruction(CpuRegister.A, CpuRegister.StarB), true },
		new object[] { new AssignInstruction(CpuRegister.StarB, CpuRegister.StarB), true },
		new object[] { new AssignInstruction(CpuRegister.StarA, CpuRegister.StarB), false },
		
		
		// jump instruction
		// right operand must be 0
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.B), true },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(1)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(-1)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(2)), CpuRegister.B), false },
		
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(CpuRegister.A)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(CpuRegister.B)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(CpuRegister.StarA)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand(CpuRegister.StarB)), CpuRegister.B), false },
		
		// Left operand must be a cpu register different from the target register
		new object[] { new JumpInstruction(new Condition(new AluOperand((short) 0), CompareOperation.Equals, new AluOperand(CpuRegister.A)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(1), CompareOperation.Equals, new AluOperand(CpuRegister.B)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.B), true },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.B), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.A), true },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.A), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.B), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.B), false },
		
		// neither register can be a star register
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.StarA), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.B), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.StarA), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.StarB), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.B), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.StarB), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.StarA), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.A), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.StarB), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.A), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.StarA), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.B), false },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.StarB), CompareOperation.Equals, new AluOperand((short) 0)), CpuRegister.B), false },
		
	};
		
	[Test]
	[TestCaseSource(nameof(ValidationTestCases))]
	public void TestValidation(IStatement statement, bool expectedResult) {
		Assert.That(m_Assembler.ValidateStatement(statement), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] ConvertInstructionTestCases() => new[] {
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand((short) 0), new AluOperand((short) 0), AluOperation.Add), 0, 0x8502u },
		new object[] { new DataWordInstruction(CpuRegister.B, 0), 0, 0x8502u },
		new object[] { new DataWordInstruction(CpuRegister.B, 1), 0, 0x8902u },
		new object[] { new DataWordInstruction(CpuRegister.B, 2), 0, 0x8A02u },
		new object[] { new DataWordInstruction(CpuRegister.B, -1), 0, 0x860Au },
		new object[] { new DataWordInstruction(CpuRegister.B, -2), 0, 0x8892u },
		new object[] { new AssignInstruction(CpuRegister.StarB, CpuRegister.B), 0, 0xD104u },
		new object[] { new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.B), new AluOperand(1), AluOperation.Add), 0, 0x9202u },
		new object[] { new DataWordInstruction(CpuRegister.A, "next"), 1, 0x0001u },
		new object[] { new JumpInstruction(true, CpuRegister.A), 0, 0xA00Fu },
		new object[] { new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.GreaterThan, new AluOperand((short) 0)), CpuRegister.B), 0, 0xA001u },
	};
	
	[Test]
	[TestCaseSource(nameof(ConvertInstructionTestCases))]
	public void TestConvertInstruction(IStatement statement, int symbolValue, uint expectedResult) {
		Assert.That(m_Assembler.ValidateStatement(statement), Is.EqualTo(true), "Statement was not validated");

		ushort result = m_Assembler.ConvertStatement(statement, _ => (short) symbolValue);
		
		Assert.That(result, Is.EqualTo((ushort) expectedResult), "Expected 0x{0:X4} received 0x{1:X4}", expectedResult, result);
	}
}
