using Assembler.Parsing;

namespace Assembler.Tests;

[TestFixture(typeof(Assembler.Parsing.Csly.CslyAssemblyParser))]
[TestFixture(typeof(Assembler.Parsing.Antlr.AntlrAssemblyParser))]
public class ParserTests<T> where T : IAssemblyParser, new() {
	private IAssemblyParser m_Parser;

	[SetUp]
	public void Setup() {
		m_Parser = new T();
	}

	public static object[][] ProgramTestCases() {
		return new object[][] {
			new object[] {
				"A = 0x0005",
				new ProgramAst(
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 5))
				)
			},
			new object[] {
				"""
				A = 0x0005
				B = A
				
				A = 0x0000
				*A = B
				""",
				new ProgramAst(
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 5)),
					new ProgramStatementAst(null, new AssignInstruction(CpuRegister.B, CpuRegister.A)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0)),
					new ProgramStatementAst(null, new AssignInstruction(CpuRegister.StarA, CpuRegister.B))
				)
			},
			new object[] {
				"""
				# increment forever.
				A = 0
				B = 2
				A = A + 1
				A > 0 JMP B
				""",
				new ProgramAst(
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.B, 2)),
					new ProgramStatementAst(null, new AluInstruction(
						new AluWriteTarget(CpuRegister.A),
						new AluOperand(CpuRegister.A),
						new AluOperand(1),
						AluOperation.Add
					)),
					new ProgramStatementAst(null, new JumpInstruction(
						new Condition(new AluOperand(CpuRegister.A), CompareOperation.GreaterThan, new AluOperand((long) 0)),
						CpuRegister.B
					))
				),
			},
			new object[] {
				"""
				# fill RAM cells with their addresses.
				B = 0
				next:
				*B = B
				B = B + 1
				A = next # hey look a comment.
				true JMP A
				""",
				new ProgramAst(
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.B, 0)),
					//new LabelElement("next"),
					new ProgramStatementAst("next", new AssignInstruction(CpuRegister.StarB, CpuRegister.B)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.B), new AluOperand(1), AluOperation.Add)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, "next")),
					new ProgramStatementAst(null, new JumpInstruction(true, CpuRegister.A))
				),
			},
			new object[] {
				"""
				# fill RAM cells with their addresses.

				B = 0      #      ALU x=0 y=0 op=add write=B   ; 100 0 01 01 00000 010 ; 0x8502
				next:      # instruction index 1               ;  
				*B = B     # Ax=B ALU x=B y=0 op=add write=*Ax ; 110 1 00 01 00000 100 ; 0xD104
				B = B + 1  #      ALU x=B y=1 op=add write=B   ; 100 1 00 10 00000 010 ; 0x9202
				A = next   # data 0x0001
				true JMP A #      JMP x=B op=true to=A         ; 10100 0000000 1 111   ; 0xA00F

				""",
				new ProgramAst(
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.B, 0)),
					//new LabelElement("next"),
					new ProgramStatementAst("next", new AssignInstruction(CpuRegister.StarB, CpuRegister.B)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.B), new AluOperand(1), AluOperation.Add)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, "next")),
					new ProgramStatementAst(null, new JumpInstruction(true, CpuRegister.A))
				),
			},
			new object[] {
				"A = 0b0011001100110011",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0b0011001100110011)))
			},
			new object[] {
				"A = 0b00110011_00110011",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0b0011001100110011)))
			},
			new object[] {
				"A = 0b0011_0011_0011_0011",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0b0011001100110011)))
			},
			new object[] {
				"A = 0x3532",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0x3532)))
			},
			new object[] {
				"A = -1",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, -1)))
			},
			new object[] {
				"A = -0x0002",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, -2)))
			},
			new object[] {
				"A = -0x00_02",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, -2)))
			},
			new object[] {
				"A = -0b0011",
				new ProgramAst(new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, -3)))
			},
			new object[] {
				"label: A = 5",
				new ProgramAst(new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.A, 5)))
			},
			new object[] {
				"labe2l: A = 5",
				new ProgramAst(new ProgramStatementAst("labe2l", new DataWordInstruction(CpuRegister.A, 5)))
			},
			new object[] {
				"label2: A = 5",
				new ProgramAst(new ProgramStatementAst("label2", new DataWordInstruction(CpuRegister.A, 5)))
			},
			new object[] {
				"label : A = 5",
				new ProgramAst(new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.A, 5)))
			},
			new object[] {
				"B = *A AND B",
				new ProgramAst(new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.StarA), new AluOperand(CpuRegister.B), AluOperation.BitwiseAnd)))
			},
			new object[] {
				"A = NOT B",
				new ProgramAst(new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), null, AluOperation.BitwiseNot)))
			},
			new object[] {
				"""
				# starting program with
				# two consecutive comments
				
				A = 5
				B = 2
				*B = 5
				*A = 10

				A = 0x20
				A = 0x40_10
				B = 0b1010010
				B = 0b10_0101_011


				label:
				A = 10
				
				label:
				B = 10
				A = 10
				label:
				A = 20
				label: B = 40
				label: A = 40

				label: A = 80
				
				label1:
				B = 80
				label2: A = 160
				labe3l: B = 160

				A = A + 0
				A = B + 1
				A = B + -1
				A = -1 - -1
				A = 1 >> A
				B = *A AND B
				B = B < *B
				*B = B < *B
				A = NOT B				
				A = *A
				A = B
				*B = *B

				A < 0 JMP B
				*B > 5 JMP A
				B != 0 JMP *A
				true JMP *B
				false JMP A
				""",
				new ProgramAst(
					// A = 5
					// B = 2
					// *B = 5
					// *A = 10
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 5)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.B, 2)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.StarB, 5)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.StarA, 10)),
					
					// A = 0x20
					// A = 0x40_10
					// B = 0b1010010
					// B = 0b10_0101_011
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0x20)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 0x4010)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.B, 0b1010010)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.B, 0b100101011)),
					
					// label:
					// A = 10
					// label:
					// B = 10
					// A = 10
					// label:
					// A = 20
					// label: B = 40
					// label: A = 40
					// label: A = 80
					// label1:
					// B = 80
					// label2: A = 160
					// labe3l: B = 160
					new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.A, 10)),
					new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.B, 10)),
					new ProgramStatementAst(null, new DataWordInstruction(CpuRegister.A, 10)),
					new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.A, 20)),
					new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.B, 40)),
					new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.A, 40)),
					new ProgramStatementAst("label", new DataWordInstruction(CpuRegister.A, 80)),
					new ProgramStatementAst("label1", new DataWordInstruction(CpuRegister.B, 80)),
					new ProgramStatementAst("label2", new DataWordInstruction(CpuRegister.A, 160)),
					new ProgramStatementAst("labe3l", new DataWordInstruction(CpuRegister.B, 160)),
					
					// A = A + 0
					// A = B + 1
					// A = B + -1
					// A = -1 - -1
					// A = 1 >> A
					// B = *A AND B
					// B = B < *B
					// *B = B < *B
					// A = NOT B
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.A), new AluOperand((long) 0), AluOperation.Add)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), new AluOperand(1), AluOperation.Add)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), new AluOperand(-1), AluOperation.Add)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(-1), new AluOperand(-1), AluOperation.Subtract)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(1), new AluOperand(CpuRegister.A), AluOperation.ShiftRight)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.StarA), new AluOperand(CpuRegister.B), AluOperation.BitwiseAnd)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.B), new AluOperand(CpuRegister.B), new AluOperand(CpuRegister.StarB), AluOperation.LessThan)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.StarB), new AluOperand(CpuRegister.B), new AluOperand(CpuRegister.StarB), AluOperation.LessThan)),
					new ProgramStatementAst(null, new AluInstruction(new AluWriteTarget(CpuRegister.A), new AluOperand(CpuRegister.B), null, AluOperation.BitwiseNot)),
					
					// A = *A
					// A = B
					// *B = *B
					new ProgramStatementAst(null, new AssignInstruction(CpuRegister.A, CpuRegister.StarA)),
					new ProgramStatementAst(null, new AssignInstruction(CpuRegister.A, CpuRegister.B)),
					new ProgramStatementAst(null, new AssignInstruction(CpuRegister.StarB, CpuRegister.StarB)),

					// A < 0 JMP B
					// *B > 5 JMP A
					// B != 0 JMP *A
					// true JMP *B
					// false JMP A
					new ProgramStatementAst(null, new JumpInstruction(new Condition(new AluOperand(CpuRegister.A), CompareOperation.LessThan, new AluOperand((long) 0)), CpuRegister.B)),
					new ProgramStatementAst(null, new JumpInstruction(new Condition(new AluOperand(CpuRegister.StarB), CompareOperation.GreaterThan, new AluOperand(5)), CpuRegister.A)),
					new ProgramStatementAst(null, new JumpInstruction(new Condition(new AluOperand(CpuRegister.B), CompareOperation.NotEquals, new AluOperand((long) 0)), CpuRegister.StarA)),
					new ProgramStatementAst(null, new JumpInstruction(true, CpuRegister.StarB)),
					new ProgramStatementAst(null, new JumpInstruction(false, CpuRegister.A))
				)
			},
		};
	}

	[Test]
	[TestCaseSource(nameof(ProgramTestCases))]
	public void ParseDataWord(string sourceCode, IAssemblyAst expectedResult) {
		ProgramAst ast = m_Parser.Parse(sourceCode);
		
		Assert.That(ast, Is.EqualTo(expectedResult), "Parse result does not match specification");
	}
}
