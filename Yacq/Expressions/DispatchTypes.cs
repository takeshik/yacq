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
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Specifies flags for symbol targets and attributes.
    /// </summary>
    [Flags()]
    public enum DispatchTypes
    {
        /// <summary>
        /// Specifies the symbol has no targets, or no condition to search symbols. This is default value of <see cref="DispatchTypes"/>.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// Specifies the symbol is targeted to member access (just a identifiers, fields, or properties).
        /// </summary>
        Member = 0x1,
        
        /// <summary>
        /// Specifies the symbol is targeted to method call (just a functions or methods).
        /// </summary>
        Method = 0x2,
        
        /// <summary>
        /// Specifies the symbol is targeted to constructor call.
        /// </summary>
        Constructor = 0x4,
        
        /// <summary>
        /// Not implemented. This is a target flag.
        /// </summary>
        MethodGroup = 0x8,
        
        /// <summary>
        /// Specifies symbol target information.
        /// </summary>
        TargetMask = 0xffff,
        
        /// <summary>
        /// Specifies the symbol is literal; it means that its implementation constantly returns same expressions, regardless of arguments of <see cref="SymbolDefinition"/>.
        /// </summary>
        Literal = 0x10000,
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
