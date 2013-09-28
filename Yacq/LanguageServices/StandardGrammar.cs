// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2012 linerlock <x.linerlock@gmail.com>
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
using Parseq.Combinators;
using XSpect.Yacq.Expressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Provides the default grammar. This grammar cannot modify.
    /// </summary>
    public sealed class StandardGrammar
        : Grammar
    {
        private readonly Boolean _isReadOnly;

        /// <summary>
        /// Gets a value indicating whether this grammar is read-only.
        /// </summary>
        /// <value><c>true</c> if this rule is read-only; otherwise, <c>false</c>.</value>
        public override Boolean IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
        }

        internal StandardGrammar()
        {
            this._isReadOnly = false;

            Parser<Char, YacqExpression> expressionRef = null;
            var expression = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => expressionRef(stream)
            );
            this.Add("root", "expression", g => expression.Value);

            #region Trivials

            var newline = Combinator.Choice(
                Chars.Sequence("\r\n"),
                Chars.OneOf('\r', '\n', '\x85', '\u2028', '\u2029')
                    .Select(c => Arrays.From(c))
            ).Select(_ => Environment.NewLine);

            var punctuation = Chars.OneOf('"', '#', '\'', '(', ')', ',', '.', ':', ';', '[', ']', '`', '{', '}');

            #endregion

            #region Comments
            {
                this.Add("comment", "eol", g => Prims.Pipe(
                    ';'.Satisfy(),
                    newline.Not().Right(Chars.Any()).Many(),
                    newline.Ignore().Or(Chars.Eof()),
                    (p, r, s) => YacqExpression.Ignore()
                ));

                Parser<Char, YacqExpression> blockCommentRef = null;
                Parser<Char, YacqExpression> blockCommentRestRef = null;
                var blockComment = new Lazy<Parser<Char, YacqExpression>>(
                    () => stream => blockCommentRef(stream)
                );
                var blockCommentRest = new Lazy<Parser<Char, YacqExpression>>(
                    () => stream => blockCommentRestRef(stream)
                );
                var blockCommentPrefix = Chars.Sequence("#|");
                var blockCommentSuffix = Chars.Sequence("|#");
                blockCommentRef = blockCommentPrefix
                    .Right(blockCommentRest.Value.Many())
                    .Left(blockCommentSuffix)
                    .Select(_ => YacqExpression.Ignore());
                blockCommentRestRef = blockCommentPrefix.Not()
                    .Right(blockCommentSuffix.Not())
                    .Right(Chars.Any())
                    .Select(_ => YacqExpression.Ignore())
                    .Or(blockComment.Value);
                this.Add("comment", "block", g => blockComment.Value);

                this.Add("comment", "expression", g => Prims.Pipe(
                    Chars.Sequence("#;"),
                    g["root", "expression"],
                    (p, r) => YacqExpression.Ignore()
                ));

                this.Add("root", "comment", g => g["comment"].Choice());
            }
            #endregion

            #region Ignore

            this.Add("root", "ignore", g => Combinator.Choice(
                this.Get["root", "comment"].Ignore(),
                Chars.Space().Ignore(),
                ','.Satisfy().Ignore(),
                newline.Ignore()
            ).Many().Select(_ => (YacqExpression) YacqExpression.Ignore()));

            #endregion

            #region Terms

            // Texts
            this.Add("term", "text", g => Chars.OneOf('\'', '\"')
                .EnterContext("text")
                .SelectMany(q => q.Satisfy()
                    .Not()
                    .Right('\\'.Satisfy()
                        .Right(q.Satisfy())
                        .Or(Chars.Any())
                    )
                    .Many()
                    .Left(q.Satisfy())
                    .Select(cs => YacqExpression.Text(q, cs.Stringify()))
                )
                .LeaveContext("text")
            );

            // Numbers
            {
                var numberPrefix = Chars.OneOf('+', '-');
                var numberSuffix = Combinator.Choice(
                    Chars.Sequence("ul"),
                    Chars.Sequence("UL"),
                    Chars.OneOf('D', 'F', 'L', 'M', 'U', 'd', 'f', 'l', 'm', 'u')
                        .Select(c => Arrays.From(c))
                );
                var digit = '_'.Satisfy().Many().Right(Chars.Digit());
                var hexPrefix = Chars.Sequence("0x");
                var hex = '_'.Satisfy().Many().Right(Chars.Hex());
                var octPrefix = Chars.Sequence("0o");
                var oct = '_'.Satisfy().Many().Right(Chars.Oct());
                var binPrefix = Chars.Sequence("0b");
                var bin = '_'.Satisfy().Many().Right(Chars.OneOf('0', '1'));
                var fraction = Prims.Pipe(
                    '.'.Satisfy(),
                    digit.Many(1),
                    (d, ds) => ds.StartWith(d)
                );
                var exponent = Prims.Pipe(
                    Chars.OneOf('E', 'e'),
                    Chars.OneOf('+', '-').Maybe(),
                    digit.Many(1),
                    (e, s, ds) => ds
                        .If(_ => s.Exists(), _ => _.StartWith(s.Value))
                        .StartWith(e)
                );

                this.Add("term", "number", g => Combinator.Choice(
                    #region Binary
                    Prims.Pipe(
                        binPrefix,
                        bin.Many(1),
                        numberSuffix.Maybe(),
                        (p, n, s) => YacqExpression.Number(
                            p.Concat(n).If(
                                _ => s.Exists(),
                                cs => cs.Concat(s.Value)
                            ).Stringify()
                        )
                    ),
                    #endregion
                    #region Octal
                    Prims.Pipe(
                        octPrefix,
                        oct.Many(1),
                        numberSuffix.Maybe(),
                        (p, n, s) => YacqExpression.Number(
                            p.Concat(n).If(
                                _ => s.Exists(),
                                cs => cs.Concat(s.Value)
                            ).Stringify()
                        )
                    ),
                    #endregion
                    #region Hexadecimal
                    Prims.Pipe(
                        hexPrefix,
                        hex.Many(1),
                        numberSuffix.Maybe(),
                        (p, n, s) => YacqExpression.Number(
                            p.Concat(n).If(
                                _ => s.Exists(),
                                cs => cs.Concat(s.Value)
                            ).Stringify()
                        )
                    ),
                    #endregion
                    #region Others
                    numberPrefix.Maybe().SelectMany(p =>
                        digit.Many(1).SelectMany(i =>
                            fraction.Maybe().SelectMany(f =>
                                exponent.Maybe().SelectMany(e =>
                                    numberSuffix.Maybe().Select(s =>
                                        YacqExpression.Number(Arrays.From(
                                            i.If(_ => p.Exists(), _ => _.StartWith(p.Value)),
                                            f.Otherwise(Enumerable.Empty<Char>),
                                            e.Otherwise(Enumerable.Empty<Char>),
                                            s.Otherwise(Enumerable.Empty<Char>)
                                        ).Concat().Stringify())
                                    )
                                )
                            )
                        )
                    )
                    #endregion
                ));
            }

            // Lists
            this.Add("term", "list", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between('('.Satisfy(), ')'.Satisfy(), "list")
                .Select(YacqExpression.List)
            );

            // Vectors
            this.Add("term", "vector", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between('['.Satisfy(), ']'.Satisfy(), "vector")
                .Select(YacqExpression.Vector)
            );

            // Lambda Lists
            this.Add("term", "lambdaList", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between('{'.Satisfy(), '}'.Satisfy(), "lambdaList")
                .Select(YacqExpression.LambdaList)
            );

            // Identifiers
            this.Add("term", "identifier", g => Combinator.Choice(
                Combinator.Choice(
                    '.'.Satisfy().Many(1),
                    ':'.Satisfy().Many(1),
                    Chars.Digit()
                        .Not()
                        .Right(Chars.Space()
                            .Or(punctuation)
                            .Not()
                            .Right(Chars.Any())
                            .Many(1)
                        )
                ).Select(cs => YacqExpression.Identifier(default(Char), cs.Stringify())),
                '`'.Satisfy()
                    .EnterContext("identifier")
                    .Let(q => q.Right(q
                        .Not()
                        .Right('\\'.Satisfy()
                            .Right('`'.Satisfy())
                            .Or(Chars.Any())
                        )
                        .Many()
                        .Left(q)
                    ))
                    .LeaveContext("identifier")
                    .Select(cs => YacqExpression.Identifier('`', cs.Stringify()))
            ));

            // Extended Terms
            this.Add("term", "ext", g => '#'.Satisfy()
                .Right(g["term.ext"].Choice())
            );

            this.Add("root", "term", g => g["term"]
                .Choice()
                .SetPosition()
                .Between(g["root", "ignore"], g["root", "ignore"])
            );

            #endregion

            #region Extended Terms (Reader Macros, #-prefixed)

            // Quotes
            this.Add("term.ext", "quote", g => '\''.Satisfy()
                .EnterContext("quote")
                .Right(g["root", "expression"])
                .LeaveContext("quote")
                .Select(e => YacqExpression.List(YacqExpression.Identifier("quote"), e))
            );

            // Quasiquotes
            this.Add("term.ext", "quasiquote", g => '`'.Satisfy()
                .EnterContext("quasiquote")
                .Right(g["root", "expression"])
                .LeaveContext("quasiquote")
                .Select(e => YacqExpression.List(YacqExpression.Identifier("quasiquote"), e))
            );

            // Unquote-Splicings
            this.Add("term.ext", "unquoteSplicing", g => Chars.Sequence(",@")
                .EnterContext("unquoteSplicing")
                .Right(g["root", "expression"])
                .LeaveContext("unquoteSplicing")
                .Select(e => YacqExpression.List(YacqExpression.Identifier("unquote-splicing"), e))
            );

            // Unquotes
            this.Add("term.ext", "unquote", g => ','.Satisfy()
                .EnterContext("unquote")
                .Right(g["root", "expression"])
                .LeaveContext("unquote")
                .Select(e => YacqExpression.List(YacqExpression.Identifier("unquote"), e))
            );

            // Transiting Expressions (Alternative Grammer)
            this.Add("term.ext", "altExpression", g => Alternative.Get.Default
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Between('('.Satisfy(), ')'.Satisfy(), "altExpression")
            );

            // Transiting Expressions (Pattern Grammer)
            this.Add("term.ext", "patternExpression", g => Pattern.Get.Default
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Between(Chars.Sequence("%("), ')'.Satisfy(), "patternExpression")
            );

            // Tuples
            this.Add("term.ext", "tuple", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between('['.Satisfy(), ']'.Satisfy(), "tuple")
                .Select(es => YacqExpression.List(es.StartWith(YacqExpression.Identifier("tuple"))))
            );

            #endregion

            #region Infixes

            // Dots
            this.Add("infix", "dot", g => Prims.Pipe(
                g["root", "term"],
                '.'.Satisfy()
                    .Select(_ => YacqExpression.Identifier("."))
                    .SetPosition()
                    .Both(g["root", "term"])
                    .Many(),
                (h, t) => t.Aggregate(h, (l, r) =>
                    YacqExpression.List(r.Item1, l, r.Item2)
                )
            ));

            // Colons
            this.Add("infix", "colon", g => Prims.Pipe(
                g["infix", "dot"],
                ':'.Satisfy()
                    .Select(_ => YacqExpression.Identifier(":"))
                    .SetPosition()
                    .Both(g["infix", "dot"])
                    .Many(),
                (h, t) => t.Aggregate(h, (l, r) =>
                    YacqExpression.List(r.Item1, l, r.Item2)
                )
            ));

            #endregion

            expressionRef = this.Get["infix"]
                .Last()
                .Do(e => Reader.State.Current.Null(s => s.SetLastExpression(e)));

            this.Set.Default = g => g["root", "expression"];

            this._isReadOnly = true;
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
