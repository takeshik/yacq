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

using System.Collections.Generic;
using Parseq;
using System;
using System.Linq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides the factory methods of combinated parsers for expressions with evaluating.
    /// </summary>
    public class YacqEvaluatingCombinator
    {
        private readonly Parser<Expression, Expression> _parser;

        internal YacqEvaluatingCombinator(Parser<Expression, Expression> parser)
        {
            this._parser = parser ?? YacqCombinators.Any();
        }

        #region Satisfy / Any

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value satisfies specified condition.
        /// </summary>
        /// <param name="predicate">A predicate function to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated value with specified test.</returns>
        public Parser<Expression, Object> Satisfy(Func<Object, Boolean> predicate)
        {
            return this._parser.AndAlso(YacqCombinators.Any()
                .Select(e => e.Evaluate())
                .Where(predicate)
            );
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value satisfies specified condition and type constraint.
        /// </summary>
        /// <typeparam name="TResult">The type of the evaluated value of the expression to match.</typeparam>
        /// <param name="predicate">A predicate function to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated value with specified test and type constraint.</returns>
        public Parser<Expression, TResult> Satisfy<TResult>(Func<TResult, Boolean> predicate)
        {
            return this._parser.AndAlso(YacqCombinators.Any()
                .Select(e => e.Evaluate())
                .Where(o => o is TResult && predicate((TResult) o))
                .Select(o => (TResult) o)
            );
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value is equals to specified value.
        /// </summary>
        /// <param name="value">A value to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated value with specified test.</returns>
        public Parser<Expression, Object> Satisfy(Object value)
        {
            return this.Satisfy(o => o.Equals(value));
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value is equals to specified value.
        /// </summary>
        /// <typeparam name="TResult">The type of the evaluated value of the expression to match.</typeparam>
        /// <param name="value">A value to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated value with specified test and type constraint.</returns>
        public Parser<Expression, TResult> Satisfy<TResult>(TResult value)
        {
            return this.Satisfy<TResult>(o => EqualityComparer<TResult>.Default.Equals(o, value));
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value satisfies specified type constraint.
        /// </summary>
        /// <typeparam name="TResult">The type of the evaluated value of the expression to match.</typeparam>
        /// <returns>A parser for evaluated value with specified type constraint.</returns>
        public Parser<Expression, TResult> Satisfy<TResult>()
        {
            return this._parser.AndAlso(YacqCombinators.Any()
                .Select(e => e.Evaluate())
                .Where(o => o is TResult)
                .Select(o => (TResult) o)
            );
        }

        /// <summary>
        /// Returns a parser which accepts all evaluated values.
        /// </summary>
        /// <returns>A parser which accepts all evaluated value.</returns>
        public Parser<Expression, Object> Any()
        {
            return this.Satisfy(e => true);
        }

        #endregion

        #region Where

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value satisfies specified condition.
        /// </summary>
        /// <param name="predicate">A predicate function to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated expression with specified test.</returns>
        public Parser<Expression, Expression> Where(Func<Object, Boolean> predicate)
        {
            return this._parser.AndAlso(YacqCombinators.Satisfy(
                e => predicate(e.Evaluate())
            ));
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value satisfies specified condition and type constraint.
        /// </summary>
        /// <typeparam name="TResult">The type of the evaluated value of the expression to match.</typeparam>
        /// <param name="predicate">A predicate function to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated expression with specified test and type constraint.</returns>
        public Parser<Expression, Expression> Where<TResult>(Func<TResult, Boolean> predicate)
        {
            return this._parser.AndAlso(YacqCombinators.Satisfy(e => e
                .Evaluate()
                .Let(o => o is TResult && predicate((TResult) o))
            ));
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value is equals to specified value.
        /// </summary>
        /// <param name="value">A value to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated expression with specified test.</returns>
        public Parser<Expression, Expression> Where(Object value)
        {
            return this.Where(o => o.Equals(value));
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose evaluated value is equals to specified value.
        /// </summary>
        /// <typeparam name="TResult">The type of the evaluated value of the expression to match.</typeparam>
        /// <param name="value">A value to test the evaluated value of the expression.</param>
        /// <returns>A parser for evaluated expression with specified test and type constraint.</returns>
        public Parser<Expression, Expression> Where<TResult>(TResult value)
        {
            return this.Where<TResult>(o => EqualityComparer<TResult>.Default.Equals(o, value));
        }

        #endregion

        #region Is

        /// <summary>
        /// Returns a parser which accepts expressions whose type of evaluated value satisfies specified predicate.
        /// </summary>
        /// <param name="typePredicate">A predicate function to test the type of the evaluated value.</param>
        /// <returns>A parser for evaluated expression with specified test.</returns>
        public Parser<Expression, Expression> Is(Func<Type, Boolean> typePredicate)
        {
            return this.Where(o => typePredicate(o.GetType()));
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose type of evaluated value satisfies specified predicate.
        /// </summary>
        /// <param name="type">A type of the evaluated value to test.</param>
        /// <returns>A parser for evaluated expression with specified test.</returns>
        public Parser<Expression, Expression> Is(Type type)
        {
            return this.Is(type.IsAppropriate);
        }

        /// <summary>
        /// Returns a parser which accepts expressions whose type of evaluated value satisfies specified predicate.
        /// </summary>
        /// <typeparam name="T">A type of the evaluated value to test.</typeparam>
        /// <returns>A parser for evaluated expression with specified test.</returns>
        public Parser<Expression, Expression> Is<T>()
        {
            return this.Where(o => o is T);
        }

        #endregion
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
