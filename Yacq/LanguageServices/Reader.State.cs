// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2012 linerlock <x.linerlock@gmail.com>
 * All rights reserved.
 * 
 * This file is part of YACQ.
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
using System.Linq;
using Parseq;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    partial class Reader
    {
        /// <summary>
        /// Provides access to state values of <see cref="Reader"/>.
        /// </summary>
        public class State
        {
            [ThreadStatic()]
            private static State _current;

            private readonly Stack<Context> _contextStack;

            /// <summary>
            /// Gets the object that indicates current reader states.
            /// </summary>
            /// <value>The object that indicates current reader states.</value>
            public static State Current
            {
                get
                {
                    return _current;
                }
            }

            /// <summary>
            /// Gets the last expression that was read successfully by the reader.
            /// </summary>
            /// <value>The last expression that was read successfully by the reader.</value>
            public YacqExpression LastExpression
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the list of reader context stack.
            /// </summary>
            /// <value>The list of reader context stack.</value>
            public IList<Context> ContextStack
            {
                get
                {
                    return this._contextStack.ToArray();
                }
            }

            private State()
            {
                this._contextStack = new Stack<Context>();
            }

            internal static IDisposable Create()
            {
                if (_current != null)
                {
                    throw new InvalidOperationException("The state object has already been created.");
                }
                var state = _current = new State();
                return Disposables.From(() =>
                {
                    if (state != Current)
                    {
                        throw new InvalidOperationException("Invalid state object disposing.");
                    }
                    _current = null;
                });
            }

            internal void SetLastExpression(YacqExpression expression)
            {
                this.LastExpression = expression;
            }

            internal void EnterContext(String name, Position position)
            {
                this._contextStack.Push(new Context(name, position));
            }

            internal void LeaveContext(String name)
            {
                var top = this._contextStack.Any()
                    ? this._contextStack.Pop()
                    : null;
                if (top == null || top.Name != name)
                {
                    // Revert the operation.
                    this._contextStack.Push(top);
                    throw new InvalidOperationException("Invalid context leaving operation.");
                }
            }
        }
    }
}