#!/bin/bash

dotnet run --project /home/foxite/dev/cpu/tools/Assembler.Cli -- compile \
	--arch proc16b \
	--output-mode raw \
	--macro-path /home/foxite/dev/cpu/asm/proc16b/macros \
	--symbol 'STACK_PTR=$0' 'STACK_BEGIN=$0x100' 'FUNC_RETV=$0x1' 'FUNC_ARGS=$0x2' 'FUNC_LOCS=$0x3' \
	--output "${1%.pa2}.out" \
	--lines \
	-- "$1" > /home/foxite/dev/cpu/asm/proc16b/output.txt
#	--verbose \
