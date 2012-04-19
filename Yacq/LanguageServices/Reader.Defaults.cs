// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;
using XSpect.Yacq.Expressions;
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.LanguageServices
{
    partial class Reader
    {
        public class Defaults
        {
            public static Parser<Char, Unit> Comment
            {
                get;
                private set;
            }

            public static Parser<Char, YacqExpression> Identifier
            {
                get;
                private set;
            }

            public static Parser<Char, YacqExpression> Number
            {
                get;
                private set;
            }

            public static Parser<Char, YacqExpression> Text
            {
                get;
                private set;
            }

            public static Parser<Char, YacqExpression> List
            {
                get;
                private set;
            }

            public static Parser<Char, YacqExpression> Vector
            {
                get;
                private set;
            }

            public static Parser<Char, YacqExpression> Lambda
            {
                get;
                private set;
            }

            public static Parser<Char, IEnumerable<YacqExpression>> Yacq
            {
                get;
                private set;
            }

            static Defaults()
            {
                Parser<Char, YacqExpression> expressionRef = null;

                var expression = new Lazy<Parser<Char, YacqExpression>>(
                    () => stream => expressionRef(stream)
                );

                var newline = Combinator.Choice(
                    '\r'.Satisfy().Pipe('\n'.Satisfy(), (a, b) => '\n'),
                    '\r'.Satisfy().Select(_ => '\n'),
                    '\n'.Satisfy()
                );

                var space = Chars.Space();

                var tab = Chars.OneOf('\t', '\v');

                var eof = Chars.Eof();

                var eolComment = ';'.Satisfy().Pipe(newline.Not().Right(Chars.Any()).Many(), newline.Ignore().Or(eof),
                    (a, b, c) => Unit.Instance
                );

                Parser<Char, Unit> blockCommentRef = null;
                Parser<Char, Unit> blockCommentRestRef = null;

                var blockComment = new Lazy<Parser<Char, Unit>>(
                    () => stream => blockCommentRef(stream)
                );

                var blockCommentRest = new Lazy<Parser<Char, Unit>>(
                    () => stream => blockCommentRestRef(stream)
                );

                var blockCommentPrefix = '#'.Satisfy().Pipe('|'.Satisfy(), (a, b) => "#|");
                var blockCommentSuffix = '|'.Satisfy().Pipe('#'.Satisfy(), (a, b) => "|#");

                blockCommentRestRef = blockCommentPrefix.Not().Right(blockCommentSuffix.Not()).Right(Chars.Any())
                    .Select(_ => Unit.Instance)
                    .Or(blockComment.Value);

                blockCommentRef = blockCommentPrefix.Right(blockCommentRest.Value.Many()).Left(blockCommentSuffix)
                    .Select(_ => Unit.Instance);

                var expressionComment = '#'.Satisfy().Pipe(';'.Satisfy(), expression.Value,
                    (a, b, c) => Unit.Instance);

                var comment = eolComment.Or(blockComment.Value).Or(expressionComment);

                var ignore = comment.Or(space.Ignore()).Or(tab.Ignore()).Or(newline.Ignore()).Many();

                var numberPrefix = Chars.OneOf('+', '-');

                var numberSuffix = Combinator.Choice(
                    Prims.Pipe('u'.Satisfy(), 'l'.Satisfy(), (x, y) => "ul"),
                    Prims.Pipe('U'.Satisfy(), 'L'.Satisfy(), (x, y) => "UL"),
                    Chars.OneOf('f', 'F', 'd', 'D', 'm', 'M', 'u', 'U', 'l', 'L').Select(_ => _.ToString())
                );

                var punctuation = Chars.OneOf(';', '\'', '\"', '`', '(', ')', '[', ']', '{', '}', ',', '.', ':');

                var digit =
                    '_'.Satisfy().Many().Right(Chars.Digit());

                var hexPrefix = Prims.Pipe('0'.Satisfy(), 'x'.Satisfy(), (x, y) => "0x");

                var hex =
                    '_'.Satisfy().Many().Right(Chars.Hex());

                var octPrefix = Prims.Pipe('0'.Satisfy(), 'o'.Satisfy(), (x, y) => "0o");

                var oct =
                    '_'.Satisfy().Many().Right(Chars.Oct());

                var binPrefix = Prims.Pipe('0'.Satisfy(), 'b'.Satisfy(), (x, y) => "0b");

                var bin =
                    '_'.Satisfy().Many().Right(Chars.OneOf('0', '1'));

                var fraction = Prims.Pipe('.'.Satisfy(), digit.Many(1),
                    (x, y) => String.Concat(x, new String(y.ToArray())));

                var exponent = Prims.Pipe(Chars.OneOf('e', 'E'), Chars.OneOf('+', '-').Maybe(), digit.Many(1),
                    (x, y, z) => y.Select(t => String.Concat(x, t, new String(z.ToArray())))
                        .Otherwise(() => String.Concat(x, new String(z.ToArray())))
                );


                var identifier = Combinator.Choice(
                    Span('.'.Satisfy().Many(1)
                        .Select(x => (YacqExpression) YacqExpression.Identifier(new String(x.ToArray()))),
                        (start, end, t) => t.SetPosition(start, end)
                    ),
                    Span(':'.Satisfy().Many(1)
                        .Select(x => (YacqExpression) YacqExpression.Identifier(new String(x.ToArray()))),
                        (start, end, t) => t.SetPosition(start, end)
                    ),
                    Span(Chars.Digit().Not().Right(space.Or(punctuation).Not().Right(Chars.Any()).Many(1))
                        .Select(x => (YacqExpression) YacqExpression.Identifier(new String(x.ToArray()))),
                        (start, end, t) => t.SetPosition(start, end)
                    )
                );

                var number = Combinator.Choice(
                    Span(Prims.Pipe(binPrefix, bin.Many(1), numberSuffix.Maybe(),
                        (x, y, z) => (YacqExpression) YacqExpression.Number(String.Concat(x, new String(y.ToArray()), z.Otherwise(()=> "")))),
                            (start, end, t) => t.SetPosition(start, end)
                    ),
                    Span(Prims.Pipe(octPrefix, oct.Many(1), numberSuffix.Maybe(),
                        (x, y, z) => (YacqExpression) YacqExpression.Number(String.Concat(x, new String(y.ToArray()), z.Otherwise(() => "")))),
                            (start, end, t) => t.SetPosition(start, end)
                    ),
                    Span(Prims.Pipe(hexPrefix, hex.Many(1), numberSuffix.Maybe(),
                        (x, y, z) => (YacqExpression) YacqExpression.Number(String.Concat(x, new String(y.ToArray()), z.Otherwise(() => "")))),
                            (start, end, t) => t.SetPosition(start, end)
                    ),
                    Span(
                        from u in numberPrefix.Maybe()
                        from w in digit.Many(1).Select(_ => new String(_.ToArray()))
                        from x in fraction.Maybe().Select(_ => _.Otherwise(() => ""))
                        from y in exponent.Maybe().Select(_ => _.Otherwise(() => ""))
                        from z in numberSuffix.Maybe().Select(_ => _.Otherwise(() => ""))
                        select u.Select(t => (YacqExpression) YacqExpression.Number(String.Concat(t, w, x, y, z)))
                            .Otherwise(() => (YacqExpression) YacqExpression.Number(String.Concat(w, x, y, z))),
                                (start, end, t) => t.SetPosition(start, end)
                    )
                );

                var text =
                    Span(
                        from x in Chars.OneOf('\'', '\"', '`')
                        let quoteMark = x.Satisfy()
                        from y in quoteMark.Not().Right(Chars.Any()).Many().Select(_ => new String(_.ToArray()))
                        from z in quoteMark
                        select (YacqExpression) YacqExpression.Text(String.Concat(x, y, z)),
                            (start, end, t) => t.SetPosition(start, end)
                    );

                var list =
                    Span(
                        expression.Value.Between(ignore, ignore).Many().Between('('.Satisfy(), ')'.Satisfy())
                            .Select(x => (YacqExpression) YacqExpression.List(x.ToArray())),
                                (start, end, t) => t.SetPosition(start, end)
                    );

                var vector =
                    Span(
                        expression.Value.Between(ignore, ignore).Many().Between('['.Satisfy(), ']'.Satisfy())
                            .Select(x => (YacqExpression) YacqExpression.Vector(x.ToArray())),
                                (start, end, t) => t.SetPosition(start, end)
                    );

                var lambda =
                    Span(
                        expression.Value.Between(ignore, ignore).Many().Between('{'.Satisfy(), '}'.Satisfy())
                            .Select(x => (YacqExpression) YacqExpression.LambdaList(x.ToArray())),
                                (start, end, t) => t.SetPosition(start, end)
                    );

                var quote = '#'.Satisfy().Pipe('\''.Satisfy(), expression.Value,
                    (x, y, t) => (YacqExpression) YacqExpression.List(YacqExpression.Identifier("quote"), t));

                var quasiquote = '#'.Satisfy().Pipe('`'.Satisfy(), expression.Value,
                    (x, y, t) => (YacqExpression) YacqExpression.List(YacqExpression.Identifier("quasiquote"), t));

                var unquote = '#'.Satisfy().Pipe(','.Satisfy(), expression.Value,
                    (x, y, t) => (YacqExpression) YacqExpression.List(YacqExpression.Identifier("unquote"), t));

                var unquoteSplicing = Chars.Sequence("#,@").Pipe(expression.Value,
                    (x, t) => (YacqExpression) YacqExpression.List(YacqExpression.Identifier("unquote-splicing"), t));

                var term = Combinator.Choice(text, number, list, vector, lambda, quote, quasiquote, unquoteSplicing, unquote, identifier)
                   .Between(ignore, ignore);

                var factor =
                    term.Pipe('.'.Satisfy().Right(term).Many(),
                        (x, y) => Enumerable.Aggregate(y, x,
                            (a, b) => (YacqExpression) YacqExpression.List(YacqExpression.Identifier("."), a, b)))
                        .Or(term)
                        .Between(ignore, ignore);

                expressionRef =
                    factor.Pipe(':'.Satisfy().Right(factor).Many(),
                        (x, y) => Enumerable.Aggregate(y, x,
                            (a, b) => (YacqExpression) YacqExpression.List(YacqExpression.Identifier(":"), a, b)))
                        .Or(factor)
                        .Between(ignore, ignore);

                Comment = comment;
                Identifier = identifier;
                Number = number;
                Text = text;
                List = list;
                Vector = vector;
                Lambda = lambda;
                Yacq = expression.Value.Many()
                    .Between(ignore, ignore)
                    .Left(Errors.FollowedBy(Chars.Eof()));
            }

            private static Parser<Char, T> Span<T>(Parser<Char, T> parser,
                Action<Position, Position, T> action)
            {
                if (parser == null)
                {
                    throw new ArgumentNullException("parser");
                }
                if (action == null)
                {
                    throw new ArgumentNullException("action");
                }

                Parser<Char, Position> pos = stream => Reply.Success(stream, stream.Position);
                return from x in pos
                       from y in parser
                       from z in pos
                       select y.Apply(_ => action(x, z, _));
            }
        }
    }
}

#pragma warning restore 1591