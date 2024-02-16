Several circuit designs exist in this folder

## alu16_1
The ALU for computer_proc16a1. Its opcodes are defined in the proc16a ISA

## alu16_2
The ALU for cpu_proc16a2. Its opcodes are defined in the proc16a ISA. The main difference between this and alu16_1 is that this one does not take care of selecting its own operands, this is done by alu16_2_operands

## alu16_2_operands
Selects operands for alu16_2 according to the proc16a ISA

## compare16_1
Used in several different circuits (both ALU and cpu circuits). It basically gives opcodes to Digital's stock "comparator" circuit. The opcodes are defined in proc16a's ALU compare instructions and jump instructions

## computer_proc16a1
A pile of spaghetti that I tricked into thinking. Do not look at it. You'll go blind.

Alternatively: My first attempt at a computer, that implements the proc16a instruction set. It does not have a discrete cpu component, but it does have a discrete alu (alu16_1)

The design uses old versions of existing circuits that were modified such that it no longer fits in this circuit. If you want to see this in action, checkout commit d29b2d91

## computer_proc16a2
My second attempt at a computer. It is very simple actually, because most of the magic happens inside the discrete cpu component (cpu_proc16a2)

## computer_proc16a2_mappedmemory
My first attempt at implementing a computer with mapped memory, built out of computer_proc16a2. Uses cpu_proc16a2

Design organized into blocks. Has a few devices. Documentation is inside design

## cpu_proc16a2
My first attempt at a proper "cpu". Still spaghetti. Uses a discrete instruction decoder (decode16a1) but does not have a form of microcode or pipelining

## decode16a1
Instruction decoder for proc16a, used by cpu_proc16a2. Design organized into blocks.

## jump16_1
Uses compare16_1, used to process jump instructions in proc16a

## jump16_2
Same, but splits the operand selection pin into its own input pin (as opposed to being part of a larger bus)

