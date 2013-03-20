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
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal class Dispatch
        : YacqNode
    {
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public DispatchTypes DispatchType
        {
            get;
            set;
        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public Node Left
        {
            get;
            set;
        }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public String Name
        {
            get;
            set;
        }

        [DataMember(Order = 3, Name = "TypeArguments", EmitDefaultValue = false)]
        private TypeRef[] _TypeArguments
        {
            get;
            set;
        }

        public TypeRef[] TypeArguments
        {
            get
            {
                return this._TypeArguments ?? Arrays.Empty<TypeRef>();
            }
            set
            {
                this._TypeArguments = value == null || value.IsEmpty()
                    ? null
                    : value;
            }
        }

        [DataMember(Order = 4, Name = "Arguments", EmitDefaultValue = false)]
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
                this._Arguments = value == null || value.IsEmpty()
                    ? null
                    : value;
            }
        }

        public override Expression Deserialize()
        {
            return YacqExpression.Dispatch(
                this.DispatchType,
                this.Left.Null(n => Deserialize()),
                this.Name,
                this.TypeArguments.SelectAll(t => t.Deserialize()),
                this.Arguments.SelectAll(n => n.Deserialize())
            );
        }

        public override String ToString()
        {
            switch (this.DispatchType & DispatchTypes.TargetMask)
            {
                case DispatchTypes.Member:
                    return this.Arguments.Any()
                        ? this.Left + "[" + String.Join(", ", this.Arguments.SelectAll(e => e.ToString())) + "]"
                        : (this.Left != null ? this.Left + "." : "") + this.Name;
                case DispatchTypes.Method:
                    return (this.Left != null ? this.Left + "." : "")
                        + this.Name
                        + (this.TypeArguments.Any() ? "<" + String.Join(", ", this.TypeArguments.SelectAll(t => t.Name)) + ">" : "")
                        + "(" + String.Join(", ", this.Arguments.Select(e => e.ToString())) + ")";
                case DispatchTypes.Constructor:
                    return this.Left + "(" + String.Join(", ", this.Arguments.SelectAll(e => e.ToString())) + ")";
                default:
                    return "Dispatch(?)";
            }
         }
    }

    partial class Node
    {
        internal static Dispatch Dispatch(DispatchExpression expression)
        {
            return new Dispatch()
            {
                DispatchType = expression.DispatchType,
                Left = expression.Left.Null(e => Serialize(e)),
                Name = expression.Name,
                TypeArguments = expression.TypeArguments.Select(TypeRef.Serialize).ToArray(),
                Arguments = expression.Arguments.Select(Serialize).ToArray(),
            }.Apply(n => n.TypeHint = expression.TryType().Null(t => TypeRef.Serialize(t)));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
