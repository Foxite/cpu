using System.Text;
using Assembler.Assembly;
using sly.buildresult;
using sly.parser;
using sly.parser.generator;

namespace Assembler.Tests.EndToEnd; 

public class Proc16aE2ETests {
	private Proc16aProgramAssembler m_Assembler;
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

		m_Assembler = new Proc16aProgramAssembler();
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

	public static object[][] AssembleTestCases() {
		return new object[][] {
			new object[] {
				"""
				# increment A forever.
				# Tests data words, ALU to A, ALU to B, jumping unconditionally.

				A = 0        # data 0x0000
				B = 2        # ALU x=1 y=1 op=add write=B  ; 100 0 10 10 00000 010 ; 0x8A02
				A = A + 1    # ALU x=A y=1 op=add write=A  ; 100 0 00 10 00000 001 ; 0x8201
				A > 0 JMP B  # JMP x=A op=gt to=B          ; 10100 0000000 0 001   ; 0xA001

				""",
				new ushort[] { 0x0000, 0x8A02, 0x8201, 0xA001 },
			},
			new object[] {
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				# initialize fib[0]
				A = 1		# data 0x0001                     ; 0b0000 0000 0000 0001   ; 0x0001
				*A = 1		# ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				A = A + 1	# ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201
				*A = 1		# ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				B = A		# ALU x=A  y=0  op=add   write=B  ; 0b100 0 00 01 00000 010 ; 0x8102
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				*A = B		# ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext: #                                ; label 0x0007
				# A = &&fib[last]
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				A = *A		# ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				B = *A		# ALU x=*A y=0  op=add   write=B  ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				A = A - 1	# ALU x=A  y=1  op=sub   write=A  ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				A = *A		# ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				B = A + B	# ALU x=A  y=B  op=add   write=B  ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				A = *A		# ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				A = A + 1	# ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				*A = B		# ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				*A = *A + 1	# ALU x=*A b=1  op=add   write=*A ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				A = computeNext # data 0x0007                 ; 0b0000 0000 0000 0111   ; 0x0007
				true JMP A  # JMP x=B op=true to=A            ; 0b10100 0000000 1 111   ; 0xA00F

				""",
				new ushort[] {
					0x0001,
					0x8904,
					0x8201,
					0x8904,
					0x8102,
					0x0000,
					0x9104,
					0x0000,
					0x8D01,
					0x8D02,
					0x8209,
					0x8D01,
					0x8002,
					0x0000,
					0x8D01,
					0x8201,
					0x9104,
					0x0000,
					0x8E04,
					0x0007,
					0xA00F,
				},
			},
			// only difference with this one and the previous one is the label is on the same line as the statement
			new object[] {
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				# initialize fib[0]
				A = 1		# data 0x0001                     ; 0b0000 0000 0000 0001   ; 0x0001
				*A = 1		# ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				A = A + 1	# ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201
				*A = 1		# ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				B = A		# ALU x=A  y=0  op=add   write=B  ; 0b100 0 00 01 00000 010 ; 0x8102
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				*A = B		# ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				#                                ; label 0x0007
				# A = &&fib[last]
				computeNext: A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				A = *A		# ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				B = *A		# ALU x=*A y=0  op=add   write=B  ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				A = A - 1	# ALU x=A  y=1  op=sub   write=A  ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				A = *A		# ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				B = A + B	# ALU x=A  y=B  op=add   write=B  ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				A = *A		# ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				A = A + 1	# ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				*A = B		# ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				A = 0		# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				*A = *A + 1	# ALU x=*A b=1  op=add   write=*A ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				A = computeNext # data 0x0007                 ; 0b0000 0000 0000 0111   ; 0x0007
				true JMP A  # JMP x=B op=true to=A            ; 0b10100 0000000 1 111   ; 0xA00F

				""",
				new ushort[] {
					0x0001,
					0x8904,
					0x8201,
					0x8904,
					0x8102,
					0x0000,
					0x9104,
					0x0000,
					0x8D01,
					0x8D02,
					0x8209,
					0x8D01,
					0x8002,
					0x0000,
					0x8D01,
					0x8201,
					0x9104,
					0x0000,
					0x8E04,
					0x0007,
					0xA00F,
				},
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
				new ushort[] { 0x8502, 0xD104, 0x9202, 0x0001, 0xA00F },
			},
		};
	}

	[Test]
	[TestCaseSource(nameof(AssembleTestCases))]
	public void TestAssemble(string sourceCode, ushort[] expectedResult) {
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

		var assembledProgram = m_Assembler.Assemble((ProgramAst) parseResult.Result).ToList();
		
		Assert.That(assembledProgram, Is.EquivalentTo(expectedResult));
	}
}
