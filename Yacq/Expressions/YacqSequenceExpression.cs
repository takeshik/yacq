// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
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
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Collections;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides the base class from which the classes that represent YACQ sequence expression tree nodes are derived. This is an abstract class.
    /// </summary>
    /// <remarks>
    /// Sequence expressions are YACQ expression that can contain 0 or more expressions as its elements.
    /// </remarks>
    public abstract class YacqSequenceExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets a sequence of expressions that represent elements of this expression.
        /// </summary>
        /// <value>A sequence of expressions that represent elements of this expression.</value>
        public YacqList Elements
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the element at the specified index in this sequence expression.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <value>The element at the specified index in this sequence expression.</value>
        public Expression this[Int32 index]
        {
            get
            {
                return this.Elements[index];
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this sequence expression is empty.
        /// </summary>
        /// <value><c>true</c> if this sequence expression has no elements; otherwise, <c>false</c>.</value>
        public Boolean IsEmpty
        {
            get
            {
                return this.Elements.IsEmpty;
            }
        }

        /// <summary>
        /// Gets the number of elements contained in this sequence expression.
        /// </summary>
        /// <value>The number of elements contained in this sequence expression.</value>
        public Int32 Length
        {
            get
            {
                return this.Elements.Length;
            }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="YacqSequenceExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table linked with this expression.</param>
        /// <param name="elements">A <see cref="YacqList"/> object that represents the elements of the expression.</param>
        protected YacqSequenceExpression(SymbolTable symbols, YacqList elements)
            : base(symbols)
        {
            this.Elements = elements ?? YacqList.Empty;
        }

        /// <summary>
        /// Creates <see cref="ListExpression"/> from the symbol table and elements of this expression.
        /// </summary>
        /// <returns><see cref="ListExpression"/> which has same symbol table and elements of this expression.</returns>
        public ListExpression AsList()
        {
            return List(this.Symbols, this.Elements);
        }

        /// <summary>
        /// Creates <see cref="VectorExpression"/> from the symbol table and elements of this expression.
        /// </summary>
        /// <returns><see cref="VectorExpression"/> which has same symbol table and elements of this expression.</returns>
        public VectorExpression AsVector()
        {
            return Vector(this.Symbols, this.Elements);
        }

        /// <summary>
        /// Creates <see cref="LambdaListExpression"/> from the symbol table and elements of this expression.
        /// </summary>
        /// <returns><see cref="LambdaListExpression"/> which has same symbol table and elements of this expression.</returns>
        public LambdaListExpression AsLambdaList()
        {
            return LambdaList(this.Symbols, this.Elements);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
