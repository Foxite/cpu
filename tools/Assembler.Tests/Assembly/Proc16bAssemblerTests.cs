using Assembler.Assembly;
using Assembler.Parsing.ProcAssemblyV2;
using IAT = Assembler.Parsing.ProcAssemblyV2.InstructionArgumentType;
using IAA = Assembler.Parsing.ProcAssemblyV2.InstructionArgumentAst;

namespace Assembler.Tests; 

public class Proc16bAssemblerTests {
	private ProgramAssembler m_Assembler;

	[SetUp]
	public void Setup() {
		m_Assembler = ProgramAssemblers.Proc16b;
	}

	public static object[][] ValidationTestCases() => new [] {
			new object[] { new InstructionAst("ldc", IAA.Register("a"),     IAA.Register("a")),    InstructionSupport.ParameterType },
			new object[] { new InstructionAst("ldc", IAA.Register("a"),     IAA.Constant(0)),      InstructionSupport.Supported },
			new object[] { new InstructionAst("ldc", IAA.Register("b"),     IAA.Constant(0)),      InstructionSupport.Supported },
			new object[] { new InstructionAst("ldc", IAA.Register("c"),     IAA.Constant(0xFFF)),  InstructionSupport.Supported },
			new object[] { new InstructionAst("ldc", IAA.Register("d"),     IAA.Constant(0xFFF)),  InstructionSupport.Supported },
			new object[] { new InstructionAst("ldc", IAA.Register("a"),     IAA.Constant(0x1000)), InstructionSupport.OtherError },
		//}
	};
		
	[Test]
	[TestCaseSource(nameof(ValidationTestCases))]
	public void TestValidation(InstructionAst instruction, InstructionSupport expectedResult) {
		Assert.That(m_Assembler.ValidateInstruction(instruction, _ => throw new Exception("Should not be called")), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] ConvertInstructionTestCases() => new[] {
			// ldc
			new object[] { new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(0)),     0u, 0b00_00_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("b"), IAA.Constant(0)),     0u, 0b00_01_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("c"), IAA.Constant(0)),     0u, 0b00_10_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("d"), IAA.Constant(0)),     0u, 0b00_11_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(0xFFF)), 0u, 0b00_00_111111111111u },
			new object[] { new InstructionAst("ldc", IAA.Register("b"), IAA.Constant(0xFFF)), 0u, 0b00_01_111111111111u },
			new object[] { new InstructionAst("ldc", IAA.Register("c"), IAA.Constant(0xFFF)), 0u, 0b00_10_111111111111u },
			new object[] { new InstructionAst("ldc", IAA.Register("d"), IAA.Constant(0xFFF)), 0u, 0b00_11_111111111111u },
			
			// alu
			new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0u, 0b01_01_10_00_00000_000u },
			new object[] { new InstructionAst("sub", IAA.Register("b"), IAA.Register("a"), IAA.Register("c")), 0u, 0b01_00_10_01_00001_000u },
			new object[] { new InstructionAst("mul", IAA.Register("c"), IAA.Register("c"), IAA.Register("c")), 0u, 0b01_10_10_10_00010_000u },
			new object[] { new InstructionAst("div", IAA.Register("d"), IAA.Register("d"), IAA.Register("d")), 0u, 0b01_11_11_11_00011_000u },
			new object[] { new InstructionAst("shr", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0u, 0b01_01_10_00_00100_000u },
			new object[] { new InstructionAst("shl", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0u, 0b01_01_10_00_00101_000u },
			
			// jump
			
			// mov
			
			// bus
			
			// misc
			
	};
	
	[Test]
	[TestCaseSource(nameof(ConvertInstructionTestCases))]
	public void TestConvertInstruction(InstructionAst statement, uint symbolValue, uint expectedResult) {
		Assert.That(m_Assembler.ValidateInstruction(statement, _ => (ushort) symbolValue), Is.EqualTo(InstructionSupport.Supported), "Statement was not validated");

		ushort result = m_Assembler.ConvertInstruction(statement, _ => (ushort) symbolValue);
		
		Assert.That(result, Is.EqualTo((ushort) expectedResult), "Expected 0x{0:X4} received 0x{1:X4}", expectedResult, result);
	}
}
