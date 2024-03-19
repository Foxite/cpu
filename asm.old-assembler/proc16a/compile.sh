#!/bin/bash

dotnet run --project ../../tools/Assembler.Cli -- compile \
	--arch proc16a \
	--assembler V2 \
	--output-mode raw \
	--output "${1%.pa2}.out" \
	-- "$1"


