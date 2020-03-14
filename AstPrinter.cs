using System;
using System.Collections.Generic;
using System.Text;

namespace Compilers
{
    class AstPrinter : Expr.IVisitor<String>
    {

        public AstPrinter(Dictionary<string, Value> environment)
        {
            Environment = environment;
        }

        public AstPrinter()
        {
            Environment = null;
        }

        private Dictionary<string, Value> Environment { get; }

        public String print(Expr expr)
        {
            return expr.Accept(this);
        }

        private String parenthesize(String name, params Expr[] exprs)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append("(").Append(name);
            foreach (Expr expr in exprs)
            {
                builder.Append(" ");
                builder.Append(expr.Accept(this));
            }
            builder.Append(")");

            return builder.ToString();
        }

        public String VisitBinaryExpr(Expr.Binary expr)
        {
            return print(expr.Left) + " " + expr.OperatorToken.Text + " " + print(expr.Right);
        }

        
        public String VisitGroupingExpr(Expr.Grouping expr)
        {
            return "(" + print(expr.Expression) + ")";
        }

        public String VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value.Val.ToString();
        }

        public String VisitUnaryExpr(Expr.Unary expr)
        {
            return parenthesize(expr.OperatorToken.Text, expr.Right);
        }

        public string VisitIdentExpr(Expr.Ident expr)
        {
            if (Environment == null)
            {
                return expr.Name.Text;
            } else
            {
                return expr.Name.Text + "(=" + Environment[expr.Name.Text].Val.ToString() + ")";
            }
            
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            return print(expr.Left) + " " + expr.OperatorToken.Text + " " + print(expr.Right);
        }
    }


}
