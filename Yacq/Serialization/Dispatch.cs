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

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public TypeRef[] TypeArguments
        {
            get;
            set;
        }

        [DataMember(Order = 4, EmitDefaultValue = false)]
        public Node[] Arguments
        {
            get;
            set;
        }

        public override Expression Deserialize()
        {
            return YacqExpression.Dispatch(
                this.DispatchType,
                this.Left.Null(n => Deserialize()),
                this.Name,
                this.TypeArguments.Null(_ => _.Select(t => t.Deserialize())),
                this.Arguments.Null(_ => _.Select(n => n.Deserialize()))
            );
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
                TypeArguments = expression.TypeArguments.Any()
                    ? expression.TypeArguments.Select(TypeRef.Serialize).ToArray()
                    : null,
                Arguments = expression.Arguments.Any()
                    ? expression.Arguments.Select(Serialize).ToArray()
                    : null,
            };
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
