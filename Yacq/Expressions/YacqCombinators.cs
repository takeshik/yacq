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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    public static class YacqCombinators
    {
        #region Satisfy / Any

        public static Parser<Expression, Expression> Satisfy(Func<Expression, Boolean> predicate)
        {
            return predicate.Satisfy();
        }

        public static Parser<Expression, TExpression> Satisfy<TExpression>(Func<TExpression, Boolean> predicate)
            where TExpression : Expression
        {
            return Satisfy(e => (e as TExpression).Null(predicate)).Select(e => (TExpression) e);
        }

        public static Parser<Expression, TExpression> Satisfy<TExpression>()
            where TExpression : Expression
        {
            return Satisfy<TExpression>(e => true);
        }

        public static Parser<Expression, Expression> Any()
        {
            return Satisfy(e => true);
        }

        #endregion

        #region Identifier

        public static Parser<Expression, IdentifierExpression> Identifier(Func<String, Boolean> namePredicate)
        {
            return Satisfy<IdentifierExpression>(e => namePredicate(e.Name));
        }

        public static Parser<Expression, IdentifierExpression> Identifier(String name)
        {
            return Identifier(n => n == name);
        }

        public static Parser<Expression, IdentifierExpression> Identifier()
        {
            return Satisfy<IdentifierExpression>();
        }

        #endregion

        #region LambdaList

        public static Parser<Expression, Expression> LambdaList<TExpression>(Parser<Expression, TExpression> selector)
            where TExpression : Expression
        {
            return LambdaList().AndAlso(
                selector.Left(Errors.FollowedBy(Prims.Eof<Expression>())),
                e => ((LambdaListExpression) e).Elements
            );
        }

        public static Parser<Expression, LambdaListExpression> LambdaList(Parser<Expression, IEnumerable<Expression>> elements)
        {
            return Satisfy<LambdaListExpression>(e => elements.Right(Prims.Eof<Expression>())(e.Elements.AsStream()).IsSuccess());
        }

        public static Parser<Expression, LambdaListExpression> LambdaList(params Parser<Expression, Expression>[] elements)
        {
            return LambdaList(Combinator.Sequence(elements));
        }

        public static Parser<Expression, LambdaListExpression> LambdaList()
        {
            return Satisfy<LambdaListExpression>();
        }

        #endregion

        #region List

        public static Parser<Expression, Expression> List<TExpression>(Parser<Expression, TExpression> selector)
            where TExpression : Expression
        {
            return List().AndAlso(
                selector.Left(Errors.FollowedBy(Prims.Eof<Expression>())),
                e => ((ListExpression) e).Elements
            );
        }

        public static Parser<Expression, ListExpression > List(Parser<Expression, IEnumerable<Expression>> elements)
        {
            return Satisfy<ListExpression>(e => elements.Right(Prims.Eof<Expression>())(e.Elements.AsStream()).IsSuccess());
        }

        public static Parser<Expression, ListExpression> List(params Parser<Expression, Expression>[] elements)
        {
            return List(Combinator.Sequence(elements));
        }

        public static Parser<Expression, ListExpression> List()
        {
            return Satisfy<ListExpression>();
        }

        #endregion

        #region Number

        public static Parser<Expression, NumberExpression> Number(Func<Object, Boolean> valuePredicate)
        {
            return Satisfy<NumberExpression>(e => valuePredicate(e.Value));
        }

        public static Parser<Expression, NumberExpression> Number<TValue>(Func<TValue, Boolean> valuePredicate)
            where TValue : struct
        {
            return Number(v => v is TValue && valuePredicate((TValue) v));
        }

        public static Parser<Expression, NumberExpression> Number<TValue>()
            where TValue : struct
        {
            return Number<TValue>(v => true);
        }

        public static Parser<Expression, NumberExpression> Number()
        {
            return Number(e => true);
        }

        #endregion

        #region Text

        public static Parser<Expression, TextExpression> Text(Func<Object, Boolean> valuePredicate)
        {
            return Satisfy<TextExpression>(e => valuePredicate(e.Value));
        }

        public static Parser<Expression, TextExpression> Text<TValue>(Func<TValue, Boolean> valuePredicate)
        {
            return Text(v => v is TValue && valuePredicate((TValue) v));
        }

        public static Parser<Expression, TextExpression> Text<TValue>()
        {
            return Text<TValue>(v => true);
        }

        public static Parser<Expression, TextExpression> Text()
        {
            return Text(v => true);
        }

        #endregion

        #region Vector

        public static Parser<Expression, Expression> Vector<TExpression>(Parser<Expression, TExpression> selector)
            where TExpression : Expression
        {
            return Vector().AndAlso(
                selector.Left(Errors.FollowedBy(Prims.Eof<Expression>())),
                e => ((VectorExpression) e).Elements
            );
        }

        public static Parser<Expression, VectorExpression> Vector(Parser<Expression, IEnumerable<Expression>> elements)
        {
            return Satisfy<VectorExpression>(e => elements.Right(Prims.Eof<Expression>())(e.Elements.AsStream()).IsSuccess());
        }

        public static Parser<Expression, TExpression> Vector<TExpression>(
            Parser<Expression, IEnumerable<Expression>> elements,
            Func<IEnumerable<Expression>, TExpression> selector
        )
            where TExpression : Expression
        {
            return Vector(elements).Select(e => selector(e.Elements));
        }

        public static Parser<Expression, VectorExpression> Vector(params Parser<Expression, Expression>[] elements)
        {
            return Vector(Combinator.Sequence(elements));
        }

        public static Parser<Expression, VectorExpression> Vector()
        {
            return Satisfy<VectorExpression>();
        }

        #endregion

        #region Utilities

        public static YacqReducingCombinator Reduce(this Parser<Expression, Expression> parser, SymbolTable symbols = null, Type expectedType = null)
        {
            return new YacqReducingCombinator(parser, symbols, expectedType);
        }

        public static YacqReducingCombinator Reduce(SymbolTable symbols = null, Type expectedType = null)
        {
            return Reduce(null, symbols, expectedType);
        }

        public static Parser<Expression, TResult> As<TResult>(
            this Parser<Expression, TResult> parser,
            SymbolTable symbols,
            String id,
            Func<TResult, Expression> selector
        )
        {
            return parser.Select(e => e.Apply(_ => symbols.Add(id, selector(_))));
        }

        public static Parser<Expression, TExpression> As<TExpression>(
            this Parser<Expression, TExpression> parser,
            SymbolTable symbols,
            String id
        )
            where TExpression : Expression
        {
            return As(parser, symbols, id, e => e);
        }

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
