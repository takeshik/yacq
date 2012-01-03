// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
        private class Candidate
            : IComparable<Candidate>
        {
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
                        ? this.MethodBase.GetGenericArguments()
                              .Zip(value, Tuple.Create)
                              .ToDictionary(_ => _.Item1, _ => _.Item2)
                        : new Dictionary<Type, Type>();
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

            public IEnumerable<Tuple<Type, Expression>> ParameterMap
            {
                get
                {
                    return this.Parameters
                        .Select(p => p.ParameterType)
                        .If(_ => this.IsParamArray, _ => _
                            .Take(this.Parameters.Count() - 1)
                            .Concat(EnumerableEx.Repeat(this.Parameters.Last().ParameterType.GetElementType()))
                        )
                        .Zip(this.Arguments, Tuple.Create);
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
                    return this.IsParamArray
                        && !this.Parameters.Last().ParameterType.IsAssignableFrom(this.Arguments.Last().Type);
                }
            }

            public Candidate(Expression instance, MemberInfo member, IDictionary<Type, Type> typeArgumentMap, IList<Expression> arguments)
            {
                this.Instance = instance;
                this.Member = member;
                this.TypeArgumentMap = typeArgumentMap ?? new Dictionary<Type, Type>();
                this.Arguments = arguments;
            }

            public Candidate(Expression instance, MemberInfo member, IList<Type> typeArguments, IList<Expression> arguments)
            {
                this.Instance = instance;
                this.Member = member;
                this.TypeArguments = typeArguments ?? new Type[0];
                this.Arguments = arguments;
            }

            public override String ToString()
            {
                return (this.Instance != null ? this.Instance + "." : "")
                    + this.Member
                    + (this.TypeArguments.Any()
                          ? "<" + String.Join(", ", this.TypeArguments.Select(t => t.Name)) + ">"
                          : String.Join(", ", this.TypeArguments.Select(t => t.Name)))
                    + "(" + String.Join(", ", this.Arguments.Select(t => t.ToString())) + ")";
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
                    : (value = this.IsParamArray
                          .CompareTo(other.IsParamArray)
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
        }
    }
}
