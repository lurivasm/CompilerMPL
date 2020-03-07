using System;
using System.Collections.Generic;

namespace Compilers
{
    public class TypeChecker : Expr.IVisitor<VALTYPE>, Stmt.IVisitor<Object>
    {

        public void CheckTypes(List<Stmt> stmts)
        {
            foreach(Stmt stmt in stmts)
            {
                try
                {
                    stmt.Accept(this);
                } catch (TypeError error)
                {
                    Program.TypeError(error);
                }
            }
        }


        private Dictionary<string, VALTYPE> SymbleTypeTable = new Dictionary<string, VALTYPE>();

        private VALTYPE GetType(Expr expr)
        {
            return expr.Accept(this);
        }

        public object VisitAssertStmt(Stmt.Assert stmt)
        {
            VALTYPE right = GetType(stmt.Expr);
            if (right.Equals(VALTYPE.BOOL))
                return null;
            else
                throw new TypeError(stmt.AssertToken, "Asserts can only be done on boolean, got " + right.ToString() + " instead.");
        }

        public object VisitAssignStmt(Stmt.Assign stmt)
        {
            if (illegalAssignments.Contains(stmt.Name.Text))
                throw new TypeError(stmt.Name, "Cannot assign to " + stmt.Name.Text + " in a for loop where it is used as a loopcounter.");
            VALTYPE right = GetType(stmt.Value);
            if (right.Equals(SymbleTypeTable[stmt.Name.Text]))
                return null;
            else
                throw new TypeError(stmt.Name, stmt.Name.Text + " has type " + SymbleTypeTable[stmt.Name.Text] + " but was tried to assign type: " + right.ToString());
        }

        public List<TokenKind> matching = new List<TokenKind> {TokenKind.Equal, TokenKind.Less};
        public List<TokenKind> numbers  = new List<TokenKind> { TokenKind.Minus, TokenKind.Mult, TokenKind.Div };
        public List<TokenKind> bools = new List<TokenKind> { TokenKind.And };

        public VALTYPE VisitBinaryExpr(Expr.Binary expr)
        {
            VALTYPE left = GetType(expr.Left);
            VALTYPE right = GetType(expr.Right);

            if (matching.Contains(expr.OperatorToken.Kind)){
                if (left.Equals(right))
                {
                    return VALTYPE.BOOL;
                }
                else
                {
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text + "' arguments should have the same type.");
                }
            } else if (numbers.Contains(expr.OperatorToken.Kind))
            {
                if (left.Equals(right) && left.Equals(VALTYPE.INT))
                {
                    return VALTYPE.INT;
                }
                else
                {
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text + "' arguments should both be " +
                        VALTYPE.INT.ToString() + " got " + left.ToString() + " and " + right.ToString() + " instead.");
                }
            } else if (bools.Contains(expr.OperatorToken.Kind))
            {
                if (left.Equals(right) && left.Equals(VALTYPE.BOOL))
                {
                    return VALTYPE.BOOL;
                }
                else
                {
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text + "' arguments should both be " +
                        VALTYPE.BOOL.ToString() + " got " + left.ToString() + " and " + right.ToString() + " instead."); //TODO remove duplication
                }
            } else if (expr.OperatorToken.Kind.Equals(TokenKind.Sum))
            {
                if (left.Equals(right))
                {
                    if (!left.Equals(VALTYPE.BOOL))
                    {
                        return left;
                    }
                    else
                    {
                        throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text + "' arguments should both be " +
                                  VALTYPE.INT.ToString() + ", got twice " + left.ToString() + " instead.");
                    }
                }
                else
                {
                    throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text + "' arguments should both be either" +
                                VALTYPE.INT.ToString() + " or " + VALTYPE.STRING.ToString() + ", got " + left.ToString() + " and " + right.ToString() + " instead.");
                }
            }

            throw new TypeError(expr.OperatorToken , "Didn't get the right types"); //Unreachable
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            return GetType(stmt.Expr); //TODO clean up, never reached
        }

        private List<string> illegalAssignments = new List<string>();

        public object VisitForStmt(Stmt.For stmt)
        {
            if (illegalAssignments.Contains(stmt.Name.Text))
                throw new TypeError(stmt.Name, "Cannot use " + stmt.Name.Text + " as a loop counter since it is already in use as a loop counter.");
            illegalAssignments.Add(stmt.Name.Text);
            VALTYPE begin = GetType(stmt.BeginValue);
            VALTYPE end = GetType(stmt.EndValue);
            
            if (begin.Equals(VALTYPE.INT) && end.Equals(VALTYPE.INT))
            {
                //EVALuate;

                illegalAssignments.Remove(stmt.Name.Text);
                return null;
            } else
            {
                throw new TypeError(stmt.Name, "The begin and end expression should be " + VALTYPE.INT.ToString() + ", got " + begin.ToString() + " and " + end.ToString() + " instead."); // TODO change to FOR token
            }

        }

        public VALTYPE VisitGroupingExpr(Expr.Grouping expr)
        {
            return GetType(expr.Expression);
        }

        public VALTYPE VisitIdentExpr(Expr.Ident expr)
        {
            if (SymbleTypeTable.ContainsKey(expr.Name.Text))
            {
                return SymbleTypeTable[expr.Name.Text];
            }
            else
            {
                throw new TypeError(expr.Name, expr.Name.Text + " got reference before it was declared");
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

            if (left.Equals(right))
            {
                return left;
            }
            else
            {
                throw new TypeError(expr.OperatorToken, "'" + expr.OperatorToken.Text + "' expects two expression of the same type, but it got " + left.ToString() + " and " + right.ToString());
            }
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            return null; //never fails
        }

        public object VisitReadStmt(Stmt.Read stmt)
        {
            if (illegalAssignments.Contains(stmt.Token.Text))
                throw new TypeError(stmt.Token, "Cannot assign to " + stmt.Token.Text + " in a for loop where it is used as a loopcounter.");
            return null;
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

        public object VisitVarStmt(Stmt.Var stmt)
        {

            if (SymbleTypeTable.ContainsKey(stmt.Name.Text))
            {
                throw new TypeError(stmt.Name, "Cannot initialize " + stmt.Name.Text + " more than once.");
            }
            else
            {
                VALTYPE type;
                switch (stmt.Type)
                {
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
                        throw new TypeError(stmt.Name, "Expected a type");

                }

                SymbleTypeTable.Add(stmt.Name.Text, type);

                if (stmt.Initializer != null)
                {
                    VALTYPE right = GetType(stmt.Initializer);

                    if (SymbleTypeTable[stmt.Name.Text].Equals(right))
                    {
                        return null;
                    }
                    else
                    {
                        throw new TypeError(stmt.Name, stmt.Name.Text + " is of type " + SymbleTypeTable[stmt.Name.Text] + ", but got " + right.ToString() + " instead.");
                    }
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
