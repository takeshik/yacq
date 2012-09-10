// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
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
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression that is ignored in the results.
    /// </summary>
    public class IgnoredExpression
        : YacqExpression
    {
        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        /// <returns><c>true</c> if the node can be reduced, otherwise <c>false</c>.</returns>
        public override Boolean CanReduce
        {
            get
            {
                return false;
            }
        }

        internal IgnoredExpression(
            SymbolTable symbols
        )
            : base(symbols)
        {
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return "(Ignored)";
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return null;
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="IgnoredExpression"/> that is ignored in the results.
        /// </summary>
        /// <returns>An <see cref="IgnoredExpression"/>.</returns>
        public static IgnoredExpression Ignore(SymbolTable symbols)
        {
            return new IgnoredExpression(symbols);
        }

        /// <summary>
        /// Creates a <see cref="IgnoredExpression"/> that is ignored in the results.
        /// </summary>
        /// <returns>An <see cref="IgnoredExpression"/>.</returns>
        public static IgnoredExpression Ignore()
        {
            return Ignore(null);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
