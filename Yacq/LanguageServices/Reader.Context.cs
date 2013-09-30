// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2012 linerlock <x.linerlock@gmail.com>
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
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    partial class Reader
    {
        /// <summary>
        /// Represents reader context (correspondence of paired tokens).
        /// </summary>
        public class Context
            : IEquatable<Context>
        {
            /// <summary>
            /// Gets the name of this context.
            /// </summary>
            /// <value>The name of this context.</value>
            public String Name
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the start postion of this context.
            /// </summary>
            /// <value>The start position of this context.</value>
            public Position Position
            {
                get;
                private set;
            }

            internal Context(String name, Position position)
            {
                this.Name = name;
                this.Position = position;
            }

            /// <summary>
            /// Indicates whether this reader context and a specified reader context are equal.
            /// </summary>
            /// <param name="other">Another reader context to compare to.</param>
            /// <returns><c>true</c> if <paramref name="other"/> and this reader context are the same value; otherwise, <c>false</c>.</returns>
            public Boolean Equals(Context other)
            {
                return this.Name == other.Name && this.Position == other.Position;
            }

            /// <summary>
            /// Returns a <see cref="String"/> that represents this instance.
            /// </summary>
            /// <returns>A <see cref="String"/> that represents this instance.</returns>
            public override String ToString()
            {
                return this.Name + " at " + this.Position;
            }
        }
    }
}