using System;
using System.IO;
using System.Collections.Generic;


namespace Compilers
{
    public class Program
    {
        private static Interpreter Interpreter = new Interpreter();
        static bool hadError = false;
        static bool hadTypeError = false; 
        static bool hadRuntimeError = false;

        enum ExitCodes : int
        {
            Success = 0,
            SyntaxError = 1,
            TypeError = 2,
            RuntimeError = 3
        }

        static void Main(string[] args)
        {
            string program = "";
            // Location of the program as an argument
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter the route to the program");
                return;
            }


            // Read all the program line by line and store it in "program"
            using (StreamReader reader = new StreamReader(args[0]))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    program += line + "\n";
                }
            }

            if (program != null)
            {
                Scanner scanner = new Scanner(program);
                List<Token> tokens = scanner.ScanTokens();

                //for (int i = 0; i < tokens.Count; i++)
                //{
                //    Console.Write(tokens[i].toString());
                //}
                Parser parser = new Parser(tokens);
                List<Stmt> stmts = parser.Parse();

                if (hadError) Environment.Exit((int)ExitCodes.SyntaxError);

                TypeChecker typeChecker = new TypeChecker();
                typeChecker.CheckTypes(stmts);

                if (hadTypeError) Environment.Exit((int)ExitCodes.TypeError);
                else
                    Console.WriteLine("No TypeErrors");

                //Interpreter.Interpret(stmts);

                //if (hadRuntimeError) Environment.Exit((int)ExitCodes.RuntimeError);

            }
            
            Environment.Exit((int)ExitCodes.Success);
        }

        //static void Main(string[] args) {
        //    Expr expression = new Expr.Binary(
        //    new Expr.Unary(
        //        new Token(TokenKind.Minus, 1, "-", null),
        //        new Expr.Literal(123)),
        //    new Token(TokenKind.Mult, 1, "*", null),
        //    new Expr.Grouping(
        //        new Expr.Literal(45.67)));

        //    String str = (new AstPrinter().print(expression));
        //    Console.WriteLine(str);
        
        //}



        public static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, String message)
        {
            if (token.Kind == TokenKind.EndOfFile)
                Report(token.Position, " at end", message);
            else
                Report(token.Position, " at '" + token.Text + "'", message);
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.Write(error.Message + "\n(line " + error.Token.Position + ").");
            hadRuntimeError = true;
        }

        public static void TypeError(TypeError error)
        {
            Console.Write(error.Message + "\n (line " + error.Token.Position + ").");
            hadTypeError = true;
        }


        private static void Report(int line, String where, String message)
        {
            Console.Write("[line " + line + "] Error" + where + ": " + message + '\n');
            hadError = true;
        }
    }
}
