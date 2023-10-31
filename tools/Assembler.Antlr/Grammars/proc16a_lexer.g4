lexer grammar proc16a_lexer;

channels { COMMENT_CHANNEL }

COMMENT
	: '#' .*? '\n'
	-> channel(COMMENT_CHANNEL);

WHITESPACE
	: ('\t' | ' ')+
	-> skip;

NEWLINE
	: '\n'
	| EOF
	;

NUMBER10
	: ('0' .. '9' | '_')+
	;

NUMBER16
	: '0x' ('0' .. '9' | 'A' .. 'F' | 'a' .. 'f' | '_')+
	;

NUMBER2
	: '0b' ('0' | '1' | '_')+
	;

NUMBER
	: NUMBER10
	| NUMBER16
	| NUMBER2
	;

REGISTER
	: '*'? ('A' | 'B')
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

BOOLEAN
	: 'true'
	| 'false'
	;

ALU_BINARY_OPERATION
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
	
	| COMPARE_OPERATION
	;

ALU_UNARY_OPERATION
	: '~'
	;

COMPARE_OPERATION
	: BOOLEAN
    | '>'
    | '=='
    | '>='
    | '<'
    | '!='
    | '<='
    ;


ASSIGN
	: '='
	;

JMP
	: 'JMP'
	;

COLON
	: ':'
	;
