using Assembler.Ast;
using IAT = Assembler.Ast.InstructionArgumentType;

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
	
	private static bool OneDistinctStarRegister(IReadOnlyList<InstructionArgumentAst> args, out string? register) {
		var distinctStars = args.Where(arg => arg.Type == IAT.StarRegister).Select(arg => arg.RslsValue!).Distinct().ToList();
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

	private abstract record Proc16aInstruction() : Instruction {
		// TODO Unit test
		protected bool ValidateArgumentTypes(IReadOnlyList<InstructionArgumentAst> arguments, params InstructionArgumentType[][] types) {
			return types.Any(overload => overload.SequenceEqual(arguments.Select(argument => argument.Type)));
		}
	}
		
	private record ldiInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) {
			if (!ValidateArgumentTypes(args,
				new[] { IAT.Register, IAT.Constant },
				new[] { IAT.StarRegister, IAT.Constant }
			)) {
				return InstructionSupport.ParameterType;
			}

			if (args[0].Type == IAT.Register && args[0].RslsValue == "a") {
				return args[1].ConstantValue!.Value is >= -2 and <= 0x7FFF ? InstructionSupport.Supported : InstructionSupport.OtherError;
			} else {
				return args[1].ConstantValue!.Value is >= -2 and <= 2 ? InstructionSupport.Supported : InstructionSupport.OtherError;
			}
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			if (args[0].Type == IAT.Register && args[0].RslsValue == "a" && args[1].ConstantValue!.Value >= 0) {
				return (ushort) args[1].ConstantValue!.Value;
			} else {
				ushort ret = 0;
				SetInstructionBit(ref ret, 15, true);
				
				ushort opcode = args[1].ConstantValue!.Value switch {
					-2 => 0b10_00_10010, // ~1
					-1 => 0b01_10_00001, // 0 - 1
					0  => 0b01_01_00000, // 0 + 0
					1  => 0b10_01_00000, // 1 + 0
					2  => 0b10_10_00000, // 1 + 1
				};

				ret |= (ushort) (opcode << 3);
				
				var outRegister = args[0];
				if (outRegister.Type == IAT.StarRegister) {
					SetInstructionBit(ref ret, 14, outRegister.RslsValue == "b");
					SetInstructionBit(ref ret, 2, true);
				} else if (outRegister.RslsValue == "b") {
					SetInstructionBit(ref ret, 1, true);
				} else if (outRegister.RslsValue == "a") {
					SetInstructionBit(ref ret, 0, true);
				}

				return ret;
			}
		}
	}

	private record brkInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) => args.Count == 0 ? InstructionSupport.Supported : InstructionSupport.ParameterType;
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => 1;
	}

	private record nopInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) => args.Count == 0 ? InstructionSupport.Supported : InstructionSupport.ParameterType;
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			// none = 0 + 0
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			return ret;
		}
	}

	private record movInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) => ValidateArgumentTypes(args,
			new[] { IAT.Register, IAT.Register },
			new[] { IAT.Register, IAT.StarRegister },
			new[] { IAT.StarRegister, IAT.Register }
		) ? InstructionSupport.Supported : InstructionSupport.ParameterType;

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			// lhs = rhs + 0
			ushort ret = 0;
			
			// ALU operation
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			
			// Ax select
			OneDistinctStarRegister(args, out string? ax);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = rhs
			InstructionArgumentAst inRegister = args[1];
			if (inRegister.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 11, true);
				SetInstructionBit(ref ret, 10, true);
			} else {
				SetInstructionBit(ref ret, 12, inRegister.RslsValue == "b");
				SetInstructionBit(ref ret, 11, false);
				SetInstructionBit(ref ret, 10, false);
			}
			
			// Y = 0
			SetInstructionBit(ref ret, 9, false);
			SetInstructionBit(ref ret, 8, true);
			
			// Out = lhs
			var outRegister = args[0];
			if (outRegister.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "a") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}

	private record AluInstruction(int Opcode) : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) {
			if (!ValidateArgumentTypes(args,
			    new[] { IAT.Register, IAT.Register, IAT.Register },
			    new[] { IAT.Register, IAT.Register, IAT.StarRegister },
			    new[] { IAT.Register, IAT.Register, IAT.Constant },
			    new[] { IAT.Register, IAT.StarRegister, IAT.Register },
			    new[] { IAT.Register, IAT.StarRegister, IAT.StarRegister },
			    new[] { IAT.Register, IAT.StarRegister, IAT.Constant },
			    new[] { IAT.Register, IAT.Constant, IAT.Register },
			    new[] { IAT.Register, IAT.Constant, IAT.StarRegister },
			    new[] { IAT.Register, IAT.Constant, IAT.Constant },
			    
			    new[] { IAT.StarRegister, IAT.Register, IAT.Register },
			    new[] { IAT.StarRegister, IAT.Register, IAT.StarRegister },
			    new[] { IAT.StarRegister, IAT.Register, IAT.Constant },
			    new[] { IAT.StarRegister, IAT.StarRegister, IAT.Register },
			    new[] { IAT.StarRegister, IAT.StarRegister, IAT.StarRegister },
			    new[] { IAT.StarRegister, IAT.StarRegister, IAT.Constant },
			    new[] { IAT.StarRegister, IAT.Constant, IAT.Register },
			    new[] { IAT.StarRegister, IAT.Constant, IAT.StarRegister },
			    new[] { IAT.StarRegister, IAT.Constant, IAT.Constant }
		    )) {
				return InstructionSupport.ParameterType;
			}

			if (!OneDistinctStarRegister(args, out _)) {
				return InstructionSupport.OtherError;
			}

			if (args[1].Type == IAT.Constant && args[1].ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			if (args[2].Type == IAT.Constant && args[2].ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			if (args[1].Type == IAT.Register && args[2].Type == IAT.Register) {
				if (args[1].RslsValue == args[2].RslsValue) {
					return InstructionSupport.OtherError;
				}
			}

			return InstructionSupport.Supported;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			
			SetInstructionBit(ref ret, 15, true);
			OneDistinctStarRegister(args, out string? ax);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = lhs
			InstructionArgumentAst lhs = args[1];
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
			InstructionArgumentAst rhs = args[2];
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
			var outRegister = args[0];
			if (outRegister.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "a") {
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


	private record notInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) {
			if (!ValidateArgumentTypes(args, new[] { IAT.Register, IAT.Register }, new[] { IAT.Register, IAT.StarRegister }, new[] { IAT.Register, IAT.Constant })) {
				return InstructionSupport.ParameterType;
			}

			if (!OneDistinctStarRegister(args, out _)) {
				return InstructionSupport.OtherError;
			}

			if (args[1].Type == IAT.Constant && args[1].ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			if (args[2].Type == IAT.Constant && args[2].ConstantValue!.Value is not (0 or 1)) {
				return InstructionSupport.OtherError;
			}

			return InstructionSupport.Supported;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			
			OneDistinctStarRegister(args, out string? ax);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = operand
			InstructionArgumentAst lhs = args[1];
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
			
			// Out bits
			var outRegister = args[0];
			if (outRegister.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 0, true);
			}

			ret |= 0b10010 << 3;

			return ret;
		}
	}


	private record trueInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) => ValidateArgumentTypes(args, new[] { IAT.Register }) ? InstructionSupport.Supported : InstructionSupport.ParameterType;

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			ret |= 0b11111 << 3;
			
			// Out bits
			var outRegister = args[0];
			if (outRegister.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 14, outRegister.RslsValue == "b");
				
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}
	
	private record falseInstruction() : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) => ValidateArgumentTypes(args, new[] { IAT.Register }) ? InstructionSupport.Supported : InstructionSupport.ParameterType;

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, false);
			ret |= 0b11000 << 3;
			
			// Out bits
			var outRegister = args[0];
			if (outRegister.Type == IAT.StarRegister) {
				SetInstructionBit(ref ret, 14, outRegister.RslsValue == "b");
				
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}
	
	
	

	private record JumpInstruction(int CompareMode) : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) {
			if (!ValidateArgumentTypes(args, new[] { IAT.Register, IAT.Register, IAT.Constant })) {
				return InstructionSupport.ParameterType;
			}

			if (!OneDistinctStarRegister(args, out _)) {
				return InstructionSupport.OtherError;
			}

			return args[0].RslsValue != args[1].RslsValue && args[2].ConstantValue!.Value == 0 ? InstructionSupport.Supported : InstructionSupport.OtherError;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, true);
			SetInstructionBit(ref ret, 11, false);

			string compareLhsRegister = args[1].RslsValue!;

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
	
	
	private record jmpInstruction : Proc16aInstruction {
		public override InstructionSupport Validate(IReadOnlyList<InstructionArgumentAst> args) => ValidateArgumentTypes(args, new[] { IAT.Register }) ? InstructionSupport.Supported : InstructionSupport.ParameterType;

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			SetInstructionBit(ref ret, 13, true);
			SetInstructionBit(ref ret, 11, false);

			string targetRegister = args[0].RslsValue!;

			SetInstructionBit(ref ret, 3, targetRegister == "a");
			
			ret |= 0b111;

			return ret;
		}
	}
}
