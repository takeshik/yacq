using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace XSpect.Yacq.Runner
{
    internal class Program
    {
        private static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                Repl();
            }
            else if (args[0] == "-")
            {
                ReadAsScript(Console.In);
            }
            else
            {
                ReadAsScript(new StreamReader(args[0]));
            }
        }

        private static void ReadAsScript(TextReader input)
        {
            var head = input.ReadLine();
            Expression.Lambda<Action>(
                Yacq.Parse((head.StartsWith("#!") ? "" : head) + input.ReadToEnd())
            ).Compile()();
        }

        private static void Repl()
        {
            String code = "";
            while (true)
            {
                Console.Write("yacq> ");
                var input = Console.ReadLine();
                if (input.StartsWith("\\"))
                {
                    switch (input.Substring(1))
                    {
                        case "exit":
                            Environment.Exit(0);
                            break;
                        case "run":
                            try
                            {
                                var expr = Yacq.Parse(code);
                                Console.WriteLine("Expression: ");
                                Console.WriteLine("  " + expr);
                                Expression.Lambda<Action>(expr).Compile()();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                            finally
                            {
                                code = "";
                            }
                            break;
                        case "clear":
                            code = "";
                            break;
                        case "show":
                            Console.WriteLine(code);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    code += Environment.NewLine + input;
                }
            }
        }
    }
}
