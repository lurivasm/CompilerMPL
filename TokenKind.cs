using System;

namespace Compilers
{
	public enum TokenKind
	{
		ErrorToken,
		BinaryToken,
		NotKeyDiccionary,

		newline,
		Whitespace,
		EndOfFile,

		Comment_start,
		Comment_end,
		Comment,

		// One or two character tokens
		Dotdot,
		Colon,
		Semicolon,
		Leftparent,
		Rightparent,
		Assign,

		// One charanter tokens
		Sum,
		Minus,
		Mult,
		Div,
		Less,
		Equal,
		And,
		Not,

		ttrue,
		ffalse,

		// Literals
		Int,
		String,
		Bool,
		Identifier,

		IntValue,
		StringValue,
		BoolValue,

		// Reserved Words
		Read,
		Print,
		Assert,
		For,
		Do,
		End,
		In,
		Var

	}
}

