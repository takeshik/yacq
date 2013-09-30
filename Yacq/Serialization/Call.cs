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
    internal class Call
        : Node
    {
        [DataMember(Order = 0)]
        public MethodRef Method
        {
            get;
            set;
        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public Node Object
        {
            get;
            set;
        }

        [DataMember(Order = 2, Name = "Arguments", EmitDefaultValue = false)]
        private Node[] _Arguments
        {
            get;
            set;
        }

        public Node[] Arguments
        {
            get
            {
                return this._Arguments ?? Arrays.Empty<Node>();
            }
            set
            {
                this._Arguments = value != null && value.Any()
                    ? value
                    : null;
            }
        }

        public Boolean IsExtension
        {
            get;
            set;
        }

        public override Expression Deserialize()
        {
            return Expression.Call(
                this.Object.Null(n => n.Deserialize()),
                this.Method.DeserializeAsMethod(),
                this.Arguments.SelectAll(n => n.Deserialize())
            );
        }

        public override String ToString()
        {
            return (this.Object.Null(n => n.ToString())
                ?? (this.IsExtension
                       ? this.Arguments[0].ToString()
                       : this.Method.Type.Describe().Name.ToString()
                   )
            )
                + "." + this.Method.Name
                + this.Method.TypeArgs.Let(ts => ts != null && ts.Any()
                      ? "<" + ts.Stringify(t => t.Describe(), ", ") + ">"
                      : ""
                  )
                + "("
                + (this.IsExtension
                      ? this.Arguments.Skip(1).ToArray()
                      : this.Arguments
                  ).Stringify(", ")
                + ")";
        }
    }

    partial class Node
    {
        internal static Call Call(MethodCallExpression expression)
        {
            return new Call()
            {
                Method = MethodRef.Serialize(expression.Method),
                Object = expression.Object.Null(e => Serialize(e)),
                Arguments = expression.Arguments.Select(Serialize).ToArray(),
                IsExtension = expression.Method.IsExtensionMethod(),
            }.Apply(n => n.Type = TypeRef.Serialize(expression.Type));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
