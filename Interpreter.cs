using System;
using System.Collections.Generic;

namespace Compilers
{
	public class Interpreter : Expr.IVisitor<Value>, Stmt.IVisitor<Object>
	{
		/**
		 * SymbleTable for storing all the declared variables
		 *		String name : name of the variable
		 *		Stmt.Var : var statements which stores the name, value and type of the variable
		 */
		private Dictionary<string, Value> SymbleTable = new Dictionary<string, Value>();

		/**
		 * Main Function of the class
		 * It executes all the statements of the parameter
		 * It catches the Runtime Errors
		 */
		public void Interpret(List<Stmt> statements)
		{
			try
			{
				printer = new AstPrinter(SymbleTable);
				foreach (Stmt stmt in statements)
					Execute(stmt);
			} 
			catch (RuntimeError error)
			{
				Program.RuntimeError(error);
			}
		}

		/**
		 * Used for printing purposes
		 */
		private AstPrinter printer;


		/**
		 * Function Execute calls all the accepts in the visitor pattern for the statements
		 * Param : stmt to accept
		 */
		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}

		/**
		 * Function for Expression Statement
		 * It evaluates the given expression as a parameter
		 */
		public object VisitExpressionStmt(Stmt.Expression stmt)
		{
			return Evaluate(stmt.Expr);	
		}

		/**
		 * Function for Print Statement --> "print" <expr>
		 * It evaluates the expression and prints it in the command line
		 */
		public Object VisitPrintStmt(Stmt.Print stmt)
		{
			Value value = Evaluate(stmt.Expr);
			Console.Write(value.Val.ToString());
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
			VALTYPE type = ExpectedType(stmt);
 
			if (SymbleTable.ContainsKey(name))
				throw new RuntimeError(stmt.Name, "This variable already exists.");
			else if (stmt.Initializer is null)
				SymbleTable.Add(name, new Value(type, null));
			else
				SymbleTable.Add(name, Evaluate(stmt.Initializer));

			return null;
		}

		private VALTYPE ExpectedType(Stmt.Var stmt)
		{
			switch (stmt.Type)
			{
				case TokenKind.Int: return VALTYPE.INT;
				case TokenKind.String: return VALTYPE.STRING;
				case TokenKind.Bool: return VALTYPE.BOOL;
				default:
					throw new RuntimeError(stmt.Name, "Cannot declarate " + stmt.Type.ToString() + " as a variable.");
			}
		}

		/**
		 * Function for Read Statements : it reads the new value for our variable
		 * and checks both if the variable was previous declared and it has the same
		 * type as the read value
		 * Param : stmt with the new value for our variable
		 */
		public Object VisitReadStmt(Stmt.Read stmt)
		{
			/* If the variable already exists and is the proper type we update the symble table */
			if (SymbleTable.ContainsKey(stmt.Token.Text))
			{
				String x = Console.ReadLine();
				Value value = CheckReadStatement(x, stmt.Token, SymbleTable[stmt.Token.Text].Type);
				SymbleTable[stmt.Token.Text] = value;
				return null;
			}
			/* Otherwise throw an error */
			throw new RuntimeError(stmt.Token, "Variable not previous declared.");
		}

		/**
		 * Function CheckReadStatement : checks if the read value and the variable
		 * has the same type (int or string)
		 * Param : the string obj we have read
		 *		   token of the variable to store the value
		 *		   value expected token to have
		 * Return : the value of the variable or an error
		 */
		private Value CheckReadStatement(String obj, Token token, VALTYPE value)
		{
			/* If the value is an int we try to parse it or throw an error */
			if (value.Equals(VALTYPE.INT))
			{
				if (int.TryParse(obj, out int output) == true)
					return new Value(VALTYPE.INT, output);
				throw new RuntimeError(token, "Read value " + obj + " is not the proper type.");
			}
			/* If the value is a string just return it */
			else if (value.Equals(VALTYPE.STRING))
				return new Value(VALTYPE.STRING, obj);

			/* We cannot read bools */
			else
				throw new RuntimeError(token, "Read value " + obj + " is not the proper type.");
		}

		/**
		 * Function for Assert Statement --> "assert" "(" <expr> ")"
		 * It evaluates the expression and prints it in the command line
		 */
		public Object VisitAssertStmt(Stmt.Assert stmt)
		{
			bool value = (bool)Evaluate(stmt.Expr).Val;
			Value right = Evaluate(stmt.Expr);
			if (right.Type.Equals(VALTYPE.BOOL)){ //unnecessary because the type system has checked this already
				if (!(bool)right.Val) 
				{
					Console.WriteLine("Assert failed:\t Expr: " + printer.print(stmt.Expr) + " is false."); //TODO
				}
			}

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
				Value val = Evaluate(stmt.Value);
				if (val.Type.Equals(SymbleTable[name].Type))
					SymbleTable[name] = val;
				else
					throw new RuntimeError(stmt.Name, "Expected " + SymbleTable[name].Type.ToString()
													   + " but found " + val.Type.ToString());
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
			int variable;
			string name = stmt.Name.Text;
			int beginvalue = (int)Evaluate(stmt.BeginValue).Val;
			int endvalue = (int)Evaluate(stmt.EndValue).Val;
			for (variable = beginvalue; variable <= endvalue; variable++)
			{
				SymbleTable[name] = new Value(VALTYPE.INT, variable);
				Interpret(stmt.Stmts);				
			}
			return null;
		}

