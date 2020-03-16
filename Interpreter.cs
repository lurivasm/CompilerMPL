using System;
using System.Collections.Generic;

namespace Compilers
{
    /**
	 * Class Interpreter : it evaluates all the statements and expressions and 
	 * execute them, throwing errors in case of runtime errors
	 */
    public class Interpreter : Expr.IVisitor<Value>, Stmt.IVisitor<Object>
    {
        /**
		 * SymbleTable for storing all the declared variables
		 *		String name : name of the variable
		 *		Value : class that stores the value of the expression and its type (int, bool, string)
		 */
        private Dictionary<string, Value> SymbleTable = new Dictionary<string, Value>();

        /**
		 * Function Interpret : Main Function of the class
		 * It catches the Runtime Errors
		 * Param : all the statements to execute and evaluate
		 */
        public void Interpret(List<Stmt> statements)
        {
            try {
                if (statements.Count == 0) {
                    Token token = new Token(TokenKind.EndOfFile, 1, "", null);
                    throw new RuntimeError(token, "Empty program, at least one valid statement.");
                }
                printer = new AstPrinter(SymbleTable);
                foreach (Stmt stmt in statements) {
                    Execute(stmt);
                }
            }
            catch (RuntimeError error) {
                Program.RuntimeError(error);
            }
        }

        /**
		 * Used for printing declared variables in the symble table
		 */
        private AstPrinter printer;


        /**
		 * Function Execute : calls all the accepts in the visitor pattern for the statements
		 * Param : stmt to accept
		 */
        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        /**
		 * Function Evaluate : it accepts an expression in the interface
		 * Param : expression to accept
		 * Return : the value of the accept in the interface
		 */
        private Value Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }


        /*****************************************************************
		 *               FUNCTIONS FOR STATEMENTS                        *
		 *****************************************************************/

        /**
		 * Function VisitPrintStmt --> "print" <expr>
		 * It evaluates the expression and prints it in the command line
		 * Param : print statement to evaluate and print
		 */
        public Object VisitPrintStmt(Stmt.Print stmt)
        {
            Value value = Evaluate(stmt.Expr);
            Console.Write(value.Val.ToString());
            return null;
        }

        /**
		 * Function VisitVarStmt --> "var" <var_ident> ":" <type> [ ":=" <expr> ]
		 * It adds the new variable to the SymbleTable
		 * If it already exists throws an error
		 * Param : var statement to evaluate
		 */
        public Object VisitVarStmt(Stmt.Var stmt)
        {
            String name = stmt.Name.Text;
            VALTYPE type = ExpectedType(stmt);

            /* Case the variable is already in the symble table */
            if (SymbleTable.ContainsKey(name)) {
                throw new RuntimeError(stmt.Name, "This variable already exists.");
            }

            /* Case the variable is not initialized : the value is null */
            else if (stmt.Initializer is null) {
                SymbleTable.Add(name, new Value(type, null));
            }

            /* Case the variable is initialized : evaluate the ini expression */
            else {
                SymbleTable.Add(name, Evaluate(stmt.Initializer));
            }
            return null;
        }

        /**
		 * Function ExpectedType : it checks the token type of the variable (int,
		 * string or bool) and it returns its corresponding VALTYPE.
		 * Param : var statement to check
		 * Return : the corresponding VALTYPE to each TokenKind or an error
		 */
        private VALTYPE ExpectedType(Stmt.Var stmt)
        {
            switch (stmt.Type) {
                case TokenKind.Int: return VALTYPE.INT;
                case TokenKind.String: return VALTYPE.STRING;
                case TokenKind.Bool: return VALTYPE.BOOL;
                default:
                    throw new RuntimeError(stmt.Name, "Cannot declarate " + stmt.Type.ToString() + " as a variable.");
            }
        }

