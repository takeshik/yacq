// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Represents a scope (reading stack entry; for parentheses, brackets, braces, and other structures) of <see cref="ReaderResult"/>.
    /// </summary>
    public class ReaderScope
    {
        /// <summary>
        /// Gets the result expressions of this scope.
        /// </summary>
        /// <value>The result expressions of this scope.</value>
        public LinkedList<YacqExpression> Expressions
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the string which indicates the kind of this scope.
        /// </summary>
        /// <value>The string which indicates the kind of this scope.</value>
        public String Tag
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the start position of this scope.
        /// </summary>
        /// <value>The start position of this scope.</value>
        public TextPosition StartPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the stack to hooks. Hook is invoked after <see cref="Add"/> was called, then the hook is popped.
        /// </summary>
        /// <value>The stack to hooks.</value>
        public Stack<Action<ReaderScope>> Hooks
        {
            get;
            private set;
        }

        internal ReaderScope(String tag, TextPosition startPosition)
        {
            this.Expressions = new LinkedList<YacqExpression>();
            this.Tag = tag;
            this.StartPosition = startPosition;
            this.Hooks = new Stack<Action<ReaderScope>>();
        }

        /// <summary>
        /// Adds the expression to <see cref="Expressions"/> at the end of the list.
        /// </summary>
        /// <param name="expression">The expression to add.</param>
        public void Add(YacqExpression expression)
        {
            this.Expressions.AddLast(expression);
            if (this.Hooks.Any())
            {
                this.Hooks.Pop()(this);
            }
        }

        /// <summary>
        /// Removes the expression at the end of the list and returns the expression.
        /// </summary>
        /// <returns>The removed expression.</returns>
        public YacqExpression Drop()
        {
            return this.Expressions.Last.Value
                .Apply(_ => this.Expressions.RemoveLast());
        }

        /// <summary>
        /// Register the hook with the specified delay count.
        /// </summary>
        /// <param name="delayCount">The number to specify how many times of calling <see cref="Add"/> the invocation of the hook is delayed.</param>
        /// <param name="action">The action as the body of the hook.</param>
        public void RegisterHook(Int32 delayCount, Action<ReaderScope> action)
        {
            Enumerable.Range(0, delayCount).ForEach(_ => this.Hooks.Push(r =>
            {
            }));
            this.Hooks.Push(action);
        }

        /// <summary>
        /// Register the hook for the next <see cref="Add"/> call.
        /// </summary>
        /// <param name="action">The action as the body of the hook.</param>
        public void RegisterHook(Action<ReaderScope> action)
        {
            RegisterHook(0, action);
        }
    }
}