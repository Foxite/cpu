# proc16c
proc16c has 8 general-purpose registers: r0 through r7.

most instruction arguments are encoded as either 3 or 4 bits. 3-bit arguments are a register and they encode the register index. in 4-bit arguments, if bit 3 is 0 then bits 2-0 are the register index, and if bit 3 is 1 then bits 2-0 are a 3-bit constant value.

# instructions
instructions, from bit 15:
- 0: ldi
- 10: ALU
- 1100: mov
- 1101: jump
- 1110: bus
- 11110: 


# ldi
write a 12-bit constant value to a register.

- 15: instruction prefix (always 0)
- 14-12: write register (3-bit)
- 11-00: constant to write


# alu
- 15-14: instruction prefix (always 10)
- 13-11: write register (3-bit)
- 10-08: lhs (3-bit)
- 07-04: rhs (4-bit)
- 03-00: opcode

## opcodes
| code | mnemonic | description                    |
|------|----------|--------------------------------|
| 0000 | add      | signed add                     |
| 0001 | sub      | signed subtract                |
| 0010 | mul      | signed multiply                |
| 0010 | div      | signed divide                  |

| 0100 | shl      | bitwise (unsigned) shift left  |
| 0101 | shr      | bitwise (unsigned) shift right |
| 0110 |          |                                |
| 0111 |          |                                |

| 1100 | and      | AND                            |
| 1101 | or       | OR                             |
| 1110 | xor      | XOR                            |
| 1111 | xnor     | XNOR (NOT: 0 XNOR operand)     |

| 1000 | eq       | output 1 if lhs == rhs         |
| 1001 | gt       | output 1 if lhs > rhs (signed) |
| 1010 | lt       | output 1 if lhs < rhs (signed) |
| 1011 |          |                                |
