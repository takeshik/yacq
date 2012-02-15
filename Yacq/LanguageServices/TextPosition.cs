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

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Represents a position in the string.
    /// </summary>
    public struct TextPosition
        : IComparable<TextPosition>
    {
        /// <summary>
        /// Gets the 0-origin index of the string.
        /// </summary>
        public Int32 Index
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the 1-origin line number of the string.
        /// </summary>
        public Int32 Line
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the 1-origin column number of the string.
        /// </summary>
        public Int32 Column
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="TextPosition"/>.
        /// </summary>
        /// <param name="index">The 0-origin index of the string.</param>
        /// <param name="line">The 1-origin line number of the string.</param>
        /// <param name="column">The 1-origin column number of the string.</param>
        public TextPosition(Int32 index, Int32 line, Int32 column)
            : this()
        {
            this.Index = index;
            this.Line = line;
            this.Column = column;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return "L" + this.Line + ":C" + this.Column + " (idx:" + this.Index + ")";
        }

        /// <summary>
        /// Compares this instance to a specified <see cref="TextPosition"/> object and indicates whether this instance is earlier than, the same as, or later than the second <see cref="TextPosition"/> object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A signed integer that indicates the relationship between this instance and <paramref name="other"/>,</returns>
        public Int32 CompareTo(TextPosition other)
        {
            return this.Index.CompareTo(other.Index);
        }
    }
}