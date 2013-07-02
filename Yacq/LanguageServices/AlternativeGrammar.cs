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
        /// Gets the reference to the parser with specified rule key.
        /// </summary>
        /// <param name="key">The rule key to get the parser.</param>
        /// <value>The reference to the parser with specified rule key.</value>
        public override Lazy<Parser<Char, YacqExpression>> this[RuleKey key]
        {
            get
            {
                return base[key];
            }
            set
            {
                this.CheckIfReadOnly();
                base[key] = value;
            }
        }

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

        /// <summary>
        /// Gets the setter for this grammar. The standard grammar cannot modify.
        /// </summary>
        /// <value>This throws <see cref="InvalidOperationException"/>.</value>
        public override RuleSetter Set
        {
            get
            {
                this.CheckIfReadOnly();
                return base.Set;
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
                '%', '%', '&', '*', '+', '-', '.', '/', '<', '=', '>', '?', '^', '|', '~'
            );

            var comma = ','.Satisfy()
                .Between(this.Get["root", "ignore"], this.Get["root", "ignore"])
                .Select(_ => YacqExpression.Ignore());

            #endregion

            #region Terms

            // Vectors
            this.Add("term", "vector", g => SetPosition(
                g["root", "expression"]
                    .Between(g["root", "ignore"], g["root", "ignore"])
                    .SepBy(comma)
                    .Between('['.Satisfy(), ']'.Satisfy())
                    .Select(YacqExpression.Vector)
            ));

            // Transiting Expressions (Standard Grammer)
            this.Add("term", "stdExpression", g => SetPosition(
                Standard.Get.Default
                    .Many()
                    .Between(g["root", "ignore"], g["root", "ignore"])
                    .Between(Chars.Sequence("#("), ')'.Satisfy())
                    .Select(YacqExpression.List)
            ));

            // Identifiers
            this.Add("term", "identifier", g => SetPosition(
                Chars.Digit()
                    .Not()
                    .Right(Chars.Space()
                        .Or(punctuation)
                        .Or(Chars.OneOf())
                        .Not()
                        .Right(Chars.Any())
                        .Many(1)
                    )
                    .Select(cs => YacqExpression.Identifier(new String(cs.ToArray())))
                )
            );

            this.Add("root", "term", g => Combinator.Choice(
                g["term", "text"],
                g["term", "number"],
                g["term", "vector"],
                g["term", "stdExpression"],
                g["term", "identifier"]
            )
                .Between(g["root", "ignore"], g["root", "ignore"])
            );

            #endregion

            #region Primaries

            Parser<Char, YacqExpression> primaryRef = null;
            var primary = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => primaryRef(stream)
            );
            this.Add("root", "primary", g => primary.Value);

            // Parentheses, Invocations and Indexer Accesses
            this.Add("primary", "invokeOrIndex", g => SetPosition(Prims.Pipe(
                g["root", "expression"].Between('('.Satisfy(), ')'.Satisfy())
                    .Or(g["root", "term"]),
                Combinator.Choice(
                    g["root", "expression"]
                        .SepBy(comma)
                        .Or(g["root", "ignore"].Select(e => Enumerable.Empty<YacqExpression>()))
                        .Between('('.Satisfy(), ')'.Satisfy())
                        .Select(ps => Tuple.Create(
                            Enumerable.Empty<IdentifierExpression>(),
                            ps
                        )),
                    g["root", "expression"]
                        .SepBy(comma)
                        .Or(g["root", "ignore"].Select(e => Enumerable.Empty<YacqExpression>()))
                        .Between('['.Satisfy(), ']'.Satisfy())
                        .Select(ps => Tuple.Create(
                            EnumerableEx.Return(YacqExpression.Identifier(".")),
                            EnumerableEx.Return<YacqExpression>(YacqExpression.Vector(ps))
                        ))
                    ).Many(),
                (h, t) => t.Aggregate(h, (r, ps) =>
                    YacqExpression.List(ps.Item1.Concat(ps.Item2.StartWith(r)))
                )
            )));

            // Dots (Method Invocations, Property Accesses and Indexer Accesses)
            this.Add("primary", "dot", g => SetPosition(Prims.Pipe(
                g["primary", "invokeOrIndex"],
                '.'.Satisfy()
                    .Right(Combinator.Choice(
                        Prims.Pipe(
                            g["term", "identifier"],
                            g["root", "expression"]
                                .SepBy(comma)
                                .Or(g["root", "ignore"].Select(e => Enumerable.Empty<YacqExpression>()))
                                .Between('('.Satisfy(), ')'.Satisfy()),
                            (n, es) => (YacqExpression) YacqExpression.List(es.StartWith(n))
                        ),
                        g["root", "expression"]
                            .SepBy(comma)
                            .Or(g["root", "ignore"].Select(e => Enumerable.Empty<YacqExpression>()))
                            .Between('['.Satisfy(), ']'.Satisfy())
                            .Select(YacqExpression.Vector),
                        g["term", "identifier"]
                    ))
                    .Many(),
                (h, t) => t.Aggregate(h, (l, r) =>
                    YacqExpression.List(YacqExpression.Identifier("."), l, r)
                )
            )));

            // Colons
            this.Add("primary", "colon", g => SetPosition(Prims.Pipe(
                g["primary", "dot"],
                ':'.Satisfy()
                    .Right(g["primary", "dot"])
                    .Many(),
                (h, t) => t.Aggregate(h, (l, r) =>
                    YacqExpression.List(YacqExpression.Identifier(":"), l, r)
                )
            )));

            primaryRef = this.Get["primary"]
                .Last()
                .Between(this.Get["root", "ignore"], this.Get["root", "ignore"]);

            #endregion

            #region Operators

            // Unary Operators
            this.Add("operator", "unary", g => g["root", "primary"]
                .Let(parent => SetPosition(Prims.Pipe(
                    Combinator.Choice(
                        Chars.Sequence("++").Select(_ => "++="),
                        Chars.Sequence("--").Select(_ => "--="),
                        Chars.Sequence("+"),
                        Chars.Sequence("-"),
                        Chars.Sequence("!"),
                        Chars.Sequence("~")
                    )
                        .Select(cs => new String(cs.ToArray()))
                        .Many(),
                    parent,
                    (cs, r) => cs.Aggregate(r, (r_, c) => YacqExpression.List(YacqExpression.Identifier(c), r_))
                )))
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
                .Let(parent => SetPosition(Prims.Pipe(
                    Prims.Pipe(
                        parent,
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
                        ).Select(cs => new String(cs.ToArray())),
                        Tuple.Create
                    ).Many(),
                    parent,
                    (ls, r) => ls.Aggregate(r, (h, t) =>
                        YacqExpression.List(YacqExpression.Identifier(t.Item2.ToString()), h, t.Item1)
                    )
                )))
            );

            #endregion

            expressionRef = this.Get["operator"].Last();

            this.Set.Default = g => g["root", "expression"];

            this._isReadOnly = true;
        }

        private void AddBinaryOperator(String name, String parentName, params Parser<Char, IEnumerable<Char>>[] opcodes)
        {
            this.Add("operator", name, g => g["operator", parentName]
                .Let(parent => SetPosition(Prims.Pipe(
                    parent,
                    Prims.Pipe(
                        opcodes.Choice().Select(cs => new String(cs.ToArray())),
                        parent,
                        Tuple.Create
                    ).Many(),
                    (l, rs) => rs.Aggregate(l, (h, t) =>
                        YacqExpression.List(YacqExpression.Identifier(t.Item1.ToString()), h, t.Item2)
                    )
                )))
            );
        }

        /// <summary>
        /// Adds the rule to this grammar. The standard grammar cannot modify.
        /// </summary>
        /// <param name="key">The rule key to add.</param>
        /// <param name="value">The reference to the parser which defines the rule.</param>
        public override void Add(RuleKey key, Lazy<Parser<Char, YacqExpression>> value)
        {
            this.CheckIfReadOnly();
            base.Add(key, value);
        }

        /// <summary>
        /// Removes all rules from this grammar. The standard grammar cannot modify.
        /// </summary>
        public override void Clear()
        {
            this.CheckIfReadOnly();
            base.Clear();
        }

        /// <summary>
        /// Removes the symbol with the specified symbol key from this symbol table. The standard grammar cannot modify.
        /// </summary>
        /// <param name="key">The rule key to remove.</param>
        /// <returns>
        /// <value>This throws <see cref="InvalidOperationException"/>.</value>
        /// </returns>
        public override Boolean Remove(RuleKey key)
        {
            this.CheckIfReadOnly();
            return base.Remove(key);
        }

        private static Parser<Char, YacqExpression> SetPosition(Parser<Char, YacqExpression> parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            Parser<Char, Position> pos = stream => Reply.Success(stream, stream.Position);
            return pos.SelectMany(s =>
                parser.SelectMany(p =>
                    pos.Select(e =>
                        p.Apply(_ => _.SetPosition(s, e))
                    )
                )
            );
        }

        private void CheckIfReadOnly()
        {
            if (this._isReadOnly)
            {
                throw new InvalidOperationException("This grammar is read-only.");
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
