// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using System.Reflection.Emit;

namespace XSpect.Yacq.SystemObjects
{
    /// <summary>
    /// Provides the foundations of dynamic type declaring and generating feature.
    /// </summary>
    public class TypeGenerator
    {
        private readonly Lazy<AssemblyBuilder> _assembly;

        private readonly Lazy<ModuleBuilder> _module;

        /// <summary>
        /// Gets the assembly of generated types.
        /// </summary>
        /// <value>The assembly of generated types.</value>
        public AssemblyBuilder Assembly
        {
            get
            {
                return this._assembly.Value;
            }
        }

        /// <summary>
        /// Gets the module of generated types.
        /// </summary>
        /// <value>The module of generated types.</value>
        public ModuleBuilder Module
        {
            get
            {
                return this._module.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeGenerator"/> class.
        /// </summary>
        /// <param name="name">Name of dynamic assembly which contains generated types.</param>
        public TypeGenerator(String name)
        {
            this._assembly = new Lazy<AssemblyBuilder>(
                () => AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(name),
                    AssemblyBuilderAccess.Run
                ),
                true
            );
            this._module = new Lazy<ModuleBuilder>(
                () => this.Assembly.DefineDynamicModule(name + ".dll"),
                true
            );
        }
    }
}