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
    /// <summary>
    /// Indicades an reference of <see cref="MethodBase"/> for serialization.
    /// </summary>
    [DataContract(Name = "Method")]
    internal partial class MethodRef
        : MemberRef
    {
        private static readonly Dictionary<MethodRef, MethodBase> _cache
            = new Dictionary<MethodRef, MethodBase>();

        private static readonly Dictionary<MethodBase, MethodRef> _reverseCache
            = new Dictionary<MethodBase, MethodRef>();

        private readonly Lazy<MethodDescriptor> _descriptor;

        /// <summary>
        /// Gets or sets the generic type arguments of this type reference.
        /// </summary>
        /// <value>The generic type arguments of this type reference.</value>
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef[] TypeArgs
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodRef"/> class.
        /// </summary>
        public MethodRef()
        {
            MethodDescriptor descriptor;
            this._descriptor = new Lazy<MethodDescriptor>(() =>
                MethodDescriptor.Parser(this.Signature.AsStream())
                    .TryGetValue(out descriptor)
                    .Let(_ => descriptor)
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MethodRef"/> class.
        /// </summary>
        /// <param name="type">The declaring type of this method reference, or <c>null</c> if the type is <see cref="Object"/>.</param>
        /// <param name="name">The name of referring method.</param>
        /// <param name="signature">The signature (<see cref="Object.ToString()"/>) of referring method.</param>
        /// <param name="typeArgs">The generic type arguments of this type reference.</param>
        public MethodRef(TypeRef type, String name, String signature, TypeRef[] typeArgs)
            : this()
        {
            this.Type = type;
            this.Name = name;
            this.Signature = signature;
            this.TypeArgs = typeArgs;
        }

        /// <summary>
        /// Returns the method reference which refers specified method.
        /// </summary>
        /// <param name="method">The method to refer.</param>
        /// <returns>The method reference which refers specified method.</returns>
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
                ?? this.Signature;
        }

        /// <summary>
        /// Returns an object to describe this method reference.
        /// </summary>
        /// <returns>An object to describe this method reference.</returns>
        protected override MemberDescriptor GetDescriptor()
        {
            return this._descriptor.Value;
        }

        /// <summary>
        /// Returns an object to describe this member reference.
        /// </summary>
        /// <returns>An object to describe this member reference.</returns>
        public new MethodDescriptor Describe()
        {
            return (MethodDescriptor) this.GetDescriptor();
        }

        /// <summary>
        /// Dereferences this method reference.
        /// </summary>
        /// <returns>The <see cref="MethodBase"/> which is referred by this method reference.</returns>
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

        /// <summary>
        /// Dereferences this method reference.
        /// </summary>
        /// <returns>The <see cref="MethodInfo"/> which is referred by this method reference.</returns>
        public MethodInfo DeserializeAsMethod()
        {
            return (MethodInfo) this.Deserialize();
        }

        /// <summary>
        /// Dereferences this method reference.
        /// </summary>
        /// <returns>The <see cref="ConstructorInfo"/> which is referred by this method reference.</returns>
        public ConstructorInfo DeserializeAsConstructor()
        {
            return (ConstructorInfo) this.Deserialize();
        }
    }

#if !SILVERLIGHT
    [Serializable()]
    partial class MethodRef
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="MethodRef"/> class that has the given serialization information and context.
        /// </summary>
        /// <param name="info">The data needed to serialize or deserialize an object. </param>
        /// <param name="context">The source and destination of a given serialized stream. </param>
        protected MethodRef(SerializationInfo info, StreamingContext context)
            : this(
                  (TypeRef) info.GetValue("Type", typeof(TypeRef)),
                  info.GetString("Name"),
                  info.GetString("Signature"),
                  (TypeRef[]) info.GetValue("TypeArgs", typeof(TypeRef[]))
              )
        {
        }

        /// <summary>
        /// Populates a serialization information object with the data needed to serialize the <see cref="MethodRef"/>.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> that holds the serialized data associated with the <see cref="MethodRef"/>.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Signature", this.Signature);
            info.AddValue("TypeArgs", this.TypeArgs, typeof(TypeRef[]));
        }
    }
#endif
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
