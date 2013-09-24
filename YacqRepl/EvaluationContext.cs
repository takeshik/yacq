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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;
using System.Security;
using System.Threading;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Serialization;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Repl
{
    public class EvaluationContext
        : MarshalByRefObject
    {
        private readonly Stopwatch _stopwatch;

        private readonly ISubject<ReturnedValue> _returnValues;

        private readonly ISubject<ParsedExpression> _parsedExpressions;

        private readonly ISubject<LogEntry> _logs;

        private readonly SymbolTable _symbols;

        private readonly IEnumerable<Char> _code;

        public IObservable<ReturnedValue> ReturnedValues
        {
            get
            {
                return this._returnValues.Remotable();
            }
        }

        public IObservable<ParsedExpression> ParsedExpressions
        {
            get
            {
                return this._parsedExpressions.Remotable();
            }
        }

        public IObservable<LogEntry> Logs
        {
            get
            {
                return this._logs.Remotable();
            }
        }

        public String Tag
        {
            get;
            set;
        }

        internal EvaluationContext(SymbolTable symbols, IEnumerable<Char> code)
        {
            this._stopwatch = new Stopwatch();
            this._returnValues = new Subject<ReturnedValue>();
            this._parsedExpressions = new Subject<ParsedExpression>();
            this._logs = new Subject<LogEntry>();
            this._symbols = symbols
                .Apply(s => s["*context*"] = Expression.Constant(this));
            this._code = code;
        }

        public void Start()
        {
            this._stopwatch.Start();
            var expressions = Arrays.Empty<Expression>();
            try
            {
                expressions = YacqServices.ParseAll(this._symbols, this._code);
                expressions
                    .Select(e => Tuple.Create(Node.Serialize(e), TypeRef.Serialize(e.Type)))
                    .ForEach(_ => this.NotifyParsed(_.Item1, _.Item2));
                this._parsedExpressions.OnCompleted();
                this.Log(LogEntry.Info, "Parsing completed.");
            }
            catch (ParseException ex)
            {
                // BUG: causes cross-AppDomain problem
                // this._parsedExpressions.OnError(ex);
                this.Log(LogEntry.Error, String.Format(
                    "Failed to parse: {0}{1}\n{2}{3}",
                    ex.Message,
                    ex.StartPosition == null && ex.EndPosition == null
                        ? ""
                        : " (at "
                              + ex.StartPosition.Nullable(p => p.ToString())
                              + "-"
                              + ex.EndPosition.Nullable(p => p.ToString())
                              + ")",
                    ex.StackTrace,
                    ex.ReaderState.Null(s => String.Format(
                        "\nLast Expression: {0}\nContext Stack:\n{1}",
                        s.LastExpression,
                        s.ContextStack.Stringify(c => "   " + c, "\n")
                    ))
                ));
            }
            catch (Exception ex)
            {
                // BUG: causes cross-AppDomain problem
                // this._parsedExpressions.OnError(ex);
                this.Log(LogEntry.Error, "Failed to parse: " + ex);
            }
            try
            {
                expressions
                    .Select(e => e.Evaluate(this._symbols)
                        .If(
                            o => o is IEnumerable && !o.GetType().Let(t => t.IsMarshalByRef || t.IsSerializable),
                            o => ((IEnumerable) o).Cast<Object>().Remotable()
                        )
                    )
                    .ForEach(this.NotifyReturned);
                this._returnValues.OnCompleted();
                this.Log(LogEntry.Info, "Evaluation completed.");
            }
            catch (Exception ex)
            {
                this._returnValues.OnError(ex);
                this.Log(LogEntry.Error, "Failed to evaluate: " + ex);
            }
        }

        public void Log(ConsoleColor color, String body)
        {
            this._logs.OnNext(new LogEntry(this._stopwatch.Elapsed, color, body));
        }

        private void NotifyReturned(Object value)
        {
            this._returnValues.OnNext(new ReturnedValue(this._stopwatch.Elapsed, value));
        }

        private void NotifyParsed(Node node, TypeRef type)
        {
            this._parsedExpressions.OnNext(new ParsedExpression(this._stopwatch.Elapsed, node, type));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
