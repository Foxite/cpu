using Assembler.Assembly;

namespace Assembler.Tests.Assembly; 

public class CpuRegisterUtilTests {
	private static object[][] CpuRegisterIsRamRegisterTestCases() => new[] {
		new object[] { CpuRegister.A, false },
		new object[] { CpuRegister.B, false },
		new object[] { CpuRegister.StarA, true },
		new object[] { CpuRegister.StarB, true },
	};
		
	[Test]
	[TestCaseSource(nameof(CpuRegisterIsRamRegisterTestCases))]
	public void TestCpuRegisterIsRamRegister(CpuRegister register, bool expectedResult) {
		Assert.That(register.CpuRegisterIsRamRegister(), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] CheckAllRamRegistersAreSameAddressTestCases() => new[] {
		new object[] { Array.Empty<CpuRegister>(), true },
		new object[] { new[] { CpuRegister.A }, true },
		new object[] { new[] { CpuRegister.B }, true },
		new object[] { new[] { CpuRegister.StarA }, true },
		new object[] { new[] { CpuRegister.StarB }, true },
		
		new object[] { new[] { CpuRegister.A, CpuRegister.B }, true },
		new object[] { new[] { CpuRegister.B, CpuRegister.B }, true },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.StarA }, true },
		new object[] { new[] { CpuRegister.StarB, CpuRegister.StarB }, true },
		new object[] { new[] { CpuRegister.A, CpuRegister.StarA }, true },
		new object[] { new[] { CpuRegister.A, CpuRegister.StarB }, true },
		new object[] { new[] { CpuRegister.B, CpuRegister.StarA }, true },
		new object[] { new[] { CpuRegister.B, CpuRegister.StarB }, true },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.A }, true },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.B }, true },
		new object[] { new[] { CpuRegister.StarB, CpuRegister.A }, true },
		new object[] { new[] { CpuRegister.StarB, CpuRegister.B }, true },
		
		new object[] { new[] { CpuRegister.B, CpuRegister.B, CpuRegister.StarB }, true },
		new object[] { new[] { CpuRegister.A, CpuRegister.B, CpuRegister.StarB }, true },
		new object[] { new[] { CpuRegister.A, CpuRegister.B, CpuRegister.StarA }, true },
		
		new object[] { new[] { CpuRegister.StarA, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.A, CpuRegister.StarA, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.B, CpuRegister.StarA, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.B, CpuRegister.StarA, CpuRegister.StarA, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.B, CpuRegister.StarA, CpuRegister.StarA, CpuRegister.StarB }, false },
		
		new object[] { new[] { CpuRegister.StarA, CpuRegister.A, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.B, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.B, CpuRegister.StarA, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.B, CpuRegister.StarA, CpuRegister.StarB }, false },
		
		new object[] { new[] { CpuRegister.StarA, CpuRegister.StarB, CpuRegister.A }, false },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.StarB, CpuRegister.B }, false },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.StarA, CpuRegister.B, CpuRegister.StarB }, false },
		new object[] { new[] { CpuRegister.StarA, CpuRegister.StarA, CpuRegister.B, CpuRegister.StarB }, false },
	};
		
	[Test]
	[TestCaseSource(nameof(CheckAllRamRegistersAreSameAddressTestCases))]
	public void TestCheckAllRamRegistersAreSameAddress(CpuRegister[] registers, bool expectedResult) {
		Assert.That(AssemblyUtil.CheckAllRamRegistersAreSameAddress(registers), Is.EqualTo(expectedResult));
	}
	
	
	private static object[][] SetBitTestCases() => new[] {
		new object[] { 0b000, 0, false, 0b000 },
		new object[] { 0b001, 0, false, 0b000 },
		new object[] { 0b010, 0, false, 0b010 },
		new object[] { 0b011, 0, false, 0b010 },
		new object[] { 0b100, 0, false, 0b100 },
		new object[] { 0b101, 0, false, 0b100 },
		new object[] { 0b110, 0, false, 0b110 },
		new object[] { 0b111, 0, false, 0b110 },
		
		new object[] { 0b000, 0, true,  0b001 },
		new object[] { 0b001, 0, true,  0b001 },
		new object[] { 0b010, 0, true,  0b011 },
		new object[] { 0b011, 0, true,  0b011 },
		new object[] { 0b100, 0, true,  0b101 },
		new object[] { 0b101, 0, true,  0b101 },
		new object[] { 0b110, 0, true,  0b111 },
		new object[] { 0b111, 0, true,  0b111 },


		new object[] { 0b000, 1, false, 0b000 },
		new object[] { 0b001, 1, false, 0b001 },
		new object[] { 0b010, 1, false, 0b000 },
		new object[] { 0b011, 1, false, 0b001 },
		new object[] { 0b100, 1, false, 0b100 },
		new object[] { 0b101, 1, false, 0b101 },
		new object[] { 0b110, 1, false, 0b100 },
		new object[] { 0b111, 1, false, 0b101 },
		
		new object[] { 0b000, 1, true,  0b010 },
		new object[] { 0b001, 1, true,  0b011 },
		new object[] { 0b010, 1, true,  0b010 },
		new object[] { 0b011, 1, true,  0b011 },
		new object[] { 0b100, 1, true,  0b110 },
		new object[] { 0b101, 1, true,  0b111 },
		new object[] { 0b110, 1, true,  0b110 },
		new object[] { 0b111, 1, true,  0b111 },


		new object[] { 0b000, 2, false, 0b000 },
		new object[] { 0b001, 2, false, 0b001 },
		new object[] { 0b010, 2, false, 0b010 },
		new object[] { 0b011, 2, false, 0b011 },
		new object[] { 0b100, 2, false, 0b000 },
		new object[] { 0b101, 2, false, 0b001 },
		new object[] { 0b110, 2, false, 0b010 },
		new object[] { 0b111, 2, false, 0b011 },
		
		new object[] { 0b000, 2, true,  0b100 },
		new object[] { 0b001, 2, true,  0b101 },
		new object[] { 0b010, 2, true,  0b110 },
		new object[] { 0b011, 2, true,  0b111 },
		new object[] { 0b100, 2, true,  0b100 },
		new object[] { 0b101, 2, true,  0b101 },
		new object[] { 0b110, 2, true,  0b110 },
		new object[] { 0b111, 2, true,  0b111 },
	};
		
	[Test]
	[TestCaseSource(nameof(SetBitTestCases))]
	public void TestSetBit(int input, int bit, bool value, int expectedResult) {
		Assert.That(AssemblyUtil.SetBit(input, bit, value), Is.EqualTo(expectedResult));
	}
}
