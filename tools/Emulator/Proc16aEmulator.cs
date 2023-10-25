namespace Emulator;

public class Proc16aEmulator {
	private readonly ushort[] m_Rom;
	
	public ushort[] Ram { get; }
	public ushort ARegister { get; private set; }
	public ushort BRegister { get; private set; }
	public ushort ProgramCounter { get; private set; }

	public Proc16aEmulator(ICollection<ushort> rom) {
		m_Rom = new ushort[rom.Count];
		rom.CopyTo(m_Rom, 0);

		Ram = new ushort[0x10000];
		ARegister = 0;
		BRegister = 0;
		ProgramCounter = 0;
	}

	private ushort ComputeAlu(byte opcode, short x, short y) {
		return AsUnsignedShort(opcode switch {
			0b00000 => x + y,
			0b00001 => x - y,
			0b00010 => x * y,
			0b00011 => x / y,
			0b00100 => x << y,
			0b00101 => x >> y,
			0b10000 => x & y,
			0b10001 => x | y,
			0b10010 => ~x,
			0b10100 => x ^ y,
			0b10101 => ~(x ^ y),
			0b10110 => ~(x | y),
			0b10111 => ~(x & y),
			
			>= 0b11000 and <= 0b11111 => opcode switch {
				0b11000 => false,
				0b11001 => x >  y,
				0b11010 => x == y,
				0b11011 => x >= y,
				0b11100 => x <  y,
				0b11101 => x != y,
				0b11110 => x <= y,
				0b11111 => true,
			} ? 0xFFFF : 0x0000,
		});
	}
	
	private ushort AsUnsignedShort(int i) {
		return (ushort) ((ushort) (i & 0xFF00) | (ushort) (i & 0x00FF));
	}

	private short AsSigned(ushort value) {
		return BitConverter.ToInt16(BitConverter.GetBytes(value));
	}

	public void Step() {
		ushort instruction = m_Rom[ProgramCounter];

		var decoded = DecodedProc16aInstruction.DecodeInstruction(instruction);

		ushort ax = decoded.AxSelect ? BRegister : ARegister;
		
		ushort aluX = decoded.AluXsel switch {
			0 => decoded.AluRsel ? BRegister : ARegister,
			1 => 0,
			2 => 1,
			3 => Ram[ax],
		};
		ushort aluY = decoded.AluYsel switch {
			0 => decoded.AluRsel ? ARegister : BRegister,
			1 => 0,
			2 => 1,
			3 => Ram[ax],
		};

		ushort alu = ComputeAlu(decoded.AluOpcode, AsSigned(aluX), AsSigned(aluY));

		ushort d = decoded.Dsel switch {
			0 => instruction,
			1 => alu,
			2 => ARegister,
			3 => BRegister,
		};

		short compareValue = AsSigned(decoded.CmpOperand ? BRegister : ARegister);
		bool compareResult = decoded.Cmp switch {
			0b000 => false,
			0b001 => compareValue > 0,
			0b010 => compareValue == 0,
			0b011 => compareValue >= 0,
			0b100 => compareValue < 0,
			0b101 => compareValue != 0,
			0b110 => compareValue <= 0,
			0b111 => true,
		};

		if (compareResult && decoded.Jmp) {
			ProgramCounter = decoded.CmpOperand ? ARegister : BRegister;
		} else {
			checked {
				ProgramCounter++;
			}
		}

		if (decoded.WriteStarAx) {
			Ram[ax] = d;
		}

		if (decoded.WriteA) {
			ARegister = d;
		}

		if (decoded.WriteB) {
			BRegister = d;
		}
	}
}

public readonly record struct DecodedProc16aInstruction(
	bool AxSelect = false,
	bool AluRsel = false,
	byte AluOpcode = 0,
	byte AluXsel = 0,
	byte AluYsel = 0,
	byte Dsel = 0,
	bool WriteStarAx = false,
	bool WriteB = false,
	bool WriteA = false,
	byte Cmp = 0,
	bool CmpOperand = false,
	bool Jmp = false,
	bool Brk = false) {
	
	public static DecodedProc16aInstruction DecodeInstruction(ushort instruction) {
		bool GetBit(int bit) {
			return (instruction & (1 << bit)) != 0;
		}
			
		byte GetBits(params int[] bits) {
			byte ret = 0;
				
			for (int i = 0; i < bits.Length; i++) {
				ret <<= 1;
				
				if (GetBit(bits[i])) {
					ret |= 1;
				}
			}

			return ret;
		}

		if (!GetBit(15)) {
			return new DecodedProc16aInstruction(WriteA: true);
		}

		bool axSelect = GetBit(14);

		if (!GetBit(13)) {
			return new DecodedProc16aInstruction(
				AxSelect: axSelect,
				AluRsel: GetBit(12),
				AluXsel: GetBits(11, 10),
				AluYsel: GetBits(9, 8),
				AluOpcode: GetBits(7, 6, 5, 4, 3),
				Dsel: 0b01,
				WriteStarAx: GetBit(2),
				WriteB: GetBit(1),
				WriteA: GetBit(0)
			);
		}

		if (!GetBit(11)) {
			return new DecodedProc16aInstruction(
				AxSelect: axSelect,
				Dsel: GetBit(3) ? (byte) 0b11 : (byte) 0b10,
				Cmp: GetBits(2, 1, 0),
				CmpOperand: GetBit(3),
				Jmp: true
			);
		}

		if (GetBit(0)) {
			return new DecodedProc16aInstruction(
				AxSelect: axSelect,
				Brk: true
			);
		}

		return new DecodedProc16aInstruction(AxSelect: axSelect);
	}
}
