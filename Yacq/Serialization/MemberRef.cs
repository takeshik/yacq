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
using System.Reflection;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract(Name = "Member", IsReference = true)]
    [KnownType(typeof(EventRef))]
    [KnownType(typeof(FieldRef))]
    [KnownType(typeof(MethodRef))]
    [KnownType(typeof(PropertyRef))]
    internal abstract class MemberRef
    {
        public const BindingFlags Binding = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public TypeRef Type
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

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public String Signature
        {
            get;
            set;
        }

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
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
