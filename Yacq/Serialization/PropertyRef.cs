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
    /// Indicades an reference of <see cref="PropertyInfo"/> for serialization.
    /// </summary>
    [DataContract(Name = "Property")]
    internal partial class PropertyRef
        : MemberRef
    {
        private static readonly Dictionary<PropertyRef, PropertyInfo> _cache
            = new Dictionary<PropertyRef, PropertyInfo>();

        private static readonly Dictionary<PropertyInfo, PropertyRef> _reverseCache
            = new Dictionary<PropertyInfo, PropertyRef>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRef"/> class.
        /// </summary>
        public PropertyRef()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyRef"/> class.
        /// </summary>
        /// <param name="type">The declaring type of this property reference, or <c>null</c> if the type is <see cref="Object"/>.</param>
        /// <param name="name">The name of referring property.</param>
        public PropertyRef(TypeRef type, String name)
            : base(type, name)
        {
        }

        /// <summary>
        /// Returns the property reference which refers specified property.
        /// </summary>
        /// <param name="property">The property to refer.</param>
        /// <returns>The property reference which refers specified property.</returns>
        public static PropertyRef Serialize(PropertyInfo property)
        {
            return _reverseCache.TryGetValue(property)
                ?? new PropertyRef()
                   {
                       Type = TypeRef.Serialize(property.ReflectedType),
                       Name = property.Name,
                   }.Apply(p => _reverseCache.Add(property, p));
        }

        /// <summary>
        /// Dereferences this property reference.
        /// </summary>
        /// <returns>The <see cref="PropertyInfo"/> which is referred by this property reference.</returns>
        public new PropertyInfo Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? this.Type.Deserialize()
                       .GetProperty(this.Name, Binding)
                       .Apply(p => _cache.Add(this, p));
        }
    }

#if !SILVERLIGHT
    [Serializable()]
    partial class PropertyRef
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="PropertyRef"/> class that has the given serialization information and context.
        /// </summary>
        /// <param name="info">The data needed to serialize or deserialize an object. </param>
        /// <param name="context">The source and destination of a given serialized stream. </param>
        protected PropertyRef(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
#endif
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
