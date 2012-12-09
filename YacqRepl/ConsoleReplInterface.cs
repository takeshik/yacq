// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id: f11cdf26be2df17b9601745f738ca022ac231687 $
/* YACQ REPL
 *   REPL and remote code evaluating system provider of YACQ
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using System.Linq;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace XSpect.Yacq.Repl
{
    public class ConsoleReplInterface
        : IReplInterface
    {
        private SandboxManager _manager;

        private ISandbox _sandbox;

        public void Dispose()
        {
        }

        public void Initialize(SandboxManager manager)
        {
            this._manager = manager;
            this._sandbox = this._manager.CreateSandbox();
        }

        public void Run()
        {
            EnumerableEx.Repeat(_sandbox)
                .Select(s =>
                    this._sandbox.Evaluate(String.Join(Environment.NewLine,
                        EnumerableEx.Generate(0, _ => true, l => l + 1, _ => _)
                            .Do(l => Console.Write("yacq[{0}:{1}]> ", s.History.Count, l))
                            .Select(l => (Console.ForegroundColor = ConsoleColor.White)
                                .Let(_ => Console.ReadLine())
                                .Apply(_ => Console.ResetColor())
                            )
                            .TakeWhile(l => l != null)
                    ))
                    .Apply(u => u.Tag = (s.History.Count - 1).ToString())
                )
                .Do(u =>
                {
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
                            Write(ConsoleColor.DarkCyan, "{0} => ? = ", e.Node.GetType().Name);
                            WriteLine(ConsoleColor.Cyan, "...");
                        });
                    u.ReturnedValues
                        .Catch(Observable.Never<ReturnedValue>())
                        .Subscribe(v =>
                        {
                            WriteHeader(u.Tag, v.Timestamp);
                            Write(ConsoleColor.Gray, "returned: ");
                            if (v.Value != null)
                            {
                                Write(ConsoleColor.DarkGreen, "{0} = ", v.Value.GetType().Name);
                                WriteLine(ConsoleColor.Green, JToken.FromObject(v.Value).ToString());
                            }
                            else
                            {
                                WriteLine(ConsoleColor.DarkGreen, "null");
                            }
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
