using Assembler.Assembly;
using Assembler.Parsing.ProcAssemblyV2;
using IAT = Assembler.Parsing.ProcAssemblyV2.InstructionArgumentType;
using IAA = Assembler.Parsing.ProcAssemblyV2.InstructionArgumentAst;

namespace Assembler.Tests; 

public class Proc16aAssemblerTests {
	private ProgramAssembler m_Assembler;

	[SetUp]
	public void Setup() {
		m_Assembler = ProgramAssemblers.Proc16a;
	}
	
	public static object[][] ValidationTestCases() => new[] {
		// data word instruction
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(0)), InstructionSupport.Supported }, // can assign any positive value to A register as well as -1 or -2 (using the ALU)
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(-1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(-2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(-3)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(3)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("a"), IAA.Constant(1234)), InstructionSupport.Supported },
		
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(0)), InstructionSupport.Supported }, // can assign values between -2 and 2 to any other register by using the ALU with only constants
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(-1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(-2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(-3)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("ldi", IAA.Register("b"), IAA.Constant(3)), InstructionSupport.OtherError },
		
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(0)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(-1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(-2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(-3)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("a"), IAA.Constant(3)), InstructionSupport.OtherError },
		
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(0)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(-1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(-2)), InstructionSupport.Supported },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(-3)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("ldi", IAA.StarRegister("b"), IAA.Constant(3)), InstructionSupport.OtherError },
		
		
		// alu instructions
		
		// cannot operate on constants other than 0 or 1
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("a"), IAA.Constant( 0)), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("a"), IAA.Constant( 1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("b"), IAA.Constant( 1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("b"), IAA.Constant( 2)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("b"), IAA.Constant( 3)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Register("b"), IAA.Constant(-1)), InstructionSupport.OtherError },
		
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 0), IAA.Register("a")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 1), IAA.Register("a")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 1), IAA.Register("b")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 2), IAA.Register("b")), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 3), IAA.Register("b")), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant(-1), IAA.Register("b")), InstructionSupport.OtherError },
		
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 0), IAA.Constant( 0)), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 1), IAA.Constant( 1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 1), IAA.Constant( 1)), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 2), IAA.Constant( 2)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant( 3), IAA.Constant( 3)), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("a"), IAA.Constant(-1), IAA.Constant(-1)), InstructionSupport.OtherError },
		
		// can operate on ram as long as all Star registers are the same one
		new object[] { new InstructionAst("add", IAA.Register("a"),     IAA.StarRegister("a"), IAA.Constant(1)      ), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.StarRegister("a"), IAA.Register("a"),     IAA.Constant(1)      ), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.StarRegister("b"), IAA.Register("a"),     IAA.Constant(1)      ), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.StarRegister("b"), IAA.StarRegister("b"), IAA.Constant(1)      ), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.StarRegister("a"), IAA.StarRegister("a"), IAA.Constant(1)      ), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.StarRegister("b"), IAA.StarRegister("a"), IAA.Constant(1)      ), InstructionSupport.OtherError },
		
		new object[] { new InstructionAst("add", IAA.Register("b"),	    IAA.StarRegister("a"), IAA.Register("a")    ), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"),	    IAA.Register("a"),     IAA.StarRegister("a")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"),	    IAA.Register("a"),     IAA.StarRegister("b")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"),	    IAA.StarRegister("b"), IAA.StarRegister("b")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"),	    IAA.StarRegister("a"), IAA.StarRegister("a")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"),	    IAA.StarRegister("a"), IAA.StarRegister("b")), InstructionSupport.OtherError },
		
		// new object[] { new InstructionAst("add", IAA.Register("a"), IAA.StarRegister("a")), new AluOperand(IAA.StarRegister("a")), new AluOperand(1), AluOperation.Add), InstructionSupport.Supported },
		// new object[] { new InstructionAst("add", IAA.Register("a"), IAA.StarRegister("b")), new AluOperand(IAA.StarRegister("a")), new AluOperand(1), AluOperation.Add), InstructionSupport.Todo },
		// new object[] { new InstructionAst("add", IAA.Register("a"), IAA.StarRegister("a"), IAA.StarRegister("b")), new AluOperand(IAA.StarRegister("a")), new AluOperand(1), AluOperation.Add), InstructionSupport.Todo },
		// new object[] { new InstructionAst("add", IAA.Register("a"), IAA.StarRegister("a")), new AluOperand(IAA.StarRegister("a")), new AluOperand(IAA.StarRegister("b")), AluOperation.Add), InstructionSupport.Todo },
		
		// if both operands are cpu registers then they cannot be the same one
		new object[] { new InstructionAst("add", IAA.Register("b"), IAA.Register("a"), IAA.Register("b")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"), IAA.Register("b"), IAA.Register("a")), InstructionSupport.Supported },
		new object[] { new InstructionAst("add", IAA.Register("b"), IAA.Register("a"), IAA.Register("a")), InstructionSupport.OtherError },
		new object[] { new InstructionAst("add", IAA.Register("b"), IAA.Register("b"), IAA.Register("b")), InstructionSupport.OtherError },
		
		
		// assign instruction
		// can assign registers to each other but not a star register to a star register
		new object[] { new InstructionAst("mov", IAA.Register("a"),     IAA.Register("a")),     InstructionSupport.Supported },
		new object[] { new InstructionAst("mov", IAA.Register("a"),     IAA.Register("b")),     InstructionSupport.Supported },
		new object[] { new InstructionAst("mov", IAA.Register("a"),     IAA.StarRegister("a")), InstructionSupport.Supported },
		new object[] { new InstructionAst("mov", IAA.Register("a"),     IAA.StarRegister("b")), InstructionSupport.Supported },
		new object[] { new InstructionAst("mov", IAA.StarRegister("b"), IAA.StarRegister("b")), InstructionSupport.ParameterType },
		new object[] { new InstructionAst("mov", IAA.StarRegister("a"), IAA.StarRegister("b")), InstructionSupport.ParameterType },
		
		
		// jump instruction
		// right operand must be 0
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Constant(0)),       InstructionSupport.Supported },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Constant(1)),       InstructionSupport.OtherError },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Constant(-1)),      InstructionSupport.OtherError },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Constant(2)),       InstructionSupport.OtherError },
		
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Register("a")),     InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Register("b")),     InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.StarRegister("a")), InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.StarRegister("b")), InstructionSupport.ParameterType },
		
		// Left operand must be a cpu register different from the target register
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Constant(0),       IAA.Register("a")),     InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Constant(1),       IAA.Register("b")),     InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("a"),     IAA.Constant(0)),       InstructionSupport.Supported },
		new object[] { new InstructionAst("jeq", IAA.Register("a"),     IAA.Register("b"),     IAA.Constant(0)),       InstructionSupport.Supported },
		new object[] { new InstructionAst("jeq", IAA.Register("a"),     IAA.Register("a"),     IAA.Constant(0)),       InstructionSupport.OtherError },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.Register("b"),     IAA.Constant(0)),       InstructionSupport.OtherError },
		
		// neither register can be a star register
		new object[] { new InstructionAst("jeq", IAA.StarRegister("a"), IAA.Register("a"),     IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.StarRegister("a"), IAA.Register("b"),     IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.StarRegister("b"), IAA.Register("a"),     IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.StarRegister("b"), IAA.Register("b"),     IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("a"),     IAA.StarRegister("a"), IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("a"),     IAA.StarRegister("b"), IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.StarRegister("a"), IAA.Constant(0)),       InstructionSupport.ParameterType },
		new object[] { new InstructionAst("jeq", IAA.Register("b"),     IAA.StarRegister("b"), IAA.Constant(0)),       InstructionSupport.ParameterType },
		
	};
		
	[Test]
	[TestCaseSource(nameof(ValidationTestCases))]
	public void TestValidation(InstructionAst instruction, InstructionSupport expectedResult) {
		Assert.That(m_Assembler.ValidateInstruction(instruction), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] ConvertInstructionTestCases() => new[] {
		new object[] { new InstructionAst("add", IAA.Register("b"),     IAA.Constant(0),     IAA.Constant(0)), 0x8502u },
		new object[] { new InstructionAst("ldi", IAA.Register("b"),     IAA.Constant(0)),                      0x8502u },
		new object[] { new InstructionAst("ldi", IAA.Register("b"),     IAA.Constant(1)),                      0x8902u },
		new object[] { new InstructionAst("ldi", IAA.Register("b"),     IAA.Constant(2)),                      0x8A02u },
		new object[] { new InstructionAst("ldi", IAA.Register("b"),     IAA.Constant(-1)),                     0x860Au },
		new object[] { new InstructionAst("ldi", IAA.Register("b"),     IAA.Constant(-2)),                     0x8892u },
		new object[] { new InstructionAst("mov", IAA.StarRegister("b"), IAA.Register("b")),                    0xD104u },
		new object[] { new InstructionAst("add", IAA.Register("b"),     IAA.Register("b"),   IAA.Constant(1)), 0x9202u },
		new object[] { new InstructionAst("add", IAA.Register("a"),     IAA.Register("a"),   IAA.Constant(1)), 0x8201u },
		//new object[] { new InstructionAst("ldi", IAA.Register("a"),     IAA.Symbol("next")),                 0x0001u },
		new object[] { new InstructionAst("ldi", IAA.Register("a"),     IAA.Constant(1)),                      0x0001u },
		new object[] { new InstructionAst("jmp", IAA.Register("a")),                                           0xA00Fu },
		new object[] { new InstructionAst("jgt", IAA.Register("b"),     IAA.Register("a"),   IAA.Constant(0)), 0xA001u },
	};
	
	[Test]
	[TestCaseSource(nameof(ConvertInstructionTestCases))]
	public void TestConvertInstruction(InstructionAst statement, uint expectedResult) {
		Assert.That(m_Assembler.ValidateInstruction(statement), Is.EqualTo(InstructionSupport.Supported), "Statement was not validated");

		ushort result = m_Assembler.ConvertInstruction(statement);
		
		Assert.That(result, Is.EqualTo((ushort) expectedResult), "Expected 0x{0:X4} received 0x{1:X4}", expectedResult, result);
	}
}
