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
    public class MethodDispatchExpression
        : YacqExpression
    {
        public Expression Instance
        {
            get;
            private set;
        }

        public ReadOnlyCollection<MethodInfo> Candidates
        {
            get;
            private set;
        }

        public ReadOnlyCollection<Type> TypeArguments
        {
            get;
            private set;
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get;
            private set;
        }

        internal MethodDispatchExpression(
            Expression instance,
            IList<MethodInfo> candidates,
            IList<Type> typeArguments,
            IList<Expression> arguments
        )
        {
            if (candidates == null || !candidates.Any())
            {
                throw new ArgumentException("candidates");
            }
            this.Instance = instance;
            this.Candidates = new ReadOnlyCollection<MethodInfo>(candidates);
            this.TypeArguments = new ReadOnlyCollection<Type>(typeArguments);
            this.Arguments = new ReadOnlyCollection<Expression>(arguments);
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Dispatcher.DispatchMethod(
                this.Instance.Null(_ => _.Reduce(symbols)),
                this.Candidates,
                this.TypeArguments,
                this.Arguments.ToArray()
            );
        }
    }

    partial class YacqExpression
    {
        public static MethodDispatchExpression MethodDispatch(
            Expression instance,
            IEnumerable<MethodInfo> candidates,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return new MethodDispatchExpression(
                instance,
                (candidates ?? Enumerable.Empty<MethodInfo>()).ToArray(),
                (typeArguments ?? Enumerable.Empty<Type>()).ToArray(),
                arguments
            );
        }

        public static MethodDispatchExpression MethodDispatch(
            Expression instance,
            IEnumerable<MethodInfo> candidates,
            params Expression[] arguments
        )
        {
            return MethodDispatch(instance, candidates, null, arguments);
        }

        public static MethodDispatchExpression MethodDispatch(
            Expression instance,
            IEnumerable<MethodInfo> candidates,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return MethodDispatch(instance, candidates, typeArguments, arguments.ToArray());
        }

        public static MethodDispatchExpression MethodDispatch(
            Expression instance,
            IEnumerable<MethodInfo> candidates,
            IEnumerable<Expression> arguments
        )
        {
            return MethodDispatch(instance, candidates, null, arguments.ToArray());
        }

        public static MethodDispatchExpression MethodDispatch(
            IEnumerable<MethodInfo> candidates,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return MethodDispatch(null, candidates, typeArguments, arguments);
        }

        public static MethodDispatchExpression MethodDispatch(
            IEnumerable<MethodInfo> candidates,
            params Expression[] arguments
        )
        {
            return MethodDispatch(null, candidates, arguments);
        }

        public static MethodDispatchExpression MethodDispatch(
            IEnumerable<MethodInfo> candidates,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return MethodDispatch(candidates, typeArguments, arguments.ToArray());
        }

        public static MethodDispatchExpression MethodDispatch(
            IEnumerable<MethodInfo> candidates,
            IEnumerable<Expression> arguments
        )
        {
            return MethodDispatch(candidates, arguments.ToArray());
        }
    }
}
