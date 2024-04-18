using Assembler.Assembly;
using Assembler.Assembly.V2;
using Assembler.Ast;
using Assembler.Parsing.Antlr;
using IAA = Assembler.Ast.InstructionArgumentAst;

namespace Assembler.Tests.Assembly;

public class ConstantExpressionTests {
	private static object[][] TestCases() => new[] {
		new object[] {
			"""
			ldc %a, [5 + 2]
			""",
			new AssemblyInstruction[] {
				new ExecutableInstruction(null, 0, new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(7))),
			},
		},
		new object[] {
			"""
			.define heyoo, $13
			ldc %a, [5 + heyoo]
			""",
			new AssemblyInstruction[] {
				new DefineSymbolCommandInstruction(null, 0, "heyoo", IAA.Constant(13)),
				new ExecutableInstruction(null, 0, new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(18))),
			},
		},
		new object[] {
			"""
			ldc %a, [5 * 2 + 1]
			""",
			new AssemblyInstruction[] {
				new ExecutableInstruction(null, 0, new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(11))),
			},
		},
		new object[] {
			"""
			ldc %a, [(5 * 2) + 1]
			""",
			new AssemblyInstruction[] {
				new ExecutableInstruction(null, 0, new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(11))),
			},
		},
		new object[] {
			"""
			ldc %a, [5 * (2 + 1)]
			""",
			new AssemblyInstruction[] {
				new ExecutableInstruction(null, 0, new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(15))),
			},
		},
		new object[] {
			"""
			ldc %a, [~5 * (2 + 1)]
			""",
			new AssemblyInstruction[] {
				new ExecutableInstruction(null, 0, new InstructionAst("ldc", IAA.Register("a"), IAA.Constant(~5 * (2 + 1)))),
			},
		},
	};
	
	[Test]
	[TestCaseSource(nameof(TestCases))]
	public void TestConvertInstruction(string source, AssemblyInstruction[] expectedResult) {
		var parser = new ProcAssemblyParser();
		var assembler = new ProgramAssemblerv2();
		var context = new AssemblyContext(new DictionaryMacroProvider(parser), new Proc16bInstructionConverter(), assembler, 0);

		ProgramAst ast = parser.Parse("test", source);
		var assemblerProgram = new AssemblerProgram("test", "test", ast);
		
		IReadOnlyList<AssemblyInstruction> instructions = assembler.CompileInstructionList(context, assemblerProgram);
		IReadOnlyList<AssemblyInstruction> symbolRenderedInstructions = assembler.RenderSymbols(context, instructions);

		Assert.That(symbolRenderedInstructions, Is.EquivalentTo(expectedResult));
	}
}
