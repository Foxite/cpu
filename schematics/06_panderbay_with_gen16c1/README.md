The gen16c1 cpu will implement the proc16c ISA and uses general-purpose memory, ie it has one bidirectional data bus and an output address bus, which it uses both to read instructions and operate on (mapped) memory. this pin interface is called "gen".
- address (output 16)
- data (bidirectional 16)
- data write (output 1)
- WX (input 1)
- clock (input 1)
- break (output 1)

Pander Bay is the codename for a computer that supports the gen pin interface.
