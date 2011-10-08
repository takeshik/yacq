// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ Runner
 *   Runner and Compiler frontend of YACQ
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
 * All rights reserved.
 * 
 * This file is part of YACQ Runner.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.Runner
{
    internal static class Program
    {
        private static readonly List<Tuple<String, Expression, Object>> _history
            = new List<Tuple<String, Expression, Object>>();

        private static Int32 Main(String[] args)
        {
            if (args.Length == 0)
            {
                Repl();
                return 0;
            }
            else if (args[0].StartsWith("-c"))
            {
                using (var reader = args[1] == "-"
                    ? Console.In
                    : new StreamReader(args[1])
                )
                {
                    var head = reader.ReadLine();
                    Compile(
                        (head.StartsWith("#!") ? "" : head) + reader.ReadToEnd(),
                        args[1] == "-"
                            ? "a.out"
                            : Path.GetFileNameWithoutExtension(args[1]),
                        args[0] == "-cw"
                            ? PEFileKinds.WindowApplication
                            : args[0] == "-cl"
                                  ? PEFileKinds.Dll
                                  : PEFileKinds.ConsoleApplication
                    );
                    return 0;
                }
            }
            else
            {
                using (var reader = args[0] == "-"
                    ? Console.In
                    : new StreamReader(args[0])
                )
                {
                    var head = reader.ReadLine();
                    var ret =  Run(
                        (head.StartsWith("#!") ? "" : head) + reader.ReadToEnd(),
                        Environment.GetCommandLineArgs().Contains("-v")
                    );
                    return ret is Int32 ? (Int32) ret : 0;
                }
            }
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
                    Console.Write("yacq[{0}]> ", _history.Count);
                }
                Console.ForegroundColor = ConsoleColor.White;
                var input = Console.ReadLine();
                Console.ResetColor();
                if (heredoc != null)
                {
                    if (input == heredoc)
                    {
                        Run(code, true);
                        code = "";
                        heredoc = null;
                    }
                    else
                    {
                        code += input + Environment.NewLine;
                    }
                }
                // EOF
                else if (input == null)
                {
                    return;
                }
                else if (input.StartsWith("\\"))
                {
                    switch (input.Substring(1))
                    {
                        case "exit":
                            return;
                        case "help":
                            Console.WriteLine(
                                #region String
@"Commands:
  \exit
    Exit this program.
  \help
    Show this message.
  \chelp
    Show command-line option help.
  \shelp
    Show special symbols only in YacqRun help.
  \man
    Open the reference manual web page.
  \about
    Show general and copyright description.
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
                        case "chelp":
                            Console.WriteLine(
                                #region String
@"Command-line Options:
  YacqRun
    Run in REPL mode.
  YacqRun PATH
    Run script file in PATH.
  YacqRun -c PATH
    Compile script file in PATH to Console Application EXE.
  YacqRun -cw PATH
    Compile script file in PATH to Windows Application EXE.
  YacqRun -cl PATH
    Compile script file in PATH to Library DLL.
  NOTE: Specify - in PATH means read script data from standard input."
                                #endregion
                            );
                            break;
                        case "shelp":
                            Console.WriteLine(
                                #region String
@"Special Symbols only in YacqRun:
  !C
    Input code history array
  !E
    Reduced expression history array
  !R
    Return value history array
  !H
    History list (you can modify, such as (Clear))
  NOTE: The history index for next input is in the prompt; like 'yacq[N]>'
        One history entry indicates a reduced expressions, not a code input.
        (one code input may create more than one history entries.)"
                                #endregion
                            );
                            break;
                        case "man":
                            Process.Start("https://github.com/takeshik/yacq/wiki");
                            break;
                        case "about":
                            Console.WriteLine(
                                #region String
@"YACQ <https://github.com/takeshik/yacq>
  Yet Another Compilable Query Language, based on Expression Trees API
  Language service is provided by Yacq.dll, the assembly name is:
    {0}
YACQ Runner (YACQRun) is part of YACQ
  Runner and Compiler of YACQ

Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
All rights reserved.

YACQ and YACQ Runner are Free Software; licensed under the MIT License.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE."
                                #endregion
                                , typeof(YacqServices).Assembly.GetName()
                            );
                            break;
                        case "debug":
                            Debugger.Launch();
                            break;
                        case "gc":
                            Console.WriteLine();
                            GC.Collect();
                            break;
                    }
                }
                else if (input.StartsWith("<<"))
                {
                    heredoc = input.Substring(2);
                }
                else if (!String.IsNullOrWhiteSpace(input))
                {
                    Run(input, true);
                    code = "";
                }
            }
        }

        private static Object Run(String code, Boolean showInfo)
        {
            try
            {
                Object ret = null;
                foreach (var expr in YacqServices.ParseAll(
                    new SymbolTable()
                    {
                        {"!C", Expression.Constant(_history.Select(t => t.Item1).ToArray())},
                        {"!E", Expression.Constant(_history.Select(t => t.Item2).ToArray())},
                        {"!R", Expression.Constant(_history.Select(t => t.Item3).ToArray())},
                        {"!H", Expression.Constant(_history)},
                    },
                    code
                ))
                {
                    if (showInfo)
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("Expression : {0} => {1}\n  {2}", expr.GetType().Name, expr.Type, expr);
                        Console.ResetColor();
                    }
                    ret = Expression.Lambda(expr).Compile().DynamicInvoke();
                    if (showInfo)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        if (ret != null)
                        {
                            Console.WriteLine("Returned : {0}\n  {1}", ret.GetType(), ret);
                        }
                        else
                        {
                            Console.WriteLine("Returned : null");
                        }
                        Console.ResetColor();
                    }
                    _history.Add(Tuple.Create(code, expr, ret));
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

        private static void Compile(String code, String name, PEFileKinds fileKind)
        {
            name += fileKind == PEFileKinds.Dll
                ? ".dll"
                : ".exe";
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName()
                {
                    Name = name,
                    Version = new Version(0, 0, 0, 0),
                    CultureInfo = CultureInfo.InvariantCulture,
                },
                AssemblyBuilderAccess.RunAndSave
            );
            var type = assembly
                .DefineDynamicModule(name)
                .DefineType(
                    "Program",
                    TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed
                );
            var method = type.DefineMethod(
                "Main",
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(void),
                Type.EmptyTypes
            );
            var expressions = YacqServices.ParseAll(code);
            Expression.Lambda<Action>(expressions.Length == 1
                ? expressions.Single()
                : Expression.Block(expressions)
            ).CompileToMethod(method);
            type.CreateType();
            if (fileKind != PEFileKinds.Dll)
            {
                assembly.SetEntryPoint(method, fileKind);
            }
            assembly.Save(name);
        }
    }
}
