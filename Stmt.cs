using System;
using System.Collections.Generic;

namespace Compilers
{
    public abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R VisitExpressionStmt(Expression stmt);
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
            R VisitReadStmt(Read stmt);
            R VisitAssertStmt(Assert stmt);

            R VisitAssignStmt(Assign stmt);

            R VisitForStmt(For stmt);

        }

        public class Expression : Stmt
        {
            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public Expr Expr { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitExpressionStmt(this);
            }
        }

        public class Print : Stmt
        {
            public Print(Expr expr)
            {
                Expr = expr;
            }

            public Expr Expr { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }
        
        public class For : Stmt
        {
            public For(Token name, Expr beginvalue, Expr endvalue, List<Stmt> stmts)
            {
                Name = name;
                BeginValue = beginvalue;
                EndValue = endvalue;
                Stmts = stmts;
            }
            public Token Name { get; }
            public Expr BeginValue { get; }
            public Expr EndValue { get; }

            public List<Stmt> Stmts { get; }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitForStmt(this);
            }
        }

        public class Assert : Stmt
        {
            public Assert(Expr expr)
            {
                Expr = expr;
            }

            public Expr Expr { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssertStmt(this);
            }
        }

        public class Assign : Stmt
        {
            public Assign(Token name, Expr value)
            {
                Name = name;
                Value = value;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssignStmt(this);
            }

            public Expr Value { get; }
            public Token Name { get; }
        }

        public class Read : Stmt
        {
            public Read(Token token)
            {
                Token = token;
            }

            public Token Token { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitReadStmt(this);
            }
        }

        public class Var : Stmt
        {
            public Var (Token name, Expr initializer, TokenKind type)
            {
                Name = name;
                Initializer = initializer;
                Type = type;
            }

            public Token Name { get; }
            public Expr Initializer { get; set;  }
            public TokenKind Type { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }
        public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
