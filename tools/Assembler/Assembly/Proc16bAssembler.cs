using Assembler.Parsing.ProcAssemblyV2;
using IAT = Assembler.Parsing.ProcAssemblyV2.InstructionArgumentType;

namespace Assembler.Assembly;

public class Proc16bAssembler : InstructionMapProgramAssembler {
	public Proc16bAssembler() : base("Proc16b") {
		AddInstruction("ldc",   new ldcInstruction());
		
		AddInstruction("add",   new aluInstruction(0b00000));
		AddInstruction("sub",   new aluInstruction(0b00001));
		AddInstruction("mul",   new aluInstruction(0b00010));
		AddInstruction("div",   new aluInstruction(0b00011));
		AddInstruction("shr",   new aluInstruction(0b00100));
		AddInstruction("shl",   new aluInstruction(0b00101));
		
		AddInstruction("and",   new aluInstruction(0b10000));
		AddInstruction("or",    new aluInstruction(0b10001));
		AddInstruction("not",   new aluInstruction(0b10010));
		// didn't miss one
		AddInstruction("xor",   new aluInstruction(0b10100));
		AddInstruction("xnor",  new aluInstruction(0b10101));
		AddInstruction("nor",   new aluInstruction(0b10110));
		AddInstruction("nand",  new aluInstruction(0b10111));
		
		AddInstruction("false", new aluInstruction(0b11000));
		AddInstruction("cgt",   new aluInstruction(0b11001));
		AddInstruction("ceq",   new aluInstruction(0b11010));
		AddInstruction("cge",   new aluInstruction(0b11011));
		AddInstruction("clt",   new aluInstruction(0b11100));
		AddInstruction("cne",   new aluInstruction(0b11101));
		AddInstruction("cle",   new aluInstruction(0b11110));
		AddInstruction("true",  new aluInstruction(0b11111));
		
		AddInstruction("jgt",   new jumpInstruction(0b000));
		AddInstruction("jeq",   new jumpInstruction(0b010));
		AddInstruction("jge",   new jumpInstruction(0b011));
		AddInstruction("jlt",   new jumpInstruction(0b100));
		AddInstruction("jne",   new jumpInstruction(0b101));
		AddInstruction("jle",   new jumpInstruction(0b110));
		AddInstruction("jmp",   new jumpInstruction(0b111));
		
		AddInstruction("mov",   new movInstruction());
		AddInstruction("ldb",   new ldbInstruction());
		AddInstruction("stb",   new stbInstruction());
		
		AddInstruction("noop",  new noopInstruction());
		AddInstruction("brk",   new brkInstruction());
	}
	
	private static void SetInstructionBit(ref ushort instruction, int bit, bool value) {
		instruction = AssemblyUtil.SetBit(instruction, bit, value);
	}

	private abstract record Proc16bInstruction(params InstructionArgumentType[] Types) : Instruction {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) {
			return args.Select(arg => arg.Type).SequenceEqual(Types);
		}
	}
	
	private record ldcInstruction() : Proc16bInstruction(IAT.Register, IAT.Constant) {
		public override bool Validate(IReadOnlyList<InstructionArgumentAst> args) {
			return base.Validate(args) && args[1].ConstantValue!.Value < 0x0FFF;
		}

		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			ret |= (ushort) (args[0].RslsValue switch {
				"a" => 0b00,
				"b" => 0b01,
				"c" => 0b10,
				"d" => 0b11,
			} << 12);

			ret |= (ushort) args[1].ConstantValue!.Value;

			return ret;
		}
	}

	private record aluInstruction(ushort Opcode) : Proc16bInstruction(IAT.Register, IAT.Register, IAT.Register) {
		public override ushort Convert(IReadOnlyList<InstructionArgumentAst> args) {
			ushort ret = 0;
			SetInstructionBit(ref ret, 14, true);
			
			ret |= (ushort) (args[0].RslsValue switch {
				"a" => 0b00,
				"b" => 0b01,
				"c" => 0b10,
				"d" => 0b11,
			} << 8);
			
			ret |= (ushort) (args[1].RslsValue switch {
				"a" => 0b00,
				"b" => 0b01,
				"c" => 0b10,
				"d" => 0b11,
			} << 12);
			
			ret |= (ushort) (args[2].RslsValue switch {
				"a" => 0b00,
				"b" => 0b01,
				"c" => 0b10,
				"d" => 0b11,
			} << 10);

			ret |= (ushort) (Opcode << 3);

			return ret;
		}
	}

	private record jumpInstruction(int CompareMode) : Proc16bInstruction(IAT.Register, InstructionArgumentType.Register, IAT.Register) {
		
	}
}
