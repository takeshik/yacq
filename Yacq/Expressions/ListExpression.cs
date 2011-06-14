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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace XSpect.Yacq.Expressions
{
    public class ListExpression
        : YacqExpression
    {
        public ReadOnlyCollection<Expression> Elements
        {
            get;
            private set;
        }

        public Expression this[Int32 index]
        {
            get
            {
                return this.Elements[index];
            }
        }

        internal ListExpression(IList<Expression> elements)
        {
            this.Elements = new ReadOnlyCollection<Expression>(elements);
        }

        public override String ToString()
        {
            return "(" + String.Join(" ", this.Elements.Select(e => e.ToString())) + ")";
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            if(!this.Elements.Any())
            {
                return expectedType != null && expectedType.IsValueType
                    ? (Expression) Default(expectedType)
                    : Constant(null);
            }
            dynamic value = this.Elements.First().ReduceOrResolve(symbols);
            if (value is LambdaExpression || value is InvocationExpression)
            {
                return Invoke(value, this.Elements.Skip(1).Select(e => e.Reduce(symbols)));
            }
            if (value is TypeCandidateExpression)
            {
                return Dispatcher.DispatchMethod(
                    null,
                    ((TypeCandidateExpression) value).Candidates
                        .SelectMany(t => t.GetConstructors()),
                    null,
                    this.Elements.Skip(1).Select(e => e.Reduce(symbols)).ToArray()
                ) ?? (this.Elements.Count == 2
                    ? Convert(this[1], ((TypeCandidateExpression) value).Candidates.Single())
                    : null
                );
            }
            if (value is Expression)
            {
                return (Expression) value;
            }
            if (value is Transformer)
            {
                return ((Transformer) value)(this, symbols);
            }
            return this;
        }
    }

    partial class YacqExpression
    {
        public static ListExpression List(params Expression[] elements)
        {
            return new ListExpression(elements);
        }

        public static ListExpression List(IEnumerable<Expression> elements)
        {
            return List(elements.ToArray());
        }
    }
}
