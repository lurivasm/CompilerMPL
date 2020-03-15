using System;
using System.Collections.Generic;

namespace Compilers
{
    /**
     * Class Expr : it implements all the classes for each expression
     * with the visitor pattern
     */
    public abstract class Expr
    {
        /**
         * Interface of the visitor pattern
         * One function per expression
         */
        public interface IVisitor<R>
        {
            R VisitIdentExpr(Ident expr);
            R VisitBinaryExpr(Binary expr);
            R VisitGroupingExpr(Grouping expr);
            R VisitLiteralExpr(Literal expr);
            R VisitLogicalExpr(Logical expr);
            R VisitUnaryExpr(Unary expr);
        }

        /**
         * Class for the binary expressions : <left> <operator> <right>
         *      Left : left operand of the expression
         *      OperatorToken : +, -, *, /, <, =
         *      Right : right operand of the expression
         */
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

        /**
         * Class for the unary expressions : <operator> <right>
         *      OperatorToken : ! (not)
         *      Right : right operand of the expression
         */
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

        /**
         * Class for the literal expressions 
         *      Value : value of the evaluated expression
         */
        public class Literal : Expr
        {
            public Literal(Value value)
            {
                Value = value;
            }

            public Value Value { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitLiteralExpr(this);
            }
        }

        /**
         * Class for the logical expressions : <left> <operator> <right>
         *      Left : left operand of the expression
         *      OperatorToken : & (and)
         *      Right : right operand of the expression
         */
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

        /**
         * Class for the identifiers expressions
         *      Name : name of the identifier
         *      Value : value of the identifier
         */
        public class Ident : Expr
        {
            public Ident(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public Token Name { get; }
            public Expr Value { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitIdentExpr(this);
            }
        }

        /**
         * Class for the grouping expressions : '(' <expr> ')'
         *      Expression : expression inside the brackets
         */
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

        /**
         * Function Accept : function for accepting the expressions
         */
        public abstract R Accept<R>(IVisitor<R> visitor);
    }
}