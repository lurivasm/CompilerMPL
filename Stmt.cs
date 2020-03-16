using System;
using System.Collections.Generic;

namespace Compilers
{
    /**
     * Class Statement : it implements all the classes for each
     * statement with the visitor pattern
     */
    public abstract class Stmt
    {
        /**
         * Interface of the visitor pattern
         * One function per statement
         */
        public interface IVisitor<R>
        {
            R VisitPrintStmt(Print stmt);
            R VisitVarStmt(Var stmt);
            R VisitReadStmt(Read stmt);
            R VisitAssertStmt(Assert stmt);

            R VisitAssignStmt(Assign stmt);

            R VisitForStmt(For stmt);

        }

        /**
         * Class for the Print Statement : 'print' <expr>
         *      PrintToken : token which represents 'print'
         *      Expr : expression to print
         */
        public class Print : Stmt
        {
            public Print(Token printToken, Expr expr)
            {

                Expr = expr;
                PrintToken = printToken;

            }

            public Expr Expr { get; }

            public Token PrintToken { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitPrintStmt(this);
            }
        }

        /**
         * Class for the For Statement : 'for' <var_ident> 'in' <expr> '..' <expr> 'do' <stmts> 'end' 'for'
         *      Name : token for the variable used for counting
         *      BeginValue : starting value of the loop
         *      EndValue : ending value of the loop
         *      Stmts : list of statements inside the loop
         */
        public class For : Stmt
        {
            public For(Token forToken, Token name, Expr beginvalue, Expr endvalue, List<Stmt> stmts)
            {
                Name = name;
                BeginValue = beginvalue;
                EndValue = endvalue;
                Stmts = stmts;
                ForToken = forToken;
            }
            public Token Name { get; }
            public Expr BeginValue { get; }
            public Expr EndValue { get; }
            public Token ForToken { get; }

            public List<Stmt> Stmts { get; }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitForStmt(this);
            }
        }

        /**
         * Class for the Assert Statement : 'assert' '(' <expr> ')'
         *      AssertToken : token which represents 'assert'
         *      Expr : expression to assert
         */
        public class Assert : Stmt
        {
            public Assert(Token assertToken, Expr expr)
            {
                AssertToken = assertToken;
                Expr = expr;
            }

            public Expr Expr { get; }

            public Token AssertToken { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitAssertStmt(this);
            }
        }

        /**
         * Class for the Assign Statement : <var_ident> ':=' <expr>
         *      Name : token for the identifier
         *      Expr : expression to assign to the identifier
         */
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

        /**
         * Class for the Read Statement : 'read' <var_ident>
         *      Token : token of the identifier where store the read text
         */
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

        /**
         * Class for the Variable Statement : 'var' <var_ident> ':' <type> [ ':=' <expr> ]
         *      Name : token for the identifier
         *      Type : type of the variable (int, string or bool)
         *      Initializer : ini value for the variable (null if empty)
         */
        public class Var : Stmt
        {
            public Var(Token name, Expr initializer, TokenKind type)
            {
                Name = name;
                Initializer = initializer;
                Type = type;
            }

            public Token Name { get; }
            public Expr Initializer { get; set; }
            public TokenKind Type { get; }
            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.VisitVarStmt(this);
            }
        }

        /**
         * Function Accept : for accepting the statements 
         * with the visitor pattern
         */
        public abstract R Accept<R>(IVisitor<R> visitor);
    }
}
