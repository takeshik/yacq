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
    internal class Switch
        : Node
    {
        [DataMember(Order = 0)]
        public Node SwitchValue
        {
            get;
            set;
        }

        [DataMember(Order = 1)]
        public SwitchCase[] Cases
        {
            get;
            set;
        }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public Node DefaultBody
        {
            get;
            set;
        }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public MethodRef Comparison
        {
            get;
            set;
        }

        public override Expression Deserialize()
        {
            return Expression.Switch(
                this.Type.Null(t => t.Deserialize()),
                this.SwitchValue.Deserialize(),
                this.DefaultBody.Null(n => n.Deserialize()),
                this.Comparison.Null(m => m.DeserializeAsMethod()),
                this.Cases.Select(c => c.Deserialize())
            );
        }

        public override String ToString()
        {
            return "switch ("
                + this.SwitchValue
                + ") { "
                + String.Join("; ", this.Cases
                      .SelectAll(c => c.ToString())
                      .EndWith(this.DefaultBody.Null(n => new[]
                      {
                          " default: " + n,
                      }, new String[0]))
                  );
        }
    }

    partial class Node
    {
        internal static Switch Switch(SwitchExpression expression)
        {
            return new Switch()
            {
                Type = expression.Type != expression.Cases[0].Body.Type
                    ? TypeRef.Serialize(expression.Type)
                    : null,
                SwitchValue = Serialize(expression.SwitchValue),
                Cases = expression.Cases.Select(SwitchCase.Serialize).ToArray(),
                DefaultBody = expression.DefaultBody.Null(e => Serialize(e)),
                Comparison = expression.Comparison.Null(m => MethodRef.Serialize(m)),
            };
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
