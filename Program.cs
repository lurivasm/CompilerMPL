using System;
using System.IO;
using System.Collections.Generic;


namespace Compilers
{
    /** Main Class where we read the full code of a program, call the scanner,
     *  parser, semantic analysis and interpreter and where the errors are printed
     */
    public class Program
    {
        /* Errors for the parser, semantic analysis and interpreter */
        static bool hadSyntaxError = false;
        static bool hadTypeError = false;
        static bool hadRuntimeError = false;
        /* Enumeration for the codes of the exit depending on the errors */
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
            /* Location of the program as an argument */
            if (args.Length == 0) {
                System.Console.WriteLine("Please enter the route to the program.");
                return;
            }

            /* Read all the program line by line and store it in "program" */
            using (StreamReader reader = new StreamReader(args[0])) {
                string line;
                while ((line = reader.ReadLine()) != null) {
                    program += line + "\n";
                }
            }

            /* Scanner, parser, semantic analysis and interpreter */
            if (program != null) {
                Scanner scanner = new Scanner(program);
                List<Token> tokens = scanner.ScanTokens();

                foreach (Token token in tokens) Console.WriteLine(token.ToString());

                Parser parser = new Parser(tokens);
                List<Stmt> stmts = parser.Parse();

                if (hadSyntaxError) Environment.Exit((int)ExitCodes.SyntaxError);

                TypeChecker typeChecker = new TypeChecker();
                typeChecker.CheckTypes(stmts);

                if (hadTypeError) Environment.Exit((int)ExitCodes.TypeError);

                Interpreter interpreter = new Interpreter();
                interpreter.Interpret(stmts);

                if (hadRuntimeError) Environment.Exit((int)ExitCodes.RuntimeError);

            }

            Environment.Exit((int)ExitCodes.Success);
        }


        /**
         * Function Error : it calls the private function report to
         * print the internal errors of the scanner
         * Param : line where the error is
         *         message with the description of the error
         */
        public static void Error(int line, String message)
        {
            Report(line, "", message);
        }

        /**
         * Function Error : it calls the private function report to
         * print the internal errors in the parser 
         * Param : token where the error is
         *         message with the description of the error
         */
        public static void Error(Token token, String message)
        {
            if (token.Kind == TokenKind.EndOfFile) {
                Report(token.Position, " at end", message);
            }
            else {
                Report(token.Position, " at '" + token.Text + "'", message);
            }
        }

        /**
         * Function RuntimeError : it prints the errors of the interpreter 
         * Param : the runtime error
         */
        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine("[line " + error.Token.Position + "] " + error.Message + ".");
            hadRuntimeError = true;
        }

        /**
         * Function TypeError : it prints the errors of the semantic analysis
         * Param : the type error
         */
        public static void TypeError(TypeError error)
        {
            Console.Write(error.Message + "\n (line " + error.Token.Position + ").");
            hadTypeError = true;
        }

        /**
         * Function Report : it prints the errors of the scanner and parser
         * Param : line where the error is
         *         token where the error is in string
         *         message with the description of the error
         */
        private static void Report(int line, String where, String message)
        {
            Console.Write("[line " + line + "] Error" + where + ": " + message + '\n');
            hadSyntaxError = true;
        }
    }
}

