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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq
{
    public partial class YacqQueryable
        : IQueryable
    {
        private readonly IQueryable _source;

        public virtual Expression Expression
        {
            get
            {
                return this._source.Expression;
            }
        }

        public virtual Type ElementType
        {
            get
            {
                return this._source.ElementType;
            }
        }

        public virtual IQueryProvider Provider
        {
            get
            {
                return this._source.Provider;
            }
        }

        public SymbolTable Symbols
        {
            get;
            private set;
        }

        public YacqQueryable(SymbolTable symbols, IQueryable source)
        {
            this.Symbols = symbols;
            this._source = source;
        }

        public IEnumerator GetEnumerator()
        {
            return this._source.GetEnumerator();
        }
    }

    public partial class YacqQueryable<TSource>
        : YacqQueryable,
          IQueryable<TSource>
    {
        private readonly IQueryable<TSource> _source;

        public new IEnumerator<TSource> GetEnumerator()
        {
            return this._source.GetEnumerator();
        }

        public YacqQueryable(SymbolTable symbols, IQueryable<TSource> source)
            : base(symbols, source)
        {
            this._source = source;
        }
    }

    public partial class YacqOrderedQueryable
        : YacqQueryable,
          IOrderedQueryable
    {
        private readonly IOrderedQueryable _source;

        public YacqOrderedQueryable(SymbolTable symbols, IOrderedQueryable source)
            : base(symbols, source)
        {
            this._source = source;
        }
    }

    public partial class YacqOrderedQueryable<TSource>
        : YacqQueryable<TSource>,
          IOrderedQueryable<TSource>
    {
        private readonly IOrderedQueryable<TSource> _source;

        public YacqOrderedQueryable(SymbolTable symbols, IOrderedQueryable<TSource> source)
            : base(symbols, source)
        {
            this._source = source;
        }
    }
}