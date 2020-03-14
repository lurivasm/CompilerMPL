using System;

namespace Compilers
{
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
