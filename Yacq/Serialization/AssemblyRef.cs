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
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using Parseq;
using Parseq.Combinators;

namespace XSpect.Yacq.Serialization
{
    /// <summary>
    /// Indicades an reference of <see cref="Assembly"/> for serialization.
    /// </summary>
    [DataContract(Name = "Assembly", IsReference = true)]
    public partial class AssemblyRef
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

        private readonly Lazy<AssemblyName> _descriptor;

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

        /// <summary>
        /// Gets the parser to generate an <see cref="AssemblyName"/> object.
        /// </summary>
        /// <value>The parser to generate an <see cref="AssemblyName"/> object.</value>
        public static Parser<Char, AssemblyName> Parser
        {
            get
            {
                return _parser.Value;
            }
        }

        /// <summary>
        /// Gets or sets the value of <see cref="Assembly.FullName"/>.
        /// </summary>
        /// <value>The value of <see cref="Assembly.FullName"/>, or <c>null</c> if referring assembly is mscorlib.</value>
        [DataMember(Order = 0, EmitDefaultValue = false)]
        public String Name
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyRef"/> class.
        /// </summary>
        public AssemblyRef()
        {
            AssemblyName assemblyRef;
            this._descriptor = new Lazy<AssemblyName>(() =>
                Parser(this.GetName().AsStream())
                    .TryGetValue(out assemblyRef)
                    .Let(_ => assemblyRef)
            );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyRef"/> class.
        /// </summary>
        /// <param name="name">The value of <see cref="Assembly.FullName"/>, or simple name if referring assembly is wellknown, or <c>null</c> for mscorlib.</param>
        public AssemblyRef(String name)
            : this()
        {
            this.Name = name;
        }

        /// <summary>
        /// Returns the string value of this assembly reference.
        /// </summary>
        /// <returns>The value of <see cref="Assembly.FullName"/>, or simple name if referring assembly is wellknown.</returns>
        public String GetName()
        {
            return this.Name ?? "mscorlib";
        }

        /// <summary>
        /// Returns the assembly reference which refers specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to refer.</param>
        /// <returns>The assembly reference which refers specified assembly.</returns>
        public static AssemblyRef Serialize(Assembly assembly)
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

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return this.Describe().Name;
        }

        /// <summary>
        /// Returns an object to describe this assembly reference.
        /// </summary>
        /// <returns>An object to describe this assembly reference.</returns>
        public AssemblyName Describe()
        {
            return this._descriptor.Value;
        }

        /// <summary>
        /// Dereferences this assembly reference.
        /// </summary>
        /// <returns>The assembly which is referred by this assembly reference.</returns>
        public Assembly Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? Assembly.Load(this.GetName())
                       .Apply(a => _cache.Add(this, a));
        }
    }

#if !SILVERLIGHT
    [Serializable()]
    partial class AssemblyRef
        : ISerializable
    {
        /// <summary>
        /// Initializes a new instance of a <see cref="AssemblyRef"/> class that has the given serialization information and context.
        /// </summary>
        /// <param name="info">The data needed to serialize or deserialize an object. </param>
        /// <param name="context">The source and destination of a given serialized stream. </param>
        protected AssemblyRef(SerializationInfo info, StreamingContext context)
            : this(info.GetString("Name"))
        {
        }

        /// <summary>
        /// Populates a serialization information object with the data needed to serialize the <see cref="AssemblyRef"/>.
        /// </summary>
        /// <param name="info">A <see cref="SerializationInfo"/> that holds the serialized data associated with the <see cref="AssemblyRef"/>.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Name", this.Name);
        }
    }
#endif
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
