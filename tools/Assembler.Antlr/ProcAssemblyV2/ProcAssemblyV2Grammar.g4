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
	| EXPR_START constantExpression EXPR_END
	| REGISTER
	| STRING
	;

constantExpression
    : nestedConstantExpression
    | EXPRSYMBOL
    | EXPRCONST
    | unaryExpression=NOT constantExpression
    | constantExpression binaryExpression=(MULTIPLY | DIVIDE) constantExpression
    | constantExpression binaryExpression=(ADD | SUBTRACT) constantExpression
    | constantExpression binaryExpression=(AND | OR | XOR | LSHIFT | RSHIFT) constantExpression
    ;

nestedConstantExpression : PARENL constantExpression PARENR ;
