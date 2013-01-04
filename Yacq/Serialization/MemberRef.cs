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
using System.Reflection;
using System.Runtime.Serialization;
using Parseq;

namespace XSpect.Yacq.Serialization
{
    /// <summary>
    /// Indicades an reference of <see cref="MemberInfo"/> for serialization.
    /// </summary>
    [DataContract(Name = "Member", IsReference = true)]
    [KnownType(typeof(EventRef))]
    [KnownType(typeof(FieldRef))]
    [KnownType(typeof(MethodRef))]
    [KnownType(typeof(PropertyRef))]
    public abstract partial class MemberRef
    {
        internal const BindingFlags Binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Lazy<MemberDescriptor> _descriptor;

        /// <summary>
        /// Gets or sets the declaring type of this member reference.
        /// </summary>
        /// <value>The declaring type of this member reference, or <c>null</c> if the type is <see cref="Object"/>.</value>
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of referring member.
        /// </summary>
        /// <value>The name of referring member.</value>
        [DataMember(Order = 1, EmitDefaultValue = false)]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the signature of referring member.
        /// </summary>
        /// <value>The signature of referring member, or <c>null</c> if the member has no detailed signature.</value>
        [DataMember(Order = 2, EmitDefaultValue = false)]
        public String Signature
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberRef"/> class.
        /// </summary>
        protected MemberRef()
        {
            MemberDescriptor descriptor;
            this._descriptor = new Lazy<MemberDescriptor>(() =>
                MemberDescriptor.Parser(this.Signature.AsStream())
                    .TryGetValue(out descriptor)
                    .Let(_ => descriptor)
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberRef"/> class.
        /// </summary>
        /// <param name="type">The declaring type of this member reference, or <c>null</c> if the type is <see cref="Object"/>.</param>
        /// <param name="name">The name of referring member.</param>
        /// <param name="signature">The signature of referring member, or <c>null</c> if the member has no detailed signature.</param>
        protected MemberRef(TypeRef type, String name, String signature = null)
            : this()
        {
            this.Type = type ?? new TypeRef();
            this.Name = name ?? "";
            this.Signature = signature ?? "";
        }

        /// <summary>
        /// Returns the member reference which refers specified member.
        /// </summary>
        /// <param name="member">The member to refer.</param>
        /// <returns>The member reference which refers specified member.</returns>
        public static MemberRef Serialize(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    return MethodRef.Serialize((MethodBase) member);
                case MemberTypes.Event:
                    return EventRef.Serialize((EventInfo) member);
                case MemberTypes.Field:
                    return FieldRef.Serialize((FieldInfo) member);
                case MemberTypes.Property:
                    return PropertyRef.Serialize((PropertyInfo) member);
                default:
                    throw new ArgumentOutOfRangeException("member.MemberType");
            }
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
                ?? this.Name;
        }

        /// <summary>
        /// Returns an object to describe this member reference.
        /// </summary>
        /// <returns>An object to describe this member reference.</returns>
        protected virtual MemberDescriptor GetDescriptor()
        {
            return this._descriptor.Value;
        }

        /// <summary>
        /// Returns an object to describe this member reference.
        /// </summary>
        /// <returns>An object to describe this member reference.</returns>
        public MemberDescriptor Describe()
        {
            return this.GetDescriptor();
        }

        /// <summary>
        /// Dereferences this member reference.
        /// </summary>
        /// <returns>The <see cref="MemberInfo"/> which is referred by this member reference.</returns>
        public MemberInfo Deserialize()
        {
            if (this is MethodRef)
            {
                return ((MethodRef) this).Deserialize();
            }
            else if (this is EventRef)
            {
                return ((EventRef) this).Deserialize();
            }
            else if (this is FieldRef)
            {
                return ((FieldRef) this).Deserialize();
            }
            else if (this is PropertyRef)
            {
                return ((PropertyRef) this).Deserialize();
            }
            else
            {
                throw new ArgumentOutOfRangeException("this");
            }
        }
    }

#if !SILVERLIGHT
    [Serializable()]
    partial class MemberRef
        : ISerializable
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="AssemblyRef"/> class that has the given serialization information and context.
        /// </summary>
        /// <param name="info">The data needed to serialize or deserialize an object. </param>
        /// <param name="context">The source and destination of a given serialized stream. </param>
        protected MemberRef(SerializationInfo info, StreamingContext context)
            : this(
                  (TypeRef) info.GetValue("Type", typeof(TypeRef)),
                  info.GetString("Name")
              )
        {
        }

        /// <summary>
        /// Populates a serialization information object with the data needed to serialize the <see cref="MemberRef"/>.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> that holds the serialized data associated with the <see cref="MemberRef"/>.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Type", this.Type, typeof(TypeRef));
            info.AddValue("Name", this.Name);
        }
    }
#endif
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
