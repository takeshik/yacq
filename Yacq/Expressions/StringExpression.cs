// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    public class TextExpression
        : YacqExpression
    {
        public Char QuoteChar
        {
            get;
            private set;
        }

        public String SourceText
        {
            get;
            private set;
        }

        public Object Value
        {
            get;
            private set;
        }

        internal TextExpression(Char quoteChar, String sourceText)
        {
            this.QuoteChar = quoteChar;
            this.SourceText = sourceText;
            this.Value = this.Parse();
        }

        public override String ToString()
        {
            return this.QuoteChar == default(Char)
                ? this.SourceText
                : this.QuoteChar + this.SourceText + this.QuoteChar;
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Constant(this.Value);
        }

        public Object Parse()
        {
            if (this.QuoteChar == default(Char))
            {
                return this.SourceText;
            }
            // TODO: escape sequence
            String text = this.SourceText;
            return this.QuoteChar == '\'' && text.Length == 1
                ? (Object) text[0]
                : text;
        }
    }

    partial class YacqExpression
    {
        public static TextExpression Text(Char quoteChar, String sourceText)
        {
            return new TextExpression(quoteChar, sourceText);
        }

        public static TextExpression Text(String text)
        {
            return Text(text.First(), text.Substring(1, text.Length - 2));
        }
    }
}
