﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    partial class TypeRef
    {
        internal class TypeDescriptor
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
                          (tn, tas, ss) => new TypeDescriptor()
                          {
                              TypeName = tn,
                              TypeArguments = tas.ToArray(),
                              Suffixes = ss
                                  .Select(_ => _
                                      .Select(cs => new String(cs.ToArray()))
                                      .ToArray()
                                  )
                                  .Otherwise(() => new String[0]),
                          }
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

            public static Parser<Char, TypeDescriptor> Parser
            {
                get
                {
                    return _parser.Value;
                }
            }

            public static Parser<Char, TypeDescriptor> ParserAssemblyQualified
            {
                get
                {
                    return _parserAssemblyQualified.Value;
                }
            }

            public TypeName TypeName
            {
                get;
                set;
            }

            public TypeDescriptor[] TypeArguments
            {
                get;
                set;
            }

            public String[] Suffixes
            {
                get;
                set;
            }

            public AssemblyName Assembly
            {
                get;
                set;
            }

            public TypeDescriptor()
            {
                this.TypeName = new TypeName();
                this.TypeArguments = new TypeDescriptor[0];
                this.Suffixes = new String[0];
                this.Assembly = new AssemblyName();
            }

            public override String ToString()
            {
                return this.TypeName + (this.TypeArguments.Any()
                    ? "<" + String.Join(", ", this.TypeArguments.SelectAll(t => t.ToString())) + ">"
                    : ""
                ) + String.Concat(this.Suffixes);
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et: