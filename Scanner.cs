using System;
using System.Collections.Generic;


namespace Compilers
{
	public class Scanner
	{
		private readonly string _text;
		private int _position = -1;
		private List<Token> Tokens = new List<Token>();
		private int start;
		private int line = 1;
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

		public Scanner(string text)
		{
			_text = text;
		}

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

		private char Current
		{
			get
			{
				return _text[_position];
			}
		}

		private char NextCurrent
		{
			get
			{
				return _text[_position+1];
			}
		}

		private bool IsAtEnd()
		{
			return _position >= _text.Length - 1;
		}

		private void Next()
		{
			_position++;
		}


		private void AddToken(TokenKind type)
		{
			AddToken(type, null);
		}

		private void AddToken(TokenKind kind, Object value)
		{
			String text = _text.Substring(start, _position - start + 1);
			Tokens.Add(new Token(kind, line, text, value));
		}

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
				case ':':
					if (Match('=')) { AddToken(TokenKind.Assign); break; }
					else AddToken(TokenKind.Colon); break;
				case '/':
					// A comment goes until the end of the line. 
					if (Match('/'))
						while (Current != '\n' && !IsAtEnd()) Next();
					else if (Match('*'))
						while (Current != '*' && Match('/') && !IsAtEnd()) Next();
					else
					{
						AddToken(TokenKind.Div);
					}
					break;
				case ' ':
				case '\r':
				case '\t':
					// Ignore whitespace.                      
					break;

				case '\n':
					line++;
					break;
				case '"': Strings(); break;
				default:
					if (char.IsDigit(Current))
						Number();
					else if (char.IsLetter(Current))
						Identifier();
					else
					{
						Program.Error(line, "Unexpected character" + " " + Current);
					}
					break;
			}
		}

		private bool Match(char expected)
		{
			Next();
			if (IsAtEnd()) return false;
			if (Current != expected) return false;

			return true;
		}

		private void Strings()
		{
			while (NextCurrent != '"' && !IsAtEnd())
			{
				// Multiline strings support
				if (NextCurrent == '\n') line++;
				Next();
			}

			// Unterminated string.                                 
			if (IsAtEnd())
			{
				Program.Error(line, "Unterminated string.");
				return;
			}

			// The closing ".                                       
			Next();
			var length = _position - start;
			String text = _text.Substring(start + 1, length - 1);
			AddToken(TokenKind.StringValue, text);
		}

		private void Number()
		{
			while (char.IsDigit(NextCurrent)) Next();

			var length = _position - start + 1;
			var text = _text.Substring(start, length);
			int.TryParse(text, out var value);

			AddToken(TokenKind.IntValue, value);
		}

		private void Identifier()
		{
			while (char.IsLetterOrDigit(NextCurrent) || NextCurrent == '_') Next();
			var length = _position - start + 1;
			var text = _text.Substring(start, length);

			// If it is a reserved word we return the value
			if (ReservedWords.ContainsKey(text))
			{
				if (!ReservedWords.TryGetValue(text, out TokenKind value))
					Program.Error(line, "Internal Error");

				AddToken(value);
			}

			// Otherwise it is an identifier
			else AddToken(TokenKind.Identifier);
		}
	}
}
