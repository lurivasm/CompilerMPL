﻿using System;
using System.Collections.Generic;


namespace Compilers
{
	public class Scanner
	{
		/* String that contains all the code to scan */
		private readonly string _text;
		/* Position when reading _text */
		private int _position = -1;
		/* Position of the starting part of the tokens */
		private int start;
		/* Number of the line we are reading for implementing errors */
		private int line = 1;
		/* List of all the scanned tokens of _text to return */
		private List<Token> Tokens = new List<Token>();

		/**
		 * Dictionary for the Reserved Words
		 *		String for the reserved words
		 *		Token Kind for each reserved word
		 */
		private static readonly Dictionary<string, TokenKind> ReservedWords = new Dictionary<string, TokenKind>()
		{
			{"for", TokenKind.For},
			{"do", TokenKind.Do},
			{"end", TokenKind.End},
			{"in", TokenKind.In},
			{"var", TokenKind.Var},
			{"assert", TokenKind.Assert},
			{"print", TokenKind.Print},
			{"read", TokenKind.Read},
			{"bool", TokenKind.Bool},
			{"string", TokenKind.String},
			{"int", TokenKind.Int}
		};

		/**
		 * Constructor of the Scanner Class
		 * Params : text to assign to the text to read _text
		 */
		public Scanner(string text)
		{
			_text = text;
		}

		/**
		 * Functions for scanning the tokens
		 * It goes through all the tokens till it finds the End Of File
		 * Return : the list of Tokens
		 */
		public List<Token> ScanTokens()
		{
			while (!IsAtEnd())
			{
				// We are at the beginning of the next word.
				Next();
				start = _position;
				NextToken();
			}

			Tokens.Add(new Token(TokenKind.EndOfFile, _position, "\0", null));
			return Tokens;
		}

		/**
		 * Variable Current
		 * Return : the current char while reading _text
		 */
		private char Current
		{
			get
			{
				return _text[_position];
			}
		}

		/**
		 * Variable NextCurrent
		 * Return : the next char while reading _text
		 */
		private char NextCurrent
		{
			get
			{
				return _text[_position+1];
			}
		}

		/**
		 * Function IsAtEnd
		 * Return : true if we have finished _text, otherwise false
		 */
		private bool IsAtEnd()
		{
			return _position >= _text.Length - 1;
		}

		/**
		 * Function Next
		 * It increases the variable _position for the next char
		 */
		private void Next()
		{
			_position++;
		}

		/**
		 * Function AddToken : it calls AddToken with null in the value
		 * Param : type of the token we are adding to the list of tokens
		 */
		private void AddToken(TokenKind type)
		{
			AddToken(type, null);
		}

		/**
		 * Function AddToken : it adds a new token to the list
		 * Param : kind of the token we are adding to the list of tokens
		 *		   value of the token we are reading
		 */
		private void AddToken(TokenKind kind, Object value)
		{
			String text = _text.Substring(start, _position - start + 1);
			Tokens.Add(new Token(kind, line, text, value));
		}

		/**
		 * Function NextToken : it checks the type of the current token
		 * and add it to the list of tokens
		 */
		private void NextToken()
		{
			switch (Current)
			{
				case '(': AddToken(TokenKind.Leftparent); break;
				case ')': AddToken(TokenKind.Rightparent); break;
				case '-': AddToken(TokenKind.Minus); break;
				case '+': AddToken(TokenKind.Sum); break;
				case ';': AddToken(TokenKind.Semicolon); break;
				case '*': AddToken(TokenKind.Mult); break;
				case '!': AddToken(TokenKind.Not); break;
				case '=': AddToken(TokenKind.Equal); break;
				case '<': AddToken(TokenKind.Less); break;
				case '.':
					if (Match('.')) AddToken(TokenKind.Dotdot); break;

				/* Assigments or just colons */
				case ':':
					if (Match('=')) { AddToken(TokenKind.Assign); break; }
					else AddToken(TokenKind.Colon); break;

				case '/':
					/* Singleline Comments goes until the end of the line */
					if (Match('/'))
						while (Current != '\n' && !IsAtEnd()) Next();
					/* Multiline comments goes until we discover the next or is the EOF */
					else if (Match('*'))
						while (Current != '*' && Match('/') && !IsAtEnd()) Next();
					/* Otherwise it is a division */
					else
						AddToken(TokenKind.Div);
					break;

				/* Ignoring whitespaces, tabs... */
				case ' ':
				case '\r':
				case '\t':                   
					break;

				/* New lines we increase the variable line */
				case '\n':
					line++;
					break;
				/* For "strings" */
				case '"': Strings(); break;
				default:
					/* Case of numbers */
					if (char.IsDigit(Current))
						Number();
					/* Case of letters */
					else if (char.IsLetter(Current))
						Identifier();
					/* Lexical error: character not recognized */
					else
						Program.Error(line, "Unexpected character" + " " + Current);
					break;
			}
		}

		/**
		 * Function Match : it calls next char and checks if it is the same as the param
		 * Param : expected char in the next one
		 * Return : true if unexpected and next are the same, otherwise false
		 */
		private bool Match(char expected)
		{
			Next();
			if (IsAtEnd()) return false;
			if (Current != expected) return false;

			return true;
		}

		/**
		 * Function Strings : it reads all the chars until finding the next "
		 */
		private void Strings()
		{
			while (NextCurrent != '"' && !IsAtEnd())
			{
				/* Multiline strings support */
				if (NextCurrent == '\n') line++;
				Next();
			}

			/* Unterminated string, never finding " */
			if (IsAtEnd())
			{
				Program.Error(line, "Unterminated string.");
				return;
			}

			/* The closing " */
			Next();
			var length = _position - start;
			String text = _text.Substring(start + 1, length - 1);
			AddToken(TokenKind.StringValue, text);
		}

		/**
		 * Function Number : reads all the characters that form a number
		 */
		private void Number()
		{
			while (char.IsDigit(NextCurrent)) Next();

			var length = _position - start + 1;
			var text = _text.Substring(start, length);
			int.TryParse(text, out var value);

			AddToken(TokenKind.IntValue, value);
		}

		/**
		 * Function Identifier : reads all the characters (letters, numbers or
		 * underscores) that form an identifier or a reserved word
		 */
		private void Identifier()
		{
			while (char.IsLetterOrDigit(NextCurrent) || NextCurrent == '_') Next();
			var length = _position - start + 1;
			var text = _text.Substring(start, length);

			/* If it is a reserved word we return the value */
			if (ReservedWords.ContainsKey(text))
			{
				if (!ReservedWords.TryGetValue(text, out TokenKind value))
					Program.Error(line, "Internal Error");

				AddToken(value);
			}

			/* Otherwise it is an identifier */
			else AddToken(TokenKind.Identifier);
		}
	}
}
