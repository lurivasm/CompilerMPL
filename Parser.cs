using System;
using System.Collections.Generic;

namespace Compilers
{
	public class Parser
	{
		private class ParseError : SystemException { }	
		private readonly List<Token> _tokens;
		private int _current;

		public Parser(List<Token> tokens)
		{
			_tokens = tokens;
		}

		/**
		 * Main function of the parser
		 * It returns the list of statements and checks if all lines end in semicolon
		 */
		public List<Stmt> Parse()
		{
			List<Stmt> statements = new List<Stmt>();
			while (!IsAtEnd())
			{
				statements.Add(Statement());
				Consume(TokenKind.Semicolon, "Expect ';' after statement.");
			}

			return statements;
		}

		


		/** 
		 * Checks if the current token is any of the given types valid for the rule
		 * and advances to the next token
		 */
		private bool Match (params TokenKind[] kinds)
		{ 
			foreach (TokenKind knd in kinds)
			{
				if (Check(knd))
				{
					Advance();
					return true;
				}
			}

			return false;
		}

		/**
		 * Checks if the current token is the desired token withoud advancing
		 */
		private bool Check(TokenKind type)
		{
			if (IsAtEnd()) return false;
			return Peek().Kind == type;
		}

		/**
		 * Advance to the next token
		 */
		private Token Advance()
		{
			if (!IsAtEnd()) _current++;
			return Previous();
		}

		/**
		 * Checks if we have run out of tokens to parse 
		 */
		private bool IsAtEnd()
		{
			return Peek().Kind == TokenKind.EndOfFile;
		}

		/**
		 * Returns the current token to parse
		 */
		private Token Peek()
		{
			return _tokens[_current];
		}

		/**
		 * Returns the previous token to the currently one
		 */
		private Token Previous()
		{
			return _tokens[_current - 1];
		}


		/**
		 * It returns the statement with respect to the token we are matching
		 * Valid for Print, Var, Identifier, For, Read, Assert
		 */
		private Stmt Statement()
		{
			try
			{
				if (Match(TokenKind.Print)) return PrintStatement();
				else if (Match(TokenKind.Var)) return VarDeclStatement();
				else if (Match(TokenKind.Identifier)) return AssignStatement();
				else if (Match(TokenKind.For)) return ForStatement();
				else if (Match(TokenKind.Read)) return ReadStatement();
				else if (Match(TokenKind.Assert)) return AssertStatement();
			}
			
			catch (ParseError)
			{
				Synchronize();
				return null;
			}
				
		}

		/**
		 * Print Statement
		 * It returns the print statement with the expression to print
		 */
		private Stmt PrintStatement()
		{
			Expr value = Expression();
			return new Stmt.Print(value);
		}

		/**
		 * Assert Statement
		 * It returns the assert statement with the expression to evaluate
		 */
		private Stmt AssertStatement()
		{
			if (Match(TokenKind.Leftparent))
			{
				Expr expr = Expression();
				Consume(TokenKind.Rightparent, "Expect ')' after Assert expression.");
				return new Stmt.Assert(expr);
			}
			throw Error(Peek(), "Expect expression after Assert statement.");
		}

		/**
		 * Read Statement
		 * It returns the read statement with the token to store
		 */
		private Stmt ReadStatement()
		{
			Consume(TokenKind.Identifier, "Expect identifier after read statement.");
			Token token = Previous();
			return new Stmt.Read(token);
		}

		/**
		 * Assign Statement
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
		 * For Statement
		 * It returns the for statement
		 */
		private Stmt ForStatement()
		{
			Token name = Consume(TokenKind.Identifier, "Expect identifier after 'for'.");
			Consume(TokenKind.In, "Expected 'in' after for statement");
			Expr beginvalue = Expression();
			Consume(TokenKind.Dotdot, "Expected '..' between range values in for statement.");
			Expr endvalue = Expression();
			Consume(TokenKind.Do, "Expected 'do' after range of for statement.");
			List<Stmt> stmts = Statements();
			Consume(TokenKind.End, "Expected 'end' after statements in for loop.");
			Consume(TokenKind.For, "Expected 'for' after end of for statement.");
			return new Stmt.For(name, beginvalue, endvalue, stmts);

		}

