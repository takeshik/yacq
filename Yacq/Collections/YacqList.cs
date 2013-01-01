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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Collections
{
    /// <summary>
    /// Represents immutable singly linked list of expressions.
    /// </summary>
    public class YacqList
        : IEnumerable<Expression>
    {
        private static readonly YacqList _empty = new YacqList();

        private readonly Expression _head;

        private readonly YacqList _tail;
        
        /// <summary>
        /// Gets empty list. Empty lists are used to indicate the terminal of the list.
        /// </summary>
        /// <value>The empty list object, its <see cref="Head"/> and <see cref="Tail"/> is <c>null</c>.</value>
        public static YacqList Empty
        {
            get
            {
                return _empty;
            }
        }

        /// <summary>
        /// Gets the first element of this list.
        /// </summary>
        /// <value>The first element of this list.</value>
        public Expression Head
        {
            get
            {
                return this._head;
            }
        }

        /// <summary>
        /// Gets the list without the first element of this list.
        /// </summary>
        /// <value>The list without the first element.</value>
        public YacqList Tail
        {
            get
            {
                return this._tail;
            }
        }

        /// <summary>
        /// Gets the element at the specified index in this list.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get.</param>
        /// <value>The element at the specified index in this list.</value>
        public Expression this[Int32 index]
        {
            get
            {
                return this.ElementAt(index);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in this list.
        /// </summary>
        /// <value>The number of elements contained in this list.</value>
        public Int32 Length
        {
            get
            {
                return this.Count();
            }
        }

        /// <summary>
        /// Gets a value that indicates whether this list is empty.
        /// </summary>
        /// <value><c>true</c> if this sequence expression has no elements; otherwise, <c>false</c>.</value>
        public Boolean IsEmpty
        {
            get
            {
                return this._head == null && this._tail == null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YacqList"/> class.
        /// </summary>
        /// <param name="head">The first element of the list.</param>
        /// <param name="tail">The list of rest elements of the list.</param>
        public YacqList(Expression head, YacqList tail)
        {
            this._head = head;
            this._tail = tail;
        }

        private YacqList()
            : this(null, null)
        {
        }

        /// <summary>
        /// Creates new <see cref="YacqList"/> object which contains specified values.
        /// </summary>
        /// <param name="head">The first element of the list.</param>
        /// <param name="tail">The list of rest elements of the list.</param>
        /// <returns>The <see cref="YacqList"/> object which contains specified values.</returns>
        public static YacqList Create(Expression head, YacqList tail)
        {
            return head == null && tail == null
                ? Empty
                : new YacqList(head, tail);
        }

        /// <summary>
        /// Creates new terminal <see cref="YacqList"/> object which contains specified values.
        /// </summary>
        /// <param name="head">The first element of the list.</param>
        /// <returns>The terminal <see cref="YacqList"/> object which contains specified values.</returns>
        public static YacqList Create(Expression head)
        {
            return new YacqList(head, Empty);
        }

        /// <summary>
        /// Creates new <see cref="YacqList"/> object which contains specified values.
        /// </summary>
        /// <param name="elements">The elements of the list.</param>
        /// <returns>The <see cref="YacqList"/> object which contains specified values.</returns>
        public static YacqList Create(IEnumerable<Expression> elements)
        {
            return elements != null && elements.Any()
                ? elements
                      .Reverse()
                      .ToArray()
                      .Let(a => a.Skip(1).Aggregate(Create(a.First()), (l, e) => Create(e, l)))
                : Empty;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{Expression}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Expression> GetEnumerator()
        {
            return EnumerableEx.Generate(this, l => !l.IsEmpty, l => l.Tail, l => l.Head)
                .GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
