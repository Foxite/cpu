using Assembler.Assembly.Proc16a;
using Assembler.Parsing.Proc16a;

namespace Assembler.Assembly; 

public class Proc16aProgramAssembler : ProgramAssembler<ProgramAst> {
	public override string ArchitectureName => "Proc16a";
	
	public override IEnumerable<ushort> Assemble(ProgramAst program) {
		var symbolDefinitions = new Dictionary<string, short>();
		var unsupportedStatements = new Stack<(IStatement statement, int index)>();

		for (int i = 0; i < program.Statements.Count; i++) {
			ProgramStatementAst statement = program.Statements[i];
			
			if (statement.Label != null) {
				symbolDefinitions[statement.Label] = (short) i;
			}

			if (!ValidateStatement(statement.Statement)) {
				unsupportedStatements.Push((statement.Statement, i));
			}
		}

		if (unsupportedStatements.Count > 0) {
			throw new UnsupportedStatementException(ArchitectureName, unsupportedStatements);
		}

		return ConvertStatements(program, symbolDefinitions);
	}

	private IEnumerable<ushort> ConvertStatements(ProgramAst program, Dictionary<string, short> symbolDefinitions) {
		foreach (ProgramStatementAst statement in program.Statements) {
			yield return ConvertStatement(statement.Statement, name => symbolDefinitions[name]);
		}
	}

	protected internal bool ValidateStatement(IStatement statement) {
		switch (statement) {
			case AluInstruction aluInstruction:
				IEnumerable<CpuRegisterAst> registers = aluInstruction.Write.Registers;

				if (aluInstruction.XOperand.IsRegister) {
					registers = registers.Append(aluInstruction.XOperand.Register!);
				} else if (aluInstruction.XOperand.Value!.Value is not (0 or 1)) {
					return false;
				}

				if (aluInstruction.YOperand != null) {
					if (aluInstruction.YOperand.IsRegister) {
						registers = registers.Append(aluInstruction.YOperand.Register!);
					} else if (aluInstruction.YOperand.Value!.Value is not (0 or 1)) {
						return false;
					}
				}

				if (!AssemblyUtil.CheckAllRamRegistersAreSameAddress(registers)) {
					return false;
				}

				if (aluInstruction.XOperand.IsRegister && aluInstruction.XOperand.Register!.CpuRegisterIsCpuRegister() &&
				    aluInstruction.YOperand != null && aluInstruction.YOperand.IsRegister && aluInstruction.YOperand.Register!.CpuRegisterIsCpuRegister() &&
				    aluInstruction.XOperand.Register == aluInstruction.YOperand.Register) {
					return false;
				}

				// todo unit test this condition
				if (aluInstruction.Operation.Operation == AluOperation.BitwiseNot && aluInstruction.YOperand != null) {
					return false;
				}

				return true;
			case AssignInstruction assignInstruction:
				return AssemblyUtil.CheckAllRamRegistersAreSameAddress(assignInstruction.Read.Register, assignInstruction.Write.Register);
			case DataWordInstruction dataWordInstruction:
				if (!dataWordInstruction.IsConstant) {
					// symbols can only be written to A
					return dataWordInstruction.Register.Register == CpuRegister.A;
				}

				// Any value can be written to A.
				// Other registers can be used but only if the value is between -2 and 2, and we use the ALU to do it:
				// 0: 0+0
				// 1: 0+1
				// 2: 1+1
				// -1: ~0
				// -2: ~1
				if (dataWordInstruction.Register.Register != CpuRegister.A && dataWordInstruction.Value!.Value is not (>= -2 and <= 2)) {
					return false;
				}

				if (dataWordInstruction.Value!.Value < -2) {
					return false;
				}

				if (dataWordInstruction.Value!.Value > 0x7FFF) {
					return false;
				}
				
				return true;
			case JumpInstruction jumpInstruction:
				if (jumpInstruction.TargetRegister.Register.CpuRegisterIsRamRegister()) {
					return false;
				}

				if (jumpInstruction.IsCondition) {
					if (jumpInstruction.Condition!.Right.IsRegister) {
						return false;
					}

					if (jumpInstruction.Condition!.Right.Value!.Value != 0) {
						return false;
					}

					if (!jumpInstruction.Condition!.Left.IsRegister) {
						return false;
					}

					if (jumpInstruction.Condition.Left.Register!.CpuRegisterIsRamRegister()) {
						return false;
					}

					if (jumpInstruction.Condition.Left.Register!.Register == jumpInstruction.TargetRegister.Register) {
						return false;
					}
				}

				return true;
			default:
				throw new NotImplementedException(statement.GetType().FullName);
		}
	}

