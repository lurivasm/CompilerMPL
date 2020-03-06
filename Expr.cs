using System;
using System.Collections.Generic;

namespace Compilers
{
    public abstract class Expr
    {
        public interface IVisitor<R>
        {
            R VisitAssignExpr(Assign expr);
            R VisitBinaryExpr(Binary expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitLogicalExpr(Logical expr);
            R VisitUnaryExpr(Unary expr);
            //R visitVariableExpr(Variable expr);
        }

        

        public class Binary : Expr
        {
            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                OperatorToken = op;
                Right = right;
            }

            public Expr Left { get; }
            public Token OperatorToken { get; }
            public Expr Right { get; }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitBinaryExpr(this);
            }
        }

          public class Unary : Expr
        {
            public Unary(Token op, Expr right)
            {
                OperatorToken = op;
                Right = right;
            }

            public Token OperatorToken { get; }
            public Expr Right { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitUnaryExpr(this);
            }
        }

          public class Literal : Expr
        {
            public Literal(Object value)
            {
                Value = value;
            }

            public Object Value { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }

        public class Logical : Expr
        {
            public Logical(Expr left, Token op, Expr right)
            {
                Left = left;
                OperatorToken = op;
                Right = right;
            }

            public Expr Left { get; }
            public Token OperatorToken { get; }
            public Expr Right { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLogicalExpr(this);
            }
        }

        public class Assign : Expr
        {
            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public Token Name { get; }
            public Expr Value { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public Expr Expression { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitGroupingExpr(this);
            }
        }

        public abstract R Accept<R>(IVisitor<R> visitor);
    }
}