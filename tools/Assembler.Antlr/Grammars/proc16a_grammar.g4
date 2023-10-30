parser grammar proc16a_grammar;

options { tokenVocab=proc16a_lexer; }

datawordInstruction
	: REGISTER ASSIGN (NUMBER | SYMBOL) 
	;

aluOperand
	: NUMBER
	| REGISTER
	| SYMBOL
	;

aluInstruction
	: REGISTER ASSIGN aluOperand ALU_BINARY_OPERATION aluOperand
	| REGISTER ASSIGN aluOperand ALU_UNARY_OPERATION
	;


comparePhrase
	: REGISTER COMPARE_OPERATION NUMBER
	;

jumpInstruction
	: comparePhrase JMP SYMBOL
	;

instruction
	: datawordInstruction
	| aluInstruction
	| jumpInstruction
	;

programStatement
	: (SYMBOL COLON NEWLINE*)? instruction
	;

program
	: (programStatement NEWLINE+)* (programStatement NEWLINE*)?
	;

