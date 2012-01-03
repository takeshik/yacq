// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
    /// Generates pre-evaluating <see cref="YacqExpression"/> by supplied rules from code string sequence.
    /// </summary>
    public partial class Reader
    {
        /// <summary>
        /// Gets the list of reading rules.
        /// </summary>
        /// <value>The list of reading rules.</value>
        public IList<Action<ReaderCursor, ReaderResult>> Rules
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="rules">The sequence of additional rules.</param>
        public Reader(IEnumerable<Action<ReaderCursor, ReaderResult>> rules)
        {
            this.Rules = new List<Action<ReaderCursor, ReaderResult>>();
            (rules ?? Enumerable.Empty<Action<ReaderCursor, ReaderResult>>()).ForEach(this.Rules.Add);
            this.InitializeRules();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="rules">The array of additional rules.</param>
        public Reader(params Action<ReaderCursor, ReaderResult>[] rules)
            : this((IEnumerable<Action<ReaderCursor, ReaderResult>>) rules)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        public Reader()
            : this(null)
        {
        }

        /// <summary>
        /// Reads the code string and generates expressions.
        /// </summary>
        /// <param name="input">The code string to read.</param>
        /// <returns>Generated expressions.</returns>
        public YacqExpression[] Read(String input)
        {
            var cursor = new ReaderCursor(this, input);
            var result = new ReaderResult();
            while (cursor.PeekCharForward(0) != '\0')
            {
                var index = cursor.Position.Index;
                if (this.Rules
                    .Do(f => f(cursor, result))
                    .FirstOrDefault(_ => cursor.Position.Index > index) == null
                )
                {
                    return null;
                }
            }
            if (result.Depth > 1)
            {
                throw new InvalidOperationException();
            }
            else
            {
                return result.EndScope(null);
            }
        }
    }
}