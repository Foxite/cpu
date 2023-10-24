using System.Text;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;

namespace Assembler.Tests;

public class ParserTests {
	private BuildResult<Parser<AssemblyToken, IAssemblyAst>> m_BuildResult;
	private Parser<AssemblyToken, IAssemblyAst> m_Parser;

	[SetUp]
	public void Setup() {
		var parserDefinition = new AssemblyParser();
		var parserBuilder = new ParserBuilder<AssemblyToken, IAssemblyAst>();

		m_BuildResult = parserBuilder.BuildParser(parserDefinition,
			ParserType.EBNF_LL_RECURSIVE_DESCENT,
			"Program");
		m_Parser = m_BuildResult.Result;
	}

	[Test]
	public void A0_TestSetupParser() {
		if (m_BuildResult.IsOk) {
			Assert.Pass();
		} else {
			Assert.Multiple(() => {
				foreach (InitializationError error in m_BuildResult.Errors) {
					Assert.Fail($"{error.Level} {error.Code}: {error.Message}");
				}
			});
		}
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
						new Condition(new AluOperand(CpuRegister.A), CompareOperation.GreaterThan, new AluOperand((short) 0)),
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
		};
	}

	[Test]
	[TestCaseSource(nameof(ProgramTestCases))]
	public void ParseDataWord(string sourceCode, IAssemblyAst expectedResult) {
		Assume.That(m_BuildResult.IsOk, Is.True, "Parser was not successfully built");

		var parseResult = m_Parser.Parse(sourceCode);

		Assert.That(parseResult.IsOk, Is.True, () => {
			var builder = new StringBuilder();
			builder.AppendLine("Parsing failed:");
			foreach (ParseError error in parseResult.Errors) {
				builder.AppendLine($"{error.ErrorType} at line {error.Line}:{error.Column}: {error.ErrorMessage}");
			}
			return builder.ToString();
		});
		
		Assert.That(parseResult.Result, Is.EqualTo(expectedResult), "Parse result does not match specification");
	}
}
