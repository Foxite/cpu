using Assembler.Parsing.Proc16a;

namespace Assembler.Assembly;

public static class AssemblyUtil {
	public static bool CpuRegisterIsRamRegister(this CpuRegisterAst register) => register.Register.CpuRegisterIsRamRegister();
	public static bool CpuRegisterIsRamRegister(this CpuRegister register) => register is CpuRegister.StarA or CpuRegister.StarB;
	public static bool CpuRegisterIsCpuRegister(this CpuRegisterAst register) => register.Register.CpuRegisterIsCpuRegister();
	public static bool CpuRegisterIsCpuRegister(this CpuRegister register) => register is CpuRegister.A or CpuRegister.B;

	public static bool CheckAllRamRegistersAreSameAddress(params CpuRegisterAst[] registers) => CheckAllRamRegistersAreSameAddress(registers.AsEnumerable());
	public static bool CheckAllRamRegistersAreSameAddress(IEnumerable<CpuRegisterAst> registers) => CheckAllRamRegistersAreSameAddress(registers.Select(register => register.Register));
	public static bool CheckAllRamRegistersAreSameAddress(params CpuRegister[] registers) => CheckAllRamRegistersAreSameAddress(registers.AsEnumerable());
	public static bool CheckAllRamRegistersAreSameAddress(IEnumerable<CpuRegister> registers) {
		CpuRegister? ramRegister = null;

		foreach (CpuRegister register in registers) {
			if (ramRegister == null) {
				if (CpuRegisterIsRamRegister(register)) {
					ramRegister = register;
				}
				continue;
			}
			
			if (CpuRegisterIsRamRegister(register) && register != ramRegister) {
				return false;
			}
		}
		
		return true;
	}
	
	public static int SetBit(int instruction, int bit, bool value) {
		if (value) {
			return instruction | (1 << bit);
		} else {
			return instruction & (~(1 << bit));
		}
	}
}
