using Assembler.Ast;

namespace Assembler.Assembly;

public class Proc16aInstructionConverter : InstructionMapInstructionConverter {
	public Proc16aInstructionConverter() : base("Proc16a") {
		AddInstruction("ldi", new ldiInstruction());
		AddInstruction("mov", new movInstruction());
		AddInstruction("nop", new nopInstruction());
		AddInstruction("brk", new brkInstruction());
		
		AddInstruction("add", new addInstruction());
		AddInstruction("sub", new subInstruction());
		AddInstruction("mul", new mulInstruction());
		AddInstruction("div", new divInstruction());
		AddInstruction("shl", new shlInstruction());
		AddInstruction("shr", new shrInstruction());
		
		AddInstruction("and",  new andInstruction());
		AddInstruction("or",   new orInstruction());
		AddInstruction("not",  new notInstruction());
		AddInstruction("xor",  new xorInstruction());
		AddInstruction("xnor", new xnorInstruction());
		AddInstruction("nor",  new norInstruction());
		
		AddInstruction("true",  new trueInstruction());
		AddInstruction("false", new falseInstruction());
		
		AddInstruction("cgt", new cgtInstruction());
		AddInstruction("ceq", new ceqInstruction());
		AddInstruction("cge", new cgeInstruction());
		AddInstruction("clt", new cltInstruction());
		AddInstruction("cne", new cneInstruction());
		AddInstruction("cle", new cleInstruction());
		
		AddInstruction("jmp", new jmpInstruction());
		AddInstruction("jgt", new jgtInstruction());
		AddInstruction("jeq", new jeqInstruction());
		AddInstruction("jge", new jgeInstruction());
		AddInstruction("jlt", new jltInstruction());
		AddInstruction("jne", new jneInstruction());
		AddInstruction("jle", new jleInstruction());
	}
	
	private static void SetInstructionBit(ref ushort instruction, int bit, bool value) {
		instruction = AssemblyUtil.SetBit(instruction, bit, value);
	}

	private static bool IsStarRegister(RegisterAst registerAst) => registerAst.Value.StartsWith("m");
	private static bool IsStarRegister(RegisterAst registerAst, out string? starValue) {
		if (IsStarRegister(registerAst)) {
			starValue = registerAst.Value[1..];
			return true;
		} else {
			starValue = null;
			return false;
		}
	}
	
	private static bool OneDistinctStarRegister(out string? register, params InstructionArgumentAst[] args) {
		var distinctStars = args
			.OfType<RegisterAst>()
			.Where(IsStarRegister)
			.Select(starRegister => starRegister.Value).Distinct().ToList();
		if (distinctStars.Count == 0) {
			register = null;
			return true;
		} else if (distinctStars.Count == 1) {
			register = distinctStars[0][1..];
			return true;
		} else {
			register = null;
			return false;
		}
	}
		
	private record ldiInstruction() : Instruction {
		[Validator]
		protected InstructionSupport Validate(RegisterAst register, ConstantAst constant) {
			if (register.Value == "a") {
				return constant.Value is >= -2 and <= 0x7FFF ? InstructionSupport.Supported : InstructionSupport.OtherError;
			} else {
				return constant.Value is >= -2 and <= 2 ? InstructionSupport.Supported : InstructionSupport.OtherError;
			}
		}

		[Converter]
		protected ushort Convert(RegisterAst register, ConstantAst constant) {
			if (register.Value == "a" && constant.Value >= 0) {
				return (ushort) constant.Value;
			} else {
				ushort ret = 0;
				SetInstructionBit(ref ret, 15, true);
				
				ushort opcode = constant.Value switch {
					-2 => 0b10_00_10010, // ~1
					-1 => 0b01_10_00001, // 0 - 1
					0  => 0b01_01_00000, // 0 + 0
					1  => 0b10_01_00000, // 1 + 0
					2  => 0b10_10_00000, // 1 + 1
				};

				ret |= (ushort) (opcode << 3);
				
				if (IsStarRegister(register, out string? starValue)) {
					SetInstructionBit(ref ret, 14, starValue == "b");
					SetInstructionBit(ref ret, 2, true);
				} else if (register.Value == "b") {
					SetInstructionBit(ref ret, 1, true);
				} else if (register.Value == "a") {
					SetInstructionBit(ref ret, 0, true);
				}

				return ret;
			}
		}
	}

	private record brkInstruction() : Instruction {
		[Converter]
		protected ushort Convert() => 1;
	}

	private record nopInstruction() : Instruction {
		[Converter]
		protected ushort Convert() {
			// none = 0 + 0
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			return ret;
		}
	}

