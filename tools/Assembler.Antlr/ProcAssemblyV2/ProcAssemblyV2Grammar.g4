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

instruction
	: DOT? SYMBOL ((instructionArgument COMMA)* instructionArgument)?
	;

instructionArgument
	: SYMBOL
	| IMMEDIATE
	| REGISTER
	| STRING
	;
