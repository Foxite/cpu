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
					new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5))
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
					new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(5)),
					new AssignInstruction(new CpuRegisterAst(CpuRegister.B), new CpuRegisterAst(CpuRegister.A)),
					new DataWordInstruction(new CpuRegisterAst(CpuRegister.A), new ConstantAst(0)),
					new AssignInstruction(new CpuRegisterAst(CpuRegister.StarA), new CpuRegisterAst(CpuRegister.B))
				)
			}
		};
	}

	[Test]
	[TestCaseSource(nameof(ProgramTestCases))]
	public void ParseDataWord(string sourceCode, IAssemblyAst expectedResult) {
		
		Assume.That(m_BuildResult.IsOk, Is.True, "Parser was not successfully built");

		var parseResult = m_Parser.Parse(sourceCode);

		Assert.That(parseResult.IsOk, Is.True, () => {
			var builder = new StringBuilder();
			foreach (ParseError error in parseResult.Errors) {
				builder.AppendLine($"{error.ErrorType} at line {error.Line}:{error.Column}: {error.ErrorMessage}");
			}
			return builder.ToString();
		});
		
		Assert.That(parseResult.Result, Is.EqualTo(expectedResult).Using(new AssemblyAstComparer()), "Parse result does not match specification");
	}
}
