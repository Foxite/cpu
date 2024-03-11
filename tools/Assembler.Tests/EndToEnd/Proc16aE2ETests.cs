using Assembler.Assembly;
using Assembler.Parsing.Antlr;
using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Tests.EndToEnd; 

public class Proc16aE2ETests {
	private ProcAssemblyParser m_Parser;
	private ProgramAssemblerFactory m_Factory;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
		m_Factory = new ProgramAssemblerFactory(new Proc16aInstructionConverter());
	}

	public static object[][] AssembleTestCases() {
		return new object[][] {
			new object[] {
				"""
				# increment A forever.
				# Tests data words, ALU to A, ALU to B, jumping unconditionally.

				ldi %a, $0     # A = 0        # data 0x0000
				ldi %b, $2     # B = 2        # ALU x=1 y=1 op=add write=B  ; 100 0 10 10 00000 010 ; 0x8A02
				add %a, %a, $1 # A = A + 1    # ALU x=A y=1 op=add write=A  ; 100 0 00 10 00000 001 ; 0x8201
				jgt %b, %a, $0 # A > 0 JMP B  # JMP x=A op=gt to=B          ; 10100 0000000 0 001   ; 0xA001

				""",
				new ushort[] { 0x0000, 0x8A02, 0x8201, 0xA001 },
			},
			new object[] {
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				# initialize fib[0]
				ldi %a, $1    # A = 1           # data 0x0001                     ; 0b0000 0000 0000 0001   ; 0x0001
				ldi *a, $1    # *A = 1          # ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				add %a, %a, $1 # A = A + 1      # ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201
				ldi *a, $1     # *A = 1         # ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				mov %b, %a     # B = A          # ALU x=A  y=0  op=add   write=B  ; 0b100 0 00 01 00000 010 ; 0x8102
				ldi %a, $0     # A = 0          # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				mov *a, %b     # *A = B         # ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext: #                                ; label 0x0007
				# A = &&fib[last]
				ldi %a, $0     # A = 0          # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				mov %a, *a     # A = *A         # ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				mov %b, *a     # B = *A         # ALU x=*A y=0  op=add   write=B  ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				sub %a, %a, $1 # A = A - 1      # ALU x=A  y=1  op=sub   write=A  ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				mov %a, *a     # A = *A         # ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				add %b, %a, %b # B = A + B      # ALU x=A  y=B  op=add   write=B  ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				ldi %a, $0     # A = 0          # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				mov %a, *a     # A = *A         # ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				add %a, %a, $1 # A = A + 1      # ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				mov *a, %b     # *A = B	        # ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				ldi %a, $0     # A = 0	        # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				add *a, *a, $1 # *A = *A + 1	# ALU x=*A b=1  op=add   write=*A ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				ldi %a, computeNext # A = computeNext # data 0x0007               ; 0b0000 0000 0000 0111   ; 0x0007
				jmp %a         # true JMP A     # JMP x=B op=true to=A            ; 0b10100 0000000 1 111   ; 0xA00F

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
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				.const fib0Ptr, $1
				.const lastFibPtrPtr, $0

				# initialize fib[0]
				ldi %a, fib0Ptr # A = 1         # data 0x0001                     ; 0b0000 0000 0000 0001   ; 0x0001
				ldi *a, $1      # *A = 1        # ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				add %a, %a, $1 # A = A + 1      # ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201
				ldi *a, $1     # *A = 1         # ALU x=1  y=0  op=add   write=*A ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				mov %b, %a     # B = A          # ALU x=A  y=0  op=add   write=B  ; 0b100 0 00 01 00000 010 ; 0x8102
				ldi %a, lastFibPtrPtr # A = 0   # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				mov *a, %b     # *A = B         # ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext: #                                ; label 0x0007
				# A = &&fib[last]
				ldi %a, lastFibPtrPtr # A = 0   # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				mov %a, *a     # A = *A         # ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				mov %b, *a     # B = *A         # ALU x=*A y=0  op=add   write=B  ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				sub %a, %a, $1 # A = A - 1      # ALU x=A  y=1  op=sub   write=A  ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				mov %a, *a     # A = *A         # ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				add %b, %a, %b # B = A + B      # ALU x=A  y=B  op=add   write=B  ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				ldi %a, lastFibPtrPtr # A = 0   # data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				mov %a, *a     # A = *A         # ALU x=*A y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				add %a, %a, $1 # A = A + 1      # ALU x=A  y=1  op=add   write=A  ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				mov *a, %b     # *A = B	        # ALU x=B  y=0  op=add   write=*A ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				ldi %a, lastFibPtrPtr # A = 0	# data 0x0000                     ; 0b0000 0000 0000 0000   ; 0x0000
				add *a, *a, $1 # *A = *A + 1	# ALU x=*A b=1  op=add   write=*A ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				ldi %a, computeNext # A = computeNext # data 0x0007               ; 0b0000 0000 0000 0111   ; 0x0007
				jmp %a         # true JMP A     # JMP x=B op=true to=A            ; 0b10100 0000000 1 111   ; 0xA00F

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

				ldi %b, $0     # B = 0      #      ALU x=0 y=0 op=add write=B   ; 100 0 01 01 00000 010 ; 0x8502
				next:          # instruction index 1               ;  
				mov *b, %b     # *B = B     # Ax=B ALU x=B y=0 op=add write=*Ax ; 110 1 00 01 00000 100 ; 0xD104
				add %b, %b, $1 # B = B + 1  #      ALU x=B y=1 op=add write=B   ; 100 1 00 10 00000 010 ; 0x9202
				ldi %a, next   # A = next   # data 0x0001
				jmp %a         # true JMP A #      JMP x=B op=true to=A         ; 10100 0000000 1 111   ; 0xA00F

				""",
				new ushort[] { 0x8502, 0xD104, 0x9202, 0x0001, 0xA00F },
			},
		};
	}

	[Test]
	[TestCaseSource(nameof(AssembleTestCases))]
	public void TestAssemble(string sourceCode, ushort[] expectedResult) {
		ProgramAst ast = m_Parser.Parse(sourceCode);

		List<ushort> assembledProgram = m_Factory.GetAssembler(ast).Assemble().ToList();
		
		Assert.That(assembledProgram, Is.EquivalentTo(expectedResult));
	}
}
