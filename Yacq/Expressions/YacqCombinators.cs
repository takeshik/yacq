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

using Parseq;
using Parseq.Combinators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Contains factory methods to create parsers for expressions.
    /// </summary>
    public static class YacqCombinators
    {
        #region Satisfy / Any
        
        /// <summary>
        /// Returns a parser which accepts expressions that satisfies specified condition.
        /// </summary>
        /// <param name="predicate">An predicate function to test the expression.</param>
        /// <returns>A parser for expression with specified test.</returns>
        public static Parser<Expression, Expression> Satisfy(Func<Expression, Boolean> predicate)
        {
            return predicate.Satisfy();
        }

        /// <summary>
        /// Returns a parser which accepts expressions that satisfies specified condition and type constraint.
        /// </summary>
        /// <typeparam name="TExpression">The type of the expression to match.</typeparam>
        /// <param name="predicate">An predicate function to test the expression.</param>
        /// <returns>A parser for expression with specified test and type constraint.</returns>
        public static Parser<Expression, TExpression> Satisfy<TExpression>(Func<TExpression, Boolean> predicate)
            where TExpression : Expression
        {
            return Satisfy(e => (e as TExpression).Null(predicate)).Select(e => (TExpression) e);
        }

        /// <summary>
        /// Returns a parser which accepts expressions that satisfies specified type constraint.
        /// </summary>
        /// <typeparam name="TExpression">The type of the expression to match.</typeparam>
        /// <returns>A parser for expression with specified type constraint.</returns>
        public static Parser<Expression, TExpression> Satisfy<TExpression>()
            where TExpression : Expression
        {
            return Satisfy<TExpression>(e => true);
        }

        /// <summary>
        /// Returns a parser which accepts all expressions.
        /// </summary>
        /// <returns>A parser which accepts all expressions.</returns>
        public static Parser<Expression, Expression> Any()
        {
            return Satisfy(e => true);
        }

        #endregion

        #region Identifier

        /// <summary>
        /// Returns a parser which accepts identifiers that satisfies specified condition.
        /// </summary>
        /// <param name="namePredicate">An predicate function to test the name of the identifier.</param>
        /// <returns>A parser for identifier with specified condition.</returns>
        public static Parser<Expression, IdentifierExpression> Identifier(Func<String, Boolean> namePredicate)
        {
            return Satisfy<IdentifierExpression>(e => namePredicate(e.Name));
        }

        /// <summary>
        /// Returns a parser which accepts identifiers that has specified name.
        /// </summary>
        /// <param name="name">A name to test the identifier.</param>
        /// <returns>A parser for identifier with specified name.</returns>
        public static Parser<Expression, IdentifierExpression> Identifier(String name)
        {
            return Identifier(n => n == name);
        }

        /// <summary>
        /// Returns a parser which accepts all identifiers.
        /// </summary>
        /// <returns>A parser which accepts all identifiers.</returns>
        public static Parser<Expression, IdentifierExpression> Identifier()
        {
            return Satisfy<IdentifierExpression>();
        }

        #endregion

        #region LambdaList

        /// <summary>
        /// Returns a parser which accepts lambda lists whose elements are parsed by specified parser.
        /// </summary>
        /// <param name="selector">A parser for elements of the lambda list.</param>
        /// <returns>A parser for lambda list with elements which is accepted by the selector.</returns>
        public static Parser<Expression, Expression> LambdaList<TExpression>(Parser<Expression, TExpression> selector)
            where TExpression : Expression
        {
            return LambdaList().AndAlso(
                selector.Left(Errors.FollowedBy(Prims.Eof<Expression>())),
                e => ((LambdaListExpression) e).Elements
            );
        }

        /// <summary>
        /// Returns a parser which accepts lambda lists whose elements are parsed by specified parser.
        /// </summary>
        /// <param name="elements">A parser for elements of the lambda list.</param>
        /// <returns>A parser for lambda list with elements which is accepted by the parser.</returns>
        public static Parser<Expression, LambdaListExpression> LambdaList(Parser<Expression, IEnumerable<Expression>> elements)
        {
            return Satisfy<LambdaListExpression>(e => elements.Right(Prims.Eof<Expression>())(e.Elements.AsStream()).IsSuccess());
        }

        /// <summary>
        /// Returns a parser which accepts lambda lists whose elements are parsed by specified parsers.
        /// </summary>
        /// <param name="elements">An array of parsers for each element of the lambda list.</param>
        /// <returns>A parser for lambda list with elements which are accepted by the parsers.</returns>
        public static Parser<Expression, LambdaListExpression> LambdaList(params Parser<Expression, Expression>[] elements)
        {
            return LambdaList(Combinator.Sequence(elements));
        }

        /// <summary>
        /// Returns a parser which accepts all lambda lists.
        /// </summary>
        /// <returns>A parser which accepts all lambda lists.</returns>
        public static Parser<Expression, LambdaListExpression> LambdaList()
        {
            return Satisfy<LambdaListExpression>();
        }

        #endregion

        #region List

        /// <summary>
        /// Returns a parser which accepts lists whose elements are parsed by specified parser.
        /// </summary>
        /// <param name="selector">A parser for elements of the list.</param>
        /// <returns>A parser for list with elements which is accepted by the selector.</returns>
        public static Parser<Expression, Expression> List<TExpression>(Parser<Expression, TExpression> selector)
            where TExpression : Expression
        {
            return List().AndAlso(
                selector.Left(Errors.FollowedBy(Prims.Eof<Expression>())),
                e => ((ListExpression) e).Elements
            );
        }

        /// <summary>
        /// Returns a parser which accepts lists whose elements are parsed by specified parser.
        /// </summary>
        /// <param name="elements">A parser for elements of the list.</param>
        /// <returns>A parser for list with elements which is accepted by the parser.</returns>
        public static Parser<Expression, ListExpression> List(Parser<Expression, IEnumerable<Expression>> elements)
        {
            return Satisfy<ListExpression>(e => elements.Right(Prims.Eof<Expression>())(e.Elements.AsStream()).IsSuccess());
        }

        /// <summary>
        /// Returns a parser which accepts lists whose elements are parsed by specified parsers.
        /// </summary>
        /// <param name="elements">An array of parsers for each element of the list.</param>
        /// <returns>A parser for list with elements which are accepted by the parsers.</returns>
        public static Parser<Expression, ListExpression> List(params Parser<Expression, Expression>[] elements)
        {
            return List(Combinator.Sequence(elements));
        }

        /// <summary>
        /// Returns a parser which accepts all lists.
        /// </summary>
        /// <returns>A parser which accepts all lists.</returns>
        public static Parser<Expression, ListExpression> List()
        {
            return Satisfy<ListExpression>();
        }

        #endregion

        #region Number

        /// <summary>
        /// Returns a parser which accepts numbers that satisfies specified condition.
        /// </summary>
        /// <param name="valuePredicate">An predicate function to test the value of the number.</param>
        /// <returns>A parser for number with specified condition.</returns>
        public static Parser<Expression, NumberExpression> Number(Func<Object, Boolean> valuePredicate)
        {
            return Satisfy<NumberExpression>(e => valuePredicate(e.Value));
        }

        /// <summary>
        /// Returns a parser which accepts numbers that satisfies specified condition and type constraint.
        /// </summary>
        /// <typeparam name="TValue">The type of the number to match.</typeparam>
        /// <param name="valuePredicate">An predicate function to test the value of the number.</param>
        /// <returns>A parser for number with specified condition and type constraint.</returns>
        public static Parser<Expression, NumberExpression> Number<TValue>(Func<TValue, Boolean> valuePredicate)
            where TValue : struct
        {
            return Number(v => v is TValue && valuePredicate((TValue) v));
        }

        /// <summary>
        /// Returns a parser which accepts numbers that satisfies specified type constraint.
        /// </summary>
        /// <typeparam name="TValue">The type of the number to match.</typeparam>
        /// <returns>A parser for number with specified type constraint.</returns>
        public static Parser<Expression, NumberExpression> Number<TValue>()
            where TValue : struct
        {
            return Number<TValue>(v => true);
        }

        /// <summary>
        /// Returns a parser which accepts all numbers.
        /// </summary>
        /// <returns>A parser which accepts all numbers.</returns>
        public static Parser<Expression, NumberExpression> Number()
        {
            return Number(e => true);
        }

        #endregion

        #region Text

        /// <summary>
        /// Returns a parser which accepts texts that satisfies specified condition.
        /// </summary>
        /// <param name="valuePredicate">An predicate function to test the value of the text.</param>
        /// <returns>A parser for text with specified condition.</returns>
        public static Parser<Expression, TextExpression> Text(Func<Object, Boolean> valuePredicate)
        {
            return Satisfy<TextExpression>(e => valuePredicate(e.Value));
        }

        /// <summary>
        /// Returns a parser which accepts texts that satisfies specified condition and type constraint.
        /// </summary>
        /// <typeparam name="TValue">The type of the text to match.</typeparam>
        /// <param name="valuePredicate">An predicate function to test the value of the text.</param>
        /// <returns>A parser for text with specified condition and type constraint.</returns>
        public static Parser<Expression, TextExpression> Text<TValue>(Func<TValue, Boolean> valuePredicate)
        {
            return Text(v => v is TValue && valuePredicate((TValue) v));
        }

        /// <summary>
        /// Returns a parser which accepts texts that satisfies specified type constraint.
        /// </summary>
        /// <typeparam name="TValue">The type of the text to match.</typeparam>
        /// <returns>A parser for text with specified type constraint.</returns>
        public static Parser<Expression, TextExpression> Text<TValue>()
        {
            return Text<TValue>(v => true);
        }

        /// <summary>
        /// Returns a parser which accepts all texts.
        /// </summary>
        /// <returns>A parser which accepts all texts.</returns>
        public static Parser<Expression, TextExpression> Text()
        {
            return Text(v => true);
        }

        #endregion

        #region Vector

        /// <summary>
        /// Returns a parser which accepts vectors whose elements are parsed by specified parser.
        /// </summary>
        /// <param name="selector">A parser for elements of the vector.</param>
        /// <returns>A parser for vector with elements which is accepted by the selector.</returns>
        public static Parser<Expression, Expression> Vector<TExpression>(Parser<Expression, TExpression> selector)
            where TExpression : Expression
        {
            return Vector().AndAlso(
                selector.Left(Errors.FollowedBy(Prims.Eof<Expression>())),
                e => ((VectorExpression) e).Elements
            );
        }

        /// <summary>
        /// Returns a parser which accepts vectors whose elements are parsed by specified parser.
        /// </summary>
        /// <param name="elements">A parser for elements of the vector.</param>
        /// <returns>A parser for vector with elements which are accepted by the parser.</returns>
        public static Parser<Expression, VectorExpression> Vector(Parser<Expression, IEnumerable<Expression>> elements)
        {
            return Satisfy<VectorExpression>(e => elements.Right(Prims.Eof<Expression>())(e.Elements.AsStream()).IsSuccess());
        }

        /// <summary>
        /// Returns a parser which accepts vectors whose elements are parsed by specified parsers.
        /// </summary>
        /// <param name="elements">An array of parsers for each element of the vector.</param>
        /// <returns>A parser for vector with elements which are accepted by the parsers.</returns>
        public static Parser<Expression, TExpression> Vector<TExpression>(
            Parser<Expression, IEnumerable<Expression>> elements,
            Func<IEnumerable<Expression>, TExpression> selector
        )
            where TExpression : Expression
        {
            return Vector(elements).Select(e => selector(e.Elements));
        }

        /// <summary>
        /// Returns a parser which accepts vectors whose elements are parsed by specified parsers.
        /// </summary>
        /// <param name="elements">An array of parsers for each element of the vector.</param>
        /// <returns>A parser for vector with elements which are accepted by the parsers.</returns>
        public static Parser<Expression, VectorExpression> Vector(params Parser<Expression, Expression>[] elements)
        {
            return Vector(Combinator.Sequence(elements));
        }

        /// <summary>
        /// Returns a parser which accepts all vectors.
        /// </summary>
        /// <returns>A parser which accepts all vectors.</returns>
        public static Parser<Expression, VectorExpression> Vector()
        {
            return Satisfy<VectorExpression>();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Returns an accessor for parsers which combinates with specified parser and reduces parsing expressions.
        /// </summary>
        /// <param name="parser">A parser to wrap for reducing.</param>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>An accessor for parsing with reduced expressions.</returns>
        public static YacqReducingCombinator Reduce(this Parser<Expression, Expression> parser, SymbolTable symbols = null, Type expectedType = null)
        {
            return new YacqReducingCombinator(parser, symbols, expectedType);
        }

        /// <summary>
        /// Returns an accessor for parsers which reduces expressions.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>An accessor for parsing with reduced expressions.</returns>
        public static YacqReducingCombinator Reduce(SymbolTable symbols = null, Type expectedType = null)
        {
            return Reduce(null, symbols, expectedType);
        }

        /// <summary>
        /// Sets the result of the parser with applying selector function to the symbol table with specified name.
        /// </summary>
        /// <typeparam name="TResult">The result type of selector.</typeparam>
        /// <param name="parser">The parser to be set the result.</param>
        /// <param name="symbols">The symbol table to set the result.</param>
        /// <param name="id">The name of the value to be set.</param>
        /// <param name="selector">The function to apply the result of the parser.</param>
        /// <returns>The input parser with applying the selector function.</returns>
        public static Parser<Expression, TResult> As<TResult>(
            this Parser<Expression, TResult> parser,
            SymbolTable symbols,
            String id,
            Func<TResult, Expression> selector
        )
        {
            return parser.Select(e => e.Apply(_ => symbols.Add(id, selector(_))));
        }

        /// <summary>
        /// Sets the result of the parser to the symbol table with specified name.
        /// </summary>
        /// <typeparam name="TExpression">The result type of parser.</typeparam>
        /// <param name="parser">The parser to be set the result.</param>
        /// <param name="symbols">The symbol table to set the result.</param>
        /// <param name="id">The name of the value to be set.</param>
        /// <returns>The input parser.</returns>
        public static Parser<Expression, TExpression> As<TExpression>(
            this Parser<Expression, TExpression> parser,
            SymbolTable symbols,
            String id
        )
            where TExpression : Expression
        {
            return As(parser, symbols, id, e => e);
        }

        /// <summary>
        /// Sets the sequential result of the parser to the symbol table with specified name.
        /// </summary>
        /// <typeparam name="TExpression">The element type of the result of parser.</typeparam>
        /// <param name="parser">The parser to be set the result.</param>
        /// <param name="symbols">The symbol table to set the result.</param>
        /// <param name="id">The name of the value to be set.</param>
        /// <returns>The input parser.</returns>
        /// <remarks>The result sequence is set as <see cref="VectorExpression"/> in the <paramref name="symbols"/>.</remarks>
        public static Parser<Expression, IEnumerable<TExpression>> As<TExpression>(
            this Parser<Expression, IEnumerable<TExpression>> parser,
            SymbolTable symbols,
            String id
        )
            where TExpression : Expression
        {
            return As(parser, symbols, id, YacqExpression.Vector);
        }

        private static Parser<Expression, TExpression> AndAlsoImpl<TExpression>(
            this Parser<Expression, Expression> parser0,
            Parser<Expression, TExpression> parser1,
            Func<IStream<Expression>, IStream<TExpression>> streamSelector
        )
            where TExpression : Expression
        {
            return stream =>
            {
                Expression result;
                ErrorMessage message = null;
                switch (parser0 != null
                    ? parser0(stream).TryGetValue(out result, out message)
                    : ReplyStatus.Success
                    )
                {
                    case ReplyStatus.Success:
                        return parser1(stream.If(s => streamSelector != null, streamSelector));
                    case ReplyStatus.Error:
                        return Reply.Error<Expression, TExpression>(stream, message);
                    default:
                        return Reply.Failure<Expression, TExpression>(stream);
                }
            };
        }

        internal static Parser<Expression, TExpression> AndAlso<TExpression>(
            this Parser<Expression, Expression> parser0,
            Parser<Expression, TExpression> parser1,
            Func<Expression, TExpression> streamSelector
        )
            where TExpression : Expression
        {
            return AndAlsoImpl(parser0, parser1, s => s.Select(streamSelector));
        }

        internal static Parser<Expression, TExpression> AndAlso<TExpression>(
            this Parser<Expression, Expression> parser0,
            Parser<Expression, TExpression> parser1,
            Func<Expression, IEnumerable<TExpression>> streamSelector
        )
            where TExpression : Expression
        {
            return AndAlsoImpl(parser0, parser1, s => s.SelectMany(e => streamSelector(e).AsStream()));
        }

        internal static Parser<Expression, Expression> AndAlso(
            this Parser<Expression, Expression> parser0,
            Parser<Expression, Expression> parser1
        )
        {
            return AndAlsoImpl(parser0, parser1, s => s);
        }

        #endregion
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
