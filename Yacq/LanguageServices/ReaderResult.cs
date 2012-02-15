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
    /// Represents and manages the result collection of reading.
    /// </summary>
    public class ReaderResult
    {
        private readonly Stack<ReaderScope> _scopes;

        /// <summary>
        /// Gets the current <see cref="ReaderScope"/> of this instance.
        /// </summary>
        /// <value>The current <see cref="ReaderScope"/> of this instance.</value>
        public ReaderScope Current
        {
            get
            {
                return this._scopes.Peek();
            }
        }

        /// <summary>
        /// Gets the count of the <see cref="ReaderScope"/> stack in this instance.
        /// </summary>
        /// <value>The count of the <see cref="ReaderScope"/> stack in this instance.</value>
        public Int32 Depth
        {
            get
            {
                return this._scopes.Count;
            }
        }

        /// <summary>
        /// Gets the sequence of tags in the <see cref="ReaderScope"/> stack in this instance.
        /// </summary>
        /// <value>The sequence of tags in the <see cref="ReaderScope"/> stack in this instance.</value>
        public IEnumerable<String> Tags
        {
            get
            {
                return this._scopes
                    .SkipLast(1)
                    .Reverse()
                    .Select(s => s.Tag);
            }
        }

        internal ReaderResult()
        {
            this._scopes = new Stack<ReaderScope>();
            this.BeginScope(null, new TextPosition(0, 1, 1));
        }

        /// <summary>
        /// Begins new <see cref="ReaderScope"/>.
        /// </summary>
        /// <param name="tag">The string which indicates the kind of new scope.</param>
        /// <param name="startPosition">The start position of new scope.</param>
        /// <returns>Begined new <see cref="ReaderScope"/>.</returns>
        public ReaderScope BeginScope(String tag, TextPosition startPosition)
        {
            return new ReaderScope(tag, startPosition)
                .Apply(this._scopes.Push);
        }

        /// <summary>
        /// Ends current <see cref="ReaderScope"/> and get the result expressions of the scope.
        /// </summary>
        /// <param name="tag">The equivalent string to the current <see cref="ReaderScope.Tag"/>.</param>
        /// <returns>An array which contains result expressions of the scope.</returns>
        public YacqExpression[] EndScope(String tag)
        {
            if (this.Current.Tag != tag)
            {
                throw new ParseException("Scope tag was not matched: expected \"" + this.Current.Tag + "\" but got \"" + tag + "\"");
            }
            else
            {
                return this._scopes.Pop().Expressions.ToArray();
            }
        }
    }
}