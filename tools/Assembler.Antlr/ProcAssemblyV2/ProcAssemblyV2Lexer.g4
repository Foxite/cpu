lexer grammar ProcAssemblyV2Lexer;

channels { COMMENT_CHANNEL }

fragment SYMBOLSTART : 'A' .. 'Z' | 'a' .. 'z' | '_' ;
fragment SYMBOLPART  : SYMBOLSTART | '0' .. '9' ;

fragment STRINGTERMINATORNT
	: '\\' ["] // an escaped double quote
	| ~('"')   // or anything that is not an (unescaped) double quote
	;

STRING     : '"' (  STRINGTERMINATORNT  )* '"' ;
COMMENT    : ('#' (~'\n')*) -> channel(COMMENT_CHANNEL) ;
NEWLINE    : '\n' ;
WHITESPACE : ('\t' | ' ')+ -> skip;
IMMEDIATE  : '$' '-'? ('0' .. '9') ('_' | '0' .. '9' | 'A' .. 'Z' | 'a' .. 'z')* ;
SYMBOL     : (SYMBOLSTART) (SYMBOLPART)* ;
REGISTER   : '%' ('a' .. 'z') ('a' .. 'z' | '0' .. '9')* ;
COLON      : ':' ;
COMMA      : ',' ;
DOT        : '.' ;
ATSIGN     : '@' ;
DEFINE     : '@define' ;
INCLUDE    : '@include' ;
EXPR_START : '[' ;
EXPR_END   : ']' ;
PARENL     : '(' ;
PARENR     : ')' ;
ADD        : '+' ;
SUBTRACT   : '-' ;
MULTIPLY   : '*' ;
DIVIDE     : '/' ;
