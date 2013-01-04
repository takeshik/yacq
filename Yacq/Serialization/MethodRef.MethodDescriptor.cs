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
    partial class MethodRef
    {
        /// <summary>
        /// Provides information of <see cref="MethodRef"/> object.
        /// </summary>
        public class MethodDescriptor
            : MemberDescriptor
        {
            private static readonly Lazy<Parser<Char, MethodDescriptor>> _parser
                = new Lazy<Parser<Char, MethodDescriptor>>(() =>
                      Chars.OneOf('`', '[', ']', '+', '.', ',', '*', '&', '(', ')').Let(ds => ds
                          .Not()
                          .Right('\\'.Satisfy().Right(ds))
                          .Or(Chars.NoneOf('`', '[', ']', '+', '.', ',', '*', '&', '(', ')'))
                          .Many()
                      )
                          .Select(cs => new String(cs.ToArray()))
                          .Let(name =>
                              TypeRef.TypeDescriptor.Parser
                                  .Pipe(' '.Satisfy(), (t, _) => t)
                                  .Pipe(
                                      name,
                                      name
                                          .SepBy(','.Satisfy())
                                          .Between('['.Satisfy(), ']'.Satisfy())
                                          .Maybe(),
                                      TypeRef.TypeDescriptor.Parser
                                          .SepBy(Chars.Sequence(", "))
                                          .Between('('.Satisfy(), ')'.Satisfy())
                                          .Select(ts => ts.ToArray())
                                          .Or(Chars.Sequence("()")
                                              .Select(_ => new TypeRef.TypeDescriptor[0])
                                          ),
                                      (r, n, tas, pts) => new MethodDescriptor(
                                          n,
                                          r,
                                          tas
                                              .Select(ns => ns
                                                  .Select(tn => new TypeRef.TypeDescriptor(
                                                      new TypeRef.TypeName(new []
                                                      {
                                                          tn,
                                                      })
                                                  ))
                                                  .ToArray()
                                              )
                                              .Otherwise(() => new TypeRef.TypeDescriptor[0]),
                                          pts
                                      )
                              )
                          )
                  );

            /// <summary>
            /// Gets the parser to generate an <see cref="MethodDescriptor"/> object from <see cref="MemberRef.Signature"/>.
            /// </summary>
            /// <value>the parser to generate an <see cref="MethodDescriptor"/> object from <see cref="MemberRef.Signature"/>.</value>
            public static new Parser<Char, MethodDescriptor> Parser
            {
                get
                {
                    return _parser.Value;
                }
            }

            /// <summary>
            /// Gets or sets the generic type arguments of the method.
            /// </summary>
            /// <value>The generic type arguments of the method.</value>
            public TypeRef.TypeDescriptor[] TypeArguments
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the parameter types in the method.
            /// </summary>
            /// <value>The parameter types in the method.</value>
            public TypeRef.TypeDescriptor[] ParameterTypes
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MethodDescriptor"/> class.
            /// </summary>
            public MethodDescriptor()
                : this(null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="MethodDescriptor"/> class.
            /// </summary>
            /// <param name="name">The name of the member.</param>
            /// <param name="returnType">The return type of the member.</param>
            /// <param name="typeArguments">The generic type arguments of the method.</param>
            /// <param name="parameterTypes">The parameter types in the method.</param>
            public MethodDescriptor(
                String name,
                TypeRef.TypeDescriptor returnType = null,
                TypeRef.TypeDescriptor[] typeArguments = null,
                TypeRef.TypeDescriptor[] parameterTypes = null
            )
                : base(name, returnType)
            {
                this.TypeArguments = typeArguments ?? new TypeRef.TypeDescriptor[0];
                this.ParameterTypes = parameterTypes ?? new TypeRef.TypeDescriptor[0];
            }

            /// <summary>
            /// Returns a <see cref="String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="String"/> that represents this instance.
            /// </returns>
            public override String ToString()
            {
                return this.Name + (this.TypeArguments.Any()
                    ? "<" + String.Join(", ", this.TypeArguments.SelectAll(t => t.ToString())) + ">("
                    : "("
                ) + String.Join(", ", this.ParameterTypes.SelectAll(t => t.ToString())) + ")";
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
