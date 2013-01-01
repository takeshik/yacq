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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using Parseq;

namespace XSpect.Yacq.Repl
{
    internal static class Program
    {
        private static readonly CommandlineParser.CommandlineOption[] _options = new[]
        {
            CommandlineParser.Option("debug", false, "Attach the debugger.", "d", "debug"),
            CommandlineParser.Option("help", false, "Show help message.", "h", "help"),
            CommandlineParser.Option("version", false, "Show version information.", "version")
        };

        private static readonly ILookup<String, String> _args
            = CommandlineParser.Parse(Environment.GetCommandLineArgs().Skip(1), _options);

        [LoaderOptimization(LoaderOptimization.MultiDomain)]
        private static void Main()
        {
            Environment.CurrentDirectory = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName;
            Console.WriteLine("YACQ ({0}) REPL", YacqServices.Version);
            ((SandboxManager) AppDomain.CreateDomain(
                    "SandboxManager",
                    new Evidence(),
                    new AppDomainSetup()
                    {
                        ApplicationBase = Environment.CurrentDirectory,
                        ApplicationName = "YacqRepl.SandboxManager",
                        LoaderOptimization = LoaderOptimization.MultiDomain,
                    }
                )
                .Let(d => Activator.CreateInstance(
                    d,
                    typeof(SandboxManager).Assembly.FullName,
                    typeof(SandboxManager).FullName,
                    false,
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Object[0],
                    CultureInfo.InvariantCulture,
                    null
                ))
                .Unwrap()
            ).Run();
            Thread.Sleep(-1);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
    