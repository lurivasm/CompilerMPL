using System;
using System.Collections.Generic;

namespace Compilers
{
	public class Interpreter : Expr.IVisitor<Object>, Stmt.IVisitor<Object>
	{
		private Dictionary<string, Stmt.Var> SymbleTable = new Dictionary<string, Stmt.Var>();
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

		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}

		public Object VisitExpressionStmt(Stmt.Expression stmt)
		{
			return Evaluate(stmt.Expr);	
		}

		public Object VisitPrintStmt(Stmt.Print stmt)
		{
			Object value = Evaluate(stmt.Expr);
			Console.Write(value.ToString());
			return null;
		}

		public Object VisitVarStmt(Stmt.Var stmt)
		{
			String name = stmt.Name.Text;
			if (SymbleTable.ContainsKey(name))
			{
				throw new RuntimeError(stmt.Name, "This variable already exists.");
			}
			else
			{
				SymbleTable.Add(name, stmt);
			}

			return null;

		}

		public Object VisitReadStmt(Stmt.Read stmt)
		{
			throw new NotImplementedException();
		}

		public Object VisitAssertStmt(Stmt.Assert stmt)
		{
			return Evaluate(stmt.Expr);
		}

		public Object VisitAssignStmt(Stmt.Assign stmt)
		{
			throw new NotImplementedException();
		}

		public Object VisitForStmt(Stmt.For stmt)
		{
			throw new NotImplementedException();
		}

		public object VisitAssignExpr(Expr.Assign expr)
		{
			String name = expr.Name.Text;
			if (SymbleTable.ContainsKey(name))
				return SymbleTable[name].Initializer;
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
