parser grammar ProcAssemblyV2Grammar;

options { tokenVocab=ProcAssemblyV2Lexer; }

program
	: NEWLINE* (programStatement NEWLINE+)* programStatement NEWLINE* EOF
	;

programStatement
	: (SYMBOL COLON NEWLINE*)? instruction
	| DEFINE SYMBOL instructionArgument
	| INCLUDE string
	;

instruction
	: INSTRUCTION ((instructionArgument COMMA)* instructionArgument)?
	;

instructionArgument
	: SYMBOL
	| IMMEDIATE
	| REGISTER
	| string
	;

stringTerminatorNt
	: ~STRINGTERMINATOR // not supported in lexer rule
	;

string
	: STRINGTERMINATOR (stringTerminatorNt)* STRINGTERMINATOR
	;
