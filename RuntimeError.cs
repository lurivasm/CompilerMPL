using System;

namespace Compilers
{
    /**
	 * Class for the Runtime Errors which implements the 
	 * System Exceptions
	 * It has the token where the error is and a descriptive message
	 */
    public class RuntimeError : SystemException
    {
        public Token Token;

        public RuntimeError(Token token, String message)
            : base(message)
        {
            Token = token;
        }
    }
}
