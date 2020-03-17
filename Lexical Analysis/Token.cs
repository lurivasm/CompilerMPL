using System;

namespace Compilers
{
    /**
	 * Class Token : a token is formed by its kind, position in the code,
	 * the text it represents and the value it has
	 */
    public class Token
    {
        /**
		 * Constructor of the class
		 * Param : TokenKind of the token
		 *         position or line where the token is
		 *         text of the token
		 *         value of the token if it is a number/string
		 */
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

        /**
		 * Function ToString
		 * Return : string with all the information about the token
		 */
        public override String ToString()
        {
            return Kind + "\tText: " + Text + "\tValue: " + Value + '\n';
        }
    }
}
