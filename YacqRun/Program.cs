using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace XSpect.Yacq.Runner
{
    internal class Program
    {
        private static Int32 Main(String[] args)
        {
            if (args.Length == 0)
            {
                Repl();
                return 0;
            }
            else if (args[0] == "-")
            {
                return ReadAsScript(Console.In);
            }
            else
            {
                return ReadAsScript(new StreamReader(args[0]));
            }
        }

        private static Int32 ReadAsScript(TextReader input)
        {
            var head = input.ReadLine();
            var ret =  Run((head.StartsWith("#!") ? "" : head) + input.ReadToEnd(), Environment.GetCommandLineArgs().Contains("-v"));
            return ret is Int32 ? (Int32) ret : 0;
        }

        private static void Repl()
        {
            String heredoc = null;
            var code = "";
            Console.WriteLine(
                #region String
@"Yacq Runner (REPL Mode)
Type \help [ENTER] to show help."
                #endregion
            );
            while (true)
            {
                if (heredoc == null)
                {
                    Console.Write("yacq> ");
                }
                Console.ForegroundColor = ConsoleColor.White;
                var input = Console.ReadLine();
                Console.ResetColor();
                if (heredoc != null)
                {
                    if (input == heredoc)
                    {
                        Run(code, true);
                        heredoc = null;
                    }
                    else
                    {
                        code += input + Environment.NewLine;
                    }
                }
                else if (input.StartsWith("\\"))
                {
                    switch (input.Substring(1))
                    {
                        case "exit":
                            Environment.Exit(0);
                            break;
                        case "help":
                            Console.WriteLine(
                            #region String
@"Commands:
  \exit
    Exit this program.
  \help
    Show this message.
  \debug
    Attach the debugger.
  \gc
    Run GC manually.
  (CODE)
    Run one-line CODE.
  <<(INPUT) [ENTER] (CODES)
    Run multi-line CODES while INPUT line was got (heredoc <<EOT).
  (CODE)
    Otherwise: Run one-line code."
                            #endregion
                            );
                            break;
                        case "debug":
                            Debugger.Launch();
                            break;
                        case "gc":
                            GC.Collect();
                            break;
                    }
                }
                else if (input.StartsWith("<<"))
                {
                    heredoc = input.Substring(2);
                }
                else
                {
                    Run(input, true);
                }
            }
        }

        private static Object Run(String code, Boolean showInfo)
        {
            try
            {
                Object ret = null;
                foreach (var expr in YacqServices.ParseAll(code))
                {
                    if (showInfo)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write("Expression: ");
                        Console.WriteLine(expr);
                        Console.ResetColor();
                    }
                    ret = Expression.Lambda(expr).Compile().DynamicInvoke();
                    if (showInfo)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write("Returns: ");
                        Console.WriteLine(ret ?? "(null)");
                        Console.ResetColor();
                    }
                }
                return ret;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(ex);
                Console.ResetColor();
                return null;
            }
        }
    }
}
