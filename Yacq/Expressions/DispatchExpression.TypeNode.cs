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
using System.Collections.Generic;
using System.Linq;

namespace XSpect.Yacq.Expressions
{
    partial class DispatchExpression
    {
        private class TypeNode
        {
            private readonly Lazy<IList<TypeNode>> _children;

            internal Type Value
            {
                get;
                private set;
            }

            internal IList<TypeNode> Children
            {
                get
                {
                    return this._children.Value;
                }
            }

            internal TypeNode(Type type)
            {
                this._children = new Lazy<IList<TypeNode>>(() =>
                    this.Value.GetGenericArguments().Select(t => new TypeNode(t)).ToArray()
                );
                this.Value = type;
            }

            internal TDictionary Match<TDictionary>(TDictionary results, TypeNode target)
                where TDictionary : IDictionary<Type, Type>
            {
                if (this.Value.IsGenericParameter)
                {
                    results[this.Value] = target.Value;
                }
                if (this.Children.Count == target.Children.Count)
                {
                    this.Children
                        .Zip(target.Children, (n, t) => n.Value.IsAppropriate(t.Value)
                            ? n.Match(results, n.Children.Count == t.Children.Count
                                  ? t
                                  : new TypeNode(t.Value.GetCorrespondingType(n.Value)))
                            : results
                        )
                        .ToArray();
                }
                return results;
            }

            internal Dictionary<Type, Type> Match(TypeNode other)
            {
                return this.Match(new Dictionary<Type, Type>(), other);
            }

            internal TDictionary Match<TDictionary>(TDictionary results, Type target)
                where TDictionary : IDictionary<Type, Type>
            {
                return this.Match(results, new TypeNode(target));
            }

            internal Dictionary<Type, Type> Match(Type target)
            {
                return this.Match(new Dictionary<Type, Type>(), target);
            }
        }
    }
}