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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract(Name = "Type", IsReference = true)]
    internal class TypeRef
    {
        private static readonly Assembly _mscorlib = typeof(Object).Assembly;

        private static readonly Dictionary<TypeRef, Type> _cache
            = new Dictionary<TypeRef, Type>();

        private static readonly Dictionary<Type, TypeRef> _reverseCache
            = new Dictionary<Type, TypeRef>();

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public AssemblyRef Assembly
        {
            get;
            set;
        }

        [DataMember(Order = 1, EmitDefaultValue = false)]
        public String Name
        {
            get;
            set;
        }

        public static TypeRef Serialize(Type type)
        {
            return _reverseCache.TryGetValue(type)
                ?? new TypeRef()
                   {
                       Assembly = type.Assembly != _mscorlib
                           ? AssemblyRef.Serialize(type.Assembly)
                           : null,
                       Name = type != typeof(Object)
                           ? type.FullName
                           : null,
                   }.Apply(t => _reverseCache.Add(type, t));
        }

        public Type Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? this.Assembly
                       .Null(a => a.Deserialize() ?? _mscorlib)
                       .GetType(this.Name ?? "System.Object")
                       .Apply(t => _cache.Add(this, t));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
