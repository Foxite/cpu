using Assembler.Assembly;
using Assembler.Ast;
using IAT = Assembler.Ast.InstructionArgumentType;
using IAA = Assembler.Ast.InstructionArgumentAst;

namespace Assembler.Tests; 

public class Proc16bAssemblerTests {
	private IInstructionConverter m_Converter;

	[SetUp]
	public void Setup() {
		m_Converter = new Proc16bInstructionConverter();
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
		Assert.That(m_Converter.ValidateInstruction(instruction), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] ConvertInstructionTestCases() => new[] {
			// ldc
			new object[] { new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(0)),     0b00_00_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("b"), IAA.Constant(0)),     0b00_01_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("c"), IAA.Constant(0)),     0b00_10_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("d"), IAA.Constant(0)),     0b00_11_000000000000u },
			new object[] { new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(0xFFF)), 0b00_00_111111111111u },
			new object[] { new InstructionAst("ldc", IAA.Register("b"), IAA.Constant(0xFFF)), 0b00_01_111111111111u },
			new object[] { new InstructionAst("ldc", IAA.Register("c"), IAA.Constant(0xFFF)), 0b00_10_111111111111u },
			new object[] { new InstructionAst("ldc", IAA.Register("d"), IAA.Constant(0xFFF)), 0b00_11_111111111111u },
			
			// alu
			new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0b01_01_10_00_00000_000u },
			new object[] { new InstructionAst("sub", IAA.Register("b"), IAA.Register("a"), IAA.Register("c")), 0b01_00_10_01_00001_000u },
			new object[] { new InstructionAst("mul", IAA.Register("c"), IAA.Register("c"), IAA.Register("c")), 0b01_10_10_10_00010_000u },
			new object[] { new InstructionAst("div", IAA.Register("d"), IAA.Register("d"), IAA.Register("d")), 0b01_11_11_11_00011_000u },
			new object[] { new InstructionAst("shr", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0b01_01_10_00_00100_000u },
			new object[] { new InstructionAst("shl", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0b01_01_10_00_00101_000u },
			
			// jump
			new object[] { new InstructionAst("jmp",  IAA.Register("a")), 0b100_00_00_00_111_0000u },
			new object[] { new InstructionAst("jump", IAA.Register("a")), 0b100_00_00_00_111_0000u },
			new object[] { new InstructionAst("jmp",  IAA.Register("b")), 0b100_00_00_01_111_0000u },
			new object[] { new InstructionAst("jump", IAA.Register("c")), 0b100_00_00_10_111_0000u },
			
			new object[] { new InstructionAst("jgt", IAA.Register("d"), IAA.Register("a"), IAA.Register("b")), 0b100_00_01_11_001_0000u },
			new object[] { new InstructionAst("jgt", IAA.Register("a"), IAA.Register("b"), IAA.Register("c")), 0b100_01_10_00_001_0000u },
			new object[] { new InstructionAst("jgt", IAA.Register("a"), IAA.Register("c"), IAA.Register("d")), 0b100_10_11_00_001_0000u },
			new object[] { new InstructionAst("jgt", IAA.Register("a"), IAA.Register("d"), IAA.Register("a")), 0b100_11_00_00_001_0000u },
			
			new object[] { new InstructionAst("jeq", IAA.Register("a"), IAA.Register("d"), IAA.Register("a")), 0b100_11_00_00_010_0000u },
			new object[] { new InstructionAst("jge", IAA.Register("a"), IAA.Register("d"), IAA.Register("a")), 0b100_11_00_00_011_0000u },
			new object[] { new InstructionAst("jlt", IAA.Register("a"), IAA.Register("d"), IAA.Register("a")), 0b100_11_00_00_100_0000u },
			new object[] { new InstructionAst("jne", IAA.Register("a"), IAA.Register("d"), IAA.Register("a")), 0b100_11_00_00_101_0000u },
			new object[] { new InstructionAst("jle", IAA.Register("a"), IAA.Register("d"), IAA.Register("a")), 0b100_11_00_00_110_0000u },
			
			// mov
			new object[] { new InstructionAst("mov", IAA.Register("a"), IAA.Register("b")), 0b1010_0000_001_00000u },
			new object[] { new InstructionAst("mov", IAA.Register("b"), IAA.Register("c")), 0b1010_0001_010_00000u },
			new object[] { new InstructionAst("mov", IAA.Register("c"), IAA.Register("d")), 0b1010_0010_011_00000u },
			new object[] { new InstructionAst("mov", IAA.Register("d"), IAA.Register("a")), 0b1010_0011_000_00000u },
			new object[] { new InstructionAst("mov", IAA.Register("r"), IAA.Register("b")), 0b1010_1000_001_00000u },
			new object[] { new InstructionAst("mov", IAA.Register("o"), IAA.Register("c")), 0b1010_1001_010_00000u },
			
			// bus
			new object[] { new InstructionAst("ldb", IAA.Register("a"), IAA.Register("b")), 0b1011_0_000_001_00000u },
			new object[] { new InstructionAst("ldb", IAA.Register("b"), IAA.Register("c")), 0b1011_0_001_010_00000u },
			new object[] { new InstructionAst("ldb", IAA.Register("c"), IAA.Register("d")), 0b1011_0_010_011_00000u },
			new object[] { new InstructionAst("ldb", IAA.Register("d"), IAA.Register("a")), 0b1011_0_011_000_00000u },
			
			new object[] { new InstructionAst("stb", IAA.Register("a"), IAA.Register("b")), 0b1011_1_000_001_00000u },
			new object[] { new InstructionAst("stb", IAA.Register("b"), IAA.Register("c")), 0b1011_1_001_010_00000u },
			new object[] { new InstructionAst("stb", IAA.Register("c"), IAA.Register("d")), 0b1011_1_010_011_00000u },
			new object[] { new InstructionAst("stb", IAA.Register("d"), IAA.Register("a")), 0b1011_1_011_000_00000u },
			
			// misc
			new object[] { new InstructionAst("nop" ), 0b1110_0000_0000_0000u },
			new object[] { new InstructionAst("noop"), 0b1110_0000_0000_0000u },
			new object[] { new InstructionAst("brk" ), 0b1110_0000_0000_0001u },
	};
	
	[Test]
	[TestCaseSource(nameof(ConvertInstructionTestCases))]
	public void TestConvertInstruction(InstructionAst statement, uint expectedResult) {
		Assert.That(m_Converter.ValidateInstruction(statement), Is.EqualTo(InstructionSupport.Supported), "Statement was not validated");

		ushort result = m_Converter.ConvertInstruction(statement);
		
		Assert.That(result, Is.EqualTo((ushort) expectedResult), "Expected 0x{0:X4} received 0x{1:X4}", expectedResult, result);
	}
}