        /**
		 * Function VisitReadStmt : it reads the new value for our variable
		 * and checks both if the variable was previous declared and it has the same
		 * type as the read value
		 * Param : stmt with the new value for our variable
		 */
        public Object VisitReadStmt(Stmt.Read stmt)
        {
            /* If the variable already exists and it is the proper type we update the symble table */
            if (SymbleTable.ContainsKey(stmt.Token.Text)) {
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
            if (value.Equals(VALTYPE.INT)) {
                if (int.TryParse(obj, out int output) == true) {
                    return new Value(VALTYPE.INT, output);
                }

                throw new RuntimeError(token, "Read value " + obj + " is not the proper type.");
            }

            /* If the value is a string just return it */
            else if (value.Equals(VALTYPE.STRING)) {
                return new Value(VALTYPE.STRING, obj);
            }

            /* We cannot read bools */
            else {
                throw new RuntimeError(token, "Read value " + obj + " is not the proper type.");
            }
        }

        /**
		 * Function VisitAssertStmt --> "assert" "(" <expr> ")"
		 * It evaluates the expression and prints it in the command line it it is false
		 */
        public Object VisitAssertStmt(Stmt.Assert stmt)
        {
            Value right = Evaluate(stmt.Expr);
            if (right.Type.Equals(VALTYPE.BOOL)) {
                if (!(bool)right.Val) {
                    Console.WriteLine("Assert failed:\tExpr: " + printer.Print(stmt.Expr) + " is false.");
                }
            }
            /* It should be unreachable, type system checks it */
            else {
                throw new RuntimeError(stmt.AssertToken, "Expression does not have the right type. Expected BOOL, got " + right.Type.ToString());
            }
            return null;
        }

        /**
		 * Function VisitAssignStmt --> <var_ident> := <expr>
		 * It updates the value of the identifier in the SymbleTable
		 * Throws an error if the variable was not previous declarated
		 * Param : assign statement to evaluate
		 */
        public Object VisitAssignStmt(Stmt.Assign stmt)
        {
            String name = stmt.Name.Text;

            /* If the var is already in the table we change its value */
            if (SymbleTable.ContainsKey(name)) {
                Value val = Evaluate(stmt.Value);
                if (val.Type.Equals(SymbleTable[name].Type)) {
                    SymbleTable[name] = val;
                }
                /* If the expected type and the type of the new assign do not match */
                else {
                    throw new RuntimeError(stmt.Name, "Expected " + SymbleTable[name].Type.ToString()
                                                       + " but found " + val.Type.ToString());
                }
            }
            /* Otherwise it was not declarated before so it is an error */
            else {
                throw new RuntimeError(stmt.Name, "This variable does not exist. Not prevoius declared.");
            }
            return null;
        }

        /**
		 * Function VisitForStmt --> "for" <var_ident> "in" <expr> ".." <expr> "do" <stmts> "end" "for"
		 * It evaluates the beginning and ending value for the variable and interpret each statement in
		 * the list, also it updates the value of the variable used to count in the symble table
         * Also it checks that the loop has at least one statement
		 * Param : for statement to evaluate
		 */
        public Object VisitForStmt(Stmt.For stmt)
        {
            int variable;
            string name = stmt.Name.Text;
            int beginvalue = (int)Evaluate(stmt.BeginValue).Val;
            int endvalue = (int)Evaluate(stmt.EndValue).Val;

            /* At least one statement */
            if (stmt.Stmts.Count == 0) {
                throw new RuntimeError(stmt.Name, "For loop must have at least one statement.");
            }
            /* Executing all the statements */
            for (variable = beginvalue; variable <= endvalue; variable++) {
                SymbleTable[name] = new Value(VALTYPE.INT, variable);
                foreach (Stmt st in stmt.Stmts) {
                    Execute(st);
                }
            }
            return null;
        }

        /*****************************************************************
		 *               FUNCTIONS FOR EXPRESSIONS                       *
		 *****************************************************************/

        /**
		 * Function VisitBinaryExpr : it evaluates both right and left expressions
		 * of the binary expression, checking that both operands are the same type
		 * and returns the value of them depending on the operator
		 * Param : binary expression to evaluate
		 * Return : the value of the binary expression
		 */
        public Value VisitBinaryExpr(Expr.Binary expr)
        {
            Value left = Evaluate(expr.Left);
            Value right = Evaluate(expr.Right);

            switch (expr.OperatorToken.Kind) {
                /* Case '-' for only numbers */
                case TokenKind.Minus:
                    CheckNumberOperand(expr.OperatorToken, left, right);
                    return new Value(VALTYPE.INT, (int)left.Val - (int)right.Val);

                /* Case '*' for only numbers */
                case TokenKind.Mult:
                    CheckNumberOperand(expr.OperatorToken, left, right);
                    return new Value(VALTYPE.INT, (int)left.Val * (int)right.Val);

                /* Case '/' for only numbers */
                case TokenKind.Div:
                    CheckNumberOperand(expr.OperatorToken, left, right);
                    return new Value(VALTYPE.INT, (int)left.Val / (int)right.Val);

                /* Case '+' for both numbers and strings */
                case TokenKind.Sum:
                    if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT)) {
                        return new Value(VALTYPE.INT, (int)left.Val + (int)right.Val);
                    }
                    else if (left.Type.Equals(VALTYPE.STRING) && right.Type.Equals(VALTYPE.STRING)) {
                        return new Value(VALTYPE.STRING, (string)left.Val + (string)right.Val);
                    }
                    break;

                /* Case '=' for both numbers, strings or bools */
                case TokenKind.Equal:
                    if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT)) {
                        return new Value(VALTYPE.BOOL, ((int)left.Val == (int)right.Val));
                    }
                    else if (left.Type.Equals(VALTYPE.STRING) && right.Type.Equals(VALTYPE.STRING)) {
                        return new Value(VALTYPE.BOOL, ((string)left.Val == (string)right.Val));
                    }
                    else if (left.Type.Equals(VALTYPE.BOOL) && right.Type.Equals(VALTYPE.BOOL)) {
                        return new Value(VALTYPE.BOOL, ((bool)left.Val == (bool)right.Val));
                    }
                    break;

                /* Case '<' for both numbers, strings or bools */
                case TokenKind.Less:
                    if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT)) {
                        return new Value(VALTYPE.BOOL, ((int)left.Val < (int)right.Val));
                    }
                    else if (left.Type.Equals(VALTYPE.STRING) && right.Type.Equals(VALTYPE.STRING)) {
                        return new Value(VALTYPE.BOOL, (string.Compare((string)left.Val, (string)right.Val) == -1));
                    }
                    else if (left.Type.Equals(VALTYPE.BOOL) && right.Type.Equals(VALTYPE.BOOL)) {
                        return new Value(VALTYPE.BOOL, (((bool)left.Val).CompareTo((bool)right.Val) < 0));
                    }
                    break;
            }
            /* Error if the operands are different type */
            throw new RuntimeError(expr.OperatorToken, "Operands must be the same type.");
        }

        /**
		 * Function CheckNumberOperand : it checks if the value of the right and left
		 * expressions are type INT, otherwise it throws an error
		 * Param : op token of the operator
		 *         left value of the binaryexpression
		 *         right value of the binary expression
		 * 
		 */
        private void CheckNumberOperand(Token op, Value left, Value right)
        {
            if (left.Type.Equals(VALTYPE.INT) && right.Type.Equals(VALTYPE.INT)) return;
            throw new RuntimeError(op, "Operands must be numbers.");
        }

        /**
		 * Funciton VisitGroupingExpr --> '(' <expr> ')'
		 * Param : grouping expression to evaluate
		 * Return : the evaluated expression
		 */
        public Value VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        /**
		 * Function VisitIdentExpr : it evaluates an identifier expression
		 * Param : identifier expression to evaluate
		 * Return : the value of the identifier in the symble table or 
		 *			throws an error if the var was not previous declared
		 */
        public Value VisitIdentExpr(Expr.Ident expr)
        {
            String name = expr.Name.Text;
            if (SymbleTable.ContainsKey(name)) {
                Value value = SymbleTable[name];
                if (value.Val == null) {
                    throw new RuntimeError(expr.Name, name + " is not initialized.");
                }
                return (SymbleTable[name]);
            }
            else {
                throw new RuntimeError(expr.Name, "Not declared variable.");
            }
        }

        /**
		 * Function VisitLiteralExpr
		 * Param : literal expression to evaluate
		 * Return : value of the expression
		 */
        public Value VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value;
        }

        /**
		 * Function VisitLogicalExpr
		 * Param : logical expression to evaluate
		 * Return : the left expression if it is already false, so the and is not valid
		 *          or the right expression if the left is true
		 */
        public Value VisitLogicalExpr(Expr.Logical expr)
        {
            /* Check that the operator is an AND */
            if (expr.OperatorToken.Kind.Equals(TokenKind.And)) {
                Value left = Evaluate(expr.Left);

                /* If the left operand is BOOL */
                if (left.Type.Equals(VALTYPE.BOOL)) {
                    if (!(bool)left.Val) {
                        return left;
                    }
                    Value right = Evaluate(expr.Right);

                    /* Check that the right expression is BOOL and return it */
                    if (right.Type.Equals(VALTYPE.BOOL)) {
                        return right;
                    }
                    /* If the right operand is not BOOL */
                    else {
                        throw new RuntimeError(expr.OperatorToken, "Expected a boolean as the rightoperand of and '&'.");
                    }
                }
                /* If the left operand is not BOOL */
                else {
                    throw new RuntimeError(expr.OperatorToken, "Expected a boolean as the leftoperand of and '&'.");
                }
            }
            /* If the operator token is not AND */
            else {
                throw new RuntimeError(expr.OperatorToken, "Got a logical operator that is not '&'.");
            }
        }

        /**
		 * Function VisitUnaryExpr
		 * Param : unary expression to evaluate
		 * Return : the negated value of the right operand or error
		 */
        public Value VisitUnaryExpr(Expr.Unary expr)
        {
            Value right = Evaluate(expr.Right);
            /* If the operator is NOT and the right operand is BOOL */
            if (expr.OperatorToken.Kind == TokenKind.Not) {
                if (right.Type.Equals(VALTYPE.BOOL)) {
                    return new Value(VALTYPE.BOOL, !((bool)right.Val));
                }
                /* If the right operand is not BOOL throw an error */
                else {
                    throw new RuntimeError(expr.OperatorToken, "Trying to negate a non bool value");
                }
            }
            return null;
        }
    }
}
