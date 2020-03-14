using System;

namespace Compilers
{
	public class Token
	{
		public Token(TokenKind ki, int pos, string txt, object val)
		{
			Kind = ki;
			Position = pos;
			Text = txt;
			Value = val;
		}

		public TokenKind Kind { get; }
		public int Position { get; }
		public string Text { get; }
		public object Value { get; }

		public override String ToString()
		{
			return Kind + "\tText: " + Text + "\tValue: " + Value + '\n';
		}
	}
}
