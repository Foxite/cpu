using Assembler.Parsing.Antlr;
using Assembler.Ast;

namespace Assembler.Tests;

public class ProcAssemblyV2ParserTests {
	private ProcAssemblyParser m_Parser;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
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
			},
			new object[] {
				"""
				# fill RAM cells with their addresses.
				
				ldi %b, $0     # B = 0      #      ALU x=0 y=0 op=add write=B   ; 100 0 01 01 00000 010 ; 0x8502
				next:          # instruction index 1               ;  
				mov *b, %b     # *B = B     # Ax=B ALU x=B y=0 op=add write=*Ax ; 110 1 00 01 00000 100 ; 0xD104
				add %b, %b, $1 # B = B + 1  #      ALU x=B y=1 op=add write=B   ; 100 1 00 10 00000 010 ; 0x9202
				ldi %a, next   # A = next   # data 0x0001
				jmp %a         # true JMP A #      JMP x=B op=true to=A         ; 10100 0000000 1 111   ; 0xA00F
				
				""",
				new ProgramAst(
					new ProgramStatementAst(null, new InstructionAst("ldi",
						InstructionArgumentAst.Register("b"),
						InstructionArgumentAst.Constant(0)
					)),
					new ProgramStatementAst("next", new InstructionAst("mov",
						InstructionArgumentAst.StarRegister("b"),
						InstructionArgumentAst.Register("b")
					)),
					new ProgramStatementAst(null, new InstructionAst("add",
						InstructionArgumentAst.Register("b"),
						InstructionArgumentAst.Register("b"),
						InstructionArgumentAst.Constant(1)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldi",
						InstructionArgumentAst.Register("a"),
						InstructionArgumentAst.Symbol("next")
					)),
					new ProgramStatementAst(null, new InstructionAst("jmp",
						InstructionArgumentAst.Register("a")
					))
				)
			},
			new object[] {
				"""
				# increment A forever.
				
				ldi %a, $0     # A = 0        # data 0x0000
				ldi %b, $2     # B = 2        # ALU x=1 y=1 op=add write=B  ; 100 0 10 10 00000 010 ; 0x8A02
				add %a, %a, $1 # A = A + 1    # ALU x=A y=1 op=add write=A  ; 100 0 00 10 00000 001 ; 0x8201
				jgt %b, %a, $0 # A > 0 JMP B  # JMP x=A op=gt to=B          ; 10100 0000000 0 001   ; 0xA001
				
				""",
				
				new ProgramAst(
					new ProgramStatementAst(null, new InstructionAst("ldi",
						InstructionArgumentAst.Register("a"),
						InstructionArgumentAst.Constant(0)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldi",
						InstructionArgumentAst.Register("b"),
						InstructionArgumentAst.Constant(2)
					)),
					new ProgramStatementAst(null, new InstructionAst("add",
						InstructionArgumentAst.Register("a"),
						InstructionArgumentAst.Register("a"),
						InstructionArgumentAst.Constant(1)
					)),
					new ProgramStatementAst(null, new InstructionAst("jgt",
						InstructionArgumentAst.Register("b"),
						InstructionArgumentAst.Register("a"),
						InstructionArgumentAst.Constant(0)
					))
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
