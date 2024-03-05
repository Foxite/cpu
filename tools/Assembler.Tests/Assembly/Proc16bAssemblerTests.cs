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

	public static object[][] ValidationTestCases() => throw new NotImplementedException();
		
	[Test]
	[TestCaseSource(nameof(ValidationTestCases))]
	public void TestValidation(InstructionAst instruction, bool expectedResult) {
		Assert.That(m_Assembler.ValidateInstruction(instruction, _ => throw new Exception("Should not be called")), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] ConvertInstructionTestCases() => throw new NotImplementedException();
	
	[Test]
	[TestCaseSource(nameof(ConvertInstructionTestCases))]
	public void TestConvertInstruction(InstructionAst statement, int symbolValue, uint expectedResult) {
		Assert.That(m_Assembler.ValidateInstruction(statement, _ => (ushort) symbolValue), Is.EqualTo(true), "Statement was not validated");

		ushort result = m_Assembler.ConvertInstruction(statement, _ => (ushort) symbolValue);
		
		Assert.That(result, Is.EqualTo((ushort) expectedResult), "Expected 0x{0:X4} received 0x{1:X4}", expectedResult, result);
	}
}
