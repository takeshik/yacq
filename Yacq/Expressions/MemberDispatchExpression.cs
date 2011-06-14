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
    public class MemberDispatchExpression
        : YacqExpression
    {
        public Expression Instance
        {
            get;
            private set;
        }

        public ReadOnlyCollection<MemberInfo> Candidates
        {
            get;
            private set;
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get;
            private set;
        }

        internal MemberDispatchExpression(
            Expression instance,
            IList<MemberInfo> candidates,
            IList<Expression> arguments
        )
        {
            if (candidates == null || !candidates.Any())
            {
                throw new ArgumentException("candidates");
            }
            this.Instance = instance;
            this.Candidates = new ReadOnlyCollection<MemberInfo>(candidates);
            this.Arguments = new ReadOnlyCollection<Expression>(arguments);
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Dispatcher.DispatchMember(
                this.Instance.Null(_ => _.Reduce(symbols)),
                this.Candidates,
                this.Arguments.Select(e => e.Reduce(symbols)).ToArray()
            );
        }
    }

    partial class YacqExpression
    {
        public static MemberDispatchExpression MemberDispatch(
            Expression instance,
            IEnumerable<MemberInfo> candidates,
            params Expression[] arguments
        )
        {
            return new MemberDispatchExpression(
                instance,
                (candidates ?? Enumerable.Empty<MemberInfo>()).ToArray(),
                arguments
            );
        }

        public static MemberDispatchExpression MemberDispatch(
            Expression instance,
            IEnumerable<MemberInfo> candidates,
            IEnumerable<Expression> arguments
        )
        {
            return MemberDispatch(instance, candidates, arguments.ToArray());
        }

        public static MemberDispatchExpression MemberDispatch(
            IEnumerable<MemberInfo> candidates,
            params Expression[] arguments
        )
        {
            return MemberDispatch(null, candidates, arguments);
        }

        public static MemberDispatchExpression MemberDispatch(
            IEnumerable<MemberInfo> candidates,
            IEnumerable<Expression> arguments
        )
        {
            return MemberDispatch(candidates, arguments.ToArray());
        }
    }
}
