namespace Emulator.Tests;

public class DecodedProc16aInstructionTests {
	private static object[][] DecodeTestCases() {
		return new[] {
			// Data words
			new object[] {
				(ushort) 0x0024,
				new DecodedProc16aInstruction(
					Dsel: 0,
					WriteA: true
				)
			},
			new object[] {
				(ushort) 0x0000,
				new DecodedProc16aInstruction(
					Dsel: 0,
					WriteA: true
				)
			},
			new object[] {
				(ushort) 0x0001,
				new DecodedProc16aInstruction(
					Dsel: 0,
					WriteA: true
				)
			},
			new object[] {
				(ushort) 0x7FFF,
				new DecodedProc16aInstruction(
					Dsel: 0,
					WriteA: true
				)
			},
			
			// Alu
			new object[] {
				(ushort) 0b100_0_10_10_00000_010,
				new DecodedProc16aInstruction(
					Dsel: 0b01,
					AxSelect: false,
					AluOpcode: 0b00000,
					AluRsel: false,
					AluXsel: 0b10,
					AluYsel: 0b10,
					WriteA: false,
					WriteB: true,
					WriteStarAx: false
				)
			},
			new object[] {
				(ushort) 0b110_0_10_10_00000_010,
				new DecodedProc16aInstruction(
					Dsel: 0b01,
					AxSelect: true,
					AluOpcode: 0b00000,
					AluRsel: false,
					AluXsel: 0b10,
					AluYsel: 0b10,
					WriteA: false,
					WriteB: true,
					WriteStarAx: false
				)
			},
			new object[] {
				(ushort) 0b100_0_01_00_00010_001,
				new DecodedProc16aInstruction(
					Dsel: 0b01,
					AxSelect: false,
					AluOpcode: 0b00010,
					AluRsel: false,
					AluXsel: 0b01,
					AluYsel: 0b00,
					WriteA: true,
					WriteB: false,
					WriteStarAx: false
				)
			},
			new object[] {
				(ushort) 0b100_1_00_11_10010_101,
				new DecodedProc16aInstruction(
					Dsel: 0b01,
					AxSelect: false,
					AluOpcode: 0b10010,
					AluRsel: true,
					AluXsel: 0b00,
					AluYsel: 0b11,
					WriteA: true,
					WriteB: false,
					WriteStarAx: true
				)
			},
			new object[] {
				(ushort) 0b110_1_00_11_10010_101,
				new DecodedProc16aInstruction(
					Dsel: 0b01,
					AxSelect: true,
					AluOpcode: 0b10010,
					AluRsel: true,
					AluXsel: 0b00,
					AluYsel: 0b11,
					WriteA: true,
					WriteB: false,
					WriteStarAx: true
				)
			},
			
			// Jump
			new object[] {
				(ushort) 0b10100_0000000_0_001,
				new DecodedProc16aInstruction(
					Jmp: true,
					Dsel: 0b10,
					Cmp: 0b001,
					CmpOperand: false
				)
			},
			new object[] {
				(ushort) 0b10100_0000000_1_010,
				new DecodedProc16aInstruction(
					Jmp: true,
					Dsel: 0b11,
					Cmp: 0b010,
					CmpOperand: true
				)
			},
			new object[] {
				(ushort) 0b10100_0000000_0_110,
				new DecodedProc16aInstruction(
					Jmp: true,
					Dsel: 0b10,
					Cmp: 0b110,
					CmpOperand: false
				)
			},
		};
	}
	
	[Test]
	[TestCaseSource(nameof(DecodeTestCases))]
	public void TestDecode(ushort instruction, DecodedProc16aInstruction expectedResult) {
		Assert.That(DecodedProc16aInstruction.DecodeInstruction(instruction), Is.EqualTo(expectedResult));
	}
}
