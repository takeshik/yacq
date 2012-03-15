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
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents a quoted expression, it returns an <see cref="Expression"/> objects.
    /// </summary>
    public class QuotedExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets an <see cref="Expression"/> that represents the return value of this expression.
        /// </summary>
        /// <value>An <see cref="Expression"/> that represents the return value of this expression.</value>
        public Expression Expression
        {
            get;
            private set;
        }

        internal QuotedExpression(
            SymbolTable symbols,
            Expression expression
        )
            : base(symbols)
        {
            this.Expression = expression;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return "'" + this.Expression;
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additioしかし、Parseq を使うとなると、Yacq の「依存ライブラリは Rx / Ix 以外無し！」の文言を消さないといけない…とはいえ、Rx / Ix だけ！っていっても、全部で 4 アセンブリなわけで、半分詐欺ですけど。nal symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return this.Expression is LambdaExpression
                ? (Expression) Quote(this.Expression)
                : Constant(this.Expression);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="QuotedExpression"/> that returns specified expression.
        /// </summary>
        /// <returns>An <see cref="QuotedExpression"/>.</returns>
        public static QuotedExpression Quote(SymbolTable symbols, Expression expression)
        {
            return new QuotedExpression(symbols, expression);
        }

        /// <summary>
        /// Creates a <see cref="QuotedExpression"/> that returns specified expression.
        /// </summary>
        /// <returns>An <see cref="QuotedExpression"/>.</returns>
        public static QuotedExpression Quote(Expression expression)
        {
            return Quote(null, expression);
        }
    }
}