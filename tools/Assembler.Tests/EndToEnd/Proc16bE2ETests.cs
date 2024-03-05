using Assembler.Assembly;
using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Tests.EndToEnd; 

public class Proc16bE2ETests {
	private ProgramAssembler m_Assembler;
	private ProcAssemblyParser m_Parser;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
		m_Assembler = ProgramAssemblers.Proc16b;
	}

	public static object[][] AssembleTestCases() {
		return new object[][] {
			new object[] {
				"""
				# increment A forever.
				# Tests data words, ALU to A, ALU to B, jumping unconditionally.

					ldc %a, $0      # address 0b00 00 000000000000
					ldc %b, $1      # one     0b00 01 000000000001
					ldc %c, loop    # label   0b00 10 000000000011
					
				loop:
					add %a, %a, %b  #         0b01 00 01 00 00000 000
					jmp %c          #         0b100 00 00 10 111 0000
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
				
					ldc %a, $0 # address       # 0b00 00 000000000000
					ldc %b, $0 # value         # 0b00 01 000000000000
					ldc %c, $1 # $1            # 0b00 10 000000000001
				
				fill:                          # label value = 3
					ldc %d, $2 # $2            # 0b00 11 000000000010
					
					# a = a + 1 (address + 1)
					add %a, %a, %c             # 0b01 00 10 00 00000 000
					
					# b = a * d (address * 2)
					mul %b, %a, %d             # 0b01 00 11 01 00010 000
					
					# *a = b
					stb %a, %b                 # 0b1011 1 000 001 00000
					
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

		List<ushort> assembledProgram = m_Assembler.Assemble(ast).ToList();
		
		Assert.That(assembledProgram, Is.EquivalentTo(expectedResult));
	}
}
