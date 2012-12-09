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
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract()]
    [KnownType(typeof(Add))]
    [KnownType(typeof(AddAssign))]
    [KnownType(typeof(AddAssignChecked))]
    [KnownType(typeof(AddChecked))]
    [KnownType(typeof(And))]
    [KnownType(typeof(AndAlso))]
    [KnownType(typeof(AndAssign))]
    [KnownType(typeof(Assign))]
    [KnownType(typeof(Coalesce))]
    [KnownType(typeof(Divide))]
    [KnownType(typeof(DivideAssign))]
    [KnownType(typeof(Equal))]
    [KnownType(typeof(ExclusiveOr))]
    [KnownType(typeof(ExclusiveOrAssign))]
    [KnownType(typeof(GreaterThan))]
    [KnownType(typeof(GreaterThanOrEqual))]
    [KnownType(typeof(LeftShift))]
    [KnownType(typeof(LeftShiftAssign))]
    [KnownType(typeof(LessThan))]
    [KnownType(typeof(LessThanOrEqual))]
    [KnownType(typeof(Modulo))]
    [KnownType(typeof(ModuloAssign))]
    [KnownType(typeof(Multiply))]
    [KnownType(typeof(MultiplyAssign))]
    [KnownType(typeof(MultiplyAssignChecked))]
    [KnownType(typeof(MultiplyChecked))]
    [KnownType(typeof(NotEqual))]
    [KnownType(typeof(Or))]
    [KnownType(typeof(OrAssign))]
    [KnownType(typeof(OrElse))]
    [KnownType(typeof(Power))]
    [KnownType(typeof(PowerAssign))]
    [KnownType(typeof(ReferenceEqual))]
    [KnownType(typeof(ReferenceNotEqual))]
    [KnownType(typeof(RightShift))]
    [KnownType(typeof(RightShiftAssign))]
    [KnownType(typeof(Subtract))]
    [KnownType(typeof(SubtractAssign))]
    [KnownType(typeof(SubtractAssignChecked))]
    [KnownType(typeof(SubtractChecked))]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal abstract class BinaryNode
        : Node
    {
        [DataMember(Order = 0)]
        public Node Left
        {
            get;
            set;
        }

        [DataMember(Order = 1)]
        public Node Right
        {
            get;
            set;
        }

        [DataMember(Order = 2, EmitDefaultValue = false)]
        public MethodRef Method
        {
            get;
            set;
        }

        [DataMember(Order = 3, EmitDefaultValue = false)]
        public Lambda Conversion
        {
            get;
            set;
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
