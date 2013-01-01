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
using System.Text.RegularExpressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression which is a string or a character.
    /// </summary>
    public class TextExpression
        : YacqExpression
    {
        private readonly List<String> _codes;

        /// <summary>
        /// Gets the character in <see cref="SourceText"/> which is used to quote the string.
        /// </summary>
        /// <value>The character in <see cref="SourceText"/> which is used to quote the string.</value>
        public Char QuoteChar
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the quoted inner string and source of constant string or character of this expression.
        /// </summary>
        /// <value>The quoted inner string and source of constant string or character of this expression.</value>
        public String SourceText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the constant string or character which this expression represents.
        /// </summary>
        /// <value>The constant string or character which this expression represents.</value>
        public Object Value
        {
            get;
            private set;
        }

        internal TextExpression(
            SymbolTable symbols,
            Char quoteChar,
            String sourceText
        )
            : base(symbols)
        {
            this._codes = new List<String>();
            this.QuoteChar = quoteChar;
            this.SourceText = sourceText ?? "";
            this.Value = this.Parse();
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
            return this._codes.Any()
                ? (Expression) TypeCandidate(typeof(String)).Method(symbols, "Format",
                      this._codes
                          .Select(c => YacqServices.Parse(symbols, c))
                          .StartWith(Constant(this.Value))
                  )
                : Constant(this.Value);
        }

        private Object Parse()
        {
            if (this.QuoteChar == default(Char))
            {
                return this.SourceText;
            }
            String text = this.SourceText;
            if (text.Contains(@"\$("))
            {
                text = text
                    .Replace("{", "{{")
                    .Replace("}", "}}");
            }
            text = this.ParseEscapeSequences(text);
            return this.QuoteChar == '\'' && text.Length == 1
                ? (Object) text[0]
                : text;
        }

        private String ParseEscapeSequences(String str)
        {
            return Regex.Replace(
                str,
                @"\\\$\([^\(]*(((?<Open>\()[^\(\)]*)+((?<Close-Open>\))[^\(\)]*)+)*\)(?(Open)(?!))"
                    + @"|\\M-\\C-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                    + @"|\\C-\\M-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                    + @"|\\C-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                    + @"|\\M-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                    + @"|\\u[0-9A-Fa-f]{1,4}"
                    + @"|\\U[0-9A-Fa-f]{1,8}"
                    + @"|\\[0-7]{1,3}"
                    + @"|\\x[0-9A-Fa-f]{1,2}"
                    + @"|\\.",
                m => this.ParseEscapeSequence(m.Value)
            );
        }

        private String ParseEscapeSequence(String str)
        {
            if (str[0] != '\\')
            {
                return str;
            }
            str = str.Substring(1);
            if (str.StartsWithInvariant("$("))
            {
                this._codes.Add(ParseEscapeSequences(str.Substring(1)));
                return "{" + (this._codes.Count - 1) + "}";
            }
            else if (str.StartsWithInvariant("M-\\C-") || str.StartsWithInvariant("C-\\M-"))
            {
                return ((Char) (ParseEscapeSequence(str.Substring(4))[0] & 0x9f | 0x80)).ToString();
            }
            else if (str.StartsWithInvariant("C-"))
            {
                return ((Char) (ParseEscapeSequence(str.Substring(2))[0] & 0x9f)).ToString();
            }
            else if (str.StartsWithInvariant("M-"))
            {
                return ((Char) (ParseEscapeSequence(str.Substring(2))[0] & 0xff | 0x80)).ToString();
            }
            else if (str[0] == 'u' && str.Length > 1)
            {
                return ((Char) System.Convert.ToInt32(str.Substring(1), 16)).ToString();
            }
            else if (str[0] == 'U' && str.Length > 1)
            {
                return ((Char) System.Convert.ToInt32(str.Substring(1), 16)).ToString();
            }
            else if (Char.IsDigit(str, 0))
            {
                return ((Char) System.Convert.ToInt32(str, 8)).ToString();
            }
            else if (str[0] == 'x' && str.Length > 1)
            {
                return ((Char) System.Convert.ToInt32(str.Substring(1), 16)).ToString();
            }
            else
            {
                switch (str[0])
                {
                    case 'a':
                        return "\a";
                    case 'b':
                        return "\b";
                    case 'e':
                        return "\x1b";
                    case 'f':
                        return "\f";
                    case 'n':
                        return "\n";
                    case 'r':
                        return "\r";
                    case 's':
                        return " ";
                    case 't':
                        return "\t";
                    case 'v':
                        return "\v";
                    case 'N':
                        return Environment.NewLine;
                    default:
                        return str;
                }
            }
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="TextExpression"/> that represents a string or a character from specified source string.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="quoteChar">The character which is used for quoting the text.</param>
        /// <param name="sourceText">The quoted inner string.</param>
        /// <returns>An <see cref="TextExpression"/> which generates a string or a character from specified string.</returns>
        public static TextExpression Text(SymbolTable symbols, Char quoteChar, String sourceText)
        {
            return new TextExpression(symbols, quoteChar, sourceText);
        }

        /// <summary>
        /// Creates a <see cref="TextExpression"/> that represents a string or a character from specified source string.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="text">The string which contains quoting characters..</param>
        /// <returns>An <see cref="TextExpression"/> which generates a string or a character from specified string.</returns>
        public static TextExpression Text(SymbolTable symbols, String text)
        {
            return String.IsNullOrEmpty(text) || text.First() != text.Last()
                ? Text(symbols, default(Char), text)
                : Text(symbols, text.First(), text.Substring(1, text.Length - 2));
        }

        /// <summary>
        /// Creates a <see cref="TextExpression"/> that represents a string or a character from specified source string.
        /// </summary>
        /// <param name="quoteChar">The character which is used for quoting the text.</param>
        /// <param name="sourceText">The quoted inner string.</param>
        /// <returns>An <see cref="TextExpression"/> which generates a string or a character from specified string.</returns>
        public static TextExpression Text(Char quoteChar, String sourceText)
        {
            return Text(null, quoteChar, sourceText);
        }

        /// <summary>
        /// Creates a <see cref="TextExpression"/> that represents a string or a character from specified source string.
        /// </summary>
        /// <param name="text">The string which contains quoting characters..</param>
        /// <returns>An <see cref="TextExpression"/> which generates a string or a character from specified string.</returns>
        public static TextExpression Text(String text)
        {
            return Text(null, text);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
