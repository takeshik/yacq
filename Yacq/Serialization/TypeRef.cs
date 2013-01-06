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
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    /// <summary>
    /// Indicades an reference of <see cref="Type"/> for serialization.
    /// </summary>
    [DataContract(Name = "Type", IsReference = true)]
#if !SILVERLIGHT
    [Serializable()]
#endif
    public partial class TypeRef
    {
        private static readonly Assembly _mscorlib = typeof(Object).Assembly;

        private static readonly Dictionary<TypeRef, Type> _cache
            = new Dictionary<TypeRef, Type>();

        private static readonly Dictionary<Type, TypeRef> _reverseCache
            = new Dictionary<Type, TypeRef>();

#if !SILVERLIGHT
        [NonSerialized()]
#endif
        private TypeDescriptor _descriptor;

        /// <summary>
        /// Gets or sets the belonging assembly of this type reference.
        /// </summary>
        /// <value>The belonging assembly of this type reference, or <c>null</c> if it is mscorlib.</value>
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public AssemblyRef Assembly
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Type.FullName"/>.
        /// </summary>
        /// <value>The value of <see cref="Type.FullName"/>, or <c>null</c> if referring type is <see cref="Object"/>.</value>
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRef"/> class.
        /// </summary>
        public TypeRef()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRef"/> class.
        /// </summary>
        /// <param name="assembly">The belonging assembly of this type reference, or <c>null</c> if it is mscorlib.</param>
        /// <param name="name">The value of <see cref="Type.FullName"/>, or <c>null</c> if referring type is <see cref="Object"/>.</param>
        public TypeRef(AssemblyRef assembly, String name)
            : this()
        {
            this.Assembly = assembly;
            this.Name = name;
        }

        /// <summary>
        /// Returns the type reference which refers specified type.
        /// </summary>
        /// <param name="type">The type to refer.</param>
        /// <returns>The type reference which refers specified type.</returns>
        public static TypeRef Serialize(Type type)
        {
            return _reverseCache.TryGetValue(type)
                ?? new TypeRef(
                    type.Assembly != _mscorlib
                        ? AssemblyRef.Serialize(type.Assembly)
                        : null,
                    type != typeof(Object)
                        ? type.FullName
                        : null
                ).Apply(t => _reverseCache.Add(type, t));
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return this.Describe()
                .Null(d => d.ToString())
                ?? this.GetName();
        }

        /// <summary>
        /// Returns the string value of this assembly reference.
        /// </summary>
        /// <returns>The value of <see cref="Type.FullName"/>.</returns>
        public String GetName()
        {
            return this.Name ?? "System.Object";
        }

        /// <summary>
        /// Returns an object to describe this type reference.
        /// </summary>
        /// <returns>An object to describe this type reference.</returns>
        public TypeDescriptor Describe()
        {
            return this._descriptor ?? (
                this._descriptor = TypeDescriptor.ParserAssemblyQualified(this.GetName().AsStream())
                    .TryGetValue(out this._descriptor)
                    .Let(_ => this._descriptor)
            );
        }

        /// <summary>
        /// Dereferences this type reference.
        /// </summary>
        /// <returns>The <see cref="Type"/> which is referred by this type reference.</returns>
        public Type Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? this.Assembly
                       .Null(a => a.Deserialize(), _mscorlib)
                       .GetType(this.GetName())
                       .Apply(t => _cache.Add(this, t));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
