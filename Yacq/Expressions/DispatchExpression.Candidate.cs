// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
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

namespace XSpect.Yacq.Expressions
{
    partial class DispatchExpression
    {
        internal class Candidate
            : IComparable<Candidate>
        {
            private readonly Lazy<IList<String>> _filledArgumentNames;

            private readonly Lazy<IList<Tuple<Type, Expression>>> _parameterMap;

            public Expression Instance
            {
                get;
                private set;
            }

            public MemberInfo Member
            {
                get;
                private set;
            }

            public IDictionary<Type, Type> TypeArgumentMap
            {
                get;
                private set;
            }

            public IList<Type> TypeArguments
            {
                get
                {
                    return this.TypeArgumentMap.ToArgumentArray();
                }
                private set
                {
                    this.TypeArgumentMap = value != null && value.Any()
                        ? (this.Method.Null(m => m.TryGetGenericMethodDefinition()) ?? this.MethodBase)
                              .GetGenericArguments()
                              .Zip(value, Tuple.Create)
                              .ToDictionary(_ => _.Item1, _ => _.Item2)
                        : new Dictionary<Type, Type>();
                }
            }

            public IList<String> ArgumentNames
            {
                get;
                private set;
            }

            public IList<String> FilledArgumentNames
            {
                get
                {
                    return this._filledArgumentNames.Value;
                }
            }

            public IList<Expression> Arguments
            {
                get;
                private set;
            }

            public FieldInfo Field
            {
                get
                {
                    return this.Member as FieldInfo;
                }
            }

            public PropertyInfo Property
            {
                get
                {
                    return this.Member as PropertyInfo;
                }
            }

            public EventInfo Event
            {
                get
                {
                    return this.Member as EventInfo;
                }
            }

            public MethodBase MethodBase
            {
                get
                {
                    return this.Member as MethodBase;
                }
            }

            public MethodInfo Method
            {
                get
                {
                    return this.Member as MethodInfo;
                }
            }

            public ConstructorInfo Constructor
            {
                get
                {
                    return this.Member as ConstructorInfo;
                }
            }

            public Type Type
            {
                get
                {
                    return this.Member as Type;
                }
            }

            public IList<ParameterInfo> Parameters
            {
                get
                {
                    return this.MethodBase != null
                        ? this.MethodBase.GetParameters()
                        : this.Property != null
                              ? this.Property.GetIndexParameters()
                              : new ParameterInfo[0];
                }
            }

            public IList<Tuple<Type, Expression>> ParameterMap
            {
                get
                {
                    return this._parameterMap.Value;
                }
            }

            public Boolean IsParamArray
            {
                get
                {
                    return this.Parameters.Any()
                        && Attribute.IsDefined(this.Parameters.Last(), typeof(ParamArrayAttribute));
                }
            }

            public Boolean IsParamArrayContext
            {
                get
                {
                    return this.IsParamArray &&
                        this.Arguments.Last().Let(e => e is YacqExpression
                            // Suppress reducing by Type property (this test not indicates whether surely in ParamArray context)
                            ? this.ArgumentNames.All(n => n == null) && this.Arguments.Count != this.Parameters.Count
                            : !this.Parameters.Last().ParameterType.IsAssignableFrom(e.Type)
                        );
                }
            }

            private Candidate(
                Expression instance,
                MemberInfo member,
                IList<String> argumentNames,
                IList<Expression> arguments
            )
            {
                this._filledArgumentNames = new Lazy<IList<String>>(() => this.ArgumentNames
                    .TakeWhile(n => n == null)
                    .Count()
                    .Let(c => this.Parameters
                        .Select(p => p.Name)
                        .Take(c)
                        .Concat(this.ArgumentNames.Skip(c))
                    )
                    .ToArray()
                );
                this._parameterMap = new Lazy<IList<Tuple<Type, Expression>>>(() =>
                    (this.IsParamArrayContext
                        // Guard for irregular parameter names, such as T[]..ctor(int <null>)
                        || this.Parameters.Any(p => String.IsNullOrWhiteSpace(p.Name))
                        ? this.ArgumentNames.All(n => n == null)
                              ? this.Parameters
                                    .SkipLast(1)
                                    .Select(p => p.ParameterType)
                                    .Concat(EnumerableEx.Repeat(
                                        this.Parameters.Last().ParameterType.GetElementType()
                                    ))
                                    .Zip(this.Arguments, Tuple.Create)
                              : null
                        : this.Arguments.Count <= this.Parameters.Count
                              && this.Parameters.All(p => this.FilledArgumentNames.Contains(p.Name) || p.IsOptional)
                              ? this.FilledArgumentNames
                                    .Zip(this.Arguments, Tuple.Create)
                                    .ToDictionary(_ => _.Item1, _ => _.Item2)
                                    .Let(d => this.Parameters.Select(p => Tuple.Create(
                                        p.ParameterType,
                                        d.ContainsKey(p.Name)
                                            ? d[p.Name]
                                            : Constant(p.DefaultValue, p.ParameterType)
                                   )))
                              : null
                    )
                    .Null(_ => _.ToArray())
                );
                this.Instance = instance;
                this.Member = member;
                this.ArgumentNames = argumentNames ?? new String[this.Method.GetParameters().Length];
                this.Arguments = arguments;
            }

            public Candidate(
                Expression instance,
                MemberInfo member,
                IDictionary<Type, Type> typeArgumentMap,
                IList<String> argumentNames,
                IList<Expression> arguments
            )
                : this(instance, member, argumentNames, arguments)
            {
                this.TypeArgumentMap = typeArgumentMap ?? new Dictionary<Type, Type>();
            }

            public Candidate(
                Expression instance,
                MemberInfo member,
                IList<Type> typeArguments,
                IList<String> argumentNames,
                IList<Expression> arguments
            )
                : this(instance, member, argumentNames, arguments)
            {
                this.TypeArguments = typeArguments ?? new Type[0];
            }

            public override String ToString()
            {
                return (this.Instance != null ? this.Instance + "." : "")
                    + this.Member
                    + (this.TypeArguments.Any()
                          ? "<" + String.Join(", ", this.TypeArguments.Select(t => t.Name)) + ">"
                          : String.Join(", ", this.TypeArguments.Select(t => t.Name)))
                    + "(" + String.Join(", ", this.ArgumentNames.Zip(this.Arguments, (n, a) => n != null
                          ? n + ": " + a
                          : a.ToString()
                      ))
                    + ")";
            }

            public Int32 CompareTo(Candidate other)
            {
                Int32 value;
                return this.Method != null
                    && other.Method != null
                    && (value = this.Method.IsExtensionMethod()
                           .CompareTo(other.Method.IsExtensionMethod())
                       ) != 0
                    ? value
                    : (value = this.IsParamArrayContext
                          .CompareTo(other.IsParamArrayContext)
                      ) != 0
                          ? value
                          : this.Parameters.Select(_ => _.ParameterType)
                                .Zip(other.Parameters.Select(_ => _.ParameterType), (l, r) =>
                                    l.GetConvertibleTypes().Let(ls =>
                                        r.GetConvertibleTypes().Let(rs =>
                                            ls.Contains(r)
                                                ? rs.Contains(l) ? 0 : -1
                                                : rs.Contains(l) ? 1 : 0
                                            )
                                        )
                                )
                                .FirstOrDefault(_ => _ != 0);
            }

            public Candidate Clone(
                Expression instance = null,
                MemberInfo member = null,
                IDictionary<Type, Type> typeArgumentMap = null,
                IList<Type> typeArguments = null,
                IList<String> argumentNames = null,
                IList<Expression> arguments = null
            )
            {
                return typeArgumentMap != null
                    ? new Candidate(
                          instance ?? this.Instance,
                          member ?? this.Member,
                          typeArgumentMap,
                          argumentNames ?? this.ArgumentNames,
                          arguments ?? this.Arguments
                      )
                    : new Candidate(
                          instance ?? this.Instance,
                          member ?? this.Member,
                          typeArguments ?? this.TypeArguments,
                          argumentNames ?? this.ArgumentNames,
                          arguments ?? this.Arguments
                      );
            }
        }
    }
}