grammar QueryFilter;

/*
 * Parser Rules
 */

query: symmetric | rightRecursive | leftRecursive | op | EOF |;

symmetric: (nestedQuery VERB nestedQuery | LBRACKET op RBRACKET);
leftRecursive: nestedQuery VERB op;
rightRecursive: op VERB nestedQuery;
nestedQuery: LBRACKET+ query RBRACKET+;
op: COLUMN VERB TARGET;

/*
 * Lexer Rules
 */

fragment AnyChar: [a-zA-Z0-9];
fragment SpecialChars: [-_@.];

LBRACKET: '(';
RBRACKET: ')';
TARGET: (TARGETSTR | TARGETBOOL);
TARGETSTR: '\'' (AnyChar | SpecialChars)+ '\'';
TARGETBOOL: ('true' | 'false');
VERB:
	(
		'eq'
		| 'ne'
		| 'gt'
		| 'ge'
		| 'lt'
		| 'le'
		| 'and'
		| 'not'
		| 'or'
	);
COLUMN: AnyChar+;
WS: (' ' | '\t' | '\r' | '\n') -> channel(HIDDEN);