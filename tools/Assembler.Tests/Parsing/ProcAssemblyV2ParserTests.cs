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
			new object[] {
				"""
				# this is a comment.
				distressSignal:
					received
					rescue %operation, willbe
					.dispatched "to your \"location\" in", $0x999F # hours
					
				""",
				new ProgramAst(
					new ProgramStatementAst("distressSignal", new InstructionAst("received")),
					new ProgramStatementAst(null, new InstructionAst("rescue",
						InstructionArgumentAst.Register("operation"),
						InstructionArgumentAst.Symbol("willbe")
					)),
					new ProgramStatementAst(null, new InstructionAst(".dispatched",
						InstructionArgumentAst.String("""to your \"location\" in"""),
						InstructionArgumentAst.Constant(0x999F)
					))
				)
			}
		};
	}

	[Test]
	[TestCaseSource(nameof(ProgramTestCases))]
	public void ParseDataWord(string sourceCode, IAssemblyAst expectedResult) {
		ProgramAst ast = m_Parser.Parse(sourceCode);
		
		Assert.That(ast, Is.EqualTo(expectedResult), "Parse result does not match specification");
	}
}
