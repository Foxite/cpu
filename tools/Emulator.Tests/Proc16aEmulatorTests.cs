using System.Security.Cryptography;
using NUnit.Framework.Constraints;

namespace Emulator.Tests; 

public class Proc16aEmulatorTests {
	private static void AssertStep(Proc16aEmulator emulator, ushort a, ushort b, ushort pc, IResolveConstraint? ram = null) {
		emulator.Step();
		
		Assert.Multiple(() => {
			Assert.That(emulator.ARegister, Is.EqualTo(a), "A register");
			Assert.That(emulator.BRegister, Is.EqualTo(b), "B register");
			Assert.That(emulator.ProgramCounter, Is.EqualTo(pc), "Program counter");

			if (ram != null) {
				Assert.That(emulator.Ram, ram, "RAM");
			}
		});
	}

	[Test]
	public void TestIncrementProgram() {
		var emulator = new Proc16aEmulator(new ushort[] {
			0x0000,
			0x8A02,
			0x8201,
			0xA001,
		});
		
		AssertStep(emulator, 0, 0, 1);
		AssertStep(emulator, 0, 2, 2);

		for (ushort i = 1; i < 50; i++) {
			AssertStep(emulator, i, 2, 3);
			AssertStep(emulator, i, 2, 2);
		}
	}

	[Test]
	public void TestRamfillProgram() {
		var emulator = new Proc16aEmulator(new ushort[] {
			0x8502,
			0xD104,
			0x9202,
			0x0001,
			0xA00F,
		});
		
		AssertStep(emulator, 0, 0, 1);

		ReusableConstraint initialRamConstraint = Has.ItemAt(0).EqualTo((ushort) 0);
		AssertStep(emulator, 0, 0, 2, initialRamConstraint);
		AssertStep(emulator, 0, 1, 3, initialRamConstraint);
		AssertStep(emulator, 1, 1, 4, initialRamConstraint);
		AssertStep(emulator, 1, 1, 1, initialRamConstraint);


		for (ushort i = 1; i < 50; i++) {
			Constraint ramConstraint = Has.ItemAt(0).EqualTo((ushort) 0);
			for (int j = 1; j < i; j++) {
				ramConstraint = ramConstraint.And.ItemAt(j).EqualTo(j);
			}

			ReusableConstraint reusableRamConstraint = ramConstraint;
			
			AssertStep(emulator, 1, i, 2, reusableRamConstraint);
			AssertStep(emulator, 1, (ushort) (i + 1), 3, reusableRamConstraint);
			AssertStep(emulator, 1, (ushort) (i + 1), 4, reusableRamConstraint);
			AssertStep(emulator, 1, (ushort) (i + 1), 1, reusableRamConstraint);
		}
	}

	[Test]
	public void TestFibonacciProgram() {
		var emulator = new Proc16aEmulator(new ushort[] {
			0x0001,
			0x8904,
			0x8201,
			0x8904,
			0x8102,
			0x0000,
			0x9104,
			0x0000,
			0x8D01,
			0x8D02,
			0x8209,
			0x8D01,
			0x8002,
			0x0000,
			0x8D01,
			0x8201,
			0x9104,
			0x0000,
			0x8E04,
			0x0007,
			0xA00F,
		});

		// Setup
		for (int i = 0; i < 7; i++) {
			emulator.Step();
		}

		void AssertFibonacci(int fibIndex) {
			Assert.That(emulator.Ram, Has.ItemAt(0).EqualTo(fibIndex), "Count of computed fibonacci numbers is unexpected");
			
			int fibA = 1;
			int fibB = 1;
			
			for (int i = 1; i < fibIndex; i++) {
				Assert.That(emulator.Ram, Has.ItemAt(i + 1).EqualTo(fibB), "Previously computed fibonacci number");
				int fibC = fibA + fibB;
				fibA = fibB;
				fibB = fibC;
			}
			
			Assert.That(emulator.Ram, Has.ItemAt(fibIndex).EqualTo(fibA), "Latest computed fibonacci number is incorrect");
		}
		
		AssertFibonacci(2);

		// the 24th fibonacci number is the highest that can be stored in a uint16
		for (int fibIndex = 3; fibIndex < 24; fibIndex++) {
			for (int i = 0; i < 14; i++) {
				emulator.Step();
			}
			AssertFibonacci(fibIndex);
		}
	}
}
