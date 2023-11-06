using Assembler.Parsing.ProcAssemblyV2;

namespace Assembler.Assembly;

public static class AssemblyUtil {
	public static int SetBit(int instruction, int bit, bool value) {
		if (value) {
			return instruction | (1 << bit);
		} else {
			return instruction & (~(1 << bit));
		}
	}
}
