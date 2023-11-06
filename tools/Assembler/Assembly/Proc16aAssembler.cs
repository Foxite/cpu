using Assembler.Parsing.ProcAssemblyV2;
using IAT = Assembler.Parsing.ProcAssemblyV2.InstructionArgumentType;

namespace Assembler.Assembly;

public class Proc16aAssembler : InstructionMapProgramAssembler {
	public Proc16aAssembler(string architectureName) : base(architectureName) {
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
		var distinctStars = args.Where(arg => arg.Type == InstructionArgumentType.StarRegister).Select(arg => arg.RslsValue!).Distinct().ToList();
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
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) {
			if (!ValidateArgumentTypes(args, new[] { IAT.Register, IAT.Constant })) {
				return false;
			}

			return args[0].RslsValue == "a" && args[1].ConstantValue!.Value is >= 0 and <= 0x7FFF;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			return (ushort) args[0].ConstantValue!.Value;
		}
	}

	private record brkInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => args.Count == 0;
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => 1;
	}

	private record nopInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => args.Count == 0;
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			// none = 0 + 0
			ushort ret = 0;
			SetInstructionBit(ref ret, 15, true);
			return ret;
		}
	}

	private record movInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => ValidateArgumentTypes(args,
			new[] { IAT.Register, IAT.Register },
			new[] { IAT.Register, IAT.StarRegister },
			new[] { IAT.StarRegister, IAT.Register },
			new[] { IAT.StarRegister, IAT.StarRegister }
		);

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
			if (inRegister.Type == InstructionArgumentType.StarRegister) {
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
			if (outRegister.Type == InstructionArgumentType.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "a") {
				SetInstructionBit(ref ret, 0, true);
			}

			return ret;
		}
	}

	private record AluInstruction(int Opcode) : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) {
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
				return false;
			}

			if (!OneDistinctStarRegister(args, out _)) {
				return false;
			}

			if (args[1].Type == InstructionArgumentType.Constant && args[1].ConstantValue!.Value is not (0 or 1)) {
				return false;
			}

			if (args[2].Type == InstructionArgumentType.Constant && args[2].ConstantValue!.Value is not (0 or 1)) {
				return false;
			}

			return true;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			
			OneDistinctStarRegister(args, out string? ax);
			SetInstructionBit(ref ret, 14, ax == "b");
			
			// X = lhs
			InstructionArgumentAst lhs = args[1];
			if (lhs.Type == InstructionArgumentType.StarRegister) {
				SetInstructionBit(ref ret, 11, true);
				SetInstructionBit(ref ret, 10, true);
			} else if (lhs.Type == InstructionArgumentType.Register) {
				SetInstructionBit(ref ret, 12, lhs.RslsValue == "b");
				SetInstructionBit(ref ret, 11, false);
				SetInstructionBit(ref ret, 10, false);
			} else if (lhs.Type == InstructionArgumentType.Constant) {
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
			if (rhs.Type == InstructionArgumentType.StarRegister) {
				SetInstructionBit(ref ret, 9, true);
				SetInstructionBit(ref ret, 8, true);
			} else if (rhs.Type == InstructionArgumentType.Register) {
				SetInstructionBit(ref ret, 9, false);
				SetInstructionBit(ref ret, 8, false);
			} else if (rhs.Type == InstructionArgumentType.Constant) {
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
			if (outRegister.Type == InstructionArgumentType.StarRegister) {
				SetInstructionBit(ref ret, 2, true);
			} else if (outRegister.RslsValue == "b") {
				SetInstructionBit(ref ret, 1, true);
			} else if (outRegister.RslsValue == "b") {
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
	private record notInstruction()  : AluInstruction(0b10010);
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
	
	
	
	private record trueInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record falseInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	
	

	private record JumpInstruction(int CompareMode) : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) {
			if (!ValidateArgumentTypes(args,
				new[] { IAT.Register, IAT.Register, IAT.Constant }
			)) {
				return false;
			}

			if (!OneDistinctStarRegister(args, out _)) {
				return false;
			}

			return args[2].ConstantValue!.Value == 0;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jmpInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jgtInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jeqInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jgeInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jltInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jneInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
	
	private record jleInstruction() : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) => throw new NotImplementedException();
	}
}
