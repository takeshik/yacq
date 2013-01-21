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
    partial class TypeRef
    {
        /// <summary>
        /// Provides information of name of <see cref="TypeDescriptor"/> object.
        /// </summary>
        public class TypeName
        {
            private static readonly Lazy<Parser<Char, TypeName>> _parser
                = new Lazy<Parser<Char, TypeName>>(() =>
                      Chars.OneOf(' ', '&', '(', ')', '*', '+', ',', '.', '[', ']', '`')
                          .Let(ds => ds
                              .Not()
                              .Right('\\'.Satisfy().Right(ds))
                              .Or(Chars.NoneOf(' ', '&', '(', ')', '*', '+', ',', '.', '[', ']', '`'))
                              .Many()
                          )
                          .Let(name =>
                              Combinator.Sequence(
                                  name,
                                  Chars.Sequence(".")
                              )
                                  .Select(_ => new String(_.SelectMany(cs => cs).SkipLast(1).ToArray()))
                                  .Many()
                                  .Pipe(
                                      name
                                          .Select(cs => new String(cs.ToArray())),
                                      Combinator.Sequence(
                                          Chars.Sequence("+"),
                                          name
                                      )
                                          .Select(_ => new String(_.SelectMany(cs => cs).Skip(1).ToArray()))
                                          .Many(),
                                      (nss, n, ins) => new TypeName(
                                          String.Join(".", nss),
                                          ins
                                              .StartWith(n)
                                              .ToArray()
                                      )
                                  )
                          )
                      );

            /// <summary>
            /// Gets the parser to generate an <see cref="TypeName"/> object from the name part of <see cref="TypeRef.Name"/>.
            /// </summary>
            /// <value>the parser to generate an <see cref="TypeName"/> object from name part of <see cref="TypeRef.Name"/>.</value>
            public static Parser<Char, TypeName> Parser
            {
                get
                {
                    return _parser.Value;
                }
            }

            /// <summary>
            /// Gets or sets the namespace of the type.
            /// </summary>
            /// <value>The namespace of the type.</value>
            public String Namespace
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the array of type names of nesting hierarchy and the type itself.
            /// </summary>
            /// <value>The array of type names of nesting hierarchy and the type itself.</value>
            public String[] HierarchicalNames
            {
                get;
                set;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeDescriptor"/> class.
            /// </summary>
            public TypeName()
                : this(null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeDescriptor"/> class.
            /// </summary>
            /// <param name="hierarchicalNames">The array of type names of nesting hierarchy and the type itself.</param>
            public TypeName(String[] hierarchicalNames)
                : this(null, hierarchicalNames)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="TypeDescriptor"/> class.
            /// </summary>
            /// <param name="namespace">The namespace of the type.</param>
            /// <param name="hierarchicalNames">The array of type names of nesting hierarchy and the type itself.</param>
            public TypeName(String @namespace, String[] hierarchicalNames)
            {
                this.Namespace = @namespace ?? "";
                this.HierarchicalNames = hierarchicalNames ?? new String[0];
            }

            /// <summary>
            /// Returns a <see cref="String"/> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="String"/> that represents this instance.
            /// </returns>
            public override String ToString()
            {
                return String.Join("+", this.HierarchicalNames);
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
