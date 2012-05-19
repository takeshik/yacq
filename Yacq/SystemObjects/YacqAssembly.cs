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
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.SystemObjects
{
    /// <summary>
    /// Defines and represents a dynamic assembly with YACQ codes.
    /// </summary>
    public class YacqAssembly
    {
        private static readonly SHA1 _sha
#if SILVERLIGHT
            = new SHA1Managed();
#else
            = Environment.OSVersion.Version.Major >= 6
                  ? (SHA1) new SHA1Cng()
                  : new SHA1CryptoServiceProvider();
#endif

        private readonly Lazy<AssemblyBuilder> _assembly;

        private readonly Lazy<ModuleBuilder> _module;

        private readonly PEFileKinds _fileKind;

        private readonly Dictionary<String, YacqType> _types; 

        private AssemblyBuilder Assembly
        {
            get
            {
                return this._assembly.Value;
            }
        }

        private ModuleBuilder Module
        {
            get
            {
                return this._module.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YacqAssembly"/> class.
        /// </summary>
        /// <param name="name">Name of dynamic assembly which contains generated types.</param>
        /// <param name="fileKind">The type of the assembly executable being built.</param>
        public YacqAssembly(String name, PEFileKinds fileKind = PEFileKinds.Dll)
        {
            this._assembly = new Lazy<AssemblyBuilder>(
                () => AppDomain.CurrentDomain.DefineDynamicAssembly(
                    new AssemblyName(name),
#if SILVERLIGHT
                    AssemblyBuilderAccess.Run
#else
                    AssemblyBuilderAccess.RunAndSave
#endif
                ),
                true
            );
            this._module = new Lazy<ModuleBuilder>(
                () => this.Assembly.DefineDynamicModule(name + (fileKind == PEFileKinds.Dll ? ".dll" : ".exe")),
                true
            );
            this._fileKind = fileKind;
            this._types = new Dictionary<String, YacqType>();
        }

        private YacqType DefineType(String name, IEnumerable<Type> baseTypes, Boolean throwIfExists)
        {
            var type = this.GetType(name);
            if (type != null)
            {
                if (throwIfExists)
                {
                    throw new InvalidOperationException("Type '" + name + "' is already exists in this assembly.");
                }
                else
                {
                    return type;
                }
            }
            else
            {
                return this.GetType(name) ?? new YacqType(this.Module, name, baseTypes)
                    .Apply(t => this._types.Add(name, t));
            }
        }

        /// <summary>
        /// Constructs a <see cref="YacqType"/> in this <see cref="YacqAssembly"/>.
        /// </summary>
        /// <param name="name">The full path of the type. name cannot contain embedded nulls.</param>
        /// <param name="baseTypes">The list of the deriving type and interfaces that the type implements. The deriving type must be first in the list.</param>
        /// <returns>A constructed <see cref="YacqType"/>.</returns>
        public YacqType DefineType(String name, params Type[] baseTypes)
        {
            return this.DefineType(name, baseTypes, true);
        }

        /// <summary>
        /// Constructs or gets a <see cref="YacqType"/> in this <see cref="YacqAssembly"/>.
        /// </summary>
        /// <param name="name">The full path of the type. name cannot contain embedded nulls.</param>
        /// <param name="baseTypes">The list of the deriving type and interfaces that the type implements. The deriving type must be first in the list.</param>
        /// <returns>A constructed <see cref="YacqType"/>, or already constructed object if <paramref name="name"/> type is already exists.</returns>
        public YacqType TryDefineType(String name, params Type[] baseTypes)
        {
            return this.DefineType(name, baseTypes, false);
        }

        private YacqType DefineType(IDictionary<String, Type> members, Boolean throwIfExists)
        {
            var name =  ":Anonymous:" + Convert.ToBase64String(_sha.ComputeHash(Encoding.Unicode.GetBytes(
                String.Concat(members.Select(_ => "<" + _.Key + ":" + _.Value.AssemblyQualifiedName + ">"))
            )))
                .Substring(0, 27)
                .Replace('+', '-')
                .Replace('/', '_');
            var type = this.GetType(name);
            if (type != null)
            {
                if (throwIfExists)
                {
                    throw new InvalidOperationException("Anonymous type which has specified members is already exists in this assembly.");
                }
                else
                {
                    return type;
                }
            }
            else
            {
                return this.DefineType(name)
                    .Apply(
                        t => members.ForEach(p => t.DefineProperty(p.Key, p.Value)),
                        t => t.DefineConstructor(
                            MethodAttributes.Public,
                            members.Values.ToArray(),
                            members
                                .Select(p => YacqExpression.AmbiguousParameter(p.Key))
                                .StartWith(YacqExpression.AmbiguousParameter("self"))
                                .ToArray()
                                .Let(ps => YacqExpression.AmbiguousLambda(
                                    typeof(void),
                                    ps
                                        .Skip(1)
                                        .Select(p => YacqExpression.Function(
                                            "=",
                                            YacqExpression.Function(
                                                ".",
                                                YacqExpression.Identifier("self"),
                                                YacqExpression.Identifier(p.Name)
                                            ),
                                            YacqExpression.Identifier(p.Name)
                                        ))
#if SILVERLIGHT
                                        .Cast<Expression>()
#endif
                                        ,
                                    ps
                                ))
                        ),
                        t => t.DefineMethod(
                            "Equals",
                            MethodAttributes.Public,
                            typeof(Boolean),
                            new [] { typeof(Object), },
                            new []
                            {
                                YacqExpression.AmbiguousParameter("self"),
                                YacqExpression.AmbiguousParameter("value"),
                            }.Let(ps =>
                                YacqExpression.AmbiguousLambda(
                                    YacqExpression.Identifier("value")
                                        .Method("as", YacqExpression.Identifier("this"))
                                        .Method("let", YacqExpression.Identifier("_"),
                                            YacqExpression.Function("&&", t.GetProperties()
                                                .Select(p => YacqExpression.TypeCandidate(
                                                    typeof(EqualityComparer<>).MakeGenericType(p.PropertyType)
                                                )
                                                    .Member("Default")
                                                    .Method("Equals",
                                                        YacqExpression.Identifier("self").Member(p.Name),
                                                        YacqExpression.Identifier("_").Member(p.Name)
                                                    )
                                                )
                                                .StartWith(YacqExpression.Function("?", YacqExpression.Identifier("_")))
#if SILVERLIGHT
                                                .Cast<Expression>()
#endif
                                            )
                                        ),
                                        ps
                                )
                            )
                        ),
                        t => t.DefineMethod(
                            "GetHashCode",
                            MethodAttributes.Public,
                            typeof(Int32),
                            Type.EmptyTypes,
                            YacqExpression.AmbiguousLambda(
                                YacqExpression.Function("^",
                                    t.GetProperties()
                                        .Select(p => (Expression) YacqExpression.Identifier("self").Member(p.Name).Method("GetHashCode")
                                            .If(e => !p.PropertyType.IsValueType, e =>
                                                YacqExpression.Function("?", YacqExpression.Identifier("self"))
                                                    .Method("cond", e, Expression.Constant(0))
                                            )
                                        )
                                        .StartWith(Expression.Constant(t.GetHashCode()))
                                ),
                                YacqExpression.AmbiguousParameter("self")
                            )
                        ),
                        t => t.DefineMethod(
                            "ToString",
                            MethodAttributes.Public,
                            typeof(String),
                            Type.EmptyTypes,
                            YacqExpression.AmbiguousLambda(
                                YacqExpression.Function("+",
                                    Expression.Constant("{ "),
                                    YacqExpression.TypeCandidate(typeof(String))
                                        .Method("Join",
                                            t.GetProperties()
                                                .Select(p => (Expression) YacqExpression.Function("+",
                                                    Expression.Constant(p.Name + " = "),
                                                    YacqExpression.Identifier("self").Member(p.Name).Method("ToString")
                                                        .If(e => !p.PropertyType.IsValueType, e =>
                                                            YacqExpression.Function("?", YacqExpression.Identifier("self").Member(p.Name))
                                                                .Method("cond", e, Expression.Constant(""))
                                                        )
                                                ))
                                                .StartWith(Expression.Constant(", "))
                                        ),
                                    Expression.Constant(" }")
                                ),
                                YacqExpression.AmbiguousParameter("self")
                            )
                        )
                    );
            }
        }

        /// <summary>
        /// Constructs an anonymous <see cref="YacqType"/> in this <see cref="YacqAssembly"/>.
        /// </summary>
        /// <param name="members">The dictionary which contains pairs of the name and its type of member property.</param>
        /// <returns>A constructed <see cref="YacqType"/>.</returns>
        public YacqType DefineType(IDictionary<String, Type> members)
        {
            return this.DefineType(members, true);
        }

        /// <summary>
        /// Constructs or gets an anonymous <see cref="YacqType"/> in this <see cref="YacqAssembly"/>.
        /// </summary>
        /// <param name="members">The dictionary which contains pairs of the name and its type of member property.</param>
        /// <returns>A constructed <see cref="YacqType"/>, or already constructed object if the type with specified members is already exists.</returns>
        public YacqType TryDefineType(IDictionary<String, Type> members)
        {
            return this.DefineType(members, false);
        }

        /// <summary>
        /// Gets the types defined in this assembly.
        /// </summary>
        /// <returns>A sequence that contains all the types that are defined in this assembly.</returns>
        public IEnumerable<YacqType> GetTypes()
        {
            return this._types.Values;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> object with the specified name in this assembly.
        /// </summary>
        /// <param name="name">The full name of the type. </param>
        /// <returns>An object that represents the specified type, or <c>null</c> if the type is not found.</returns>
        public YacqType GetType(String name)
        {
            return this._types.ContainsKey(name)
                ? this._types[name]
                : null;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Saves this <see cref="YacqAssembly"/> to disk.
        /// </summary>
        /// <param name="entryMethod">A reference to the method that represents the entry point for this <see cref="YacqAssembly"/>, or <c>null</c> if the entry point is automatically searched.</param>
        public void Save(MethodInfo entryMethod = null)
        {
            if (this._fileKind != PEFileKinds.Dll)
            {
                this.Assembly.SetEntryPoint(
                    entryMethod ?? this.GetTypes()
                        .SelectMany(t => t.GetMethods())
                        .Single(m =>
                            m.IsStatic &&
                            m.Name == "Main" &&
                            (m.ReturnType == typeof(void) || m.ReturnType == typeof(Int32))
                        ),
                    this._fileKind
                );
            }
            this.Assembly.Save(this.Module.ScopeName);
        }
#endif
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
