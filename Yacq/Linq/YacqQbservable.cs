﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Reactive.Linq;

namespace XSpect.Yacq.Linq
{
    /// <summary>
    /// Represents an <see cref="IQbservable" /> which is enabled querying with YACQ code strings.
    /// </summary>
    public partial class YacqQbservable
        : IQbservable
    {
        private readonly IQbservable _source;

        /// <summary>
        /// Gets the expression tree that is associated with the instance of <see cref="IQbservable"/>.
        /// </summary>
        /// <value>The expression tree that is associated with the instance of <see cref="IQbservable"/>.</value>
        public virtual Expression Expression
        {
            get
            {
                return this._source.Expression;
            }
        }

        /// <summary>
        /// Gets the type of the element(s) that are returned when the expression tree associated with this instance of <see cref="IQbservable"/> is executed.
        /// </summary>
        /// <value>The type of the element(s) that are returned when the expression tree associated with this instance of <see cref="IQbservable"/> is executed.</value>
        public virtual Type ElementType
        {
            get
            {
                return this._source.ElementType;
            }
        }

        /// <summary>
        /// Gets the query provider that is associated with this data source.
        /// </summary>
        /// <value>The query provider that is associated with this data source.</value>
        public virtual IQbservableProvider Provider
        {
            get
            {
                return this._source.Provider;
            }
        }

        /// <summary>
        /// Gets the additional <see cref="SymbolTable"/> for resolve symbols.
        /// </summary>
        /// <value>The additional <see cref="SymbolTable"/> for resolve symbols.</value>
        public SymbolTable Symbols
        {
            get;
            private set;
        }

        internal YacqQbservable(SymbolTable symbols, IQbservable source)
        {
            this.Symbols = symbols;
            this._source = source;
        }
    }

    /// <summary>
    /// Represents an <see cref="IQbservable{TSource}" /> which is enabled querying with YACQ code strings.
    /// </summary>
    /// <typeparam name="TSource">The type of element in the source sequence.</typeparam>
    public partial class YacqQbservable<TSource>
        : YacqQbservable,
          IQbservable<TSource>
    {
        private readonly IQbservable<TSource> _source;

        /// <summary>
        /// Notifies the provider that an observer is to receive notifications.
        /// </summary>
        /// <param name="observer">The object that is to receive notifications.</param>
        /// <returns>The <paramref name="observer"/>'s interface that enables resources to be disposed.</returns>
        public IDisposable Subscribe(IObserver<TSource> observer)
        {
            return this._source.Subscribe(observer);
        }

        internal YacqQbservable(SymbolTable symbols, IQbservable<TSource> source)
            : base(symbols, source)
        {
            this._source = source;
        }
    }
}