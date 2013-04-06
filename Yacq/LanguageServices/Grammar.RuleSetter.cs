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
using System.Linq;
using Parseq;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    partial class Grammar
    {
        /// <summary>
        /// Provides setter access for the grammar.
        /// </summary>
        public class RuleSetter
        {
            private readonly Grammar _grammar;

            internal RuleSetter(Grammar grammar)
            {
                this._grammar = grammar;
            }

            /// <summary>
            /// Sets the parser with specified rule key.
            /// </summary>
            /// <param name="category">The category to set the parser.</param>
            /// <param name="priority">The priority to set the parser.</param>
            /// <param name="id">The ID to set the parser.</param>
            /// <value>The parser for specified rule key.</value>
            public Func<RuleGetter, Parser<Char, YacqExpression>> this[String category, Int32 priority, String id]
            {
                set
                {
                    this._grammar[category, priority, id] = this._grammar.MakeValue(value);
                }
            }

            /// <summary>
            /// Sets the parser with specified rule key.
            /// </summary>
            /// <param name="category">The category to set the parser.</param>
            /// <param name="priority">The priority to set the parser.</param>
            /// <value>The parser for specified rule key.</value>
            public Func<RuleGetter, Parser<Char, YacqExpression>> this[String category, Int32 priority]
            {
                set
                {
                    this._grammar[category, priority] = this._grammar.MakeValue(value);
                }
            }

            /// <summary>
            /// Sets the parser with specified rule key.
            /// </summary>
            /// <param name="category">The category to set the parser.</param>
            /// <param name="id">The ID to set the parser.</param>
            /// <value>The parser for specified rule key.</value>
            public Func<RuleGetter, Parser<Char, YacqExpression>> this[String category, String id]
            {
                set
                {
                    this._grammar[category, id] = this._grammar.MakeValue(value);
                }
            }

            /// <summary>
            /// Sets the parser of the default rule.
            /// </summary>
            /// <value>The parser for the default rule.</value>
            public Func<RuleGetter, Parser<Char, YacqExpression>> Default
            {
                set
                {
                    this._grammar.DefaultRule = this._grammar.MakeValue(value);
                }
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
