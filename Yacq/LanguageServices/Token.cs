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
using System.Collections.Generic;
using System.Linq;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Represents code token.
    /// </summary>
    public struct Token
    {
        /// <summary>
        /// Gets the kind of this token.
        /// </summary>
        /// <value>Kind of this token.</value>
        public TokenType Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the string expression of this token.
        /// </summary>
        /// <value>The string expression of this token.</value>
        public String Text
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the 0-origin position in the sequence of the input of this token.
        /// </summary>
        /// <value>The 0-origin position in the sequence of the input of this token.</value>
        public Int32 Position
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the 1-origin line number of this token,
        /// </summary>
        /// <value>The 1-origin line number of this token.</value>
        public Int32 Line
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the 1-origin column number of this token,
        /// </summary>
        /// <value>The 1-origin column number of this token.</value>
        public Int32 Column
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> structure.
        /// </summary>
        /// <param name="type">The kind of this token.</param>
        /// <param name="text">The string expression of this token.</param>
        /// <param name="position">The 0-origin position in the sequence of the input of this token.</param>
        /// <param name="line">The 1-origin line number of this token.</param>
        /// <param name="column">The 1-origin column number of this token.</param>
        public Token(TokenType type, String text, Int32 position, Int32 line, Int32 column)
            : this()
        {
            this.Type = type;
            this.Text = text;
            this.Position = position;
            this.Line = line;
            this.Column = column;
        }
    }
}