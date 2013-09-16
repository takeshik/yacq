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
using Parseq.Combinators;
using XSpect.Yacq.Expressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Provides the pattern grammar. This grammar cannot modify.
    /// </summary>
    public sealed class PatternGrammar
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

        internal PatternGrammar()
        {
            this._isReadOnly = false;

            Parser<Char, YacqExpression> expressionRef = null;
            var expression = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => expressionRef(stream)
            );
            this.Add("root", "expression", g => expression.Value);

            #region Importing Rules

            this.Add("root", "comment", g => Standard.Get["root", "comment"]);
            this.Add("root", "ignore", g => Standard.Get["root", "ignore"]);

            #endregion

            #region Trivials

            var punctuation = Chars.OneOf(
                // From the Standard Grammar:
                '"', '#', '\'', '(', ')', ',', '.', ':', ';', '[', ']', '`', '{', '}',
                // Additional punctuation characters:
                '%', '*', '+', '<', '>', '?', '@', '|'
            );

            #endregion

            #region Terms

            this.Add("root", "term", g =>
                Combinator.Choice(
                    Standard.Get["term", "text"],
                    Standard.Get["term", "number"],
                    '#'.Satisfy().Right(Combinator.Choice(
                        Standard.Get["term", "list"],
                        Standard.Get["term", "vector"],
                        Standard.Get["term", "lambdaList"]
                    )),
                    // Identifiers
                    Combinator.Choice(
                        Chars.Digit()
                            .Not()
                            .Right(Chars.Space()
                                .Or(punctuation)
                                .Not()
                                .Right(Chars.Any())
                                .Many(1)
                            )
                    ).Select(cs =>
                        YacqExpression.Identifier(default(Char), cs.Stringify())
                    )
                )
                .Let(ps => Prims.Pipe(
                    ps,
                    '.'.Satisfy()
                        .Right(ps)
                        .Many(),
                    (h, t) => t.Aggregate(h, (l, r) =>
                        YacqExpression.List(YacqExpression.Identifier("."), l, r)
                    )
                ))
                .Select(et => et.Reduce().If(
                    e => e.Type().IsAppropriate(typeof(Parser<Expression, Expression>)),
                    e => et,
                    e => YacqExpression.TypeCandidate(typeof(YacqCombinators))
                        .Method("Evaluate")
                        .Method("Where", e)
                ))
            );

            #endregion

            #region Primaries

            Parser<Char, YacqExpression> primaryRef = null;
            var primary = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => primaryRef(stream)
            );
            this.Add("root", "primary", g => primary.Value);

            // Parentheses
            this.Add("primary", "parenthesis", g => g["root", "expression"]
                .Between('('.Satisfy(), ')'.Satisfy())
                .Or(g["root", "term"])
            );

            // Lists
            this.Add("primary", "list", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between(Chars.Sequence("%("), ')'.Satisfy())
                .Select(es => YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("List", es))
            );

            // Vectors
            this.Add("primary", "vector", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between(Chars.Sequence("%["), ']'.Satisfy())
                .Select(es => YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("Vector", es))
            );

            // Lambda Lists
            this.Add("primary", "lambdaList", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between(Chars.Sequence("%{"), '}'.Satisfy())
                .Select(es => YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("LambdaList", es))
            );

            // Identifiers
            this.Add("primary", "identifier", g => '`'.Satisfy()
                .Let(q =>
                    q.Right(q
                        .Not()
                        .Right('\\'.Satisfy()
                            .Right('`'.Satisfy())
                            .Or(Chars.Any())
                        )
                        .Many()
                        .Left(q)
                    )
                )
                .Select(cs => YacqExpression.TypeCandidate(typeof(YacqCombinators))
                    .Method("Identifier", YacqExpression.Text("\"" + cs.Stringify() + "\""))
                )
            );

            primaryRef = this.Get["primary"]
                .Choice()
                .SetPosition()
                .Between(this.Get["root", "ignore"], this.Get["root", "ignore"]);

            #endregion

            #region Operators

            // Dots
            this.Add("operator", "dot", g => g["root", "primary"]
                .Let(parent => Prims.Pipe(
                    parent,
                    '.'.Satisfy()
                        .Right(parent)
                        .Many(),
                    (l, rs) => rs.Aggregate(l, (h, t) =>
                        YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("List",
                            YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("Identifier", Expression.Constant(".")),
                            h,
                            t
                        )
                    )
                ))
            );

            // Colons
            this.Add("operator", "colon", g => g["operator", "dot"]
                .Let(parent => Prims.Pipe(
                    parent,
                    ':'.Satisfy()
                        .Right(parent)
                        .Many(),
                    (l, rs) => rs.Aggregate(l, (h, t) =>
                        YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("List",
                            YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("Identifier", Expression.Constant(":")),
                            h,
                            t
                        )
                    )
                ))
            );

            // Unary Operators
            this.Add("operator", "unary", g => g["operator", "colon"]
                .Let(parent => Prims.Pipe(
                    parent,
                    Combinator.Choice(
                        Chars.Sequence("?")
                            .Select(_ => Tuple.Create("Maybe", Arrays.Empty<Expression>())),
                        Combinator.Choice(
                            Chars.Sequence("*")
                                .Select(_ => Arrays.Empty<Expression>()),
                            Chars.Sequence("+")
                                .Select(_ => new Expression[] { Expression.Constant(1), }),
                            Chars.Number()
                                .Many(1)
                                .Select(cs => Int32.Parse(cs.Stringify()))
                                .Between(g["root", "ignore"], g["root", "ignore"])
                                .Let(p => Combinator.Choice(
                                    p.Pipe('-'.Satisfy().Right(p), (m, n) => new Expression[] { Expression.Constant(m), Expression.Constant(n), }),
                                    p.Left('-'.Satisfy()).Select(n => new Expression[] { Expression.Constant(n), }),
                                    p.Select(n => new Expression[] { Expression.Constant(n), Expression.Constant(n), })
                                ))
                                .Between('{'.Satisfy(), '}'.Satisfy())
                        ).Select(_ => Tuple.Create("Many", _))
                    ).Many(),
                    (l, cs) => cs.Aggregate(l, (l_, c) => YacqExpression.TypeCandidate(typeof(Combinator)).Method(c.Item1, c.Item2.StartWith(l_)))
                ))
            );

            this.Add("operator", "binary", g => g["operator", "unary"]
                .Let(parent => Prims.Pipe(
                    parent,
                    Prims.Pipe(
                        Combinator.Choice(
                            '<'.Satisfy().Select(_ => Tuple.Create(typeof(Prims), "Left", Arrays.Empty<Expression>())),
                            '>'.Satisfy().Select(_ => Tuple.Create(typeof(Prims), "Right", Arrays.Empty<Expression>())),
                            '|'.Satisfy().Select(_ => Tuple.Create(typeof(Combinator), "Or", Arrays.Empty<Expression>()))
                        ),
                        parent,
                        Tuple.Create
                    ).Many(),
                    (l, rs) => rs.Aggregate(l, (h, t) =>
                        YacqExpression.TypeCandidate(t.Item1.Item1).Method(t.Item1.Item2, t.Item1.Item3.StartWith(h, t.Item2))
                    )
                ))
            );

            this.Add("operator", "as", g => g["operator", "binary"]
                .Let(parent => Prims.Pipe(
                    Chars.Digit()
                        .Not()
                        .Right(Chars.Space()
                            .Or(punctuation)
                            .Not()
                            .Right(Chars.Any())
                            .Many(1)
                        )
                        .Select(cs => YacqExpression.Text(default(Char), cs.Stringify()))
                        .Left('@'.Satisfy())
                        .Many(),
                    parent,
                    (ls, r) => ls.Reverse().Aggregate(r, (h, t) =>
                        YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("As", h, YacqExpression.Variable("$here"), t)
                    )
                ))
            );

            #endregion

            expressionRef = this.Get["operator"].Last();

            this.Set.Default = g => g["root", "expression"];

            this._isReadOnly = true;
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
