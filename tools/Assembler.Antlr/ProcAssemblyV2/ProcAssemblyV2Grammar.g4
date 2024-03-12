parser grammar ProcAssemblyV2Grammar;

options { tokenVocab=ProcAssemblyV2Lexer; }

program
	: NEWLINE* (programStatement NEWLINE+)* programStatement NEWLINE* EOF
	;

programStatement
	: (SYMBOL COLON NEWLINE*)? instruction
	//| DEFINE SYMBOL instructionArgument
	//| INCLUDE STRING
	;

instructionMnemonic
	: (DOT | ATSIGN)? SYMBOL
	;

instruction
	: instructionMnemonic ((instructionArgument COMMA)* instructionArgument)?
	;

instructionArgument
	: SYMBOL
	| IMMEDIATE
	| REGISTER
	| STRING
	;
