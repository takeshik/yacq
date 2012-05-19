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

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression which is an identifier.
    /// </summary>
    public class IdentifierExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets the name of this expression.
        /// </summary>
        /// <value>The name of this expression.</value>
        public String Name
        {
            get;
            private set;
        }

        internal IdentifierExpression(
            SymbolTable symbols,
            String name
        )
            : base(symbols)
        {
            this.Name = name;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return this.Name;
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Variable(symbols, this.Name).TryReduce
                (symbols).Let(e => (e as MacroExpression).Null(m => m.Evaluate(symbols)) ?? e);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="IdentifierExpression"/> that represents identifier with specified name.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name of this expression.</param>
        /// <returns>An <see cref="IdentifierExpression"/> that has the specified name.</returns>
        public static IdentifierExpression Identifier(SymbolTable symbols, String name)
        {
            return new IdentifierExpression(symbols, name);
        }

        /// <summary>
        /// Creates a <see cref="IdentifierExpression"/> that represents identifier with specified name.
        /// </summary>
        /// <param name="name">The name of this expression.</param>
        /// <returns>An <see cref="IdentifierExpression"/> that has the specified name.</returns>
        public static IdentifierExpression Identifier(String name)
        {
            return Identifier(null, name);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
