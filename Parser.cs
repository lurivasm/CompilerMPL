using System;
using System.Collections.Generic;

namespace Compilers
{
    /**
	 * Class Parser : it implements all the functions for the syntax analysis and
	 * throw the parser erros with respect to the building tree
	 */
    public class Parser
    {
        /* Parse Errors */
        private class ParseError : SystemException { }
        /* Flag for the errors in the loop for */
        private bool ErrorFor = false;
        /* List of tokens to read */
        private readonly List<Token> _tokens;
        /* Position of the current token we are reading */
        private int _current;

        /**
		 * Constructor of the Parser
		 * Params : list of tokens returned by the scanner
		 */
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        /**
		 * Main function of the parser : it adds all the statements
		 * to the list of statements and checks the ; after them
		 * Return : list of all the statements 
		 */
        public List<Stmt> Parse()
        {
            List<Stmt> statements = new List<Stmt>();
            while (!IsAtEnd()) {
                try {
                    statements.Add(Statement());
                    Consume(TokenKind.Semicolon, "Expected ';' after statement.");
                }
                /* In case we handle one Parse Error we synchronize and return null */
                catch (ParseError) {
                    Synchronize();
                }
            }
            return statements;
        }


        /** 
		 * Function Match : checks if the current token is any of the given 
		 * types valid for the rule and advances to the next token
		 * Param : kinds of tokens to check
		 * Return : true if the tokenkinds match, otherwise false
		 */
        private bool Match(params TokenKind[] kinds)
        {
            foreach (TokenKind knd in kinds) {
                if (Check(knd)) {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        /**
		 * Function Chech : checks if the current token is the desired tokenkind withoud advancing
		 * Param : type of tokenkind we want to match
		 * Return : true if the tokenkinds match, otherwise false
		 */
        private bool Check(TokenKind type)
        {
            if (IsAtEnd()) return false;
            return Peek().Kind == type;
        }

        /**
		 * Function Consume : checks if the next token is the type kind of the parameters
		 * If the token kind matches, it advances to the next token
		 * Otherwise it throws a parse error
		 * Param : kind of the token to check
		 *         message in case of error
		 * Return : the function advance
		 */
        private Token Consume(TokenKind kind, String message)
        {
            if (Check(kind)) {
                return Advance();
            }
            else {
                throw Error(Previous(), message);
            }
        }

        /**
		 * Function Advance : increases _current for the next token
		 * Return : previous token
		 */
        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        /**
		 * Function IsAtEnd : Checks if we have run out of tokens to parse 
		 * Return : true if we do not have more tokens, otherwise false
		 */
        private bool IsAtEnd()
        {
            return Peek().Kind == TokenKind.EndOfFile;
        }

        /**
		 * Function Peek
		 * Return : the current token to parse
		 */
        private Token Peek()
        {
            return _tokens[_current];
        }

        /**
		 * Function Previous
		 * Return : the previous token to the currently one
		 */
        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        /**
		 * Function Statement : it matches all the valid statements and call them
		 * Valid for Print, Var, Identifier, For, Read, Assert
		 * Return : the statement with respect to the token we are matching
		 */
        private Stmt Statement()
        {
            if (Match(TokenKind.Print)) return PrintStatement(Previous());
            else if (Match(TokenKind.Var)) return VarDeclStatement();
            else if (Match(TokenKind.Identifier)) return AssignStatement();
            else if (Match(TokenKind.For)) return ForStatement();
            else if (Match(TokenKind.Read)) return ReadStatement();
            else if (Match(TokenKind.Assert)) return AssertStatement(Previous());
            else {
                throw Error(Peek(), "Not valid statement.");
            }

        }

        /**
		 * Function PrintStatement : "print" <expr>
		 * Param : token to read
		 * Return : print statement with the expression to print
		 */
        private Stmt PrintStatement(Token readtoken)
        {
            Expr value = Expression();
            return new Stmt.Print(readtoken, value);
        }

        /**
		 * Function AssertStatement : "assert" "(" <expr> ")"
		 * It checks if there are in order a left parenthesis, one expression
		 * and one right parenthesis, otherwise it throws an error with consume
		 * Param : token to assert
		 * Return : assert statement with the expression to evaluate
		 */
        private Stmt AssertStatement(Token assertToken)
        {
            if (Match(TokenKind.Leftparent)) {
                Expr expr = Expression();
                Consume(TokenKind.Rightparent, "Expected ')' after Assert expression.");
                return new Stmt.Assert(assertToken, expr);
            }
            throw Error(Peek(), "Expected '(' before Assert expression.");
        }

        /**
		 * Function ReadStatement : "read" <var_ident> 
		 * It checks if there is an identifiers, otherwise it throws an error with consume
		 * Return : the read statement with the token to store
		 */
        private Stmt ReadStatement()
        {
            Consume(TokenKind.Identifier, "Expected identifier after read statement.");
            Token token = Previous();
            return new Stmt.Read(token);
        }

        /**
		 * Function AssignStatement : <var_ident> ":=" <expr>
		 * It checks if there is := , otherwise it throws an error with consume
		 * It returns the assign statement with the variable and its value
		 */
        private Stmt AssignStatement()
        {
            Token name = Previous();
            Consume(TokenKind.Assign, "Expected ':=' after an identifier");
            Expr value = Expression();
            return new Stmt.Assign(name, value);
        }

        /**
		 * Function ForStatement : 
		 * "for" <var_ident> "in" <expr> ".." <expr> "do" <stmts> "end" "for"
		 * It checks if there are in order an identifier, the token in, the two dots
		 * and the tokens do, end and for, otherwise it throws an error with consume
		 * Return : the for statement with the identifier, the values of the loop and
		 * the statements inside the loop
		 */
        private Stmt ForStatement()
        {
            if (Peek().Kind == TokenKind.Semicolon && ErrorFor) {
                ErrorFor = false;
                throw Error(Peek(), "Expected 'end' after statements in for loop.");
            }

            ErrorFor = true;
            Token name = Consume(TokenKind.Identifier, "Expected identifier after 'for'.");
            Consume(TokenKind.In, "Expected 'in' after for statement");
            Expr beginvalue = Expression();
            Consume(TokenKind.Dotdot, "Expected '..' between range values in for statement.");
            Expr endvalue = Expression();
            Consume(TokenKind.Do, "Expected 'do' after range of for statement.");
            List<Stmt> stmts = ForStatements();
            Consume(TokenKind.End, "Expected 'end' after statements in for loop.");
            Consume(TokenKind.For, "Expected 'for' after end of for statement.");
            ErrorFor = false;
            return new Stmt.For(name, beginvalue, endvalue, stmts);

        }

        /**
		 * Function Statements : returns all the statements inside the loop for
		 * till it finds the token end of the loop for checking the ; between them
		 * Return : the list of statements
		 */
        public List<Stmt> ForStatements()
        {
            List<Stmt> statements = new List<Stmt>();
            while (Peek().Kind != TokenKind.End && !IsAtEnd()) {
                statements.Add(Statement());
                Consume(TokenKind.Semicolon, "Expected ';' after statement.");
            }
            return statements;
        }

        /**
		 * Function VarStatement : "var" <var_ident> ":" <type> [ ":=" <expr> ] 
		 * It checks if there are in order an identifier, the token : and one int, string
		 * or bool variable, otherwise it throws an error with consume
		 * Return : the var statement with the name of the var, its value and its type
		 */
        private Stmt VarDeclStatement()
        {
            Token name = Consume(TokenKind.Identifier, "Expected identifier after 'var'.");
            Consume(TokenKind.Colon, "Expected ':' in var statement.");

            if (Match(TokenKind.Int, TokenKind.String, TokenKind.Bool)) {
                Token type = Previous();
                Expr value = null;

                /* Case we initialize the variable we check if there is  := and update the value */
                if (Match(TokenKind.Assign)) {
                    value = Expression();
                    return new Stmt.Var(name, value, type.Kind);
                }
                /* Case next is ; so the variable is not initialized */
                else if (Peek().Kind == TokenKind.Semicolon) {
                    return new Stmt.Var(name, value, type.Kind);
                }
                /* Error Case : next token is not ';' either ':=' */
                else {
                    throw Error(Peek(), "Expected ';' or ':=' in the var statement.");
                }
            }
            /* Error if the kind of the token is not INT, STRING or BOOL */
            throw Error(Peek(), "Expected type (int, string or bool) in var statement.");


        }

        /**
		 * Function Expression : check if the expression is one of the following ones
		 *		Not <opnd>
		 *		<opnd> Binary/Logical operator <opnd>
		 *		<opnd>
		 * Return : the new expression (unary, binary or logical) with the corresponding
		 *          values and operands
		 */
        private Expr Expression()
        {
            /* Unary operator NOT */
            if (Match(TokenKind.Not)) {
                Token unaryop = Previous();
                Expr right = Opnd();
                return new Expr.Unary(unaryop, right);
            }
            else {
                Expr expr = Opnd();

                /* Binary operators +, -, *, /, <, = */
                if (Match(TokenKind.Sum, TokenKind.Minus, TokenKind.Mult,
                          TokenKind.Div, TokenKind.Less, TokenKind.Equal)) {
                    Token binaryop = Previous();
                    Expr right = Opnd();
                    return new Expr.Binary(expr, binaryop, right);
                }

                /* Logical operator AND */
                else if (Match(TokenKind.And)) {
                    Token logicalop = Previous();
                    Expr right = Opnd();
                    return new Expr.Logical(expr, logicalop, right);
                }

                /* Only an operand */
                return expr;
            }
        }

        /**
		 * Function Opnd : Expression for numbers, strings, brackets and identifiers
		 * Return : the corresponding expression with the values
		 */
        private Expr Opnd()
        {
            /* Literal expression for number expressions or strings */
            if (Match(TokenKind.IntValue)) {
                return new Expr.Literal(new Value(VALTYPE.INT, Previous().Value));
            }
            if (Match(TokenKind.StringValue)) {
                return new Expr.Literal(new Value(VALTYPE.STRING, Previous().Value));
            }

            /* Identifiers */
            if (Match(TokenKind.Identifier)) {
                Token name = Previous();
                return new Expr.Ident(name, null);
            }

            /* Grouping expression for values inside brackets */
            if (Match(TokenKind.Leftparent)) {
                Expr expr = Expression();
                Consume(TokenKind.Rightparent, "Expected ')' after expression.");
                return new Expr.Grouping(expr);
            }

            throw Error(Peek(), "Expected expression.");
        }

        /**
		 * Function ParseError : throws the parse error in the main file
		 * Param : token where we found the error
		 *         message to print about the error
		 * Return : the new parse error
		 */
        private ParseError Error(Token token, String message)
        {
            Program.Error(token, message);
            return new ParseError();
        }

        /**
		 * Function Synchronize : in case of error, this function advances 
		 * till the next safe point like a ;
		 */
        private void Synchronize()
        {
            if (ErrorFor == true) Advance();
            while (!IsAtEnd()) {
                if (Previous().Kind == TokenKind.Semicolon) return;

                switch (Peek().Kind) {
                    case TokenKind.Var:
                    case TokenKind.For:
                        if (ErrorFor == true) {
                            Advance();
                            continue;
                        }
                        return;
                    case TokenKind.Print:
                    case TokenKind.Read:
                    case TokenKind.Assert:
                        return;
                }
                Advance();
            }
        }



    }
}