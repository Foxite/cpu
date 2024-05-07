Proc16c will be done in the future.

# proposed features
not all of these may be implemented into proc16c.

obviously, there will be more registers, there's nothing to elaborate on there. i think we'll go to 8 general purpose registers this time.

## addressing modes (and getting rid of jgt)
i did some reading on wikipedia and it seems a lot of modern cpu's (actually not just modern ones) have something called addressing modes. in proc16b there are no instruction overloads; they each have a fixed set of argument types. most take only registers, ldc takes a constant, etc

the addressing mode basically encodes the type of instruction argument along with the actual argument. so it could be a constant or a register or even a memory address (being a register containing an address or a constant address). i even saw that x86 has support for (limited?) runtime expressions *within* instruction arguments, adding two registers and dereferencing the result within a single instruction.

this probably requires significant changes to ProcAssembly.

if we're going to implement this we'll probably want to use a proper register file, either the one built into Digital or one that we design ourselves. the builtin one only supports reading from two registers at once. this works for all proc16b instructions except for the conditional jumps, which read from *three* registers and write to the PC. so we'll either have to design our own register file, or (and this isn't a bad idea as it removes a lot of complexity) replace all conditional jumps with a single "jump if (not) zero" instruction that takes a comparand and a PC target. this can be combined with the current ALU comparison instructions.


## definedly signed/unsigned comparison/arithmetic operations
the proc16b spec doesn't ever say a word about signed arithmetic or comparisons. when designing the ALU I ticked "signed operation" in most of the components but I didn't test it as thoroughly (or maybe I did? i forgot honestly) and I mostly avoided negative numbers when writing programs.

the proc16c spec should clearly define which integer operations are signed and unsigned and it should probably have both variants of every operation.


## flags register
overflow register is not properly implemented, never used, and a whole register for just one flag is kind of a waste. replace it with a FLAGS register that holds up to 16 flags that are changed by any CPU operation.


## break with value
an instruction that sets the address bus to some value, and maybe also writes a value to it, and sets the break pin to high. this is something i really missed when developing pong. a feature like this will seriously help when debugging.


## stack machine
a potential feature/alternative paradigm we could investigate is a hardware implementation of a stack machine. we could fully commit to this and develop an entirely new line of ISAs that *only* do this, because it might be very hard to integrate so many features with a CPU that also does normal instructions.
- an array of hardware frames each with a hardware stack, local registers, and argument registers
- push/pop on current frame's stack
- call instruction: jump to a label and switch to the next stack frame
- return instruction: jump back to the instruction pointer after the call
- push items to next frame, to prepare calling the function
- all the stack operations present in the macros

i found a few homebrew cpu's that seem to have stack functionality in their instruction sets:
- [CHUNGUS-2](https://github.com/sammyuri/chungus-2-assembler/blob/main/saves/breakout.s) has a "POP" instruction (idk what it does), and "CAL" and "RET" instructions (it seems CAL pushes the current PC and jumps somewhere, and RET pops a value and jumps back there)

Should investigate more.


## microcode
the instruction decoder will output a pointer to the microcode ROM inside the CPU and some amount of parameters. a control unit will then drive the control lines based on the microcode instructions and the parameters from the instruction decoder.


## floating point arithmetic
i don't know about this one, I didn't need it for pong and i can't *honestly* imagine what I'll need it for in anything less complicated than 3d graphics. and that's some ways away. but it could be done anyway.


# other proposed features
these are not necessarily going to be part of the ISA but these are some interesting things to develop/experiment with for future cpu designs.

## general-purpose memory, simplified pinout
the current cpu requires a bunch of pins:
- instruction word: 16 input pins
- data bus: 16 input pins
- instruction word pointer: 16 output pins
- address: 16 output pins
- data bus: 16 output pins
- bus write: 1 output pin
- break: 1 output pin

you can already fix one problem, which is that the data bus is actually *two* buses, and you can make it one bidirectional bus. the direction is controlled by the cpu's write pin. that's 16 pins removed.

furthermore, if we add general-purpose memory, we can remove 32 pins to do with the instruction word.

so the pinout will become:
- address bus: 16 output pins
- data bus: 16 bidirectional pins
- bus write: 1 output pin
- break: 1 output pin

if general-purpose memory is to be implemented, an important feature to have is a WX pin. this is an input pin on the CPU and it determines if the cpu is able to execute the data on the data bus. if it is, then writing to that cell is not allowed. this is to reduce the possibility of accidentally executing data that isn't an instruction. in the real world, it's an important security measure that makes it harder for attackers to perform arbitrary code execution.

cells that are writable and not executable are known as W cells. cells that are executable and not writable are known as X cells.

if an instruction tries to write to an X cell or the cpu attempts to load an instruction from a W cell, it should raise some kind of error. in the real world, this is done with traps and interrupts, but this is out of my reach for now. we'll just set the BRK pin and output a special error value (see the wishlist feature about breaking with a value).

if we're ever going to be implementing an OS (or anything that facilitates running code that becomes available at runtime) then we'll want to build a mechanism for updating the WX status of cells. modern computers use virtual memory and memory pages for this, but this is also out of my reach for now.


## pipelining
it seems like a cool idea in the same vein as microcode.

modern cpus have a pipeline that looks roughly like this:
- load instruction
- decode instruction
- fetch memory values for the instruction
- execute the instruction
- write the result back to register/memory

various cpus merge these steps or split them up further. i think most desktop cpus have a 4-stage pipeline. but we can do whatever we want.

i think this is a good one to start with:
- load and decode instruction
- fetch memory values
- execute
- write back
