lexer grammar ProcAssemblyV2Lexer;

channels { COMMENT_CHANNEL }

fragment SYMBOLSTART : 'A' .. 'Z' | 'a' .. 'z' | '_' ;
fragment SYMBOLPART  : SYMBOLSTART | '0' .. '9' ;

fragment STRINGTERMINATORNT
	: '\\' ["] // an escaped double quote
	| ~('"')   // or anything that is not an (unescaped) double quote
	;

fragment NUMERIC : '-'? ('0' .. '9') ('_' | '0' .. '9' | 'A' .. 'Z' | 'a' .. 'z')* ;
fragment FSYMBOL : (SYMBOLSTART) (SYMBOLPART)* ;

EXPR_START : '[' -> pushMode(CONSTANTEXPRESSION) ;
STRING     : '"' (  STRINGTERMINATORNT  )* '"' ;
COMMENT    : ('#' (~'\n')*) -> channel(COMMENT_CHANNEL) ;
NEWLINE    : '\n' ;
WHITESPACE : ('\t' | ' ')+ -> skip;
IMMEDIATE  : '$' NUMERIC ;
SYMBOL     : FSYMBOL ;
REGISTER   : '%' ('a' .. 'z') ('a' .. 'z' | '0' .. '9')* ;
COLON      : ':' ;
COMMA      : ',' ;
DOT        : '.' ;
ATSIGN     : '@' ;
DEFINE     : '@define' ;
INCLUDE    : '@include' ;


mode CONSTANTEXPRESSION;
EXPRCOMMENT : ('#' (~'\n')*) -> channel(COMMENT_CHANNEL) ;
EXPRWS      : ('\t' | ' ' | '\n')+ -> skip;
EXPRCONST   : NUMERIC ;
EXPRSYMBOL  : FSYMBOL ;
EXPR_END    : ']' -> popMode ;
PARENL      : '('  ;
PARENR      : ')'  ;
ADD         : '+'  ;
SUBTRACT    : '-'  ;
MULTIPLY    : '*'  ;
DIVIDE      : '/'  ;
AND         : '&'  ;
OR          : '|'  ;
XOR         : '^'  ;
LSHIFT      : '<<' ;
RSHIFT      : '>>' ;
NOT         : '~'  ;