	private record movInstruction() : Instruction {
		[Converter]
		protected ushort Convert(RegisterAst target, RegisterAst source) {
			// lhs = rhs + 0
			ushort ret = 0;
			
			// ALU operation
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			
			// Ax select
			OneDistinctStarRegister(out string? ax, source, target);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = rhs
			if (IsStarRegister(source)) {
				SetInstructionBit(ref ret, 11, true);
				SetInstructionBit(ref ret, 10, true);
			} else {
				SetInstructionBit(ref ret, 12, source.Value == "b");
				SetInstructionBit(ref ret, 11, false);
				SetInstructionBit(ref ret, 10, false);
			}
			
			// Y = 0
			SetInstructionBit(ref ret, 9, false);
			SetInstructionBit(ref ret, 8, true);
			
			// Out = lhs
			if (IsStarRegister(target)) {
				SetInstructionBit(ref ret, 2, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.Value == "a") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}

	private record AluInstruction(int Opcode) : Instruction {
		[Validator] protected InstructionSupport ValidateRegister(RegisterAst target, RegisterAst lhs, RegisterAst rhs) => Validate(target, lhs, rhs);
		[Validator] protected InstructionSupport ValidateConstant(RegisterAst target, RegisterAst lhs, ConstantAst rhs) => Validate(target, lhs, rhs);
		
		protected InstructionSupport Validate(RegisterAst target, InstructionArgumentAst lhs, InstructionArgumentAst rhs) {
			if (!OneDistinctStarRegister(out _, target, lhs, rhs)) {
				return InstructionSupport.OtherError;
			}

			if (lhs is ConstantAst { Value: not (0 or 1) }) {
				return InstructionSupport.OtherError;
			}

			if (rhs is ConstantAst { Value: not (0 or 1) }) {
				return InstructionSupport.OtherError;
			}

			if (lhs is RegisterAst registerLhs &&
			    rhs is RegisterAst registerRhs &&
			    !IsStarRegister(registerLhs) &&
			    !IsStarRegister(registerRhs) &&
			    registerLhs.Value == registerRhs.Value
			) {
				return InstructionSupport.OtherError;
			}

			return InstructionSupport.Supported;
		}

		[Converter] protected ushort ConvertRR(RegisterAst target, RegisterAst lhs, RegisterAst rhs) => Convert(target, lhs, rhs);
		[Converter] protected ushort ConvertRC(RegisterAst target, RegisterAst lhs, ConstantAst rhs) => Convert(target, lhs, rhs);
		[Converter] protected ushort ConvertCR(RegisterAst target, ConstantAst lhs, RegisterAst rhs) => Convert(target, lhs, rhs);
		[Converter] protected ushort ConvertCC(RegisterAst target, ConstantAst lhs, ConstantAst rhs) => Convert(target, lhs, rhs);
			
		protected ushort Convert(RegisterAst target, InstructionArgumentAst lhs, InstructionArgumentAst rhs) {
			ushort ret = 0;
			
			SetInstructionBit(ref ret, 15, true);
			OneDistinctStarRegister(out string? ax, target, lhs, rhs);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = lhs
			if (lhs is RegisterAst registerLhs) {
				if (IsStarRegister(registerLhs)) {
					SetInstructionBit(ref ret, 11, true);
					SetInstructionBit(ref ret, 10, true);
				} else {
					SetInstructionBit(ref ret, 12, registerLhs.Value == "b");
					SetInstructionBit(ref ret, 11, false);
					SetInstructionBit(ref ret, 10, false);
				}
			} else if (lhs is ConstantAst constantLhs) {
				if (constantLhs.Value == 0) {
					SetInstructionBit(ref ret, 11, false);
					SetInstructionBit(ref ret, 10, true);
				} else {
					SetInstructionBit(ref ret, 11, true);
					SetInstructionBit(ref ret, 10, false);
				}
			}
			
			// Y = rhs
			if (rhs is RegisterAst registerRhs) {
				if (IsStarRegister(registerRhs)) {
					SetInstructionBit(ref ret, 9, true);
					SetInstructionBit(ref ret, 8, true);
				} else {
					SetInstructionBit(ref ret, 9, false);
					SetInstructionBit(ref ret, 8, false);
				}
			} else if (rhs is ConstantAst constantRhs) {
				if (constantRhs.Value == 0) {
					SetInstructionBit(ref ret, 9, false);
					SetInstructionBit(ref ret, 8, true);
				} else {
					SetInstructionBit(ref ret, 9, true);
					SetInstructionBit(ref ret, 8, false);
				}
			}

			// Out bits
			if (IsStarRegister(target)) {
				SetInstructionBit(ref ret, 2, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.Value == "a") {
				SetInstructionBit(ref ret, 0, true);
			}

			ret |= (ushort) (Opcode << 3);

			return ret;
		}
	}
	
	private record addInstruction()  : AluInstruction(0b00000);
	private record subInstruction()  : AluInstruction(0b00001);
	private record mulInstruction()  : AluInstruction(0b00010);
	private record divInstruction()  : AluInstruction(0b00011);
	private record shlInstruction()  : AluInstruction(0b00100);
	private record shrInstruction()  : AluInstruction(0b00101);
	// 0b00110
	// 0b00111
	private record andInstruction()  : AluInstruction(0b10000);
	private record orInstruction ()  : AluInstruction(0b10001);
	// 0b10010 (notInstruction)
	// 0b10011
	private record xorInstruction () : AluInstruction(0b10100);
	private record xnorInstruction() : AluInstruction(0b10101);
	private record norInstruction () : AluInstruction(0b10110);
	private record nandInstruction() : AluInstruction(0b10111);
	// 0b11000 (falseInstruction)
	private record cgtInstruction()  : AluInstruction(0b11001);
	private record ceqInstruction()  : AluInstruction(0b11010);
	private record cgeInstruction()  : AluInstruction(0b11011);
	private record cltInstruction()  : AluInstruction(0b11100);
	private record cneInstruction()  : AluInstruction(0b11101);
	private record cleInstruction()  : AluInstruction(0b11110);
	// 0b11111 (trueInstruction)


	private record notInstruction() : Instruction {
		[Validator] protected InstructionSupport ValidateConstant(RegisterAst target, ConstantAst operand) => Validate(target, operand);
		[Validator] protected InstructionSupport ValidateRegister(RegisterAst target, RegisterAst operand) => Validate(target, operand);
			
		protected InstructionSupport Validate(RegisterAst target, InstructionArgumentAst operand) {
			if (!OneDistinctStarRegister(out _, target, operand)) {
				return InstructionSupport.OtherError;
			}

			if (operand is ConstantAst { Value: not (0 or 1) }) {
				return InstructionSupport.OtherError;
			}

			return InstructionSupport.Supported;
		}

		[Converter] protected ushort ConvertConstant(RegisterAst target, ConstantAst operand) => Convert(target, operand);
		[Converter] protected ushort ConvertRegister(RegisterAst target, RegisterAst operand) => Convert(target, operand);
		
		protected ushort Convert(RegisterAst target, InstructionArgumentAst operand) {
			ushort ret = 0;
			
			OneDistinctStarRegister(out string? ax, target, operand);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = operand
			if (operand is RegisterAst registerOperand) {
				if (IsStarRegister(registerOperand)) {
					SetInstructionBit(ref ret, 11, true);
					SetInstructionBit(ref ret, 10, true);
				} else {
					SetInstructionBit(ref ret, 12, registerOperand.Value == "b");
					SetInstructionBit(ref ret, 11, false);
					SetInstructionBit(ref ret, 10, false);
				}
			} else if (operand is ConstantAst constantOperand) {
				if (constantOperand.Value == 0) {
					SetInstructionBit(ref ret, 11, false);
					SetInstructionBit(ref ret, 10, true);
				} else {
					SetInstructionBit(ref ret, 11, true);
					SetInstructionBit(ref ret, 10, false);
				}
			}
			
			// Out bits
			if (IsStarRegister(target)) {
				SetInstructionBit(ref ret, 2, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.Value == "b") { // pretty sure this is wrong, but i'm not fixing bugs here, just updating it with the refactoring
				SetInstructionBit(ref ret, 0, true);
			}

			ret |= 0b10010 << 3;

			return ret;
		}
	}


	private record trueInstruction() : Instruction {
		[Converter]
		protected ushort Convert(RegisterAst target) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			ret |= 0b11111 << 3;
			
			// Out bits
			if (IsStarRegister(target, out string? starTarget)) {
				SetInstructionBit(ref ret, 14, starTarget == "b");
				
				SetInstructionBit(ref ret, 2, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}
	
	private record falseInstruction() : Instruction {
		[Converter]
		protected ushort Convert(RegisterAst target) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			ret |= 0b11000 << 3;
			
			// Out bits
			if (IsStarRegister(target, out string? starTarget)) {
				SetInstructionBit(ref ret, 14, starTarget == "b");

				SetInstructionBit(ref ret, 2, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.Value == "b") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}
	
	
	

	private record JumpInstruction(int CompareMode) : Instruction {
		[Validator]
		protected InstructionSupport Validate(RegisterAst target, RegisterAst lhs, ConstantAst rhs) {
			return target.Value != lhs.Value && rhs.Value == 0 ? InstructionSupport.Supported : InstructionSupport.OtherError;
		}

		[Converter]
		protected ushort Convert(RegisterAst target, RegisterAst lhs, ConstantAst rhs) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, true);
			SetInstructionBit(ref ret, 11, false);

			string compareLhsRegister = lhs.Value;

			SetInstructionBit(ref ret, 3, compareLhsRegister == "b");
			
			ret |= (ushort) CompareMode;

			return ret;
		}
	}
	
	private record jgtInstruction() : JumpInstruction(0b001);
	private record jeqInstruction() : JumpInstruction(0b010);
	private record jgeInstruction() : JumpInstruction(0b011);
	private record jltInstruction() : JumpInstruction(0b100);
	private record jneInstruction() : JumpInstruction(0b101);
	private record jleInstruction() : JumpInstruction(0b110);
	
	
	private record jmpInstruction : Instruction {
		[Converter]
		protected ushort Convert(RegisterAst target) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, true);
			SetInstructionBit(ref ret, 11, false);

			SetInstructionBit(ref ret, 3, target.Value == "a");
			
			ret |= 0b111;

			return ret;
		}
	}
}
