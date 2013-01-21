// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ REPL
 *   REPL and remote code evaluating system provider of YACQ
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
 * All rights reserved.
 * 
 * This file is part of YACQ REPL.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Serialization;

namespace XSpect.Yacq.Repl
{
    public class ConsoleReplInterface
        : IReplInterface
    {
        private SandboxManager _manager;

        private ISandbox _sandbox;

        private Int32 _dumpLimit;

        public ConsoleReplInterface()
        {
            this._dumpLimit = 100;
        }

        public void Dispose()
        {
        }

        public void Initialize(SandboxManager manager)
        {
            this._manager = manager;
            this._sandbox = this._manager.DefaultSandbox;
        }

        public void Run()
        {
            EnumerableEx.Repeat(this._sandbox)
                .Do(s => Console.Write("yacq[{0}]> ", s.History.Count))
                .Select(s => (Console.ForegroundColor = ConsoleColor.White)
                    .Let(_ => Console.ReadLine())
                    .Apply(_ => Console.ResetColor())
                    .If(l => l == null, l => ":exit")
                    .If(
                        l => l.FirstOrDefault() == ':' && !l.StartsWith(":<<"),
                        l => l.Substring(1).Split(' ').Let(_ =>
                        {
                            switch (_[0])
                            {
                                case "exit":
                                    Environment.Exit(0);
                                    return "";
                                case "sandbox":
                                    this._manager.Unload(this._sandbox);
                                    this._sandbox = this._manager.CreateSandbox();
                                    return "(type 'System.AppDomain').CurrentDomain.FriendlyName";
                                case "privileged":
                                    this._manager.Unload(this._sandbox);
                                    this._sandbox = this._manager.DefaultSandbox;
                                    return "(type 'System.AppDomain').CurrentDomain.FriendlyName";
                                case "limit":
                                    if (_.Length > 1)
                                    {
                                        this._dumpLimit = Int32.Parse(_[1]);
                                    }
                                    return this._dumpLimit.ToString();
                                default:
                                    return "";
                            }
                        })
                    )
                    .If(
                        l => l.StartsWith(":<<"),
                        h => EnumerableEx.Repeat(Unit.Default)
                            .Do(_ => Console.Write("yacq[{0}]| ", _sandbox.History.Count))
                            .Select(_ => (Console.ForegroundColor = ConsoleColor.White)
                                .Let(__ => Console.ReadLine())
                                .Apply(l => Console.ResetColor())
                            )
                            .TakeWhile(l => l != h.Substring(3)),
                        EnumerableEx.Return
                    )
                )
                .Select(ls => String.Join(Environment.NewLine, ls))
                .Where(s => !String.IsNullOrWhiteSpace(s))
                .Select(s => this._sandbox.Evaluate(s))
                .Do(u =>
                {
                    u.Tag = (this._sandbox.History.Count - 1).ToString();
                    u.Logs.Subscribe(e =>
                    {
                        WriteHeader(u.Tag, e.Timestamp);
                        WriteLine(e.Color, e.Body);
                    });
                    u.ParsedExpressions
                        .Subscribe(e =>
                        {
                            WriteHeader(u.Tag, e.Timestamp);
                            Write(ConsoleColor.Gray, "parsed: ");
                            Write(ConsoleColor.DarkCyan, "{0} => {1} = ", e.Node.GetType().Name, e.Type);
                            WriteLine(ConsoleColor.Cyan, e.Node.ToString());
                        });
                    u.ReturnedValues
                        .Catch(Observable.Never<ReturnedValue>())
                        .Subscribe(v =>
                        {
                            WriteHeader(u.Tag, v.Timestamp);
                            Write(ConsoleColor.Gray, "returned: ");
                            if (v.Value != null)
                            {
                                Write(ConsoleColor.DarkGreen, TypeRef.Serialize(v.Value.GetType()).ToString());
                                var stringified = v.Value.GetType().GetMethod("ToString", Type.EmptyTypes).DeclaringType != typeof(Object);
                                if (stringified)
                                {
                                    Write(ConsoleColor.DarkGreen, " = ");
                                    if (v.Value is Expression)
                                    {
                                        Write(ConsoleColor.Green, Node.Serialize((Expression) v.Value).ToString());
                                    }
                                    else
                                    {
                                        Write(ConsoleColor.Green, v.Value.ToString());
                                    }
                                }

                                if (!(v.Value is String) && v.Value.GetType().GetInterface("IEnumerable") != null)
                                {
                                    Write(ConsoleColor.DarkGreen, (stringified ? " [" : " = [") + Environment.NewLine + "    ");
                                    ((IEnumerable) v.Value)
                                        .Cast<Object>()
                                        .Take(this._dumpLimit)
                                        .ForEach(e =>
                                        {
                                            var line = Console.CursorTop;
                                            if (Console.CursorLeft == 0)
                                            {
                                                Console.Write("    ");
                                            }
                                            Write(
                                                ConsoleColor.Green,
                                                e != null ? e.ToString() : "(null)"
                                            );
                                            Write(ConsoleColor.DarkGreen, ", ");
                                            if (Console.CursorTop != line || Console.CursorLeft > Console.BufferWidth * 0.8)
                                            {
                                                Console.WriteLine();
                                            }
                                        });
                                    Write(ConsoleColor.DarkGreen, Environment.NewLine + "  ]");
                                }
                            }
                            else
                            {
                                Write(ConsoleColor.DarkGreen, "null");
                            }
                            Console.WriteLine();
                        });
                })
                .ForEach(u => u.Start());
        }

        private static void Write(ConsoleColor color, String str)
        {
            var prevColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(str);
            Console.ForegroundColor = prevColor;
        }

        private static void Write(ConsoleColor color, String format, params Object[] args)
        {
            Write(color, String.Format(format, args));
        }

        private static void WriteLine(ConsoleColor color, String str)
        {
            Write(color, str + Environment.NewLine);
        }

        private static void WriteLine(ConsoleColor color, String format, params Object[] args)
        {
            WriteLine(color, String.Format(format, args));
        }

        private static void WriteHeader(String tag, TimeSpan timestamp)
        {
            Write(ConsoleColor.DarkGray, @"[{0}] {1:mm\:ss\.fff} ", tag, timestamp);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
