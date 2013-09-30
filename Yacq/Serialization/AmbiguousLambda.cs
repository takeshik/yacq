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
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal class AmbiguousLambda
        : YacqNode
    {
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef ReturnType
        {
            get;
            set;
        }

        [DataMember(Order = 1, Name = "Parameters", EmitDefaultValue = false)]
        private AmbiguousParameter[] _Parameters
        {
            get;
            set;
        }

        public AmbiguousParameter[] Parameters
        {
            get
            {
                return this._Parameters ?? Arrays.Empty<AmbiguousParameter>();
            }
            set
            {
                this._Parameters = value != null && value.Any()
                    ? value
                    : null;
            }
        }

        [DataMember(Order = 2, Name = "Bodies", EmitDefaultValue = false)]
        private Node[] _Bodies
        {
            get;
            set;
        }

        public Node[] Bodies
        {
            get
            {
                return this._Bodies ?? Arrays.Empty<Node>();
            }
            set
            {
                this._Bodies = value != null && value.Any()
                    ? value
                    : null;
            }
        }

        public override Expression Deserialize()
        {
            return YacqExpression.AmbiguousLambda(
                this.ReturnType.Null(t => t.Deserialize()),
                this.Bodies.Select(n => n.Deserialize()),
                this.Parameters.SelectAll(p => p.Deserialize<AmbiguousParameterExpression>())
            );
        }

        public override String ToString()
        {
            return (this.Parameters.Length != 1
                ? "(" + this.Parameters.Stringify(", ") + ")"
                : this.Parameters[0].ToString()
            ) + " => " + (this.Bodies.Length != 1
                ? "{ " + this.Bodies.Stringify("; ") + " }"
                : this.Bodies[0].ToString()
            );
        }
    }

    partial class Node
    {
        internal static AmbiguousLambda AmbiguousLambda(AmbiguousLambdaExpression expression)
        {
            return new AmbiguousLambda()
            {
                ReturnType = expression.ReturnType.Null(t => TypeRef.Serialize(t)),
                Parameters = expression.Parameters.Select(AmbiguousParameter).ToArray(),
                Bodies = expression.Bodies.Select(Serialize).ToArray(),
            }.Apply(n => n.TypeHint = expression.TryType().Null(t => TypeRef.Serialize(t)));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