		/**
		 * Function for the loop range
		 * It reads statements till it finds the token end
		 */
		public List<Stmt> Statements()
		{
			List<Stmt> statements = new List<Stmt>();
			while (Peek().Kind != TokenKind.End)
			{
				statements.Add(Statement());
				Consume(TokenKind.Semicolon, "Expect ';' after statement.");
			}

			return statements;
		}

		/**
		 * Var Statement
		 * It returns the var statement with the name of the var, its value and its type
		 */
		private Stmt VarDeclStatement()
		{
			Token name = Consume(TokenKind.Identifier, "Expected a identifier after 'var'.");
			Consume(TokenKind.Colon, "Expect ':' in var statement.");
			
			if (Match(TokenKind.Int, TokenKind.String, TokenKind.Bool))
			{
				Token type = Previous();
				Expr value = null;
				if (Match(TokenKind.Assign))
				{
					value = Expression();
				}
				return new Stmt.Var(name, value, type.Kind);
			}

			throw Error(Peek(), "Expect type in var statement.");


		}

		/**
		 * Expressions in the Context-Free Grammar that accept
		 *		Not <opnd>
		 *		<opnd> Binary/Logical operator <opnd>
		 *		<opnd>
		 */
		private Expr Expression()
		{
			// Unary operator NOT
			if (Match(TokenKind.Not))
			{
				Token unaryop = Previous();
				Expr right = Opnd();
				return new Expr.Unary(unaryop, right);
			}
						
			else
			{
				Expr expr = Opnd();
				// Binary operators +, -, *, /, <, =
				if (Match(TokenKind.Sum, TokenKind.Minus, TokenKind.Mult,
				          TokenKind.Div, TokenKind.Less, TokenKind.Equal))
				{
					Token binaryop = Previous();
					Expr right = Opnd();
					return new Expr.Binary(expr, binaryop, right);
				}

				// Logical operator AND
				else if (Match(TokenKind.And))
				{
					Token logicalop = Previous();
					Expr right = Opnd();
					return new Expr.Logical(expr, logicalop, right);
				}

				// Only an operand
				return expr;
			}
		}

		/**
		 * Expression for numbers, strings, brackets and identifiers
		 */
		private Expr Opnd()
		{
			// Literal expression for number expressions or strings
			if (Match(TokenKind.IntValue, TokenKind.StringValue)) return new Expr.Literal(Previous().Value);

			// Identifiers
			if (Match(TokenKind.Identifier))
			{
				Token name = Previous();
				return new Expr.Assign(name, null);
			}

			// Grouping expression for values inside brackets
			if (Match(TokenKind.Leftparent))
			{
				Expr expr = Expression();
				Consume(TokenKind.Rightparent, "Expect ')' after expression.");
				return new Expr.Grouping(expr);
			}

			throw Error(Peek(), "Expect expression.");
		}


		/**
		 * Checks if the next token is the type kind of the parameters
		 * If the token kind matches, it advances to the next token
		 * Otherwise it throws a parse error
		 */
		private Token Consume(TokenKind kind, String message)
		{
			if (Check(kind)) return Advance();
			else
				throw Error(Peek(), message);	
		}

		/**
		 * Throws the parse error in the main
		 */
		private ParseError Error(Token token, String message)
		{
			Program.Error(token, message);
			return new ParseError();
		}

		private void Synchronize()
		{
			Advance();

			while (!IsAtEnd())
			{
				if (Previous().Kind == TokenKind.Semicolon) return;

				switch (Peek().Kind)
				{
					case TokenKind.Var:
					case TokenKind.For:
					case TokenKind.Print:
					case TokenKind.Int:
					case TokenKind.String:
					case TokenKind.Bool:
					case TokenKind.Read:
					case TokenKind.Assert:
						return;
				}

				Advance();
			}
		}



	}
}