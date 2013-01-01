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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Parseq;

namespace XSpect.Yacq.Serialization
{
    [DataContract(Name = "Method")]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal partial class MethodRef
        : MemberRef
    {
        private static readonly Dictionary<MethodRef, MethodBase> _cache
            = new Dictionary<MethodRef, MethodBase>();

        private static readonly Dictionary<MethodBase, MethodRef> _reverseCache
            = new Dictionary<MethodBase, MethodRef>();

        private readonly Lazy<MethodDescriptor> _descriptor;

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef[] TypeArgs
        {
            get;
            set;
        }

        public MethodRef()
        {
            MethodDescriptor descriptor;
            this._descriptor = new Lazy<MethodDescriptor>(() =>
                MethodDescriptor.Parser(this.Signature.AsStream())
                    .TryGetValue(out descriptor)
                    .Let(_ => descriptor)
            );
        }

        public static MethodRef Serialize(MethodBase method)
        {
            return _reverseCache.TryGetValue(method)
                ?? new MethodRef()
                    {
                        Type = TypeRef.Serialize(method.ReflectedType),
                        Name = method.Name != ".ctor"
                            ? method.Name
                            : null,
                        Signature = method.ToString(),
                        TypeArgs = method.IsGenericMethod && !method.IsGenericMethodDefinition
                            ? method.GetGenericArguments().SelectAll(TypeRef.Serialize)
                            : null,
                    }.Apply(m => _reverseCache.Add(method, m));
        }

        public new MethodBase Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? (this.Name != null
                       ? (MethodBase) this.Type.Deserialize()
                             .GetMethods(Binding)
                             .Select(m => this.Name == m.Name && this.Signature == m.ToString()
                                 ? m
                                 : this.TypeArgs != null &&
                                   this.TypeArgs.Length == m.GetGenericArguments().Length
                                       ? m.MakeGenericMethod(this.TypeArgs.SelectAll(t => t.Deserialize()))
                                             .If(mg => this.Signature != mg.ToString(), default(MethodInfo))
                                       : null
                             )
                             .First(m => m != null)
                       : this.Type.Deserialize()
                             .GetConstructors(Binding)
                             .First(c => this.Signature == c.ToString())
                   ).Apply(m => _cache.Add(this, m));
        }

        public override String ToString()
        {
            return this.Describe()
                .Null(d => d.ToString())
                ?? this.Signature;
        }

        protected override MemberDescriptor GetDescriptor()
        {
            return this._descriptor.Value;
        }

        public new MethodDescriptor Describe()
        {
            return (MethodDescriptor) this.GetDescriptor();
        }

        public MethodInfo DeserializeAsMethod()
        {
            return (MethodInfo) this.Deserialize();
        }

        public ConstructorInfo DeserializeAsConstructor()
        {
            return (ConstructorInfo) this.Deserialize();
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
