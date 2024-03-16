#!/bin/bash

../../../tools/Assembler.Cli/bin/Debug/net7.0/Assembler.Cli compile \
	--arch proc16b \
	--assembler V2 \
	--output-mode raw \
	--macro-path macros \
	--symbol 'STACK_PTR=$0' 'STACK_BEGIN=$0x100' \
	--output "${1%.pa2}.out" \
	-- "$1"
