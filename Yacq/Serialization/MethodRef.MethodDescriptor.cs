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
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    partial class MethodRef
    {
        internal class MethodDescriptor
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
                                      (r, n, tas, pts) => new MethodDescriptor()
                                      {
                                          Name = n,
                                          TypeArguments = tas
                                              .Select(ns => ns
                                                  .Select(tn => new TypeRef.TypeDescriptor()
                                                  {
                                                      TypeName = new TypeRef.TypeName()
                                                      {
                                                          HierarchicalNames = new []
                                                          {
                                                              tn,
                                                          },
                                                      },
                                                  })
                                                  .ToArray()
                                              )
                                              .Otherwise(() => new TypeRef.TypeDescriptor[0]),
                                          ParameterTypes = pts,
                                          ReturnType = r,
                                      }
                              )
                          )
                  );

            public static new Parser<Char, MethodDescriptor> Parser
            {
                get
                {
                    return _parser.Value;
                }
            }

            public TypeRef.TypeDescriptor[] TypeArguments
            {
                get;
                set;
            }

            public TypeRef.TypeDescriptor[] ParameterTypes
            {
                get;
                set;
            }

            public MethodDescriptor()
            {
                this.TypeArguments = new TypeRef.TypeDescriptor[0];
                this.ParameterTypes = new TypeRef.TypeDescriptor[0];
            }

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
