﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Collections;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents a list, the basic expression of YACQ to call functions, methods and constructors.
    /// </summary>
    public class ListExpression
        : YacqSequenceExpression
    {
        internal ListExpression(
            SymbolTable symbols,
            YacqList elements
        )
            : base(symbols, elements)
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
            return (this.List(".") ?? this.List(":")).Null(_ => _.Stringify(this[0].Id()))
                ?? "(" + this.Elements.Stringify(" ") + ")";
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            Expression value = null;
            if (!this.Elements.Any())
            {
                return Empty();
            }
            if (!(this[0] is IdentifierExpression)
                || symbols.ResolveMatch(DispatchTypes.Member, this[0].Id()) != null
            )
            {
                value = this[0].TryReduce(symbols);
                if (value is MacroExpression)
                {
                    return ((MacroExpression) value).Evaluate(symbols, this.Elements.Skip(1));
                }
                if (value != null && value.Type(symbols).GetDelegateSignature() != null)
                {
                    return Invoke(value, this.Elements.Skip(1).ReduceAll(symbols));
                }
                if (value is TypeCandidateExpression)
                {
                    return Dispatch(
                        symbols,
                        DispatchTypes.Constructor,
                        value,
                        null,
                        this.Elements.Skip(1)
                    );
                }
            }
            if (this[0] is IdentifierExpression
                && symbols.ResolveMatch(DispatchTypes.Method, this[0].Id()) != null
                || symbols.Missing != DispatchExpression.DefaultMissing
            )
            {
                return Function(
                    symbols,
                    this[0].Id(),
                    this.Elements.Skip(1)
                );
            }
            if (value != null && this.Length == 1)
            {
                return value;
            }
            throw new ParseException("List evaluation failed: " + this, this);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="ListExpression"/> that represents the list.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="ListExpression"/> that has specified elements.</returns>
        public static ListExpression List(SymbolTable symbols, YacqList elements)
        {
            return new ListExpression(symbols, elements);
        }

        /// <summary>
        /// Creates a <see cref="ListExpression"/> that represents the list.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="elements">An array of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="ListExpression"/> that has specified elements.</returns>
        public static ListExpression List(SymbolTable symbols, IEnumerable<Expression> elements)
        {
            return List(symbols, YacqList.Create(elements));
        }

        /// <summary>
        /// Creates a <see cref="ListExpression"/> that represents the list.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="ListExpression"/> that has specified elements.</returns>
        public static ListExpression List(SymbolTable symbols, params Expression[] elements)
        {
            return List(symbols, (IEnumerable<Expression>) elements);
        }

        /// <summary>
        /// Creates a <see cref="ListExpression"/> that represents the list.
        /// </summary>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="ListExpression"/> that has specified elements.</returns>
        public static ListExpression List(YacqList elements)
        {
            return List(null, elements);
        }

        /// <summary>
        /// Creates a <see cref="ListExpression"/> that represents the list.
        /// </summary>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="ListExpression"/> that has specified elements.</returns>
        public static ListExpression List(IEnumerable<Expression> elements)
        {
            return List(null, elements);
        }

        /// <summary>
        /// Creates a <see cref="ListExpression"/> that represents the list.
        /// </summary>
        /// <param name="elements">An array of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="ListExpression"/> that has specified elements.</returns>
        public static ListExpression List(params Expression[] elements)
        {
            return List(null, elements);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
