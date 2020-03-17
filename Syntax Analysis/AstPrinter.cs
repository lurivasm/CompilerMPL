using System;
using System.Collections.Generic;
using System.Text;

namespace Compilers
{
    /**
     * Class AstPrinter : methods for building parser expressions trees
     * It implements the visitor interface
     */
    class AstPrinter : Expr.IVisitor<String>
    {

        /**
         * Constructor of the class
         * Param : dictionary made of strings and values of the expressions
         */
        public AstPrinter(Dictionary<string, Value> environment)
        {
            Environment = environment;
        }

        /**
         * Constructor of the class : the environment is null
         */
        public AstPrinter()
        {
            Environment = null;
        }

        /**
         * Dictionary made of strings and values 
         */
        private Dictionary<string, Value> Environment { get; }

        /**
         * Function Print : it accepts the param expression
         * Param : expression to print as a tree
         * Return : string to print
         */
        public String Print(Expr expr)
        {
            return expr.Accept(this);
        }

        /**
         * Function Parenthesize : it takes a name and a list of subexpressions
         * and wraps them all up in parenthesis
         * Param : name of the expression
         *         list of subexpressions
         * Return : string of the expressions inside parenthesis
         */
        private String Parenthesize(String name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs) {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }

        /**
         * Function VisitBinaryExpression
         * Param : binary expression to visit
         * Return : the parse tree of the left and right with the operator
         */
        public String VisitBinaryExpr(Expr.Binary expr)
        {
            return Print(expr.Left) + " " + expr.OperatorToken.Text + " " + Print(expr.Right);
        }

        /**
         * Function VisitGroupingExpr
         * Param : grouping expression to visit
         * Return : string with the expression inside parenthesis
         */
        public String VisitGroupingExpr(Expr.Grouping expr)
        {
            return "(" + Print(expr.Expression) + ")";
        }

        /**
         * Function VisitLiteralExpr
         * Param : literal expression to visit
         * Return : null if the value is null or the string of the value
         */
        public String VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "null";
            return expr.Value.Val.ToString();
        }

        /**
         * Function VisitUnaryExpr
         * Param : unary expression to visit
         * Return : parenthesized string of the value
         */
        public String VisitUnaryExpr(Expr.Unary expr)
        {
            return Parenthesize(expr.OperatorToken.Text, expr.Right);
        }

        /**
         * Function VisitIdentExpr
         * Param : identifier expression to visit
         * Return : text if environment is null or the value inside the environment
         */
        public string VisitIdentExpr(Expr.Ident expr)
        {
            if (Environment == null) {
                return expr.Name.Text;
            }
            else {
                return expr.Name.Text + "(=" + Environment[expr.Name.Text].Val.ToString() + ")";
            }

        }

        /**
         * Function VisitLogicalExpr
         * Param : logical expression to visit
         * Return : the tree with the right and left operands
         */
        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return Print(expr.Left) + " " + expr.OperatorToken.Text + " " + Print(expr.Right);
        }
    }


}
