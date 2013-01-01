// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
    /// Represents an ambiguous parameter expression, a parameter expression which may be type-unfixed.
    /// </summary>
    public class AmbiguousParameterExpression
        : YacqExpression
    {
        private readonly Type _type;

        /// <summary>
        /// Gets the static type of the expression that this expression represents.
        /// </summary>
        /// <value>The <see cref="System.Type"/> that represents the static type of the expression.</value>
        public override Type Type
        {
            get
            {
                if (this._type == null)
                {
                    throw new InvalidOperationException("The type of this parameter is null. Use Type() extension method instead.");
                }
                return this._type;
            }
        }

        /// <summary>
        /// Gets the name of this parameter.
        /// </summary>
        /// <value>The name of this parameter.</value>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the type of this parameter is unfixed.
        /// </summary>
        /// <value><c>true</c> if the type of this parameter is unfixed; otherwise, <c>false</c>.</value>
        public Boolean IsUnfixed
        {
            get
            {
                return
                    this._type == null ||
                    this._type.ContainsGenericParameters ||
                    this._type.IsGenericTypeDefinition;
            }
        }

        internal AmbiguousParameterExpression(
            SymbolTable symbols,
            Type type,
            String name
        )
            : base(symbols)
        {
            this._type = type;
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
            return this.Name ?? "?";
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return this.IsUnfixed
                ? null
                : Parameter(this.Type, this.Name);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="type">>The type of the parameter.</param>
        /// <param name="name">The name of this parameter.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has the specified name and type.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(SymbolTable symbols, Type type, String name)
        {
            return new AmbiguousParameterExpression(symbols, type, name);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="type">>The type of the parameter.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has no name and specified type.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(SymbolTable symbols, Type type)
        {
            return AmbiguousParameter(symbols, type, null);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name of this parameter.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has the specified name and its type is not fixed.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(SymbolTable symbols, String name)
        {
            return AmbiguousParameter(symbols, null, name);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has no name and its type is not fixed.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(SymbolTable symbols)
        {
            return AmbiguousParameter(symbols, null, null);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="type">>The type of the parameter.</param>
        /// <param name="name">The name of this parameter.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has the specified name and type.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(Type type, String name)
        {
            return AmbiguousParameter(null, type, name);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="type">>The type of the parameter.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has no name and specified type.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(Type type)
        {
            return AmbiguousParameter(null, type, null);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        /// <param name="name">The name of this parameter.</param>
        /// <returns>An <see cref="AmbiguousParameterExpression"/> that has the specified name and its type is not fixed.</returns>
        public static AmbiguousParameterExpression AmbiguousParameter(String name)
        {
            return AmbiguousParameter(null, null, name);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousParameterExpression"/> that represents the type-unfixed parameter expression.
        /// </summary>
        public static AmbiguousParameterExpression AmbiguousParameter()
        {
            return AmbiguousParameter(null, null, null);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
