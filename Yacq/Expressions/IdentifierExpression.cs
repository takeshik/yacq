﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression which is an identifier.
    /// </summary>
    public class IdentifierExpression
        : YacqExpression
    {
        private readonly Lazy<Tuple<String, IList<String>>> _result;

        /// <summary>
        /// Gets the character in <see cref="SourceText"/> which is used to quote the identifier.
        /// </summary>
        /// <value>The character in <see cref="SourceText"/> which is used to quote the identifier.</value>
        public Char QuoteChar
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quoted string which is the source of the identifier.
        /// </summary>
        /// <value>The quoted string which is the source the identifier.</value>
        public String SourceText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of this expression.
        /// </summary>
        /// <value>The name of this expression.</value>
        public String Name
        {
            get
            {
                // TODO: Problematic code
                return this._result.Value.Item2.Any()
                    ? String.Format(
                          this._result.Value.Item1,
                          this._result.Value.Item2
                              .Select(s => YacqServices.Parse(s).Evaluate())
                              .ToArray()
                      )
                    : this._result.Value.Item1;
            }
        }

        internal IdentifierExpression(
            SymbolTable symbols,
            Char quoteChar,
            String sourceText
        )
            : base(symbols)
        {
            this._result = new Lazy<Tuple<String, IList<String>>>(this.Parse);
            this.QuoteChar = quoteChar;
            this.SourceText = sourceText;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return this.QuoteChar == default(Char)
                ? this.SourceText
                : this.QuoteChar + this.SourceText + this.QuoteChar;
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            if (this._result.Value.Item2.Any())
            {
                return TypeCandidate(typeof(YacqExpression))
                    .Method(symbols, "Identifier",
                        Default(typeof(Char)),
                        TypeCandidate(typeof(String)).Method(symbols, "Format",
                            this._result.Value.Item2
                                .Select(c => YacqServices.Parse(symbols, c))
                                .StartWith(Constant(this._result.Value.Item1))
                        )
                    )
                    .Method(symbols, "eval");
            }
            else if (symbols.ResolveMatch(DispatchTypes.Member, this.Name) != null
                || symbols.Missing != DispatchExpression.DefaultMissing
            )
            {
                return Variable(symbols, this.Name)
                    .ReduceOnce(symbols, expectedType)
                    .Let(e => (e as MacroExpression).Null(m => m.Evaluate(symbols)) ?? e);
            }
            else
            {
                throw new ParseException("Identifier evaluation failed: " + this, this);
            }
        }

        private Tuple<String, IList<String>> Parse()
        {
            if (this.QuoteChar == default(Char))
            {
                return MakeTuple(this.SourceText, new String[0]);
            }
            var text = this.SourceText;
            var codes = new List<String>();

            if (text.Contains(@"\$("))
            {
                text = text
                    .Replace("{", "{{")
                    .Replace("}", "}}");
            }

            return MakeTuple(EscapeSequences.Parse(text, codes), codes);
        }

        private static Tuple<String, IList<String>> MakeTuple(String name, IList<String> codes)
        {
            return Tuple.Create(name, codes);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="IdentifierExpression"/> that represents an identifier from specified source string.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="quoteChar">The character which is used for quoting the identifier.</param>
        /// <param name="sourceText">The quoted inner string.</param>
        /// <returns>An <see cref="IdentifierExpression"/> that has the name represented by specified source string.</returns>
        public static IdentifierExpression Identifier(SymbolTable symbols, Char quoteChar, String sourceText)
        {
            return new IdentifierExpression(symbols, quoteChar, sourceText);
        }

        /// <summary>
        /// Creates a <see cref="IdentifierExpression"/> that represents an identifier with specified name.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name of this expression.</param>
        /// <returns>An <see cref="IdentifierExpression"/> that has the specified name.</returns>
        public static IdentifierExpression Identifier(SymbolTable symbols, String name)
        {
            return String.IsNullOrEmpty(name) || name.Length < 2 || name.First() != name.Last()
                ? Identifier(symbols, default(Char), name)
                : Identifier(symbols, name.First(), name.Substring(1, name.Length - 2));
        }

        /// <summary>
        /// Creates a <see cref="IdentifierExpression"/> that represents an identifier from specified source string.
        /// </summary>
        /// <param name="quoteChar">The character which is used for quoting the identifier.</param>
        /// <param name="sourceText">The quoted inner string.</param>
        /// <returns>An <see cref="IdentifierExpression"/> that has the name represented by specified source string.</returns>
        public static IdentifierExpression Identifier(Char quoteChar, String sourceText)
        {
            return Identifier(null, quoteChar, sourceText);
        }

        /// <summary>
        /// Creates a <see cref="IdentifierExpression"/> that represents an identifier with specified name.
        /// </summary>
        /// <param name="name">The name of this expression.</param>
        /// <returns>An <see cref="IdentifierExpression"/> that has the specified name.</returns>
        public static IdentifierExpression Identifier(String name)
        {
            return Identifier(null, name);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
