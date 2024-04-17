using Assembler.Assembly;
using Assembler.Assembly.V2;
using Assembler.Parsing.Antlr;
using Assembler.Ast;

namespace Assembler.Tests.EndToEnd; 

public class ParserAssemblerTests {
	private ProcAssemblyParser m_Parser;
	private DictionaryMacroProvider m_MacroProvider;

	[SetUp]
	public void Setup() {
		m_Parser = new ProcAssemblyParser();
		
		m_MacroProvider = new DictionaryMacroProvider(m_Parser);
		
		m_MacroProvider.AddMacro(
			"testmacro",
			"""
			ldc %a, $15
			add %b, %a, %c
			"""
		);
		
		m_MacroProvider.AddMacro(
			"parammacro",
			"""
			ldc %a, macro0
			add %b, %a, macro1
			"""
		);
	}

	public static object[][] TestCases() {
		return new object[][] {
			#region Proc16a
			new object[] {
				new Proc16aInstructionConverter(),
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
				new Proc16aInstructionConverter(),
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				# initialize fib[0]
				ldi %a, $1       # A = 1           # data 0x0001                       ; 0b0000 0000 0000 0001   ; 0x0001
				ldi %ma, $1      # %ma = 1         # ALU x=1  y=0  op=add   write=%ma  ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				add %a, %a, $1   # A = A + 1       # ALU x=A  y=1  op=add   write=A    ; 0b100 0 00 10 00000 001 ; 0x8201
				ldi %ma, $1      # %ma = 1         # ALU x=1  y=0  op=add   write=%ma  ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				mov %b, %a       # B = A           # ALU x=A  y=0  op=add   write=B    ; 0b100 0 00 01 00000 010 ; 0x8102
				ldi %a, $0       # A = 0           # data 0x0000                       ; 0b0000 0000 0000 0000   ; 0x0000
				mov %ma, %b      # %ma = B         # ALU x=B  y=0  op=add   write=%ma  ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext: #                                ; label 0x0007
				# A = &&fib[last]
				ldi %a, $0       # A = 0            # data 0x0000                      ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				mov %a, %ma      # A = %ma          # ALU x=%ma y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				mov %b, %ma      # B = %ma          # ALU x=%ma y=0  op=add   write=B  ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				sub %a, %a, $1   # A = A - 1        # ALU x=A  y=1  op=sub   write=A   ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				mov %a, %ma      # A = %ma          # ALU x=%ma y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				add %b, %a, %b   # B = A + B        # ALU x=A  y=B  op=add   write=B   ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				ldi %a, $0       # A = 0            # data 0x0000                      ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				mov %a, %ma      # A = %ma          # ALU x=%ma y=0  op=add   write=A  ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				add %a, %a, $1   # A = A + 1        # ALU x=A  y=1  op=add   write=A   ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				mov %ma, %b      # %ma = B	        # ALU x=B  y=0  op=add   write=%ma ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				ldi %a, $0       # A = 0	        # data 0x0000                       ; 0b0000 0000 0000 0000   ; 0x0000
				add %ma, %ma, $1 # %ma = %ma + 1	# ALU x=%ma b=1  op=add   write=%ma ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				ldi %a, computeNext # A = computeNext # data 0x0007                     ; 0b0000 0000 0000 0111   ; 0x0007
				jmp %a           # true JMP A       # JMP x=B op=true to=A              ; 0b10100 0000000 1 111   ; 0xA00F

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
				new Proc16aInstructionConverter(),
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				.const fib0Ptr, $1
				.const lastFibPtrPtr, $0

				# initialize fib[0]
				ldi %a, fib0Ptr  # A = 1           # data 0x0001                        ; 0b0000 0000 0000 0001   ; 0x0001
				ldi %ma, $1      # %ma = 1         # ALU x=1  y=0  op=add   write=%ma   ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				add %a, %a, $1   # A = A + 1       # ALU x=A  y=1  op=add   write=A     ; 0b100 0 00 10 00000 001 ; 0x8201
				ldi %ma, $1      # %ma = 1         # ALU x=1  y=0  op=add   write=%ma   ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				mov %b, %a       # B = A           # ALU x=A  y=0  op=add   write=B     ; 0b100 0 00 01 00000 010 ; 0x8102
				ldi %a, lastFibPtrPtr # A = 0      # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000
				mov %ma, %b      # %ma = B         # ALU x=B  y=0  op=add   write=%ma   ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext:     #                                ; label 0x0007
				# A = &&fib[last]
				ldi %a, lastFibPtrPtr # A = 0      # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				mov %a, %ma      # A = %ma         # ALU x=%ma y=0  op=add   write=A    ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				mov %b, %ma      # B = %ma         # ALU x=%ma y=0  op=add   write=B    ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				sub %a, %a, $1   # A = A - 1       # ALU x=A  y=1  op=sub   write=A     ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				mov %a, %ma      # A = %ma         # ALU x=%ma y=0  op=add   write=A    ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				add %b, %a, %b   # B = A + B       # ALU x=A  y=B  op=add   write=B     ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				ldi %a, lastFibPtrPtr # A = 0      # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				mov %a, %ma      # A = %ma         # ALU x=%ma y=0  op=add   write=A    ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				add %a, %a, $1   # A = A + 1       # ALU x=A  y=1  op=add   write=A     ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				mov %ma, %b      # %ma = B	       # ALU x=B  y=0  op=add   write=%ma   ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				ldi %a, lastFibPtrPtr # A = 0	   # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000
				add %ma, %ma, $1 # %ma = %ma + 1   # ALU x=%ma b=1  op=add   write=%ma  ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				ldi %a, computeNext # A = computeNext # data 0x0007                     ; 0b0000 0000 0000 0111   ; 0x0007
				jmp %a           # true JMP A      # JMP x=B op=true to=A               ; 0b10100 0000000 1 111   ; 0xA00F

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
				new Proc16aInstructionConverter(),
				"""
				# fill RAM cells with their addresses.

				ldi %b, $0     # B = 0      #      ALU x=0 y=0 op=add write=B   ; 100 0 01 01 00000 010 ; 0x8502
				next:          # instruction index 1               ;  
				mov %mb, %b    # %mb = B     # Ax=B ALU x=B y=0 op=add write=%max ; 110 1 00 01 00000 100 ; 0xD104
				add %b, %b, $1 # B = B + 1  #      ALU x=B y=1 op=add write=B   ; 100 1 00 10 00000 010 ; 0x9202
				ldi %a, next   # A = next   # data 0x0001
				jmp %a         # true JMP A #      JMP x=B op=true to=A         ; 10100 0000000 1 111   ; 0xA00F

				""",
				new ushort[] { 0x8502, 0xD104, 0x9202, 0x0001, 0xA00F },
			},
			new object[] {
				new Proc16aInstructionConverter(),
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
				new Proc16aInstructionConverter(),
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				# initialize fib[0]
				ldi %a, $1       # A = 1            # data 0x0001                        ; 0b0000 0000 0000 0001   ; 0x0001
				ldi %ma, $1      # %ma = 1          # ALU x=1  y=0  op=add   write=%ma   ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				add %a, %a, $1   # A = A + 1        # ALU x=A  y=1  op=add   write=A     ; 0b100 0 00 10 00000 001 ; 0x8201
				ldi %ma, $1      # %ma = 1          # ALU x=1  y=0  op=add   write=%ma   ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				mov %b, %a       # B = A            # ALU x=A  y=0  op=add   write=B     ; 0b100 0 00 01 00000 010 ; 0x8102
				ldi %a, $0       # A = 0            # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000
				mov %ma, %b      # %ma = B          # ALU x=B  y=0  op=add   write=%ma   ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext:     #                                                       ; label 0x0007
				# A = &&fib[last]
				ldi %a, $0       # A = 0            # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				mov %a, %ma      # A = %ma          # ALU x=%ma y=0  op=add   write=A    ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				mov %b, %ma      # B = %ma          # ALU x=%ma y=0  op=add   write=B    ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				sub %a, %a, $1   # A = A - 1        # ALU x=A  y=1  op=sub   write=A     ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				mov %a, %ma      # A = %ma          # ALU x=%ma y=0  op=add   write=A    ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				add %b, %a, %b   # B = A + B        # ALU x=A  y=B  op=add   write=B     ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				ldi %a, $0       # A = 0            # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				mov %a, %ma      # A = %ma          # ALU x=%ma y=0  op=add   write=A    ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				add %a, %a, $1   # A = A + 1        # ALU x=A  y=1  op=add   write=A     ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				mov %ma, %b      # %ma = B	        # ALU x=B  y=0  op=add   write=%ma   ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				ldi %a, $0       # A = 0	        # data 0x0000                        ; 0b0000 0000 0000 0000   ; 0x0000
				add %ma, %ma, $1 # %ma = %ma + 1	# ALU x=%ma b=1  op=add   write=%ma  ; 0b100 0 11 10 00000 100 ; 0x8E04
				
				ldi %a, computeNext # A = computeNext # data 0x0007                      ; 0b0000 0000 0000 0111   ; 0x0007
				jmp %a           # true JMP A        # JMP x=B op=true to=A              ; 0b10100 0000000 1 111   ; 0xA00F

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
				new Proc16aInstructionConverter(),
				"""
				# Fill RAM addresses with the fibonacci sequence.
				# Tests addition, subtraction, reading and writing from RAM, data words, jumping unconditionally.

				.const fib0Ptr, $1
				.const lastFibPtrPtr, $0

				# initialize fib[0]
				ldi %a, fib0Ptr   # A = 1           # data 0x0001                       ; 0b0000 0000 0000 0001   ; 0x0001
				ldi %ma, $1       # %ma = 1         # ALU x=1  y=0  op=add   write=%ma  ; 0b100 0 10 01 00000 100 ; 0x8904

				# fib[1]
				add %a, %a, $1    # A = A + 1       # ALU x=A  y=1  op=add   write=A    ; 0b100 0 00 10 00000 001 ; 0x8201
				ldi %ma, $1       # %ma = 1         # ALU x=1  y=0  op=add   write=%ma  ; 0b100 0 10 01 00000 100 ; 0x8904

				# lastFibPtr = 0
				# *lastFibPtr = &fib[1]
				mov %b, %a        # B = A           # ALU x=A  y=0  op=add   write=B    ; 0b100 0 00 01 00000 010 ; 0x8102
				ldi %a, lastFibPtrPtr # A = 0       # data 0x0000                       ; 0b0000 0000 0000 0000   ; 0x0000
				mov %ma, %b       # %ma = B         # ALU x=B  y=0  op=add   write=%ma  ; 0b100 1 00 01 00000 100 ; 0x9104

				computeNext: #                                ; label 0x0007
				# A = &&fib[last]
				ldi %a, lastFibPtrPtr # A = 0       # data 0x0000                       ; 0b0000 0000 0000 0000   ; 0x0000

				# A = &fib[last]
				mov %a, %ma       # A = %ma         # ALU x=%ma y=0  op=add   write=A   ; 0b100 0 11 01 00000 001 ; 0x8D01

				# D = fib[last]
				mov %b, %ma       # B = %ma         # ALU x=%ma y=0  op=add   write=B   ; 0b100 0 11 01 00000 010 ; 0x8D02

				# A = &fib[last - 1]
				sub %a, %a, $1    # A = A - 1       # ALU x=A  y=1  op=sub   write=A    ; 0b100 0 00 10 00001 001 ; 0x8209

				# A = fib[last - 1]
				mov %a, %ma       # A = %ma         # ALU x=%ma y=0  op=add   write=A   ; 0b100 0 11 01 00000 001 ; 0x8D01

				# B = fib[last - 1] + fib[last]
				# B = fib[next]
				add %b, %a, %b    # B = A + B       # ALU x=A  y=B  op=add   write=B    ; 0b100 0 00 00 00000 010 ; 0x8002

				# A = &&fib[last]
				ldi %a, lastFibPtrPtr # A = 0       # data 0x0000                       ; 0b0000 0000 0000 0000   ; 0x0000
				# A = &fib[last]
				mov %a, %ma       # A = %ma         # ALU x=%ma y=0  op=add   write=A   ; 0b100 0 11 01 00000 001 ; 0x8D01
				# A = &fib[next]
				add %a, %a, $1    # A = A + 1       # ALU x=A  y=1  op=add   write=A    ; 0b100 0 00 10 00000 001 ; 0x8201

				# Save fib[next] to memory
				mov %ma, %b       # %ma = B	        # ALU x=B  y=0  op=add   write=%ma  ; 0b100 1 00 01 00000 100 ; 0x9104

				# Update pointer
				ldi %a, lastFibPtrPtr # A = 0	    # data 0x0000                       ; 0b0000 0000 0000 0000   ; 0x0000
				add %ma, %ma, $1  # %ma = %ma + 1	# ALU x=%ma b=1  op=add   write=%ma ; 0b100 0 11 10 00000 100 ; 0x8E04
				 
				ldi %a, computeNext # A = computeNext # data 0x0007                     ; 0b0000 0000 0000 0111   ; 0x0007
				jmp %a            # true JMP A      # JMP x=B op=true to=A              ; 0b10100 0000000 1 111   ; 0xA00F

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
				new Proc16aInstructionConverter(),
				"""
				# fill RAM cells with their addresses.

				ldi %b, $0     # B = 0      #      ALU x=0 y=0 op=add write=B   ; 100 0 01 01 00000 010 ; 0x8502
				next:          # instruction index 1
				mov %mb, %b    # %mb = B     # Ax=B ALU x=B y=0 op=add write=%max ; 110 1 00 01 00000 100 ; 0xD104
				add %b, %b, $1 # B = B + 1  #      ALU x=B y=1 op=add write=B   ; 100 1 00 10 00000 010 ; 0x9202
				ldi %a, next   # A = next   # data 0x0001
				jmp %a         # true JMP A #      JMP x=B op=true to=A         ; 10100 0000000 1 111   ; 0xA00F

				""",
				new ushort[] { 0x8502, 0xD104, 0x9202, 0x0001, 0xA00F },
			},
			#endregion Proc16a
			
			#region Proc16b
			new object[] {
				new Proc16bInstructionConverter(),
				"""
				# increment A forever.
				# Tests data words, ALU to A, ALU to B, jumping unconditionally.

				.reg val,   %a
				.reg one,   %b
				.reg label, %c

					ldc val, $0        # 0b00 00 000000000000
					ldc one,     $1    # 0b00 01 000000000001
					ldc label,   loop  # 0b00 10 000000000011
					
				loop:
					add val, val, one   # 0b01 00 01 00 00000 000
					jmp label           # 0b100 00 00 10 111 0000
				""",
				new ushort[] {
					0x0000,
					0x1001,
					0x2003,
					0x4400,
					0x8170,
				},
			},
			new object[] {
				new Proc16bInstructionConverter(),
				"""
				# fill RAM cells with their addresses times two.
				
				.reg address, %a
				.reg value,   %b
				.reg one,     %c
				
					ldc address, $0 # 0b00 00 000000000000
					ldc value,   $0 # 0b00 01 000000000000
					ldc one,     $1 # 0b00 10 000000000001
				
				fill:                          # label value = 3
					ldc %d, $2 # $2            # 0b00 11 000000000010
					
					# a = a + 1 (address + 1)
					add address, address, one  # 0b01 00 10 00 00000 000
					
					# b = a * d (address * 2)
					mul value, address, %d     # 0b01 00 11 01 00010 000
					
					# %ma = b
					stb address, value         # 0b1011 1 000 001 00000
					
					ldc %d, fill               # 0b00 11 000000000011 (label value = 3)
					jump %d                    # 0b100 00 00 11 111 0000
				""",
				new ushort[] {
					0x0000,
					0x1000,
					0x2001,
					0x3002,
					0x4800,
					0x4D10,
					0xB820,
					0x3003,
					0x81F0,
				},
			},
			#endregion

			#region  Proc16b macros
			new object[] {
				new Proc16bInstructionConverter(),
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
				new Proc16bInstructionConverter(),
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
			#endregion
		};
	}
	
	[Test]
	[TestCaseSource(nameof(TestCases))]
	public void TestV2Assembler(IInstructionConverter instructionConverter, string sourceCode, ushort[] expectedResult) {
		ProgramAst ast = m_Parser.Parse("test", sourceCode);
		var factory = new AssemblyContextFactory(m_MacroProvider, instructionConverter);
		var context = factory.CreateContext(null, new ProgramAssemblerv2(), 0);

		IReadOnlyList<ushort> assembledProgram;
		try {
			assembledProgram = context.Assembler.AssembleAst(context, new AssemblerProgram(null, null, ast));
		} catch (InvalidProcAssemblyProgramException ex) {
			Assert.Fail(
				"Test failed due to {0}:\n{1}",
				nameof(InvalidProcAssemblyProgramException),
				string.Join("\n", ex.Instructions.Select(instruction => $"{instruction.Instruction}: {instruction.Message}"))
			);
			return;
		}

		Assert.That(assembledProgram, Is.EquivalentTo(expectedResult));
	}
}
