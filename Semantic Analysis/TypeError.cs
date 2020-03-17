using System;

namespace Compilers
{
    /**
	 * Class for the Type Errors which implements the 
	 * System Exceptions
	 * It has the token where the error is and a descriptive message
	 */
    public class TypeError : SystemException
    {
        public Token Token;

        public TypeError(Token token, String message)
            : base(message)
        {
            Token = token;
        }
    }
}
