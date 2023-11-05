using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Tests;

public class ProcAssemblyV2ParserTests {
	private ProcAssemblyV2Parser m_Parser;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyV2Parser();
	}

	public static object[][] ProgramTestCases() {
		return new object[][] {
		};
	}

	[Test]
	[TestCaseSource(nameof(ProgramTestCases))]
	public void ParseDataWord(string sourceCode, IAssemblyAst expectedResult) {
		ProgramAst ast = m_Parser.Parse(sourceCode);
		
		Assert.That(ast, Is.EqualTo(expectedResult), "Parse result does not match specification");
	}
}
