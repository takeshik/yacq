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
    public abstract partial class YacqExpression
        : Expression
    {
        private Boolean _canReduce;

        private Expression _reducedExpression;

        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        public override Boolean CanReduce
        {
            get
            {
                return this._canReduce;
            }
        }

        public override Type Type
        {
            get
            {
                if (this._reducedExpression == null && this.CanReduce)
                {
                    this.Reduce();
                }
                return this._reducedExpression != this
                    ? this._reducedExpression.Type
                    : null;
            }
        }

        public override Expression Reduce()
        {
            return this.Reduce(new SymbolTable());
        }

        protected YacqExpression()
        {
            this._canReduce = true;
        }

        public Expression Reduce(SymbolTable symbols)
        {
            return this.Reduce(symbols, null);
        }

        public Expression Reduce(SymbolTable symbols, Type expectedType)
        {
            if ((this._reducedExpression == null || this._reducedExpression.Type != expectedType) && this.CanReduce)
            {
                this._reducedExpression = this.ReduceImpl(symbols, expectedType);
                if (this._reducedExpression == null || this._reducedExpression == this)
                {
                    this._reducedExpression = this;
                    this._canReduce = false;
                }
            }
            return this._reducedExpression.TryConvert(expectedType);
        }

        protected abstract Expression ReduceImpl(SymbolTable symbols, Type expectedType);

        public virtual Boolean CanReduceAs(Type expectedType)
        {
            return expectedType == null || this.Type.GetConvertibleTypes().Contains(expectedType);
        }
    }
}
