using System;
using System.Collections.Generic;

namespace Compilers
{
    /**
	 * Class TypeChecker : it evaluates and checks all the types of
	 * the variables we can have, throwing type errors 
	 */
    public class TypeChecker : Expr.IVisitor<VALTYPE>, Stmt.IVisitor<Object>
    {
        /**
         * Class CheckTypes : Main Function of the Class
         * It accepts all the statements of the list and catch the errors
         * Param : list of statements to accept
         */
        public void CheckTypes(List<Stmt> stmts)
        {
            foreach (Stmt stmt in stmts) {
                try {
                    stmt.Accept(this);
                }
                catch (TypeError error) {
                    Program.TypeError(error);
                }
            }
        }

        /**
         * Dictionary SymbleTypeTable : symble table to store the types and values of vars
         */
        private Dictionary<string, VALTYPE> SymbleTypeTable = new Dictionary<string, VALTYPE>();

        /**
         * Function GetType
         * Param : expr to accept
         * Return : VALTYPE of the expression
         */
        private VALTYPE GetType(Expr expr)
        {
            return expr.Accept(this);
        }

        /*****************************************************************
		 *               FUNCTIONS FOR STATEMENTS                        *
		 *****************************************************************/

        /**
         * List illegalAssigments : the elements in this list are the variables
         * in used  for the loops so that they cannot be assigned a new value
         * or used in a secondary loop inside the main loop
         * The elements are added when the loop begins and removed when it ends
         */
        private List<string> illegalAssignments = new List<string>();

        /**
         * Function VisitVarStmt : it adds a new variable to the table and checks :
         *      The variable was not previous declared
         *      The variable and the initialized value have the same type
         * Param : var statement to check
         */
        public object VisitVarStmt(Stmt.Var stmt)
        {
            /* Error if the identifier has been initialized before */
            if (SymbleTypeTable.ContainsKey(stmt.Name.Text)) {
                throw new TypeError(stmt.Name, "Cannot initialize " + stmt.Name.Text + " more than once.");
            }
            else {
                /* Check the VALTYPE of the variable and add it to the symble table */
                VALTYPE type;
                switch (stmt.Type) {
                    case TokenKind.Int:
                        type = VALTYPE.INT;
                        break;
                    case TokenKind.String:
                        type = VALTYPE.STRING;
                        break;
                    case TokenKind.Bool:
                        type = VALTYPE.BOOL;
                        break;
                    default:
                        throw new TypeError(stmt.Name, "Expected a type.");
                }

                SymbleTypeTable.Add(stmt.Name.Text, type);

                /* If the variable is gonna be initialized we check the types */
                if (stmt.Initializer != null) {
                    VALTYPE right = GetType(stmt.Initializer);
                    if (SymbleTypeTable[stmt.Name.Text].Equals(right)) {
                        return null;
                    }
                    /* The type of the identifier and the initialized value are not the same */
                    else {
                        throw new TypeError(stmt.Name, stmt.Name.Text + " is of type " + SymbleTypeTable[stmt.Name.Text] + ", but got " + right.ToString() + " instead.");
                    }
                }
                /* The variable is initialized to null so we don't check types */
                else {
                    return null;
                }
            }
        }

        /**
         * Function AssertStmt : it checks that the expression we are gonna
         * assert is of type BOOL, otherwise it throws an error
         * Param : assert statement to check
         */
        public object VisitAssertStmt(Stmt.Assert stmt)
        {
            VALTYPE right = GetType(stmt.Expr);
            if (right.Equals(VALTYPE.BOOL))
                return null;
            else
                throw new TypeError(stmt.AssertToken, "Asserts can only be done on boolean, got " + right.ToString() + " instead.");
        }

        /**
         * Function VisitAssignStmt : it checks that the variable and the assignment
         * has the same type. Also it throws an error if we try to assign a new value
         * for the loopcounter inside the loop or if the type is wrong
         * Param : assign statement to check
         */
        public object VisitAssignStmt(Stmt.Assign stmt)
        {
            /* Error when trying to assign to the loopcounter inside the loop */
            if (illegalAssignments.Contains(stmt.Name.Text))
                throw new TypeError(stmt.Name, "Cannot assign to " + stmt.Name.Text + " in a for loop where it is used as a loopcounter.");

            /* Check that the var and the assigment have same type */
            VALTYPE right = GetType(stmt.Value);
            if (right.Equals(SymbleTypeTable[stmt.Name.Text]))
                return null;
            else
                throw new TypeError(stmt.Name, stmt.Name.Text + " has type " + SymbleTypeTable[stmt.Name.Text] + " but was tried to assign type: " + right.ToString());
        }

        /**
         * Function VisitForStatement : 
         * Param : for statement to check
         */
        public object VisitForStmt(Stmt.For stmt)
        {
            /* Error when trying to use the var of the main loop in a loop inside it */
            if (illegalAssignments.Contains(stmt.Name.Text)) {
                throw new TypeError(stmt.Name, "Cannot use " + stmt.Name.Text + " as a loop counter since it is already in use as a loop counter.");
            }

            /* Add the loopcounter to the illegalAssigments list */
            illegalAssignments.Add(stmt.Name.Text);
            VALTYPE begin = GetType(stmt.BeginValue);
            VALTYPE end = GetType(stmt.EndValue);

            /* If the begin and end value are INT we check the types of the inside statements */
            if (begin.Equals(VALTYPE.INT) && end.Equals(VALTYPE.INT)) {
                CheckTypes(stmt.Stmts);
                illegalAssignments.Remove(stmt.Name.Text);
                return null;
            }
            /* The begin or the end value were not INT type */
            else {
                throw new TypeError(stmt.Name, "The begin and end expression should be " + VALTYPE.INT.ToString() + ", got " + begin.ToString() + " and " + end.ToString() + " instead."); // TODO change to FOR token
            }

        }

