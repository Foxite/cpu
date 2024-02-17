using Assembler.Assembly;

namespace Assembler.Tests.Assembly; 

public class CpuRegisterUtilTests {
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
		Assert.That(AssemblyUtil.SetBit((ushort) input, bit, value), Is.EqualTo(expectedResult));
	}
}
