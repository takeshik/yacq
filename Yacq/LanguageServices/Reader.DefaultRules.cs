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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using XSpect.Yacq.Expressions;

using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.LanguageServices
{
    partial class Reader {
        private void InitializeRules(){

            var newline =
                Combinator.Choice(
                    '\r'.Satisfy().Pipe('\n'.Satisfy(), (x, y) => "\r\n"),
                    '\r'.Satisfy().Select(_ => "\r"),
                    '\n'.Satisfy().Select(_ => "\n")
                    );

            var eolComment =
                ';'.Satisfy().Pipe(newline.Not().Right(Chars.Any()), newline,
                    (x, y, z) => Unit.Instance);
                            

            var space = Chars.Space();
            var spaces = space.Many();

            var numberPrefix =
                Chars.OneOf('+', '-').Maybe()
                    .Select(x => x.Select(y => y.ToString()).Otherwise(() => String.Empty));

            var numberSuffix =
                Combinator.Choice(
                    Prims.Pipe('u'.Satisfy(), 'l'.Satisfy(), (x, y) => "ul"),
                    Prims.Pipe('U'.Satisfy(), 'L'.Satisfy(), (x, y) => "UL"),
                    Chars.OneOf('f', 'F', 'd', 'D', 'm', 'M', 'u', 'U', 'l', 'L')
                        .Select(_ => _.ToString())
                    ).Maybe()
                        .Select(_ => _.Otherwise(() => String.Empty));

            var punctuation =
                Chars.OneOf(';', '\'', '\"', '`', '(', ')', '[', ']', '{', '}', ',', '.', ':');

            var digit =
                '_'.Satisfy().Many().Right(Chars.Digit());

            var hex =
                '_'.Satisfy().Many().Right(Chars.Hex());

            var oct =
                '_'.Satisfy().Many().Right(Chars.Oct());

            var bin =
                '_'.Satisfy().Many().Right(Chars.OneOf('0', '1'));


            var identifier =
                Combinator.Choice(
                    Span('.'.Satisfy().Many(1)
                        .Select(x => (YacqExpression)YacqExpression.Identifier(new String(x.ToArray()))),
                        (start, end, t) => t.SetPosition(start, end)),
                    Span(':'.Satisfy().Many(1)
                        .Select(x => (YacqExpression)YacqExpression.Identifier(new String(x.ToArray()))),
                        (start, end, t) => t.SetPosition(start, end)),
                    Span(Chars.Digit().Not().Right(space.Or(punctuation).Not().Right(Chars.Any()).Many(1))
                        .Select(x => (YacqExpression)YacqExpression.Identifier(new String(x.ToArray()))),
                        (start, end, t) => t.SetPosition(start, end))
                );

            var number =
                Combinator.Choice(
                    Span('0'.Satisfy().Pipe('b'.Satisfy(), (x, y) => "0b").Pipe(bin.Many(1), numberSuffix,
                        (x, y, z) => (YacqExpression)YacqExpression.Number(String.Concat(x, new String(y.ToArray()), z))),
                            (start, end, t) => t.SetPosition(start, end)),
                    Span('0'.Satisfy().Pipe('o'.Satisfy(), (x, y) => "0o").Pipe(oct.Many(1), numberSuffix,
                        (x, y, z) => (YacqExpression)YacqExpression.Number(String.Concat(x, new String(y.ToArray()), z))),
                            (start, end, t) => t.SetPosition(start, end)),
                    Span('0'.Satisfy().Pipe('x'.Satisfy(), (x, y) => "0x").Pipe(hex.Many(1), numberSuffix,
                        (x, y, z) => (YacqExpression)YacqExpression.Number(String.Concat(x, new String(y.ToArray()), z))),
                            (start, end, t) => t.SetPosition(start, end)),
                    Span(
                        from u in numberPrefix
                        from w in digit.Many(1)
                        from x in Combinator.Maybe('.'.Satisfy().Pipe(digit.Many(1),
                            (a, b) => String.Concat(a, new String(b.ToArray()))))
                                .Select(_ => _.Otherwise(() => String.Empty))
                        from y in Combinator.Maybe(Chars.OneOf('e', 'E').Pipe(numberPrefix, digit.Many(1),
                            (a, b, c) => String.Concat(a, b, new String(c.ToArray()))))
                                .Select(_ => _.Otherwise(() => String.Empty))
                        from z in numberSuffix
                        select (YacqExpression)YacqExpression.Number(String.Concat(u, new String(w.ToArray()), x, y, z)),
                            (start, end, t) => t.SetPosition(start, end))
                );

            var text =
                Span(
                    from x in Chars.OneOf('\'', '\"', '`')
                    let quote = x.Satisfy()
                    from y in quote.Not().Right(Chars.Any()).Many()
                    from z in quote
                    select (YacqExpression)YacqExpression.Text(String.Concat(x, new String(y.ToArray()), z)),
                        (start, end, t) => t.SetPosition(start, end)
                );

            Parser<Char, YacqExpression> expressionRef = null;

            var expression = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => expressionRef(stream));

            var list =
                Span(
                    expression.Value.Between(spaces, spaces).Many().Between('('.Satisfy(), ')'.Satisfy())
                        .Select(x => (YacqExpression)YacqExpression.List(x.ToArray())),
                            (start, end, t) => t.SetPosition(start, end)
                );

            var vector =
                Span(
                    expression.Value.Between(spaces, spaces).Many().Between('['.Satisfy(), ']'.Satisfy())
                        .Select(x => (YacqExpression)YacqExpression.Vector(x.ToArray())),
                            (start, end, t) => t.SetPosition(start, end)
                );

            var lambda =
                Span(
                    expression.Value.Between(spaces, spaces).Many().Between('{'.Satisfy(), '}'.Satisfy())
                        .Select(x => (YacqExpression)YacqExpression.LambdaList(x.ToArray())),
                            (start, end, t) => t.SetPosition(start, end)
                );

            var term = Combinator.Choice(text, number, list, vector, lambda, identifier)
               .Between(spaces, spaces);

            var factor =
                term.Pipe('.'.Satisfy(), (term).Many(), (x, y, z) => Enumerable.Aggregate(z, x,
                    (a, b) => (YacqExpression)YacqExpression.List(YacqExpression.Identifier("."), a, b)))
                    .Or(term);

            expressionRef =
                factor.Pipe(':'.Satisfy(), (factor).Many(), (x, y, z) => Enumerable.Aggregate(z, x,
                    (a, b) => (YacqExpression)YacqExpression.List(YacqExpression.Identifier(":"), a, b)))
                    .Or(factor);


            this.Parser = eolComment.Maybe().Right(expression.Value.Between(spaces, spaces)).Many();
            //(let [x:Func.[Int32 Int32]] x) 
        }

        private static Parser<Char, T> Span<T>(Parser<Char, T> parser,
            Action<Position,Position,T> action)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            if (action == null)
                throw new ArgumentNullException("action");

            var pos = (Parser<Char, Position>)(stream => Reply.Success(stream, stream.Position));
            return from x in pos
                   from y in parser
                   from z in pos
                   select y.Apply(_ => action(x, z, _));
        }
    }
}