using System;

namespace Compilers
{
    /**
	 * Enumeration TokenKind 
	 * It has all the kind of tokens we can have
	 */
    public enum TokenKind
    {
        EndOfFile,

        // One or two character tokens
        Dotdot,
        Colon,
        Semicolon,
        Leftparent,
        Rightparent,
        Assign,

        /* One charanter tokens */
        Sum,
        Minus,
        Mult,
        Div,
        Less,
        Equal,
        And,
        Not,

        /* Literals */
        Int,
        String,
        Bool,
        Identifier,

        /* Values */
        IntValue,
        StringValue,
        BoolValue,

        /* Reserved Words */
        Read,
        Print,
        Assert,
        For,
        Do,
        End,
        In,
        Var
    }
}

