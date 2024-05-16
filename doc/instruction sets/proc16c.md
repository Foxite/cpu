# proc16c
proc16c uses fixed-width instruction words of 16 bits. when a program is stored in a file it must be written in big-endian, that is, bits 15-08 are written first and then bits 07-00. to be clear, bit 15 is the most signficant bit and bit 00 is the least significant bit.

proc16c has 8 general-purpose registers: r0 through r7.

when this document speaks of a "3-bit", it's the register index.

when this document speaks of a "4-bit" or a "5-bit" or anything bigger, then it's a value where the MSB determines what the rest means.
- 0: value is a register index
- 1: value is an unsigned constant

as such, using 00010 in place of a 5-bit refers to r2. using 10110 means the constant 6. when referring to a register, value bits more significant than bit 2 must be 0, otherwise the behavior is undefined.


# instructions
instructions, from bit 15:
- 0: ldc
- 10: ALU
- 1100: mov
- 1101: bus
- 1110: jump
- 111100: noop/break
- 111101: lde



## ldc
write an unsigned 12-bit constant value to a register.

to write bigger constants, use this in conjunction with lde.

- 15: instruction prefix (always 0)
- 14-12: write register (3-bit)
- 11-00: unsigned 12-bit constant to write


## alu
- 15-14: instruction prefix (always 10)
- 13-11: write register (3-bit)
- 10-08: lhs (3-bit)
- 07-04: rhs (4-bit)
- 03-00: opcode

### opcodes
| code | mnemonic | description                      |
|------|----------|----------------------------------|
| 0000 | add      | signed add                       |
| 0001 | sub      | signed subtract                  |
| 0010 | mul      | signed multiply                  |
| 0010 | div      | signed divide                    |
| 0100 | shl      | bitwise signed shift left        |
| 0101 | shr      | bitwise signed shift right       |
| 0110 | neq      | output 1 if lhs != rhs           |
| 0111 | eq       | output 1 if lhs == rhs           |
| 1000 | gtu      | output 1 if lhs > rhs (unsigned) |
| 1001 | ltu      | output 1 if lhs < rhs (unsigned) |
| 1010 | gt       | output 1 if lhs > rhs (signed)   |
| 1011 | lt       | output 1 if lhs < rhs (signed)   |
| 1100 | and      | AND                              |
| 1101 | or       | OR                               |
| 1110 | xor      | XOR                              |
| 1111 | xnor     | XNOR (NOT: operand XNOR 0)       |


## mov
- 15-12: prefix (1100)
- 11-09: write register (3-bit)
- 08-06: read register (3-bit)

remaining bits unused


## bus
### ldb
- 15-11: prefix (11010)
- 10-08: address register (3-bit)
- 07-05: value register (3-bit)

remaining bits unused

### stb
- 15-11: prefix (11011)
- 10-08: address register (3-bit)
- 07-00: value (8-bit)


## jump
- 15-12: prefix (1110)
- 11-10: condition
 - 00: always jump
 - 01: jump if comparand is 0
 - 10: jump if comparand is not 0
 - 11: undefined
- 09-07: comparand (3-bit)
- 06-00: jump target (7-bit)


## noop/break
- 15-08: prefix (111100)
- 09: BRK pin value (0: noop, 1: break)
- 08-00: address to read (9-bit)

bus input will be unused. used for diagnostic purposes.


## lde
load extended constant.

read a register, left shift by 4 and then OR with an unsigned 4-bit constant, write back to the same register.

use in conjunction with ldc to load 16-bit constants in 2 instructions.
- 15-09: prefix (111101)
- 08-06: register (3-bit)
- 03-00: value (unsigned 4-bit constant)
