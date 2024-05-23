First implementation of the proc16c ISA. It uses a register file; my first CPU design to do so. it also uses an instruction decoder which outputs a 3-bit mode and 4 16-bit arguments.

It's also expected to be the last CPU design that implements the "proc" pin interface, which has several one-directional buses, including:
- instruction word (input 16)
- instruction pointer (output 16)
- address (output 16)
- data (input 16)
- data (output 16)
- data write (output 1)
- break (output 1)
- clock (input 1)
