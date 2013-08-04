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

            #endregion

            #region Trivials

            var punctuation = Chars.OneOf(
                // From the Standard Grammar:
                '"', '#', '\'', '(', ')', '.', ':', ';', '[', ']', '`', '{', '}',
                // Additional punctuation characters:
                '%', '?', '*'
            );

            #endregion

            #region Ignore

            this.Add("root", "ignore", g => Combinator.Choice(
                this.Get["root", "comment"].Ignore(),
                Chars.Space().Ignore(),
                ','.Satisfy().Ignore(),
                Combinator.Choice(
                    Chars.Sequence("\r\n"),
                    Chars.OneOf('\r', '\n', '\x85', '\u2028', '\u2029')
                        .Select(EnumerableEx.Return)
                ).Select(_ => Environment.NewLine).Ignore()
            ).Many().Select(_ => (YacqExpression) YacqExpression.Ignore()));

            #endregion

            #region Primaries

            Parser<Char, YacqExpression> primaryRef = null;
            var primary = new Lazy<Parser<Char, YacqExpression>>(
                () => stream => primaryRef(stream)
            );
            this.Add("root", "primary", g => primary.Value);

            // Parentheses
            this.Add("primary", "parenthesis", g => SetPosition(
                g["root", "expression"].Between('('.Satisfy(), ')'.Satisfy())
            ));

            // Lists
            this.Add("primary", "list", g => SetPosition(
                g["root", "expression"]
                    .Between(g["root", "ignore"], g["root", "ignore"])
                    .Many()
                    .Between(Chars.Sequence("%("), ')'.Satisfy())
                    .Select(es => YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("List", es))
            ));

            // Vectors
            this.Add("primary", "vector", g => SetPosition(
                g["root", "expression"]
                    .Between(g["root", "ignore"], g["root", "ignore"])
                    .Many()
                    .Between(Chars.Sequence("%["), ']'.Satisfy())
                    .Select(es => YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("Vector", es))
            ));

            // Lambda Lists
            this.Add("primary", "lambdaList", g => SetPosition(
                g["root", "expression"]
                    .Between(g["root", "ignore"], g["root", "ignore"])
                    .Many()
                    .Between(Chars.Sequence("%{"), '}'.Satisfy())
                    .Select(es => YacqExpression.TypeCandidate(typeof(YacqCombinators)).Method("LambdaList", es))
            ));

            // Identifiers
            this.Add("primary", "identifier", g => SetPosition(
                '`'.Satisfy().Let(q =>
                    q.Right(q
                        .Not()
                        .Right('\\'.Satisfy()
                            .Right('`'.Satisfy())
                            .Or(Chars.Any())
                        )
                        .Many()
                        .Left(q)
                    )
                ).Select(cs => YacqExpression.TypeCandidate(typeof(YacqCombinators))
                    .Method("Identifier", YacqExpression.Text("\"" + new String(cs.ToArray()) + "\""))
                )
            ));

            primaryRef = this.Get["primary"]
                .Choice()
                .Between(this.Get["root", "ignore"], this.Get["root", "ignore"]);

            #endregion

            #region Operators

            // Unary Operators
            this.Add("operator", "unary", g => g["root", "primary"]
                .Let(parent => SetPosition(Prims.Pipe(
                    parent,
                    Combinator.Choice(
                        Chars.Sequence("?").Select(_ => Tuple.Create("Maybe", Arrays.Empty<Expression>())),
                        Chars.Sequence("*").Select(_ => Tuple.Create("Many", Arrays.Empty<Expression>())),
                        Chars.Sequence("+").Select(_ => Tuple.Create("Many", new Expression[] { Expression.Constant(1), }))
                    )
                        .Many(),
                    (l, cs) => cs.Aggregate(l, (l_, c) => YacqExpression.TypeCandidate(typeof(Combinator)).Method(c.Item1, c.Item2.StartWith(l_)))
                )))
            );

            this.Add("operator", "binary", g => g["operator", "unary"]
                .Let(parent => SetPosition(Prims.Pipe(
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
                )))
            );

            #endregion

            expressionRef = this.Get["operator"].Last();

            this.Set.Default = g => g["root", "expression"];

            this._isReadOnly = true;
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