	protected internal ushort ConvertStatement(IStatement statement, Func<string, short> getSymbolValue) {
		int instruction = 0;

		void SetInstructionBit(int bit, bool value) {
			instruction = AssemblyUtil.SetBit(instruction, bit, value);
		}

		if (statement is AssignInstruction assignInstruction) {
			statement = new AluInstruction(new AluWriteTarget(assignInstruction.Write), new AluOperand(assignInstruction.Read.Register), new AluOperand((long) 0), AluOperation.Add);
		}

		{
			if (statement is DataWordInstruction { IsConstant: true, Value: { Value: -2 or -1 } val } dwi) {
				statement = val.Value switch {
					-1 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand((long) 0), new AluOperand(1), AluOperation.Subtract),
					-2 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand(1), null, AluOperation.BitwiseNot),
					_ => throw new NotImplementedException("This should literally never happen"),
				};
			}
		}

		{
			if (statement is DataWordInstruction { IsConstant: true, Register: { Register: not CpuRegister.A }, Value: { Value: >= -2 and <= 2 } val } dwi) {
				statement = val.Value switch {
					 2 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand(1), new AluOperand(1), AluOperation.Add),
					 1 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand(1), new AluOperand((long) 0), AluOperation.Add), 
					 0 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand((long) 0), new AluOperand((long) 0), AluOperation.Add),
					-1 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand((long) 0), new AluOperand(1), AluOperation.Subtract),
					-2 => new AluInstruction(new AluWriteTarget(dwi.Register.Register), new AluOperand(1), null, AluOperation.BitwiseNot),
					_ => throw new NotImplementedException("This should literally never happen"),
				};
			}
		}

		switch (statement) {
			case AluInstruction aluInstruction:
				SetInstructionBit(15, true);
				SetInstructionBit(13, false);

				CpuRegister? starRegister = aluInstruction.Write.Registers.FirstOrDefault(reg => reg.CpuRegisterIsRamRegister())?.Register;
				if (!starRegister.HasValue && aluInstruction.XOperand.IsRegister && aluInstruction.XOperand.Register!.CpuRegisterIsRamRegister()) {
					starRegister = aluInstruction.XOperand.Register!.Register;
				}
				if (!starRegister.HasValue && aluInstruction.YOperand != null && aluInstruction.YOperand.IsRegister && aluInstruction.YOperand.Register!.CpuRegisterIsRamRegister()) {
					starRegister = aluInstruction.XOperand.Register!.Register;
				}

				SetInstructionBit(14, starRegister == CpuRegister.StarB);

				if (aluInstruction.XOperand.IsRegister && aluInstruction.XOperand.Register!.CpuRegisterIsCpuRegister()) {
					SetInstructionBit(12, aluInstruction.XOperand.Register!.Register == CpuRegister.B);
				} else if (aluInstruction.YOperand != null && aluInstruction.YOperand.IsRegister && aluInstruction.YOperand.Register!.CpuRegisterIsCpuRegister()) {
					SetInstructionBit(12, aluInstruction.YOperand.Register!.Register == CpuRegister.A);
				}

				if (aluInstruction.XOperand.IsRegister) {
					if (aluInstruction.XOperand.Register!.Register is CpuRegister.A or CpuRegister.B) {
						SetInstructionBit(11, false);
						SetInstructionBit(10, false);
					} else {
						SetInstructionBit(11, true);
						SetInstructionBit(10, true);
					}
				} else {
					if (aluInstruction.XOperand.Value!.Value == 0) {
						SetInstructionBit(11, false);
						SetInstructionBit(10, true);
					} else {
						SetInstructionBit(11, true);
						SetInstructionBit(10, false);
					}
				}

				if (aluInstruction.YOperand != null) {
					if (aluInstruction.YOperand.IsRegister) {
						if (aluInstruction.YOperand.Register!.Register is CpuRegister.A or CpuRegister.B) {
							SetInstructionBit(9, false);
							SetInstructionBit(8, false);
						} else {
							SetInstructionBit(9, true);
							SetInstructionBit(8, true);
						}
					} else {
						if (aluInstruction.YOperand.Value!.Value == 0) {
							SetInstructionBit(9, false);
							SetInstructionBit(8, true);
						} else {
							SetInstructionBit(9, true);
							SetInstructionBit(8, false);
						}
					}
				}

				int opcode = aluInstruction.Operation.Operation switch {
					AluOperation.Add					=> 0b00000,
					AluOperation.Subtract				=> 0b00001,
					AluOperation.Multiply				=> 0b00010,
					AluOperation.Divide					=> 0b00011,
					AluOperation.ShiftLeft				=> 0b00100,
					AluOperation.ShiftRight				=> 0b00101,
					
					AluOperation.BitwiseAnd				=> 0b10000,
					AluOperation.BitwiseOr				=> 0b10001,
					AluOperation.BitwiseNot				=> 0b10010,
					AluOperation.BitwiseXor				=> 0b10100,
					AluOperation.BitwiseXnor			=> 0b10101,
					AluOperation.BitwiseNor				=> 0b10110,
					AluOperation.BitwiseNand			=> 0b10111,
					
					AluOperation.False					=> 0b11000,
					AluOperation.GreaterThan			=> 0b11001,
					AluOperation.Equals					=> 0b11010,
					AluOperation.GreaterThanOrEquals	=> 0b11011,
					AluOperation.LessThan				=> 0b11100,
					AluOperation.NotEquals				=> 0b11101,
					AluOperation.LessThanOrEquals		=> 0b11110,
					AluOperation.True					=> 0b11111,
					
					_ => throw new ArgumentOutOfRangeException("Unrecognized operation " + aluInstruction.Operation),
				};

				const int opcodeMask = 0b11111000;
				instruction = (instruction & ~opcodeMask) | (opcode << 3);
				
				foreach (CpuRegisterAst cpuRegisterAst in aluInstruction.Write.Registers) {
					var register = cpuRegisterAst.Register;
					SetInstructionBit(register switch {
						CpuRegister.A => 0,
						CpuRegister.B => 1,
						CpuRegister.StarA => 2,
						CpuRegister.StarB => 2,
						_ => throw new ArgumentOutOfRangeException("Unrecognized register " + register),
					}, true);
				}

				break;
			case DataWordInstruction dataWordInstruction:
				ushort word;
				if (dataWordInstruction.IsConstant) {
					long astValue = dataWordInstruction.Value!.Value;
					
					byte[] astValueBytes = BitConverter.GetBytes(astValue);
					
					// Ignore sign of ast value.

					byte msbyte;
					byte lsbyte;

					if (BitConverter.IsLittleEndian) {
						msbyte = astValueBytes[1];
						lsbyte = astValueBytes[0];
					} else {
						msbyte = astValueBytes[^2];
						lsbyte = astValueBytes[^1];
					}

					word = (ushort) (msbyte << 8 | lsbyte);
				} else {
					word = (ushort) getSymbolValue(dataWordInstruction.Symbol!.Name);
				}

				instruction = word;
				
				SetInstructionBit(15, false);
				
				break;
			case JumpInstruction jumpInstruction:
				SetInstructionBit(15, true);
				SetInstructionBit(13, true);
				SetInstructionBit(11, false);
				
				SetInstructionBit(3, jumpInstruction.TargetRegister.Register == CpuRegister.A);

				if (jumpInstruction.IsCondition) {
					int compareOpcode = jumpInstruction.Condition!.CompareOperation.Operation switch {
						CompareOperation.GreaterThan			=> 0b001,
						CompareOperation.Equals					=> 0b010,
						CompareOperation.GreaterThanOrEquals	=> 0b011,
						CompareOperation.LessThan				=> 0b100,
						CompareOperation.NotEquals				=> 0b101,
						CompareOperation.LessThanOrEquals		=> 0b110,
						_ => throw new ArgumentOutOfRangeException("Unrecognized compare operation " + jumpInstruction.Condition!.CompareOperation),
					};
					
					const int compareMask = 0b111;
					instruction = (instruction & ~compareMask) | compareOpcode;
				} else {
					SetInstructionBit(2, jumpInstruction.Value!.Value);
					SetInstructionBit(1, jumpInstruction.Value!.Value);
					SetInstructionBit(0, jumpInstruction.Value!.Value);
				}
				
				break;
			default:
				throw new NotImplementedException(statement.GetType().FullName);
		}

		return (ushort) instruction;
	}
}
