using System;
using System.Collections.Generic;

namespace Compilers
{
	public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
	{
		/**
		 * SymbleTable for storing all the declared variables
		 *		String name : name of the variable
		 *		Stmt.Var : var statements which stores the name, value and type of the variable
		 */
		private Dictionary<string, Stmt.Var> SymbleTable = new Dictionary<string, Stmt.Var>();

		/**
		 * Main Function of the class
		 * It executes all the statements of the parameter
		 * It catches the Runtime Errors
		 */
		public void Interpret(List<Stmt> statements)
		{
			try
			{
				foreach (Stmt stmt in statements)
					Execute(stmt);
			} 
			catch (RuntimeError error)
			{
				Program.RuntimeError(error);
			}
		}

		/**
		 * It calls all the accepts in the visitor pattern for the statements
		 */
		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}

		/**
		 * Function for Expression Statement
		 * It evaluates the given expression as a parameter
		 */
		public Object VisitExpressionStmt(Stmt.Expression stmt)
		{
			return Evaluate(stmt.Expr);	
		}

		/**
		 * Function for Print Statement --> "print" <expr>
		 * It evaluates the expression and prints it in the command line
		 */
		public Object VisitPrintStmt(Stmt.Print stmt)
		{
			Object value = Evaluate(stmt.Expr);
			Console.Write(value.ToString());
			return null;
		}

		/**
		 * Function for Var Statement --> "var" <var_ident> ":" <type> [ ":=" <expr> ]
		 * It adds the new variable to the SymbleTable
		 * If it already exists throws an error
		 */
		public Object VisitVarStmt(Stmt.Var stmt)
		{
			String name = stmt.Name.Text;
			if (SymbleTable.ContainsKey(name))
				throw new RuntimeError(stmt.Name, "This variable already exists.");
			else
				SymbleTable.Add(name, stmt);

			return null;

		}

		public Object VisitReadStmt(Stmt.Read stmt)
		{
			throw new NotImplementedException();
		}

		/**
		 * Function for Assert Statement --> "assert" "(" <expr> ")"
		 * It evaluates the expression and prints it in the command line
		 */
		public Object VisitAssertStmt(Stmt.Assert stmt)
		{
			bool value = (bool)Evaluate(stmt.Expr);
			Console.Write(value.ToString());
			return null;
		}

		/**
		 * Function for Assign Statement --> <var_ident> := <expr>
		 * It updates the value of the identifier in the SymbleTable
		 * Throws an error if the variable was not previous declarated
		 */
		public Object VisitAssignStmt(Stmt.Assign stmt)
		{
			String name = stmt.Name.Text;
			
			// If the var is already in the table we change its value;
			if (SymbleTable.ContainsKey(name))
			{				
				if (stmt.Name.Kind == SymbleTable[name].Type)
					SymbleTable[name].Initializer = stmt.Value;
				else
					throw new RuntimeError(stmt.Name, "Expected " + SymbleTable[name].Type.ToString() 
													   + " but found " + stmt.Name.Kind.ToString());
			}
			// Otherwise it was not declarated before so it is an error
			else
				throw new RuntimeError(stmt.Name, "This variable does not exist.");

			return null;
		}

		/**
		 * Function for For Statement --> "for" <var_ident> "in" <expr> ".." <expr> "do" <stmts> "end" "for"
		 */
		public Object VisitForStmt(Stmt.For stmt)
		{
			throw new NotImplementedException();
		}

		public object VisitIdentExpr(Expr.Ident expr)
		{
			String name = expr.Name.Text;
			if (SymbleTable.ContainsKey(name))
				return Evaluate(SymbleTable[name].Initializer);
			else
			throw new RuntimeError(expr.Name, "Not declared variable.");
		}

		public object VisitBinaryExpr(Expr.Binary expr)
		{
			Object left = Evaluate(expr.Left);
			Object right = Evaluate(expr.Right);

			switch (expr.OperatorToken.Kind)
			{
				case TokenKind.Minus:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return (int)left - (int)right;
				case TokenKind.Mult:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return (int)left * (int)right;
				case TokenKind.Div:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return (int)left / (int)right;
				case TokenKind.Sum:
					if (left is int && right is int)
						return (int)left + (int)right;
					else if (left is string && right is string)
						return (string)left + (string)right;
					break;
				case TokenKind.Equal:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return ((int)left == (int)right);
				case TokenKind.Less:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return ((int)left < (int)right);
			}

			throw new RuntimeError(expr.OperatorToken, "Operands must be two numbers or two strings.");
		}

		public object VisitGroupingExpr(Expr.Grouping expr)
		{
			return Evaluate(expr.Expression);
		}

		public object VisitLiteralExpr(Expr.Literal expr)
		{
			return expr.Value;
		}

		public object VisitLogicalExpr(Expr.Logical expr)
		{
			Object left = Evaluate(expr.Left);
			if (!IsTruth(left)) return left;
			return Evaluate(expr.Right);
		}

		public object VisitUnaryExpr(Expr.Unary expr)
		{
			Object right = Evaluate(expr.Right);
			if (expr.OperatorToken.Kind == TokenKind.Not)
				return !IsTruth(right);

			return null;
		}

		private Object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		private bool IsTruth(Object obj)
		{
			if (obj == null) return false;
			if (obj is bool) return (bool)obj;

			return true;
		}


		private void CheckNumberOperand(Token op, Object left, Object right)
		{
			if (left is int && right is int) return;
			throw new RuntimeError(op, "Operands must be numbers.");
		}

		
	}
}
