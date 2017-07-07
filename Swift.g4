grammar Swift;

options {
    language = CSharp;
}

/*
 Parser rules
 */

//

/*
 Lexer rules
 */

ID : [a-zA-Z][a-zA-Z0-9]* ;

STRING : '"' ( ~('\r' | '\n' | '"') | '\\"' )* '"' ;

INT : DIGIT+ ;
FLOAT : DIGIT+ ('.' DIGIT+)? ;

DIGIT : [0-9] ;

NEWLINE : ('\r' | '\n')+ ;

WS : ( '\t' | ' ' | '\r' | '\n' )+ -> channel(HIDDEN) ;
