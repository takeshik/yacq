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
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal class MemberInit
        : Node
    {
        [DataMember(Order = 0)]
        public New NewExpression
        {
            get;
            set;
        }

        [DataMember(Order = 1, Name = "Bindings", EmitDefaultValue = false)]
        private MemberBinding[] _Bindings
        {
            get;
            set;
        }

        public MemberBinding[] Bindings
        {
            get
            {
                return this._Bindings ?? Arrays.Empty<MemberBinding>();
            }
            set
            {
                this._Bindings = value != null && value.Any()
                    ? value
                    : null;
            }
        }

        public override Expression Deserialize()
        {
            return Expression.MemberInit(
                this.NewExpression.Deserialize<NewExpression>(),
                this.Bindings.SelectAll(b => b.Deserialize())
            );
        }

        public override String ToString()
        {
            return this.NewExpression
                + " { " + this.Bindings.Stringify(", ") + " }";
        }
    }

    partial class Node
    {
        internal static MemberInit MemberInit(MemberInitExpression expression)
        {
            return new MemberInit()
            {
                NewExpression = New(expression.NewExpression),
                Bindings = expression.Bindings.Select(MemberBinding.Serialize).ToArray(),
            }.Apply(n => n.Type = TypeRef.Serialize(expression.Type));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
