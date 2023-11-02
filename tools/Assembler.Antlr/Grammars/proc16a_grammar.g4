parser grammar proc16a_grammar;

options { tokenVocab=proc16a_lexer; }

program
	: NEWLINE* (programStatement NEWLINE+)* programStatement NEWLINE* EOF
	;

programStatement
	: (SYMBOL COLON NEWLINE*)? instruction
	;

instruction
	: datawordInstruction
	| aluInstruction
	| jumpInstruction
	| assignInstruction
	;

assignInstruction
	: REGISTER ASSIGN REGISTER
	;

datawordInstruction
	: REGISTER ASSIGN (NUMBER | SYMBOL | BOOLEAN)
	;

aluInstruction
	: REGISTER ASSIGN aluOperand BINARY_OPERATION aluOperand
	| REGISTER ASSIGN UNARY_OPERATION aluOperand
	;

aluOperand
	: NUMBER
	| REGISTER
	;

jumpInstruction
	: comparePhrase JMP REGISTER
	| BOOLEAN JMP REGISTER
	;

comparePhrase
	: REGISTER BINARY_OPERATION NUMBER
	;
