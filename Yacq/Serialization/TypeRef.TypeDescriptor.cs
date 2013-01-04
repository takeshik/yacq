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
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    partial class TypeRef
    {
        /// <summary>
        /// Provides information of <see cref="TypeRef"/> object.
        /// </summary>
        public class TypeDescriptor
        {
            private static readonly Lazy<Parser<Char, TypeDescriptor>> _parser
                = new Lazy<Parser<Char, TypeDescriptor>>(() => stream =>
                      TypeName.Parser.Pipe(
                          '`'.Satisfy()
                              .Pipe(
                                  Chars.Digit()
                                      .Many()
                                      .Select(ds => Int32.Parse(new String(ds.ToArray()))),
                                  ParserAssemblyQualified
                                      .Between('['.Satisfy(), ']'.Satisfy())
                                      .Or(Parser)
                                      .SepBy(','.Satisfy())
                                      .Between('['.Satisfy(), ']'.Satisfy()),
                                  (_, n, tas) => tas.ToArray()
                              )
                              .Maybe()
                              .Select(_ => _.Otherwise(() => new TypeDescriptor[0])),
                          Combinator.Choice(
                              Chars.Sequence("*"),
                              Chars.Sequence("&"),
                              Chars.Sequence("*")
                                  .Or(Chars.Sequence(","))
                                  .Many()
                                  .Between('['.Satisfy(), ']'.Satisfy())
                                  .Select(_ => _
                                      .SelectMany(cs => cs)
                                      .StartWith('[')
                                      .EndWith(']')
                                  )
                          )
                              .Many()
                              .Maybe(),
                          (tn, tas, ss) => new TypeDescriptor(
                              tn,
                              tas.ToArray(),
                              ss
                                  .Select(_ => _
                                      .Select(cs => new String(cs.ToArray()))
                                      .ToArray()
                                  )
                                  .Otherwise(() => new String[0])
                          )
                      )(stream)
                  );

            private static readonly Lazy<Parser<Char, TypeDescriptor>> _parserAssemblyQualified
                = new Lazy<Parser<Char, TypeDescriptor>>(() => Parser
                      .Pipe(
                          ','.Satisfy().Pipe(
                              ' '.Satisfy().Many(),
                              AssemblyRef.Parser,
                              (c, s, an) => an
                          ).Maybe(),
                          (t, an) => t.If(_ => an.Exists(), _ => _.Assembly = an.Perform())
                      )
                  );

            /// <summary>
            /// Gets the parser to generate an <see cref="TypeDescriptor"/> object from <see cref="TypeRef.Name"/>.
            /// </summary>
            /// <value>the parser to generate an <see cref="TypeDescriptor"/> object from <see cref="TypeRef.Name"/>.</value>
            public static Parser<Char, TypeDescriptor> Parser
            {
                get
                {
                    return _parser.Value;
                }
            }

            /// <summary>
            /// Gets the parser to generate an <see cref="TypeDescriptor"/> object from <see cref="TypeRef.Name"/> which is qualified with assembly name.
            /// </summary>
            /// <value>the parser to generate an <see cref="TypeDescriptor"/> object from <see cref="TypeRef.Name"/> which is qualified with assembly name.</value>
            public static Parser<Char, TypeDescriptor> ParserAssemblyQualified
            {
                get
                {
                    return _parserAssemblyQualified.Value;
                }
            }

            /// <summary>
            /// Gets or sets the name of the type.
            /// </summary>
            /// <value>The name of the type.</value>
            public TypeName Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the generic type arguments of the type.
            /// </summary>
            /// <value>The generic type arguments of the type.</value>
            public TypeDescriptor[] TypeArguments
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the array of type suffixes (array, reference, pointer).
            /// </summary>
            /// <value>The array of type suffixes (array, reference, pointer).</value>
            public String[] Suffixes
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the assembly which the type belongs.
            /// </summary>
            /// <value>The assembly which the type belongs.</value>
            public AssemblyName Assembly
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeDescriptor"/> class.
            /// </summary>
            public TypeDescriptor()
                : this(null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeDescriptor"/> class.
            /// </summary>
            /// <param name="name">The name of the type.</param>
            /// <param name="typeArguments">The generic type arguments of the type.</param>
            /// <param name="suffixes">The array of type suffixes (array, reference, pointer).</param>
            /// <param name="assembly">The assembly which the type belongs.</param>
            public TypeDescriptor(
                TypeName name,
                TypeDescriptor[] typeArguments = null,
                String[] suffixes = null,
                AssemblyName assembly = null
            )
            {
                this.Name = name ?? new TypeName();
                this.TypeArguments = typeArguments ?? new TypeDescriptor[0];
                this.Suffixes = suffixes ?? new String[0];
                this.Assembly = assembly ?? new AssemblyName();
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
                    ? "<" + String.Join(", ", this.TypeArguments.SelectAll(t => t.ToString())) + ">"
                    : ""
                ) + String.Concat(this.Suffixes);
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
