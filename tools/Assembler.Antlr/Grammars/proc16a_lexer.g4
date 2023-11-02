lexer grammar proc16a_lexer;

channels { COMMENT_CHANNEL }

COMMENT
	: ('#' (~'\n')*)
	-> channel(COMMENT_CHANNEL);

NEWLINE
	: '\n'
	;

WHITESPACE
	: ('\t' | ' ')+
	-> skip;

/*
fragment NUMBER10
	: '-'? ('0' .. '9' | '_')+
	;

fragment NUMBER16
	: '-'? '0x' ('0' .. '9' | 'A' .. 'F' | 'a' .. 'f' | '_')+
	;

fragment NUMBER2
	: '-'? '0b' ('0' | '1' | '_')+
	;

NUMBER
	: NUMBER10
	| NUMBER16
	| NUMBER2
	;*/

NUMBER
	: '-'? ('0' .. '9') ('_' | '0' .. '9' | 'A' .. 'Z' | 'a' .. 'z')*
	;

REGISTER
	: '*'? ('A' | 'B')
	;

JMP
	: 'JMP'
	;

BOOLEAN
	: 'true'
	| 'false'
	;

BINARY_OPERATION
	: '+'
	| '-'
	| '*'
	| '/'
	| '<<'
	| '>>'
	
    | 'AND'
    | 'OR'
	| 'XOR'
	| 'XNOR'
	| 'NOR'
	| 'NAND'
	
	| '>'
	| '=='
	| '>='
	| '<'
	| '!='
	| '<='
	;

UNARY_OPERATION
	: ('NOT' | '~')
	;

fragment SYMBOLSTART
	: ('A' .. 'Z' | 'a' .. 'z' | '_')
	;

fragment SYMBOLPART
	: (SYMBOLSTART | '0' .. '9')
	;

SYMBOL
	: SYMBOLSTART SYMBOLPART*
	;

ASSIGN
	: '='
	;

COLON
	: ':'
	;
