using System;

namespace Compilers
{
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
