#!/bin/bash

../../../tools/Assembler.Cli/bin/Debug/net7.0/Assembler.Cli compile -a proc16b -m raw --macro-path macros -- "$1" > "${1%.pa2}.out"
