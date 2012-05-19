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
using System.Collections.Generic;
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
        /// Gets the type of quoting of this expression.
        /// </summary>
        /// <value>The type of quoting of this expression</value>
        public QuoteType QuoteType
        {
            get;
            private set;
        }

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
            QuoteType quoteType,
            Expression expression
        )
            : base(symbols)
        {
            this.QuoteType = quoteType;
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
            return (this.QuoteType == QuoteType.Quote ? "#'" : "#`") + this.Expression;
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return this.QuoteType == QuoteType.Quasiquote
                ? ProcessQuasiquote(this.Expression, symbols, 1)
                : this.Expression is LambdaExpression
                      ? (Expression) Quote(this.Expression)
                      : Constant(this.Expression);
        }

        private static Expression ProcessQuasiquote(Expression expression, SymbolTable symbols, Int32 level)
        {
            return (expression as ListExpression).Null(el => el[0].Id().Let(i => i == "quasiquote"
                ? ProcessQuasiquote(el[1], symbols, ++level)
                : i == "unquote"
                      ? Unquote(el, symbols, level)
                      : el.Elements
                            .PartitionBy(_ => _.List("unquote-splicing") != null)
                            .Select(p => (p.First().List("unquote-splicing") != null
                                ? p.Select(e => ((ListExpression) e)[1]
                                      .Reduce(symbols)
                                      .If(_ => typeof(YacqSequenceExpression).IsAppropriate(_.Type), _ => _.Member(symbols, "Elements"))
                                  )
                                : p.Select(e => Unquote(e, symbols, level))
                            ).ToArray())
                            .ToArray()
                            .Let(ps => ps
                                .Skip(1)
                                .Aggregate((Expression) Vector(symbols, ps.First()), (v, p) => v.Method(symbols, "+", p))
                                .Method(symbols, "list")
                            )
            ))
                ?? (expression as VectorExpression).Null(ev => Vector(symbols, ev.Elements.Select(e => ProcessQuasiquote(e, symbols, level))))
                ?? (expression as LambdaListExpression).Null(el => LambdaList(symbols, el.Elements.Select(e => ProcessQuasiquote(e, symbols, level))))
                ?? (Expression) Quote(symbols, expression);
        }

        private static Expression Unquote(Expression expression, SymbolTable symbols, Int32 level)
        {
            return expression.List("unquote") != null
                ? EnumerableEx.Generate(
                      (ListExpression) expression,
                      _ => _ != null && _[0].Id() == "unquote",
                      _ => _[1] as ListExpression,
                      _ => _[1]
                  )
                      .ToArray()
                      .Let(es => Enumerable.Range(0, level - es.Length)
                          .Aggregate(es.Last(), (e, i) => Quote(symbols, e))
                          .If(e => !(e as IdentifierExpression).Null(_ => _.Type(symbols) == typeof(IdentifierExpression)),
                              e => e.Reduce(symbols).If(_ => !typeof(Expression).IsAppropriate(_.Type),
                                  _ => Function(symbols, "const", _)
                              )
                          )
                      )
                : ProcessQuasiquote(expression, symbols, level);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="QuotedExpression"/> that returns specified expression.
        /// </summary>
        /// <returns>A <see cref="QuotedExpression"/>.</returns>
        public static QuotedExpression Quote(SymbolTable symbols, Expression expression)
        {
            return new QuotedExpression(symbols, QuoteType.Quote, expression);
        }

        /// <summary>
        /// Creates a <see cref="QuotedExpression"/> that returns specified expression.
        /// </summary>
        /// <returns>A <see cref="QuotedExpression"/>.</returns>
        public static new QuotedExpression Quote(Expression expression)
        {
            return Quote(null, expression);
        }

        /// <summary>
        /// Creates a <see cref="QuotedExpression"/> that returns specified expression with unquoting.
        /// </summary>
        /// <returns>A <see cref="QuotedExpression"/>.</returns>
        public static QuotedExpression Quasiquote(SymbolTable symbols, Expression expression)
        {
            return new QuotedExpression(symbols, QuoteType.Quasiquote, expression);
        }

        /// <summary>
        /// Creates a <see cref="QuotedExpression"/> that returns specified expression with unquoting.
        /// </summary>
        /// <returns>A <see cref="QuotedExpression"/>.</returns>
        public static QuotedExpression Quasiquote(Expression expression)
        {
            return Quasiquote(null, expression);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
