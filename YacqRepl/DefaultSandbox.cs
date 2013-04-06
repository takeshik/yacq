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
    internal class DefaultSandbox
        : MarshalByRefObject,
          ISandbox
    {
        public Guid Id
        {
            get
            {
                return Guid.Empty;
            }
        }

        public AppDomain Domain
        {
            get;
            private set;
        }

        public IPAddress RemoteAddress
        {
            get
            {
                return IPAddress.None;
            }
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

        internal DefaultSandbox()
        {
            this.Domain = AppDomain.CurrentDomain;
            this.Symbols = new SymbolTable(typeof(ReplSymbols))
                .Apply(s => s["*context*"] = Expression.Default(typeof(EvaluationContext)));
            this.History = new SortedList<DateTime, String>();
            RuntimeHelpers.RunClassConstructor(typeof(StandardGrammar).TypeHandle);
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

        public Object EvaluateWithoutContext(IEnumerable<Char> code, params Object[] args)
        {
            return YacqServices.Parse(this.Symbols, code).Evaluate(null, args);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
