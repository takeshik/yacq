// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq
{
    /// <summary>
    /// Indicates that a method or field is exported as symbols with specified <see cref="SymbolEntry"/> keys.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class YacqSymbolAttribute
        : Attribute
    {
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
        /// Initializes a new instance of <see cref="YacqSymbolAttribute"/> class.
        /// </summary>
        /// <param name="dispatchType">The target <see cref="Expressions.DispatchTypes"/> of this symbol.</param>
        /// <param name="leftType">The target of this symbol.</param>
        /// <param name="name">The name of this symbol.</param>
        public YacqSymbolAttribute(DispatchTypes dispatchType, Type leftType, String name)
        {
            this.DispatchType = dispatchType;
            this.LeftType = leftType;
            this.Name = name;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="YacqSymbolAttribute"/> class.
        /// </summary>
        /// <param name="dispatchType">The target <see cref="Expressions.DispatchTypes"/> of this symbol.</param>
        /// <param name="name">The name of this symbol.</param>
        public YacqSymbolAttribute(DispatchTypes dispatchType, String name)
            : this(dispatchType, null, name)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="YacqSymbolAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of this symbol as a literal.</param>
        public YacqSymbolAttribute(String name)
            : this(DispatchTypes.Member | DispatchTypes.Literal, name)
        {
        }
    }
}