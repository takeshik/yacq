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

namespace XSpect.Yacq.Serialization
{
    /// <summary>
    /// Indicades an reference of <see cref="FieldRef"/> for serialization.
    /// </summary>
    [DataContract(Name = "Field")]
#if !SILVERLIGHT
    [Serializable()]
#endif
    public class FieldRef
        : MemberRef
    {
        private static readonly Dictionary<FieldRef, FieldInfo> _cache
            = new Dictionary<FieldRef, FieldInfo>();

        private static readonly Dictionary<FieldInfo, FieldRef> _reverseCache
            = new Dictionary<FieldInfo, FieldRef>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldRef"/> class.
        /// </summary>
        public FieldRef()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldRef"/> class.
        /// </summary>
        /// <param name="type">The declaring type of this field reference, or <c>null</c> if the type is <see cref="Object"/>.</param>
        /// <param name="name">The name of referring field.</param>
        public FieldRef(TypeRef type, String name)
            : base(type, name)
        {
        }

        /// <summary>
        /// Returns the field reference which refers specified field.
        /// </summary>
        /// <param name="field">The field to refer.</param>
        /// <returns>The field reference which refers specified field.</returns>
        public static FieldRef Serialize(FieldInfo field)
        {
            return _reverseCache.TryGetValue(field)
                ?? new FieldRef(
                       TypeRef.Serialize(field.ReflectedType),
                       field.Name
                   ).Apply(f => _reverseCache.Add(field, f));
        }

        /// <summary>
        /// Dereferences this field reference.
        /// </summary>
        /// <returns>The <see cref="FieldInfo"/> which is referred by this field reference.</returns>
        public new FieldInfo Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? this.Type.Deserialize()
                       .GetField(this.Name, Binding)
                       .Apply(f => _cache.Add(this, f));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