        /**
         * Function VisitExpressionStatement :
         */
        //public object VisitExpressionStmt(Stmt.Expression stmt)
        //{
        //    return GetType(stmt.Expr); //TODO clean up, never reached
        //}

        /**
         * Function VisitPrintStmt : it never fails
         * Param : print statement to check
         */
        public object VisitPrintStmt(Stmt.Print stmt)
        {
            return null;
        }

        /**
         * Function VisitReadStatement :
         * Param : read statement to check
         */
        public object VisitReadStmt(Stmt.Read stmt)
        {
            /* Error if the variable was not previous declared */
            if (!SymbleTypeTable.ContainsKey(stmt.Token.Text))
                throw new TypeError(stmt.Token, "Cannot read to a variable before it was declared.");

            /* Error if we try to read the loopcounter inside the loop */
            if (illegalAssignments.Contains(stmt.Token.Text))
                throw new TypeError(stmt.Token, "Cannot assign to " + stmt.Token.Text + " in a for loop where it is used as a loopcounter.");
            return null;
        }


        /*****************************************************************
		 *               FUNCTIONS FOR EXPRESSIONS                       *
		 *****************************************************************/

        /* List Matching : contains '=' and '<' */
        public List<TokenKind> matching = new List<TokenKind> { TokenKind.Equal, TokenKind.Less };
        /* List Number : contains '-', '*', '/' */
        public List<TokenKind> numbers = new List<TokenKind> { TokenKind.Minus, TokenKind.Mult, TokenKind.Div };
        /* List Bools : contains '&' */
        public List<TokenKind> bools = new List<TokenKind> { TokenKind.And };

        /**
         * Function VisitBinaryExpr : it checks :
         *      Right and left operands are the same type
         *      For '+' only INT and STRING are allowed
         *      For '-', '*', '/' only INT are allowed
         * Param : binary expression to check
         * Return : type of the right and left operands or error
         */
        public VALTYPE VisitBinaryExpr(Expr.Binary expr)
        {
            VALTYPE left = GetType(expr.Left);
            VALTYPE right = GetType(expr.Right);

            /* Case the binary operator is '=' or '<' */
            if (matching.Contains(expr.OperatorToken.Kind)) {
                /* If left and right are the same type return BOOL */
                if (left.Equals(right))
                    return VALTYPE.BOOL;

                /* Error not same type */
                else
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text
                                        + "' arguments should have the same type.");
            }

            /* Case the binary operator is '-', '*' or '/' */
            else if (numbers.Contains(expr.OperatorToken.Kind)) {
                /* Case left and right are both INT */
                if (left.Equals(right) && left.Equals(VALTYPE.INT))
                    return VALTYPE.INT;

                /* Error not same type*/
                else {
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text
                                        + "' arguments should both be " + VALTYPE.INT.ToString() + ", got "
                                        + left.ToString() + " and " + right.ToString() + " instead.");
                }
            }

            /* Case the binary operator is '+' */
            else if (expr.OperatorToken.Kind.Equals(TokenKind.Sum)) {
                /* If left and right are the same type */
                if (left.Equals(right)) {
                    /* If the type is not bool we return the type they are */
                    if (!left.Equals(VALTYPE.BOOL)) {
                        return left;
                    }
                    /* Error it cannot happen BOOL + BOOL */
                    else {
                        throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text
                                            + "' arguments should both be " + VALTYPE.INT.ToString()
                                            + ", got twice " + left.ToString() + " instead.");
                    }
                }
                /* Error not same type */
                else {
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text
                                        + "' arguments should both be either " + VALTYPE.INT.ToString()
                                        + " or " + VALTYPE.STRING.ToString() + ", got " + left.ToString()
                                        + " and " + right.ToString() + " instead.");
                }
            }

            throw new TypeError(expr.OperatorToken, "Didn't get the right types."); /* Unreachable */
        }

        /**
         * Function VisitGroupingExpr
         * Param : grouping expression to check
         * Return : the type of the expression inside brackets
         */
        public VALTYPE VisitGroupingExpr(Expr.Grouping expr)
        {
            return GetType(expr.Expression);
        }

        /**
         * Function VisitIdentExpr : it checks if the variable exists in 
         * the table (previous declared) and returns its value.
         * Otherwise it throws an error cause it was not declared
         * Param : identifier expression
         * Return : the type of the identifier
         */
        public VALTYPE VisitIdentExpr(Expr.Ident expr)
        {
            if (SymbleTypeTable.ContainsKey(expr.Name.Text)) {
                return SymbleTypeTable[expr.Name.Text];
            }
            else {
                throw new TypeError(expr.Name, expr.Name.Text + " got referenced before it was declared.");
            }
        }


        public VALTYPE VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.Value.Type;
        }

        public VALTYPE VisitLogicalExpr(Expr.Logical expr)
        {
            VALTYPE left = GetType(expr.Left);
            VALTYPE right = GetType(expr.Right);

            if (left.Equals(right)) {
                return left;
            }

            else {
                throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text
                                     + "' expects two expression of the same type, but it got "
                                    + left.ToString() + " and " + right.ToString());
            }
        }


        public VALTYPE VisitUnaryExpr(Expr.Unary expr)
        {
            VALTYPE right = GetType(expr.Right);
            if (expr.OperatorToken.Kind == TokenKind.Not)
                if (right.Equals(VALTYPE.BOOL))
                    return right;
                else
                    throw new TypeError(expr.OperatorToken, "Expected a BOOL, got " + right.ToString() + " instead.");
            else
                return right;
        }


    }
}
