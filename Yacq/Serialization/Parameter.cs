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
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
    internal class Parameter
        : Node
    {
        private static readonly Dictionary<Parameter, ParameterExpression> _cache
            = new Dictionary<Parameter, ParameterExpression>();

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public String Name
        {
            get;
            set;
        }

        public override Expression Deserialize()
        {
            return _cache.ContainsKey(this)
                ? _cache[this]
                : Expression.Parameter(
                      this.Type.Deserialize(),
                      this.Name
                  ).Apply(p => _cache.Add(this, p));
        }
    }

    partial class Node
    {
        private static readonly Dictionary<ParameterExpression, Parameter> _parameterReverseCache
            = new Dictionary<ParameterExpression, Parameter>();

        internal static Parameter Parameter(ParameterExpression expression)
        {
            return _parameterReverseCache.ContainsKey(expression)
                ? _parameterReverseCache[expression]
                : new Parameter()
                  {
                      Type = TypeRef.Serialize(expression.Type),
                      Name = expression.Name,
                  }.Apply(p => _parameterReverseCache.Add(expression, p));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
