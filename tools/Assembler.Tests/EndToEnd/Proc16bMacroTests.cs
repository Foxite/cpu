using Assembler.Assembly;
using Assembler.Parsing.Antlr;
using Assembler.Ast;

namespace Assembler.Tests.EndToEnd; 

public class Proc16bMacroTests {
	private ProcAssemblyParser m_Parser;
	private ProgramAssemblerFactory m_Factory;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
		var macroProvider = new DictionaryMacroProvider(m_Parser);
		
		macroProvider.AddMacro(
			"testmacro",
			"""
			ldc %a, $15
			add %b, %a, %c
			"""
		);
		
		macroProvider.AddMacro(
			"parammacro",
			"""
			ldc %a, macro0
			add %b, %a, macro1
			"""
		);
		
		m_Factory = new ProgramAssemblerFactory(new Proc16bInstructionConverter(), macroProvider);
	}

	public static object[][] AssembleTestCases() {
		return new object[][] {
			new object[] {
				"""
				ldc %c, $2
				@testmacro
				stb %d, %a
				""",
				new ushort[] {
					0b00_10_000000000010,
					0b00_00_000000001111,
					0b01_00_10_01_00000_000,
					0b1011_1_011_000_00000,
				},
			},
			new object[] {
				"""
				ldc %c, $2
				@parammacro $5, %d
				stb %d, %a
				""",
				new ushort[] {
					0b00_10_000000000010,
					0b00_00_000000000101,
					0b01_00_11_01_00000_000,
					0b1011_1_011_000_00000,
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
			assembledProgram = m_Factory.GetAssembler(new AssemblerProgram("Main", null, ast)).Assemble().ToList();
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
