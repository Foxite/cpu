using Assembler.Assembly;
using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Tests.EndToEnd; 

public class Proc16bMacroTests {
	private ProcAssemblyParser m_Parser;
	private ProgramAssemblerFactory m_Factory;
	private MacroProcessor m_MacroProcessor;
	private DirectoryInfo m_TempDirectory;

	[OneTimeSetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
		m_Factory = new ProgramAssemblerFactory(new Proc16bInstructionConverter());

		m_TempDirectory = Directory.CreateTempSubdirectory("Proc16bMacroTests");

		m_MacroProcessor = new MacroProcessor(m_Parser, m_Factory, new [] { m_TempDirectory.FullName });
	}

	[OneTimeTearDown]
	public void Teardown() {
		m_TempDirectory.Delete(true);
	}

	public static object[][] AssembleTestCases() {
		return new object[][] {
			new object[] {
				"""
				
				""",
				new ushort[] {
					
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
			assembledProgram = m_Factory.GetAssembler(new AssemblerProgram("Main", null, ast), m_MacroProcessor).Assemble().ToList();
		} catch (InvalidProcAssemblyProgramException ex) {
			Assert.Fail(
				"Test failed due to {0} in program {1}:\n{2}",
				nameof(InvalidProcAssemblyProgramException),
				ex.Program.Name,
				string.Join("", ex.Instructions.Select(instruction => $"index {instruction.Index} ({instruction.Instruction}): {instruction.Message}\n"))
			);
			return;
		}

		Assert.That(assembledProgram, Is.EquivalentTo(expectedResult));
	}
}
