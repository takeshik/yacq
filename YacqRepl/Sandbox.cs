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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Policy;
using Parseq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Repl
{
    public class Sandbox
        : MarshalByRefObject,
          ISandbox
    {
        public Guid Id
        {
            get;
            private set;
        }

        public AppDomain Domain
        {
            get;
            private set;
        }

        public IPAddress RemoteAddress
        {
            get;
            private set;
        }

        public SymbolTable Symbols
        {
            get;
            private set;
        }

        public SortedList<DateTime, String> History
        {
            get;
            private set;
        }

        private Sandbox(
            Guid id,
            IPAddress remoteAddress
        )
        {
            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                throw new InvalidOperationException("Current AppDomain is default; activate on sandbox AppDomain.");
            }
            this.Id = id;
            this.Domain = AppDomain.CurrentDomain;
            this.RemoteAddress = remoteAddress;
            this.Symbols = new SymbolTable(typeof(ReplSymbols))
                .Apply(s => s["*context*"] = Expression.Default(typeof(EvaluationContext)));
            this.History = new SortedList<DateTime, String>();
            RuntimeHelpers.RunClassConstructor(typeof(Reader.Defaults).TypeHandle);
        }

        public static Sandbox Create(
            Guid id,
            Evidence evidence,
            IPAddress remoteAddress,
            CultureInfo culture
        )
        {
            return (Sandbox) Activator.CreateInstance(
                AppDomain.CreateDomain(
                    "Sandbox." + id.ToString("d"),
                    evidence,
                    new AppDomainSetup()
                    {
                        ApplicationBase = Environment.CurrentDirectory,
                        ApplicationName = "YacqRepl.Sandbox." + id.ToString("d"),
                        LoaderOptimization = LoaderOptimization.MultiDomain,
                    },
                    SecurityManager.GetStandardSandbox(evidence).Apply(
                        ps => ps.AddPermission(new FileIOPermission(
                            FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read,
                            AccessControlActions.View,
                            new []
                            {
                                Environment.CurrentDirectory,
                                Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                                Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                                    "Reference Assemblies"
                                )
                            }
                        )),
                        ps => ps.AddPermission(new ReflectionPermission(PermissionState.Unrestricted)),
                        ps => ps.AddPermission(new SecurityPermission(SecurityPermissionFlag.AllFlags))
                    ),
                    Directory.GetFiles(Environment.CurrentDirectory, "*.dll")
                        .Select(Assembly.LoadFrom)
                        .EndWith(Assembly.GetExecutingAssembly())
                        .Choose(a => a.Evidence.GetHostEvidence<StrongName>())
                        .ToArray()
                ),
                typeof(Sandbox).Assembly.FullName,
                typeof(Sandbox).FullName,
                false,
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new Object[]
                {
                    id,
                    remoteAddress,
                },
                culture,
                null
            ).Unwrap();
        }

        public override Object InitializeLifetimeService()
        {
            return null;
        }

        public EvaluationContext Evaluate(IEnumerable<Char> code)
        {
            return new EvaluationContext(this.Symbols, new String(code.ToArray())
                .Apply(c => this.History.Add(DateTime.Now, c))
            );
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
