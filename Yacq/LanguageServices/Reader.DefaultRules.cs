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
    partial class Reader
    {
        private void InitializeRules(){
            var position = (Parser<Char, Position>)(stream => Reply.Success(stream, stream.Position));

            var space = Chars.Space();
            var spaces = Chars.Space().Many();

            var blockCommentPrefix = '#'.Satisfy().Right('|'.Satisfy()).Select(_ => "#|");
            var blockCommentSuffix = '|'.Satisfy().Right('#'.Satisfy()).Select(_ => "|#");

            var numberPrefix = Chars.OneOf('+', '-').Maybe().Select(
                x => x.Select(y => y.ToString()).Otherwise(() => String.Empty));

            var numberSuffix = Combinator.Choice(
                'u'.Satisfy().Right('l'.Satisfy()).Select(_ => "ul"),
                'U'.Satisfy().Right('L'.Satisfy()).Select(_ => "UL"),
                Chars.OneOf('f', 'F', 'd', 'D', 'm', 'M', 'u', 'U', 'l', 'L').Select(_ => _.ToString()))
                .Maybe().Select(_ => _.Otherwise(() => String.Empty));

            var punctuation = Chars.OneOf(';', '\'', '\"', '`', '(', ')', '[', ']', '{', '}', ',', '.', ':');

            var digit = '_'.Satisfy().Many().Right(Chars.Digit());

            var hex = '_'.Satisfy().Many().Right(Chars.Hex());

            var oct = '_'.Satisfy().Many().Right(Chars.Oct());

            var bin = '_'.Satisfy().Many().Right(Chars.OneOf('0', '1'));

            var identifier = Combinator.Choice(
                from s in position
                from x in '.'.Satisfy().Many(1)
                from e in position
                select (YacqExpression)YacqExpression.Identifier(new String(x.ToArray()))
                    .Apply(node => node.SetPosition(s, e)),

                from s in position
                from x in ':'.Satisfy().Many(1)
                from e in position
                select (YacqExpression)YacqExpression.Identifier(new String(x.ToArray()))
                    .Apply(node => node.SetPosition(s, e)),

                from s in position
                from y in Chars.Digit().Not()
                from z in space.Or(punctuation).Not().Right(Chars.Any()).Many(1)
                from e in position
                select (YacqExpression)YacqExpression.Identifier(new StringBuilder().Append(z.ToArray()).ToString())
                    .Apply(node => node.SetPosition(s, e)));

            var number = Combinator.Choice(
                 from s in position
                 from x in Combinator.Sequence('0'.Satisfy(), 'b'.Satisfy())
                 from y in bin.Many(1)
                 from z in numberPrefix
                 from e in position
                 select (YacqExpression)YacqExpression.Number(
                     new StringBuilder().Append(x.ToArray()).Append(y.ToArray()).Append(z).ToString())
                        .Apply(node => node.SetPosition(s, e)),

                 from s in position
                 from x in Combinator.Sequence('0'.Satisfy(), 'o'.Satisfy())
                 from y in oct.Many(1)
                 from z in numberSuffix
                 from e in position
                 select (YacqExpression)YacqExpression.Number(
                     new StringBuilder().Append(x.ToArray()).Append(y.ToArray()).Append(z).ToString())
                        .Apply(node => node.SetPosition(s, e)),

                 from s in position
                 from x in Combinator.Sequence('0'.Satisfy(), 'x'.Satisfy())
                 from y in hex.Many(1)
                 from z in numberSuffix
                 from e in position
                 select (YacqExpression)YacqExpression.Number(
                     new StringBuilder().Append(x.ToArray()).Append(y.ToArray()).Append(z).ToString())
                        .Apply(node => node.SetPosition(s, e)),

                 from s in position
                 from u in numberPrefix
                 from w in digit.Many(1)
                 from x in
                     Combinator.Maybe(from a in '.'.Satisfy()
                                      from b in digit.Many(1)
                                      select new StringBuilder().Append(a).Append(b.ToArray()).ToString())
                               .Select(_ => _.Otherwise(() => String.Empty))

                 from y in
                     Combinator.Maybe(from a in Chars.OneOf('e', 'E')
                                      from b in numberPrefix
                                      from c in digit.Many(1)
                                      select new StringBuilder().Append(a).Append(b).Append(c.ToArray()).ToString())
                               .Select(_ => _.Otherwise(() => String.Empty))

                 from z in numberSuffix
                 from e in position
                 select (YacqExpression)YacqExpression.Number(
                     new StringBuilder().Append(u).Append(w.ToArray()).Append(x).Append(y).Append(z).ToString())
                        .Apply(node => node.SetPosition(s, e)));

            var text = from x in Chars.OneOf('\'', '\"', '`')
                       from y in (x.Satisfy().Not().Right(Chars.Any())).Many()
                       from z in x.Satisfy()
                       select (YacqExpression)YacqExpression.Text(
                           new StringBuilder().Append(x).Append(y.ToArray()).Append(x).ToString());

            var eolComment = from x in ';'.Satisfy()
                             from y in '\n'.Satisfy().Not().Right(Chars.Any()).Many()
                             from z in '\n'.Satisfy()
                             select Unit.Instance;


            Parser<Char, YacqExpression> expressionRef = null;
            Parser<Char, YacqExpression> expressionRestRef = null;

            var expression = new Lazy<Parser<Char, YacqExpression>>(
                () => expressionRef);

            var expressionRest = new Lazy<Parser<Char, YacqExpression>>(
                () => expressionRestRef);

            var list = from start in position
                       from x in '('.Satisfy().Left(spaces)
                       from y in expression.Value.Between(spaces, spaces).Many()
                       from z in ')'.Satisfy().Left(spaces)
                       from end in position
                       select (YacqExpression)YacqExpression.List(y.ToArray())
                            .Apply(node => node.SetPosition(start, end));

            var vector = from start in position
                         from x in '['.Satisfy()
                         from y in expression.Value.Between(spaces, spaces).Many()
                         from z in ']'.Satisfy()
                         from end in position
                         select (YacqExpression)YacqExpression.Vector(y.ToArray())
                            .Apply(node => node.SetPosition(start, end));

            var lambda = from start in position
                         from x in '{'.Satisfy().Left(spaces)
                         from y in expression.Value.Between(spaces, spaces).Many()
                         from z in '}'.Satisfy().Left(spaces)
                         from end in position
                         select (YacqExpression)YacqExpression.LambdaList(y.ToArray())
                            .Apply(node => node.SetPosition(start,end));

            var term = Combinator.Choice(text, number, list, vector, lambda, identifier)
                .Between(spaces, spaces);

            expressionRef =
                term.Pipe('.'.Satisfy().Right(term).Many(), (x, y) => Enumerable.Aggregate(y, x,
                    (a, b) => (YacqExpression)YacqExpression.List(YacqExpression.Identifier("."), a, b)))
                    .Or(term);


            this.Parser = expression.Value.Between(spaces, spaces).Many(1);
        }
    }
}