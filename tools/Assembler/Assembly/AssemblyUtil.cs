using Assembler.Ast;

namespace Assembler.Assembly;

public static class AssemblyUtil {
	public static ushort SetBit(ushort instruction, int bit, bool value) {
		if (value) {
			return (ushort) (instruction | (1 << bit));
		} else {
			return (ushort) (instruction & (~(1 << bit)));
		}
	}
}
