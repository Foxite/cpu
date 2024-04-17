using Assembler.Parsing.Antlr;
using Assembler.Ast;
using IAA = Assembler.Ast.InstructionArgumentAst; 

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
						IAA.Register("operation"),
						IAA.Symbol("willbe")
					)),
					new ProgramStatementAst(null, new InstructionAst(".dispatched",
						IAA.String("""to your \"location\" in"""),
						IAA.Constant(0x999F)
					))
				)
			},
			new object[] {
				"""
				# fill RAM cells with their addresses.
				
				ldi %b,  $0     # B = 0      #      ALU x=0 y=0 op=add write=B   ; 100 0 01 01 00000 010 ; 0x8502
				next:           # instruction index 1               ;  
				mov %mb, %b     # *B = B     # Ax=B ALU x=B y=0 op=add write=*Ax ; 110 1 00 01 00000 100 ; 0xD104
				add %b,  %b, $1 # B = B + 1  #      ALU x=B y=1 op=add write=B   ; 100 1 00 10 00000 010 ; 0x9202
				ldi %a,  next   # A = next   # data 0x0001
				jmp %a          # true JMP A #      JMP x=B op=true to=A         ; 10100 0000000 1 111   ; 0xA00F
				
				""",
				new ProgramAst(
					new ProgramStatementAst(null, new InstructionAst("ldi",
						IAA.Register("b"),
						IAA.Constant(0)
					)),
					new ProgramStatementAst("next", new InstructionAst("mov",
						IAA.Register("mb"),
						IAA.Register("b")
					)),
					new ProgramStatementAst(null, new InstructionAst("add",
						IAA.Register("b"),
						IAA.Register("b"),
						IAA.Constant(1)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldi",
						IAA.Register("a"),
						IAA.Symbol("next")
					)),
					new ProgramStatementAst(null, new InstructionAst("jmp",
						IAA.Register("a")
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
						IAA.Register("a"),
						IAA.Constant(0)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldi",
						IAA.Register("b"),
						IAA.Constant(2)
					)),
					new ProgramStatementAst(null, new InstructionAst("add",
						IAA.Register("a"),
						IAA.Register("a"),
						IAA.Constant(1)
					)),
					new ProgramStatementAst(null, new InstructionAst("jgt",
						IAA.Register("b"),
						IAA.Register("a"),
						IAA.Constant(0)
					))
				)
			},
			new object[] {
				"""
				ldc %a, [5 + 2]
				ldc %a, [5 + heyoo]
				ldc %a, [5 * 2 + 1]
				ldc %a, [(5 * 2) + 1]
				ldc %a, [5 * (2 + 1)]
				ldc %a, [~5 * (2 + 1)]
				""",
				new ProgramAst(
					new ProgramStatementAst(null, new InstructionAst("ldc",
						IAA.Register("a"),
						IAA.Expression(IAA.Constant(5), BinaryExpressionOp.Add, IAA.Constant(2))
					)),
					new ProgramStatementAst(null, new InstructionAst("ldc",
						IAA.Register("a"),
						IAA.Expression(IAA.Constant(5), BinaryExpressionOp.Add, IAA.Symbol("heyoo"))
					)),
					new ProgramStatementAst(null, new InstructionAst("ldc",
						IAA.Register("a"),
						IAA.Expression(
							IAA.Expression(IAA.Constant(5), BinaryExpressionOp.Multiply, IAA.Constant(2)),
							BinaryExpressionOp.Add,
							IAA.Constant(1)
						)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldc",
						IAA.Register("a"),
						IAA.Expression(
							IAA.Expression(IAA.Constant(5), BinaryExpressionOp.Multiply, IAA.Constant(2)),
							BinaryExpressionOp.Add,
							IAA.Constant(1)
						)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldc",
						IAA.Register("a"),
						IAA.Expression(
							IAA.Constant(5),
							BinaryExpressionOp.Multiply,
							IAA.Expression(IAA.Constant(2), BinaryExpressionOp.Add, IAA.Constant(1))
						)
					)),
					new ProgramStatementAst(null, new InstructionAst("ldc",
						IAA.Register("a"),
						IAA.Expression(
							IAA.Expression(UnaryExpressionOp.Not, IAA.Constant(5)),
							BinaryExpressionOp.Multiply,
							IAA.Expression(IAA.Constant(2), BinaryExpressionOp.Add, IAA.Constant(1))
						)
					))
				)
			},
		};
	}

	[Test]
	[TestCaseSource(nameof(ProgramTestCases))]
	public void ParseDataWord(string sourceCode, IAssemblyAst expectedResult) {
		ProgramAst ast = m_Parser.Parse("main", sourceCode);
		
		Assert.That(ast, Is.EqualTo(expectedResult), "Parse result does not match specification");
	}
}
