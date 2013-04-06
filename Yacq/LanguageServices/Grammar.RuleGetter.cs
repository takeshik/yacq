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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XSpect.Yacq.Expressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    partial class Grammar
    {
        /// <summary>
        /// Provides getter access for the grammar.
        /// </summary>
        public class RuleGetter
            : IEnumerable<KeyValuePair<RuleKey, Parser<Char, YacqExpression>>>
        {
            private readonly Grammar _grammar;

            internal RuleGetter(Grammar grammar)
            {
                this._grammar = grammar;
            }

            /// <summary>
            /// Gets the parser with specified rule key.
            /// </summary>
            /// <param name="category">The category to get the parser.</param>
            /// <param name="priority">The priority to get the parser.</param>
            /// <value>The parser with specified rule key.</value>
            public Parser<Char, YacqExpression> this[String category, Int32 priority]
            {
                get
                {
                    return this._grammar[category, priority].Value;
                }
            }

            /// <summary>
            /// Gets the parser with specified rule key.
            /// </summary>
            /// <param name="category">The category to get the parser.</param>
            /// <param name="id">The ID to get the parser.</param>
            /// <value>The parser with specified rule key.</value>
            public Parser<Char, YacqExpression> this[String category, String id]
            {
                get
                {
                    return this._grammar[category, id].Value;
                }
            }

            /// <summary>
            /// Gets the sequence of the parser with specified category.
            /// </summary>
            /// <param name="category">The category to get the parser.</param>
            /// <value>The sequence of the parser with specified category.</value>
            public IEnumerable<Parser<Char, YacqExpression>> this[String category]
            {
                get
                {
                    return this._grammar[category]
                        .Select(v => v.Value);
                }
            }

            /// <summary>
            /// Gets the parser of the default rule.
            /// </summary>
            /// <value>The parser of the default rule.</value>
            public Parser<Char, YacqExpression> Default
            {
                get
                {
                    return this._grammar.DefaultRule.Value;
                }
            }

            /// <summary>
            /// Gets a collection containing the parsers in this grammar.
            /// </summary>
            /// <value>A collection containing the parsers in this grammar.</value>
            public IEnumerable<Parser<Char, YacqExpression>> Values
            {
                get
                {
                    return this._grammar.Values
                        .Select(v => v.Value);
                }
            }

            /// <summary>
            /// Returns an enumerator that iterates through rules in this grammar.
            /// </summary>
            /// <returns>
            /// A <see cref="IEnumerator{T}"/> that can be used to iterate through rules in this grammar.
            /// </returns>
            public IEnumerator<KeyValuePair<RuleKey, Parser<Char, YacqExpression>>> GetEnumerator()
            {
                return this._grammar
                    .Select(p => new KeyValuePair<RuleKey, Parser<Char, YacqExpression>>(p.Key, p.Value.Value))
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
