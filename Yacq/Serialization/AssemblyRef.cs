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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    [DataContract(Name = "Assembly", IsReference = true)]
#if !SILVERLIGHT
    [Serializable()]
#endif
    internal class AssemblyRef
    {
        private static readonly Dictionary<AssemblyRef, Assembly> _cache
            = new Dictionary<AssemblyRef, Assembly>();

        private static readonly Dictionary<Assembly, AssemblyRef> _reverseCache
            = new Dictionary<Assembly, AssemblyRef>();

        private static readonly Lazy<Parser<Char, AssemblyName>> _parser
            = new Lazy<Parser<Char, AssemblyName>>(() => Tuple.Create(
                  ','.Satisfy().Pipe(' '.Satisfy().Many(), (l, r) => r.StartWith(l)),
                  '='.Satisfy().Pipe(' '.Satisfy().Many(), (l, r) => r.StartWith(l))
              ).Let((comma, equal) =>
                  Combinator.Choice(
                      Chars.OneOf('`', '[', ']', '+', ',').Let(ds => ds
                          .Not()
                          .Right('\\'.Satisfy().Right(ds))
                          .Or(Chars.NoneOf('`', '[', ']', '+', ','))
                          .Many()
                      ),
                      Combinator.Sequence(
                          Chars.Sequence("Version"),
                          equal,
                          Chars.Digit()
                              .Many()
                              .SepBy(3, '.'.Satisfy())
                              .Select(cs => cs.SelectMany(_ => _))
                      ).Select(_ => _.SelectMany(cs => cs)),
                      Combinator.Sequence(
                          Chars.Sequence("Culture"),
                          equal,
                          Chars.Letter()
                              .Many()
                      ).Select(_ => _.SelectMany(cs => cs)),
                      Combinator.Sequence(
                          Chars.Sequence("PublicKeyToken"),
                          equal,
                          Chars.Hex()
                              .Many(16)
                              .Or(Chars.Sequence("null"))
                      ).Select(_ => _.SelectMany(cs => cs))
                  )
                      .SepBy(comma)
                      .Select(fs => new String(fs
                          .SelectMany(cs => cs.EndWith(','))
                          .ToArray()
                      ).If(String.IsNullOrWhiteSpace,
                          _ => new AssemblyName(),
                          s => new AssemblyName(s)
                      ))
              ));

        private static readonly String[] _loadedAssemblies = new []
        {
            "Parseq",
            "System",
            "System.Core",
            "System.Interactive",
            "System.Interactive.Providers",
            "System.Reactive.Core",
            "System.Reactive.Interfaces",
            "System.Reactive.Linq",
            "System.Reactive.PlatformServices",
            "System.Reactive.Providers",
            "System.Runtime.Serialization",
            "System.Xml",
            "System.Xml.Linq",
            "Yacq",
        };

        internal static Parser<Char, AssemblyName> Parser
        {
            get
            {
                return _parser.Value;
            }
        }

        [DataMember(Order = 0, EmitDefaultValue = false)]
        public string Name
        {
            get;
            set;
        }

        internal static AssemblyRef Serialize(Assembly assembly)
        {
            return _reverseCache.TryGetValue(assembly) ??
#if SILVERLIGHT
                assembly.FullName.Split(',')[0]
#else
                assembly.GetName().Name
#endif
                    .Let(n => new AssemblyRef()
                    {
                        Name = n == "mscorlib"
                            ? null
                            : _loadedAssemblies.Contains(n)
                                  ? n
                                  : assembly.FullName,
                    })
                    .Apply(a => _reverseCache.Add(assembly, a));
        }

        public override String ToString()
        {
            AssemblyName assemblyRef;
            return this.Name == null
                ? "mscorlib"
                : Parser(this.Name.AsStream())
                      .TryGetValue(out assemblyRef)
                      .If(_ => _, _ => assemblyRef.Name, _ => this.Name);
        }

        internal Assembly Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? Assembly.Load(
                       String.IsNullOrWhiteSpace(this.Name)
                           ? "mscorlib"
                           : this.Name
                   ).Apply(a => _cache.Add(this, a));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
