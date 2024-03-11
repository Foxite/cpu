using Assembler.Assembly;
using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Tests.EndToEnd; 

public class Proc16bE2ETests {
	private ProcAssemblyParser m_Parser;
	private ProgramAssemblerFactory m_Factory;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
		m_Factory = new ProgramAssemblerFactory(new Proc16bInstructionConverter());
	}

	public static object[][] AssembleTestCases() {
		return new object[][] {
			new object[] {
				"""
				# increment A forever.
				# Tests data words, ALU to A, ALU to B, jumping unconditionally.

				.reg val,   %a
				.reg one,   %b
				.reg label, %c

					ldc val, $0        # 0b00 00 000000000000
					ldc one,     $1    # 0b00 01 000000000001
					ldc label,   loop  # 0b00 10 000000000011
					
				loop:
					add val, val, one   # 0b01 00 01 00 00000 000
					jmp label           # 0b100 00 00 10 111 0000
				""",
				new ushort[] {
					0x0000,
					0x1001,
					0x2003,
					0x4400,
					0x8170,
				},
			},/*
			new object[] {
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				""",
				new ushort[] {
					
				},
			},*/
			new object[] {
				"""
				# fill RAM cells with their addresses times two.
				
				.reg address, %a
				.reg value,   %b
				.reg one,     %c
				
					ldc address, $0 # 0b00 00 000000000000
					ldc value,   $0 # 0b00 01 000000000000
					ldc one,     $1 # 0b00 10 000000000001
				
				fill:                          # label value = 3
					ldc %d, $2 # $2            # 0b00 11 000000000010
					
					# a = a + 1 (address + 1)
					add address, address, one  # 0b01 00 10 00 00000 000
					
					# b = a * d (address * 2)
					mul value, address, %d     # 0b01 00 11 01 00010 000
					
					# *a = b
					stb address, value         # 0b1011 1 000 001 00000
					
					ldc %d, fill               # 0b00 11 000000000011 (label value = 3)
					jump %d                    # 0b100 00 00 11 111 0000
				""",
				new ushort[] {
					0x0000,
					0x1000,
					0x2001,
					0x3002,
					0x4800,
					0x4D10,
					0xB820,
					0x3003,
					0x81F0,
				},
			},
		};
	}

	[Test]
	[TestCaseSource(nameof(AssembleTestCases))]
	public void TestAssemble(string sourceCode, ushort[] expectedResult) {
		ProgramAst ast = m_Parser.Parse(sourceCode);

		List<ushort> assembledProgram;
		try {
			assembledProgram = m_Factory.GetAssembler(new AssemblerProgram(null, null, ast), null).Assemble().ToList();
		} catch (InvalidProcAssemblyProgramException ex) {
			Assert.Fail(
				"Test failed due to {0}:\n{1}",
				nameof(InvalidProcAssemblyProgramException),
				string.Join("", ex.Instructions.Select(instruction => $"index {instruction.Index} ({instruction.Instruction}): {instruction.Message}\n"))
			);
			return;
		}

		Assert.That(assembledProgram, Is.EquivalentTo(expectedResult));
	}
}
