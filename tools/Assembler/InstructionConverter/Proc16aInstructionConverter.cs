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
	
	private static bool OneDistinctStarRegister(out string? register, params InstructionArgumentAst[] args) {
		var distinctStars = args.OfType<StarRegisterAst>().Select(starRegister => starRegister.Value).Distinct().ToList();
		if (distinctStars.Count == 0) {
			register = null;
			return true;
		} else if (distinctStars.Count == 1) {
			register = distinctStars[0];
			return true;
		} else {
			register = null;
			return false;
		}
	}
		
	private record ldiInstruction() : Instruction {
		[Validator]
		protected InstructionSupport Validate(RegisterAst register, ConstantAst constant)
		protected InstructionSupport Validate(StarRegisterAst register, ConstantAst constant) {
			if (register.Type == IAT.Register && register.RslsValue == "a") {
				return constant.ConstantValue!.Value is >= -2 and <= 0x7FFF ? InstructionSupport.Supported : InstructionSupport.OtherError;
			} else {
				return constant.ConstantValue!.Value is >= -2 and <= 2 ? InstructionSupport.Supported : InstructionSupport.OtherError;
			}
		}

		[Converter]
		protected ushort Convert(RegisterAst register, ConstantAst constant)
		protected ushort Convert(StarRegisterAst register, ConstantAst constant) {
			if (register.Type == IAT.Register && register.RslsValue == "a" && constant.ConstantValue!.Value >= 0) {
				return (ushort) constant.ConstantValue!.Value;
			} else {
				ushort ret = 0;
				SetInstructionBit(ref ret, 15, true);
				
				ushort opcode = constant.ConstantValue!.Value switch {
					-2 => 0b10_00_10010, // ~1
					-1 => 0b01_10_00001, // 0 - 1
					0  => 0b01_01_00000, // 0 + 0
					1  => 0b10_01_00000, // 1 + 0
					2  => 0b10_10_00000, // 1 + 1
				};

				ret |= (ushort) (opcode << 3);
				
				if (register.Type == IAT.StarRegister) {
					SetInstructionBit(ref ret, 14, register.RslsValue == "b");
					SetInstructionBit(ref ret, 2, true);
				} else if (register.RslsValue == "b") {
					SetInstructionBit(ref ret, 1, true);
				} else if (register.RslsValue == "a") {
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
		protected ushort Convert(    RegisterAst target, RegisterAst source)
		protected ushort Convert(    RegisterAst target, StarRegisterAst source)
		protected ushort Convert(StarRegisterAst target, RegisterAst source) {
			// lhs = rhs + 0
			ushort ret = 0;
			
			// ALU operation
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			
			// Ax select
			OneDistinctStarRegister(out string? ax, source, target);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = rhs
			if (source.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 11, true);
				SetInstructionBit(ref ret, 10, true);
			} else {
				SetInstructionBit(ref ret, 12, source.RslsValue == "b");
				SetInstructionBit(ref ret, 11, false);
				SetInstructionBit(ref ret, 10, false);
			}
			
			// Y = 0
			SetInstructionBit(ref ret, 9, false);
			SetInstructionBit(ref ret, 8, true);
			
			// Out = lhs
			if (target.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (target.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.RslsValue == "a") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}

	private record AluInstruction(int Opcode) : Instruction {
		[Validator]
		protected InstructionSupport Validate(    RegisterAst target,     RegisterAst lhs, RegisterAst rhs)
		protected InstructionSupport Validate(    RegisterAst target,     RegisterAst lhs, ConstantAst rhs)
		protected InstructionSupport Validate(    RegisterAst target,     RegisterAst lhs, StarRegisterAst rhs)
		
		protected InstructionSupport Validate(    RegisterAst target, StarRegisterAst lhs, RegisterAst rhs)
		protected InstructionSupport Validate(    RegisterAst target, StarRegisterAst lhs, ConstantAst rhs)
		protected InstructionSupport Validate(    RegisterAst target, StarRegisterAst lhs, StarRegisterAst rhs)
		
		protected InstructionSupport Validate(StarRegisterAst target,     RegisterAst lhs, RegisterAst rhs)
		protected InstructionSupport Validate(StarRegisterAst target,     RegisterAst lhs, ConstantAst rhs)
		protected InstructionSupport Validate(StarRegisterAst target,     RegisterAst lhs, StarRegisterAst rhs)
		
		protected InstructionSupport Validate(StarRegisterAst target, StarRegisterAst lhs, RegisterAst rhs)
		protected InstructionSupport Validate(StarRegisterAst target, StarRegisterAst lhs, ConstantAst rhs)
		protected InstructionSupport Validate(StarRegisterAst target, StarRegisterAst lhs, StarRegisterAst rhs) {
			if (!OneDistinctStarRegister(out _, target, lhs, rhs)) {
				return InstructionSupport.OtherError;
			}

			if (lhs.Type == IAT.Constant && lhs.ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			if (rhs.Type == IAT.Constant && rhs.ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			if (lhs.Type == IAT.Register && rhs.Type == IAT.Register) {
				if (lhs.RslsValue == rhs.RslsValue) {
					return InstructionSupport.OtherError;
				}
			}

			return InstructionSupport.Supported;
		}

		[Converter]
		protected ushort Convert(StarRegisterAst target, StarRegisterAst lhs, StarRegisterAst rhs) {
			ushort ret = 0;
			
			SetInstructionBit(ref ret, 15, true);
			OneDistinctStarRegister(out string? ax, target, lhs, rhs);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = lhs
			if (lhs.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 11, true);
				SetInstructionBit(ref ret, 10, true);
			} else if (lhs.Type == IAT.Register) {
				SetInstructionBit(ref ret, 12, lhs.RslsValue == "b");
				SetInstructionBit(ref ret, 11, false);
				SetInstructionBit(ref ret, 10, false);
			} else if (lhs.Type == IAT.Constant) {
				if (lhs.ConstantValue!.Value == 0) {
					SetInstructionBit(ref ret, 11, false);
					SetInstructionBit(ref ret, 10, true);
				} else {
					SetInstructionBit(ref ret, 11, true);
					SetInstructionBit(ref ret, 10, false);
				}
			}
			
			// Y = rhs
			if (rhs.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 9, true);
				SetInstructionBit(ref ret, 8, true);
			} else if (rhs.Type == IAT.Register) {
				SetInstructionBit(ref ret, 9, false);
				SetInstructionBit(ref ret, 8, false);
			} else if (rhs.Type == IAT.Constant) {
				if (rhs.ConstantValue!.Value == 0) {
					SetInstructionBit(ref ret, 9, false);
					SetInstructionBit(ref ret, 8, true);
				} else {
					SetInstructionBit(ref ret, 9, true);
					SetInstructionBit(ref ret, 8, false);
				}
			}

			// Out bits
			if (target.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (target.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.RslsValue == "a") {
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
		[Validator]
		protected InstructionSupport Validate(    RegisterAst target, ConstantAst operand)
		protected InstructionSupport Validate(    RegisterAst target, StarRegisterAst operand)
		protected InstructionSupport Validate(    RegisterAst target, RegisterAst operand)
		protected InstructionSupport Validate(StarRegisterAst target, ConstantAst operand)
		protected InstructionSupport Validate(StarRegisterAst target, StarRegisterAst operand)
		protected InstructionSupport Validate(StarRegisterAst target, RegisterAst operand) {
			if (!OneDistinctStarRegister(out _, target, operand)) {
				return InstructionSupport.OtherError;
			}

			if (operand.Type == IAT.Constant && operand.ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			return InstructionSupport.Supported;
		}

		[Converter]
		protected ushort Convert(    RegisterAst target, ConstantAst operand)
		protected ushort Convert(    RegisterAst target, StarRegisterAst operand)
		protected ushort Convert(    RegisterAst target, RegisterAst operand)
		protected ushort Convert(StarRegisterAst target, ConstantAst operand)
		protected ushort Convert(StarRegisterAst target, StarRegisterAst operand)
		protected ushort Convert(StarRegisterAst target, RegisterAst operand) {
			ushort ret = 0;
			
			OneDistinctStarRegister(out string? ax, target, operand);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = operand
			if (operand.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 11, true);
				SetInstructionBit(ref ret, 10, true);
			} else if (operand.Type == IAT.Register) {
				SetInstructionBit(ref ret, 12, operand.RslsValue == "b");
				SetInstructionBit(ref ret, 11, false);
				SetInstructionBit(ref ret, 10, false);
			} else if (operand.Type == IAT.Constant) {
				if (operand.ConstantValue!.Value == 0) {
					SetInstructionBit(ref ret, 11, false);
					SetInstructionBit(ref ret, 10, true);
				} else {
					SetInstructionBit(ref ret, 11, true);
					SetInstructionBit(ref ret, 10, false);
				}
			}
			
			// Out bits
			if (target.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (target.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.RslsValue == "b") {
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
			if (target.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 14, target.RslsValue == "b");
				
				SetInstructionBit(ref ret, 2, true);
			} else if (target.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.RslsValue == "b") {
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
			if (target.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 14, target.RslsValue == "b");
				
				SetInstructionBit(ref ret, 2, true);
			} else if (target.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (target.RslsValue == "b") {
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
