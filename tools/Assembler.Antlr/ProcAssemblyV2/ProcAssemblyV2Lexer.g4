lexer grammar ProcAssemblyV2Lexer;

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

IMMEDIATE
	: '$' '-'? ('0' .. '9') ('_' | '0' .. '9' | 'A' .. 'Z' | 'a' .. 'z')*
	;

fragment SYMBOLSTART
	: 'A' .. 'Z' | 'a' .. 'z' | '_'
	;

fragment SYMBOLPART
	: SYMBOLSTART | '0' .. '9'
	;

SYMBOL
	: (SYMBOLSTART) (SYMBOLPART)*
	;

INSTRUCTION
	: '.'? ('a' .. 'z') ('a' .. 'z' | '0' .. '9')*
	;

REGISTER
	: ('%' | '*') ('a' .. 'z') ('a' .. 'z' | '0' .. '9')*
	;

COLON
	: ':'
	;

COMMA
	: ','
	;

DEFINE
	: '@define'
	;

INCLUDE
	: '@include'
	;

fragment STRINGTERMINATOR
	: ~'\\' '"'
	;

fragment STRINGTERMINATORNT
	: ~STRINGTERMINATOR // not supported
	;

STRING
	: STRINGTERMINATOR (STRINGTERMINATORNT)* STRINGTERMINATOR
	;