		public Value VisitIdentExpr(Expr.Ident expr)
		{
			String name = expr.Name.Text;
			if (SymbleTable.ContainsKey(name))
				return (SymbleTable[name]);
			else
			throw new RuntimeError(expr.Name, "Not declared variable.");
		}

		public Value VisitBinaryExpr(Expr.Binary expr)
		{
			Value left = Evaluate(expr.Left);
			Value right = Evaluate(expr.Right);

			switch (expr.OperatorToken.Kind)
			{
				case TokenKind.Minus:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return new Value(VALTYPE.INT, (int)left.Val - (int)right.Val);
				case TokenKind.Mult:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return new Value(VALTYPE.INT, (int)left.Val * (int)right.Val);
				case TokenKind.Div:
					CheckNumberOperand(expr.OperatorToken, left, right);
					return new Value(VALTYPE.INT, (int)left.Val / (int)right.Val);
				case TokenKind.Sum:
					if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT))
						return new Value(VALTYPE.INT, (int)left.Val + (int)right.Val);
					else if (left.Type.Equals(VALTYPE.STRING) && right.Type.Equals(VALTYPE.STRING))
						return new Value(VALTYPE.STRING, (string)left.Val + (string)right.Val);
					break;
				case TokenKind.Equal:
					if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT))
						return new Value(VALTYPE.BOOL, ((int)left.Val == (int)right.Val));
					else if (left.Type.Equals(VALTYPE.STRING) && right.Type.Equals(VALTYPE.STRING))
						return new Value(VALTYPE.BOOL, ((string)left.Val == (string)right.Val));
					else if (left.Type.Equals(VALTYPE.BOOL) && right.Type.Equals(VALTYPE.BOOL))
						return new Value(VALTYPE.BOOL, ((bool)left.Val == (bool)right.Val));
					break;
				case TokenKind.Less:
					if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT))
						return new Value(VALTYPE.BOOL, ((int)left.Val < (int)right.Val));
					else if (left.Type.Equals(VALTYPE.STRING) && right.Type.Equals(VALTYPE.STRING))
						return new Value(VALTYPE.BOOL, (string.Compare((string)left.Val, (string)right.Val) == -1));
					else if (left.Type.Equals(VALTYPE.BOOL) && right.Type.Equals(VALTYPE.BOOL))
						return new Value(VALTYPE.BOOL, (((bool)left.Val).CompareTo((bool)right.Val) < 0));
					break;
			}

			throw new RuntimeError(expr.OperatorToken, "Operands must be two numbers or two strings.");
		}

		public Value VisitGroupingExpr(Expr.Grouping expr)
		{
			return Evaluate(expr.Expression);
		}

		public Value VisitLiteralExpr(Expr.Literal expr)
		{
			return expr.Value;
		}

		public Value VisitLogicalExpr(Expr.Logical expr)
		{
			if (expr.OperatorToken.Kind.Equals(TokenKind.And))
			{
				Value left = Evaluate(expr.Left);

				if (left.Type.Equals(VALTYPE.BOOL))
				{
					if (!(bool)left.Val)
						return left;
					Value right = Evaluate(expr.Right);
					if (right.Type.Equals(VALTYPE.BOOL))
					{
						return right;
					}
					else
					{
						throw new RuntimeError(expr.OperatorToken, "Expected a boolean as the rightoperand of and '&&'.");
					}

				}
				else
				{
					throw new RuntimeError(expr.OperatorToken, "Expected a boolean as the leftoperand of and '&&'.");
				}
			}
			else
			{
				throw new RuntimeError(expr.OperatorToken, "Got a logical operator that is not '&&'.");
			}

		}

		public Value VisitUnaryExpr(Expr.Unary expr)
		{
			Value right = Evaluate(expr.Right);
			if (expr.OperatorToken.Kind == TokenKind.Not)
				if (right.Type.Equals(VALTYPE.BOOL))
					return new Value(VALTYPE.BOOL, !((bool)right.Val));
				else
					throw new RuntimeError(expr.OperatorToken, "Trying to negate a non bool value");

			return null;
		}

		private Value Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		//private bool IsTruth(Object obj)
		//{
		//	if (obj == null) return false;
		//	if (obj is bool) return (bool)obj;

		//	return true;
		//}


		private void CheckNumberOperand(Token op, Value left, Value right)
		{
			if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT)) return;
			throw new RuntimeError(op, "Operands must be numbers.");
		}

		
	}
}
