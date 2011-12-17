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
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using XSpect.Yacq.SystemObjects;

namespace XSpect.Yacq.Runner
{
    internal static class Program
    {
        private static SymbolTable _symbols = new SymbolTable(typeof(ReplSymbols));

        private static readonly Stopwatch _stopwatch = new Stopwatch();

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
Type (!help) [ENTER] to show help."
                #endregion
            );
            while (true)
            {
                if (heredoc == null)
                {
                    Console.Write("yacq[{0}]> ", ReplSymbols.ReplHistory.Count);
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
                else if (input.StartsWith("<<"))
                {
                    heredoc = input.Substring(2);
                }
                else if (!String.IsNullOrWhiteSpace(input))
                {
                    Run(input, ReplSymbols.ReplVerbose);
                    if (!ReplSymbols.ReplContinuous)
                    {
                        _symbols = new SymbolTable(typeof(ReplSymbols));
                    }
                    code = "";
                }
            }
        }

        private static Object Run(String code, Boolean showInfo)
        {
            try
            {
                Object ret = null;
                _stopwatch.Restart();
                var exprs = YacqServices.ParseAll(_symbols, code).ToArray();
                _stopwatch.Stop();
                if (showInfo)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Parsed Expressions (time: {0}):", _stopwatch.Elapsed);
                    exprs.ForEach(e =>
                    {
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        Console.Write("  {0} => {1} = ", GetTypeName(e.GetType()), GetTypeName(e.Type));
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(e);
                    });
                    Console.ResetColor();
                }
                foreach (var expr in exprs)
                {
                    ret = Expression.Lambda(expr).Compile().DynamicInvoke();
                    if (showInfo)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        if (ret != null)
                        {
                            Console.Write("Returned: ");
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.Write("{0}", GetTypeName(ret.GetType()));
                            Console.ForegroundColor = ConsoleColor.Green;
                            if (ret.GetType().GetMethod("ToString", Type.EmptyTypes).DeclaringType != typeof(Object))
                            {
                                Console.ForegroundColor = ConsoleColor.DarkGreen;
                                Console.Write(" = ");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Write("{0}", ret);
                            }
                            if (ret is IEnumerable && !(ret is String))
                            {
                                var data = ((IEnumerable) ret)
                                    .Cast<Object>()
                                    .Select(_ => (_ ?? "(null)").ToString())
                                    .Take(101)
                                    .ToArray();
                                if (data.Any(s => s.Length > 40))
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(" = [");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(String.Join(
                                        Environment.NewLine,
                                        data.Take(100)
                                            .Select(s => "    " + s)
                                    ));
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.WriteLine(data.Length > 100
                                        ? "    (more...)\n  ]"
                                        : "  ]"
                                    );
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.Write(" = [ \n    ");
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write(String.Join(" ", data.Take(100)));
                                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                                    Console.Write(data.Length > 100
                                        ? " (more...)\n  ]"
                                        : "\n  ]"
                                    );
                                }
                            }
                            Console.WriteLine();
                        }
                        else
                        {
                            Console.WriteLine("Returned: null");
                        }
                        Console.ResetColor();
                    }
                    ReplSymbols.ReplHistory.Add(Tuple.Create(code, expr, ret));
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
            var assembly = new YacqAssembly(name, fileKind);
            var type = assembly.DefineType(":Program");
            var expressions = YacqServices.ParseAll(new SymbolTable()
            {
                {"*assembly*", Expression.Constant(assembly)},
            }, code);
            var method = type.DefineMethod(
                "Main",
                MethodAttributes.Static,
                typeof(void),
                Type.EmptyTypes,
                Expression.Lambda<Action>(expressions.Length == 1
                    ? expressions.Single()
                    : Expression.Block(expressions)
                )
            );
            type.Create();
            assembly.Save(method);
        }

        private static String GetTypeName(Type type)
        {
            return type.IsArray
                ? GetTypeName(type.GetElementType()) + "[]"
                : (type.IsNested
                      ? GetTypeName(type.DeclaringType) + "."
                      : ""
                  ) + (type.IsGenericType && !type.IsGenericTypeDefinition
                      ? (type.Name.Contains("`")
                            ? type.Name.Remove(type.Name.LastIndexOf('`'))
                            : type.Name
                        ) + "<" + String.Join(",", type.GetGenericArguments().Select(GetTypeName)) + ">"
                      : type.Name
                  );
        }
    }
}
