// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ Runner
 *   Runner and Compiler frontend of YACQ
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;
using XSpect.Yacq.SystemObjects;

namespace XSpect.Yacq.Runner
{
    internal static class Program
    {
        private enum Phase
        {
            Read = 250,
            Parse = 251,
            Evaluate = 252,
            Emit = 253,
        }

        private static readonly CommandlineParser.CommandlineOption[] _options = new []
        {
            CommandlineParser.Option("compile", false, "Compile the code.", "c", "compile"),
            CommandlineParser.Option("winexe", false, "Set output type to window executable.", "w", "winexe"),
            CommandlineParser.Option("library", false, "Set output type to library.", "l", "library"),
            CommandlineParser.Option("parse", false, "Only parse inputs.", "p", "parse"),
            CommandlineParser.Option("dump-tree", false, "Dump parsed expression tree.", "t", "dump-tree"),
            CommandlineParser.Option("expr", true, "Specify code to input", "e", "expr"),
            CommandlineParser.Option("debug", false, "Attach the debugger.", "d", "debug"),
            CommandlineParser.Option("help", false, "Show help message.", "h", "help"),
            CommandlineParser.Option("version", false, "Show version information.", "", "version")
        };

        private static readonly ILookup<String, String> _args
            = CommandlineParser.Parse(Environment.GetCommandLineArgs().Skip(1), _options);

        private static Int32 Main()
        {
            if (_args.Contains("debug"))
            {
                Debugger.Launch();
            }

            if (_args.Contains("help"))
            {
                Console.WriteLine("Usage: {0} [switches] [--] [inputs]", Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]));
                Console.WriteLine(
                    String.Join(Environment.NewLine, _options
                        .SelectMany(o => new []
                        {
                            "  " + String.Join(", ", o.ShortNames
                                .Select(n => "-" + n)
                                .Concat(o.LongNames.Select(n => "--" + n))
                            ),
                            "    " + o.Description,
                        }
                    ))
                );
                return 0;
            }

            if (_args.Contains("version"))
            {
                Console.WriteLine(
                    #region String
@"YACQ <http://yacq.net/>
  Yet Another Compilable Query Language, based on Expression Trees API
  Language service is provided by Yacq.dll, the assembly name is:
    {0}
YACQ Runner (YACQRun) is part of YACQ
  Runner and Compiler of YACQ

Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
All rights reserved.
YACQ and YACQ Runner are Free Software; licensed under the MIT License."
                    #endregion
                    , typeof(YacqServices).Assembly.FullName
                );
                return 0;
            }

            var input = Read();

            if (_args.Contains("compile"))
            {
                Compile(
                    input,
                    _args["_param"]
                        .FirstOrDefault(p => p != "-")
                        .Null(p => Path.GetFileNameWithoutExtension(p))
                            ?? "a.out",
                    _args.Contains("winexe")
                        ? PEFileKinds.WindowApplication
                        : _args.Contains("library")
                                ? PEFileKinds.Dll
                                : PEFileKinds.ConsoleApplication
                );
            }
            else
            {
                var ret = 0;
                var expression = Parse(null, input);
                if (!_args.Contains("parse"))
                {
                    try
                    {
                        Expression.Lambda(expression)
                            .Evaluate()
                            .If(o => o is Int32, o => ret = (Int32) o);
                    }
                    catch (Exception ex)
                    {
                        Fail(ex, Phase.Evaluate);
                    }
                }
                if (_args.Contains("dump-tree"))
                {
                    Console.Error.WriteLine(YacqServices.SaveText(expression));
                }
                return ret;
            }
            return 0;
        }

        private static String Read()
        {
            try
            {
                return String.Concat(_args["expr"]
                    .Concat(_args["_param"].Select(p => p == "-"
                        ? Console.In.ReadToEnd()
                        : String.Join(Environment.NewLine, File.ReadAllLines(p)
                            .If(ls => ls[0].StartsWith("#!"), ls => ls[0] = "")
                            )
                    ))
                ).If(String.IsNullOrEmpty, s => Console.In.ReadToEnd());
            }
            catch (Exception ex)
            {
                Fail(ex, Phase.Read);
                return "";
            }
        }

        private static Expression Parse(SymbolTable symbols, String code)
        {
            try
            {
                return YacqServices.ParseAll(symbols, code)
                    .Let(es => es.Length == 1
                        ? es.Single()
                        : Expression.Block(es)
                    );
            }
            catch (Exception ex)
            {
                Fail(ex, Phase.Parse);
                return null;
            }
        }

        private static void Compile(String code, String name, PEFileKinds fileKind)
        {
            var assembly = new YacqAssembly(name, fileKind);
            var type = assembly.DefineType(":Program");
            try
            {
                try
                {
                    var method = type.DefineMethod(
                        "Main",
                        MethodAttributes.Static,
                        typeof(void),
                        Type.EmptyTypes,
                        Expression.Lambda<Action>(Parse(
                            new SymbolTable()
                            {
                                {"*assembly*", Expression.Constant(assembly)},
                            },
                            code
                        ))
                    );
                    type.Create();
                    assembly.Save(method);
                }
                catch (Exception ex)
                {
                    Fail(ex, Phase.Emit);
                }
            }
            catch (Exception ex)
            {
                Fail(ex, Phase.Parse);
            }
        }

        private static void Fail(Exception ex, Phase phase)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Unhandled exception during '{0}' - {1}: {2}", phase, ex.GetType().Name, ex.Message);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine(ex.StackTrace);
            Console.ResetColor();
            Environment.Exit((Int32) phase);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
