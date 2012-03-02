﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
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
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq
{
    /// <summary>
    /// Represents key of symbol,
    /// </summary>
    public struct SymbolEntry
        : IEquatable<SymbolEntry>
    {
        /// <summary>
        /// Represents the key of the missing symbol, the failback code if any symbol is matched.
        /// </summary>
        public static readonly SymbolEntry Missing = default(SymbolEntry);

        /// <summary>
        /// Gets the target <see cref="Expressions.DispatchTypes"/> of this symbol.
        /// </summary>
        /// <value>
        /// The target <see cref="Expressions.DispatchTypes"/> of this symbol.
        /// </value>
        public DispatchTypes DispatchType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the target type of this symbol.
        /// </summary>
        /// <value>
        /// The target type of this symbol.
        /// </value>
        /// <remarks>
        /// If this value is <c>null</c>, this symbol is targeted to non-objective (global) call.
        /// </remarks>
        public Type LeftType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of this symbol.
        /// </summary>
        /// <value>
        /// The name of this symbol.
        /// </value>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SymbolEntry"/>.
        /// </summary>
        /// <param name="dispatchType">The target <see cref="Expressions.DispatchTypes"/> of this symbol.</param>
        /// <param name="leftType">The target of this symbol.</param>
        /// <param name="name">The name of this symbol.</param>
        public SymbolEntry(DispatchTypes dispatchType, Type leftType, String name)
            : this()
        {
            this.DispatchType = dispatchType;
            this.LeftType = leftType;
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SymbolEntry"/>.
        /// </summary>
        /// <param name="dispatchType">The target <see cref="Expressions.DispatchTypes"/> of this symbol.</param>
        /// <param name="name">The name of this symbol.</param>
        public SymbolEntry(DispatchTypes dispatchType, String name)
            : this(dispatchType, null, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="SymbolEntry"/> as a literal.
        /// </summary>
        /// <param name="name">The name of this symbol as a literal.</param>
        public SymbolEntry(String name)
            : this(DispatchTypes.Member | DispatchTypes.Literal, null, name)
        {
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean Equals(Object obj)
        {
            return obj is SymbolEntry && this.Equals((SymbolEntry) obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override Int32 GetHashCode()
        {
            return unchecked(
                (this.DispatchType & DispatchTypes.TargetMask).GetHashCode() ^
                (this.LeftType != null ? this.LeftType.GetHashCode() : 0) ^
                (this.Name != null ? this.Name.GetHashCode() : 0)
            );
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return (this.LeftType
                .Null(t => (t.TryGetGenericTypeDefinition() == typeof(Static<>)
                         ? "[" + t.GetGenericArguments()[0].Name + "]"
                         : t.Name
                ) + ".")
                ?? ""
            ) + (this.DispatchType.HasFlag(DispatchTypes.Method)
                ? "(" + this.Name + ")"
                : this.Name
            );
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the current object is equal to the other parameter; otherwise, <c>false</c>.
        /// </returns>
        public Boolean Equals(SymbolEntry other)
        {
            return
                (this.DispatchType & DispatchTypes.TargetMask) == (other.DispatchType & DispatchTypes.TargetMask) &&
                this.LeftType == other.LeftType &&
                this.Name == other.Name;
        }

        /// <summary>
        /// Determines whether the specified type matches with the target type.
        /// </summary>
        /// <param name="test">The type to test match.</param>
        /// <param name="target">The targe type of match.</param>
        /// <returns><c>true</c> if <paramref name="test"/> matches with <paramref name="target"/>; otherwise, <c>false</c>.</returns>
        public static Boolean TypeMatch(Type test, Type target)
        {
            return
                (test == target || (test != null && test.IsAssignableFrom(target) &&
                (test != typeof(Object) || target.TryGetGenericTypeDefinition() != typeof(Static<>)) || (
                    test.TryGetGenericTypeDefinition() == typeof(Static<>) &&
                    target.TryGetGenericTypeDefinition() == typeof(Static<>) &&
                    test.GetGenericArguments()[0].Let(t =>
                        target.GetGenericArguments()[0].Let(kt =>
                            t == kt || t.IsAssignableFrom(kt)
                        )
                    )
                )));
        }

        /// <summary>
        /// Determines whether <see cref="LeftType"/> matches with the target type.
        /// </summary>
        /// <param name="target">The targe type of match.</param>
        /// <returns><c>true</c> if <see cref="LeftType"/> matches with <paramref name="target"/>; otherwise, <c>false</c>.</returns>
        public Boolean TypeMatch(Type target)
        {
            return TypeMatch(this.LeftType, target);
        }
    }
}