#!/bin/bash

../../../tools/Assembler.Cli/bin/Debug/net7.0/Assembler.Cli compile -a proc16b -m raw --macro-path macros --symbol 'STACK_PTR=$0' 'STACK_BEGIN=$0x100' --output "${1%.pa2}.out" -- "$1"
