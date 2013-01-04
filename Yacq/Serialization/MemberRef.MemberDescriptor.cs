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
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    partial class MemberRef
    {
        /// <summary>
        /// Provides information of <see cref="MemberRef"/> (and its derived classes, excepts <see cref="MethodRef"/>) object.
        /// </summary>
        public class MemberDescriptor
        {
            private static readonly Lazy<Parser<Char, MemberDescriptor>> _parser
                = new Lazy<Parser<Char, MemberDescriptor>>(() =>
                      TypeRef.TypeDescriptor.Parser
                          .Pipe(
                              Chars.OneOf('`', '[', ']', '+', '.', ',', '*', '&', '(', ')').Let(ds => ds
                                  .Not()
                                  .Right('\\'.Satisfy().Right(ds))
                                  .Or(Chars.NoneOf('`', '[', ']', '+', '.', ',', '*', '&', '(', ')'))
                                  .Many()
                              ).Select(cs => new String(cs.ToArray())),
                              (t, n) => new MemberDescriptor(n, t)
                          )
                  );

            /// <summary>
            /// Gets the parser to generate an <see cref="MemberDescriptor"/> object from <see cref="AssemblyRef.Name"/>.
            /// </summary>
            /// <value>the parser to generate an <see cref="MemberDescriptor"/> object from <see cref="AssemblyRef.Name"/>.</value>
            public static Parser<Char, MemberDescriptor> Parser
            {
                get
                {
                    return _parser.Value;
                }
            }

            /// <summary>
            /// Gets or sets the name of the member.
            /// </summary>
            /// <value>The name of the member.</value>
            public String Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the return type of the member.
            /// </summary>
            /// <value>The return type of the member.</value>
            public TypeRef.TypeDescriptor ReturnType
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MemberDescriptor"/> class.
            /// </summary>
            public MemberDescriptor()
                : this(null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MemberDescriptor"/> class.
            /// </summary>
            /// <param name="name">The name of the member.</param>
            /// <param name="returnType">The return type of the member.</param>
            public MemberDescriptor(String name, TypeRef.TypeDescriptor returnType = null)
            {
                this.Name = name ?? "";
                this.ReturnType = returnType ?? new TypeRef.TypeDescriptor();
            }

            /// <summary>
            /// Returns a <see cref="String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="String"/> that represents this instance.
            /// </returns>
            public override String ToString()
            {
                return this.Name;
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
