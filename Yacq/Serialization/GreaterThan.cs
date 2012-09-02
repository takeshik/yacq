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
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
    internal class GreaterThan
        : BinaryNode
    {
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public Boolean LiftToNull
        {
            get;
            set;
        }

        public override Expression Deserialize()
        {
            return Expression.GreaterThan(
                this.Left.Deserialize(),
                this.Right.Deserialize(),
                this.LiftToNull,
                this.Method.Null(m => m.DeserializeAsMethod())
            );
        }
    }

    partial class Node
    {
        internal static GreaterThan GreaterThan(BinaryExpression expression)
        {
            return new GreaterThan()
            {
                Left = Serialize(expression.Left),
                Right = Serialize(expression.Right),
                LiftToNull = expression.IsLiftedToNull,
                Method = expression.Method.Null(m => MethodRef.Serialize(m)),
            };
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
