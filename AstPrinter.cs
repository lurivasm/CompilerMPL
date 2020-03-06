using System;
using System.Text;

namespace Compilers
{
    class AstPrinter : Expr.IVisitor<String>
    {
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
            return parenthesize(expr.OperatorToken.Text, expr.Left, expr.Right);
        }

        
        public String VisitGroupingExpr(Expr.Grouping expr)
        {
            return parenthesize("group", expr.Expression);
        }

        public String VisitLiteralExpr(Expr.Literal expr)
        {
            if (expr.Value == null) return "nil";
            return expr.Value.ToString();
        }

        public String VisitUnaryExpr(Expr.Unary expr)
        {
            return parenthesize(expr.OperatorToken.Text, expr.Right);
        }

        public string VisitIdentExpr(Expr.Ident expr)
        {
            throw new NotImplementedException();
        }

        public string VisitLogicalExpr(Expr.Logical expr)
        {
            throw new NotImplementedException();
        }
    }


}
