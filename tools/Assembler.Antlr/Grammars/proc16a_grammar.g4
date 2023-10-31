parser grammar proc16a_grammar;

options { tokenVocab=proc16a_lexer; }

program
	: NEWLINE* programStatement*
	;

programStatement
	: (SYMBOL COLON NEWLINE*)? instruction NEWLINE+
	;

instruction
	: datawordInstruction
	| aluInstruction
	| jumpInstruction
	;

datawordInstruction
	: REGISTER ASSIGN (NUMBER | SYMBOL) 
	;

aluInstruction
	: REGISTER ASSIGN aluOperand ALU_BINARY_OPERATION aluOperand
	| REGISTER ASSIGN aluOperand ALU_UNARY_OPERATION
	;

aluOperand
	: NUMBER
	| REGISTER
	| SYMBOL
	;

jumpInstruction
	: comparePhrase JMP SYMBOL
	;

comparePhrase
	: REGISTER COMPARE_OPERATION NUMBER
	;
