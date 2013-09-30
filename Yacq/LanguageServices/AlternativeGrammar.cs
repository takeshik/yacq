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
using Parseq.Combinators;
using XSpect.Yacq.Expressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Provides the alternative grammar. This grammar cannot modify.
    /// </summary>
    public sealed class AlternativeGrammar
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

        internal AlternativeGrammar()
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
            this.Add("term", "text", g => Standard.Get["term", "text"]);
            this.Add("term", "number", g => Standard.Get["term", "number"]);

            #endregion

            #region Trivials

            var punctuation = Chars.OneOf(
                // From the Standard Grammar:
                '"', '#', '\'', '(', ')', ',', '.', ':', ';', '[', ']', '`', '{', '}',
                // Additional punctuation characters:
                '%', '&', '*', '+', '-', '/', '<', '=', '>', '?', '^', '|', '~'
            );

            #endregion

            #region Terms

            // Vectors
            this.Add("term", "vector", g => g["root", "expression"]
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between('['.Satisfy(), ']'.Satisfy(), "vector")
                .Select(YacqExpression.Vector)
            );

            // Identifiers
            this.Add("term", "identifier", g => Combinator.Choice(
                Combinator.Choice(
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

            this.Add("root", "term", g => Combinator.Choice(
                g["term", "text"],
                g["term", "number"],
                g["term", "vector"],
                g["term", "identifier"],
                g["term", "ext"]
            ));

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
                .EnterContext("unquoteSplicing")
                .Right(g["root", "expression"])
                .LeaveContext("unquoteSplicing")
                .Select(e => YacqExpression.List(YacqExpression.Identifier("unquote"), e))
            );

            // Transiting Expressions (Alternative Grammer)
            this.Add("term.ext", "stdExpression", g => Standard.Get.Default
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Many()
                .Between('('.Satisfy(), ')'.Satisfy(), "stdExpression")
                .Select(YacqExpression.List)
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

            #region Primaries

            Parser<Char, YacqExpression> primaryRef = null;
            var primary = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => primaryRef(stream)
            );
            this.Add("root", "primary", g => primary.Value);

            // Parentheses, Invocations and Indexer Accesses
            this.Add("primary", "invokeOrIndex", g => Prims.Pipe(
                g["root", "expression"].Between('('.Satisfy(), ')'.Satisfy(), "parenthesis")
                    .Or(g["root", "term"])
                    .SetPosition(),
                Combinator.Choice(
                    g["root", "expression"]
                        .Between(g["root", "ignore"], g["root", "ignore"])
                        .Many()
                        .Between('('.Satisfy(), ')'.Satisfy(), "invoke")
                        .Select(ps => Tuple.Create(
                            Arrays.Empty<IdentifierExpression>(),
                            ps
                        )),
                    g["root", "expression"]
                        .Between(g["root", "ignore"], g["root", "ignore"])
                        .Many()
                        .Between('['.Satisfy(), ']'.Satisfy(), "index")
                        .Select(ps => Tuple.Create(
                            Arrays.From(YacqExpression.Identifier(".")),
                            (IEnumerable<YacqExpression>) Arrays.From(YacqExpression.Vector(ps))
                        ))
                    ).Many(),
                (h, t) => t.Aggregate(h, (r, ps) =>
                    YacqExpression.List(ps.Item1.Concat(ps.Item2.StartWith(r)))
                )
            ));

            // Dots (Method Invocations, Property Accesses and Indexer Accesses)
            this.Add("primary", "dot", g => Prims.Pipe(
                g["primary", "invokeOrIndex"],
                '.'.Satisfy()
                    .Select(_ => YacqExpression.Identifier("."))
                    .SetPosition()
                    .Both(Combinator.Choice(
                        Prims.Pipe(
                            g["term", "identifier"],
                            g["root", "expression"]
                                .Between(g["root", "ignore"], g["root", "ignore"], "dotInvoke")
                                .Many()
                                .Between('('.Satisfy(), ')'.Satisfy()),
                            (n, es) => (YacqExpression) YacqExpression.List(es.StartWith(n))
                        ),
                        g["root", "expression"]
                            .Between(g["root", "ignore"], g["root", "ignore"])
                            .Many()
                            .Between('['.Satisfy(), ']'.Satisfy(), "dotIndex")
                            .Select(YacqExpression.Vector),
                        g["term", "identifier"]
                            .SetPosition()
                    ))
                    .Many(),
                (h, t) => t.Aggregate(h, (l, rs) =>
                    YacqExpression.List(rs.Item1, l, rs.Item2)
                )
            ));

            // Colons
            this.Add("primary", "colon", g => Prims.Pipe(
                g["primary", "dot"],
                ':'.Satisfy()
                    .Select(_ => YacqExpression.Identifier(":"))
                    .SetPosition()
                    .Both(g["primary", "dot"])
                    .Many(),
                (h, t) => t.Aggregate(h, (l, r) =>
                    YacqExpression.List(r.Item1, l, r.Item2)
                )
            ));

            primaryRef = this.Get["primary"]
                .Last()
                .SetPosition()
                .Between(this.Get["root", "ignore"], this.Get["root", "ignore"]);

            #endregion

            #region Operators

            // Unary Operators
            this.Add("operator", "unary", g => g["root", "primary"]
                .Let(parent => Prims.Pipe(
                    Combinator.Choice(
                        Chars.Sequence("++").Select(_ => "++="),
                        Chars.Sequence("--").Select(_ => "--="),
                        Chars.Sequence("+"),
                        Chars.Sequence("-"),
                        Chars.Sequence("!"),
                        Chars.Sequence("~")
                    )
                        .Select(cs => cs.Stringify())
                        .Many(),
                    parent,
                    (cs, r) => cs.Aggregate(r, (r_, c) => YacqExpression.List(YacqExpression.Identifier(c), r_))
                ))
            );

            // Multiplicative Operators
            this.AddBinaryOperator("multiplicative", "unary",
                Chars.Sequence("*"),
                Chars.Sequence("/"),
                Chars.Sequence("%")
            );

            // Additive Operators
            this.AddBinaryOperator("additive", "multiplicative",
                Chars.Sequence("+"),
                Chars.Sequence("-")
            );
            
            // Shift Operators
            this.AddBinaryOperator("shift", "additive",
                Chars.Sequence("<<"),
                Chars.Sequence(">>")
            );
            
            // Relational Operators
            this.AddBinaryOperator("relational", "shift",
                Chars.Sequence("<="),
                Chars.Sequence(">="),
                Chars.Sequence("<"),
                Chars.Sequence(">")
            );
            
            // Equality Operators
            this.AddBinaryOperator("equality", "relational",
                Chars.Sequence("=="),
                Chars.Sequence("!=")
            );
            
            // And Operators
            this.AddBinaryOperator("and", "equality",
                Chars.Sequence("&")
                    .Left('&'.Satisfy().Not())
            );
            
            // Xor Operators
            this.AddBinaryOperator("xor", "and",
                Chars.Sequence("^")
            );
            
            // Or Operators
            this.AddBinaryOperator("or", "xor",
                Chars.Sequence("|")
                    .Left('|'.Satisfy().Not())
            );
            
            // AndAlso Operators
            this.AddBinaryOperator("andAlso", "or",
                Chars.Sequence("&&")
            );
            
            // OrElse Operators
            this.AddBinaryOperator("orElse", "andAlso",
                Chars.Sequence("||")
            );
            
            // Coalesce Operators
            this.AddBinaryOperator("coalesce", "orElse",
                Chars.Sequence("??")
            );

            // Assignment Operators
            this.Add("operator", "assignment", g => g["operator", "coalesce"]
                .Let(parent => Prims.Pipe(
                    Prims.Pipe(
                        parent
                            .EnterContext("assignment"),
                        Combinator.Choice(
                            Chars.Sequence("="),
                            Chars.Sequence("+="),
                            Chars.Sequence("-="),
                            Chars.Sequence("*="),
                            Chars.Sequence("/="),
                            Chars.Sequence("%="),
                            Chars.Sequence("&="),
                            Chars.Sequence("|="),
                            Chars.Sequence("^="),
                            Chars.Sequence("<<="),
                            Chars.Sequence(">>=")
                        )
                            .Select(cs => YacqExpression.Identifier(cs.Stringify()))
                            .SetPosition(),
                        Tuple.Create
                    ).Many(),
                    parent.LeaveContext("assignment"),
                    (ls, r) => ls.Reverse().Aggregate(r, (t, h) =>
                        YacqExpression.List(h.Item2, h.Item1, t)
                    )
                ))
            );

            #endregion

            expressionRef = this.Get["operator"].Last();

            this.Set.Default = g => g["root", "expression"];

            this._isReadOnly = true;
        }

        private void AddBinaryOperator(String name, String parentName, params Parser<Char, IEnumerable<Char>>[] opcodes)
        {
            this.Add("operator", name, g => g["operator", parentName]
                .Let(parent => Prims.Pipe(
                    parent,
                    Prims.Pipe(
                        opcodes
                            .Choice()
                            .EnterContext(name)
                            .Select(cs => YacqExpression.Identifier(cs.Stringify()))
                            .SetPosition(),
                        parent
                            .LeaveContext(name),
                        Tuple.Create
                    ).Many(),
                    (l, rs) => rs.Aggregate(l, (h, t) =>
                        YacqExpression.List(t.Item1, h, t.Item2)
                    )
                ))
            );
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
