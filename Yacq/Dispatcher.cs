// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using System.Runtime.CompilerServices;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq
{
    public static partial class Dispatcher
    {
        public static Expression DispatchMethod(
            Expression instance,
            IEnumerable<MethodBase> methods,
            IList<Type> typeArguments,
            IList<Expression> arguments
        )
        {
            return methods
                .Select(m => m is MethodInfo && ((MethodInfo) m).IsExtensionMethod()
                    ? new Candidate(null, m, null, arguments.StartWith(instance).ToArray())
                    : new Candidate(instance, m, null, arguments)
                )
                .If(_ => typeArguments != null && typeArguments.Any(), s => s
                    .Where(_ => _.MethodBase != null && _.MethodBase.IsGenericMethodDefinition && _.MethodBase.GetGenericArguments().Length == typeArguments.Count)
                    .Select(_ => _.Method.MakeGenericMethod(typeArguments.ToArray())
                        .Let(m => new Candidate(_.Instance, m, m.GetGenericParameterMap(), arguments))
                    )
                )
                .Where(t => t.Instance != null ^ (t.MethodBase.IsStatic || t.Constructor != null)
                    && t.MethodBase.GetParameters().If(_ => t.MethodBase.IsParamArrayMethod(),
                           ps => t.Arguments.Count >= ps.Length - 1
                               && ps.SkipLast(1).Zip(t.Arguments,
                                      (p, a) => IsAppropriate(p.ParameterType, a.Type)
                                  ).All(_ => _)
                               && EnumerableEx.Repeat(ps.Last()).Zip(t.Arguments.Skip(ps.Length - 1),
                                      (p, a) => IsAppropriate(p.ParameterType.GetElementType(), a.Type)
                                  ).All(_ => _),
                           ps => t.Arguments.Count == ps.Length
                               && ps.Zip(t.Arguments,
                                      (p, a) => IsAppropriate(p.ParameterType, a.Type)
                                  ).All(_ => _)
                       )
                )
                .Select(t => t
                    .If(_ => _.MethodBase != null && _.MethodBase.IsGenericMethod, _ =>
                        InferTypeArguments(t.MethodBase.GetParameters().Select(p => p.ParameterType), t.Arguments.Select(e => e.Type))
                            .Let(m => new Candidate(_.Instance, _.Method.MakeGenericMethod(m.Values.ToArray()), m, _.Arguments))
                    )
                    .If(_ => _.MethodBase.IsParamArrayMethod(), _ =>
                        _.MethodBase.GetParameters().Let(ps =>
                            ps.Last().ParameterType.GetElementType().Let(et =>
                                new Candidate(_.Instance, _.MethodBase, _.TypeArgumentMap, _.Arguments
                                    .Take(ps.Length - 1)
                                    .Concat(EnumerableEx.Return(Expression.NewArrayInit(et, _.Arguments
                                        .Skip(ps.Length - 1)
                                        .Select(e => e.TryConvert(et))
                                    )))
                                    .ToArray()
                                )
                            )
                        )
                    )
                )
                .OrderBy(t => t.MethodBase.IsParamArrayMethod())
                .ThenBy(t => t.Method != null && t.Method.IsExtensionMethod())
                .ThenByDescending(t => t.TypeArgumentMap.Count)
                .FirstOrDefault()
                .Null(_ => _.If(
                    t => t.Method != null,
                    t => t.Instance != null
                        ? (Expression) Expression.Call(t.Instance, t.Method, t.Arguments)
                        : Expression.Call(t.Method, t.Arguments),
                    t => Expression.New(t.Constructor, t.Arguments)
                ));
        }

        public static Expression DispatchMember(
            Expression instance,
            IEnumerable<MemberInfo> members,
            IList<Expression> arguments
        )
        {
            return members
                .Select(m => new Candidate(instance, m, null, arguments))
                .Where(t => !t.Arguments.Any()
                    || t.Property != null
                    && t.Property.DeclaringType.GetDefaultMembers().Contains(t.Property)
                )
                .FirstOrDefault()
                .Null(t => Expression.MakeMemberAccess(t.Instance, t.Member));
        }

        public static Dictionary<Type, Type> InferTypeArguments(IEnumerable<Type> parameters, IEnumerable<Type> arguments)
        {
            return parameters
                .Where(t => t.IsGenericParameter || t.IsGenericType)
                .Zip(arguments, (p, a) => a != null
                    ? p.IsGenericParameter
                          ? EnumerableEx.Return(Tuple.Create(p, a))
                          : p.GetGenericArguments()
                                .Zip(a.GetConvertibleTypes()
                                    .Single(t => t.IsGenericType && !t.IsGenericTypeDefinition && p.GetGenericTypeDefinition() == t.GetGenericTypeDefinition())
                                    .GetGenericArguments(),
                                    Tuple.Create
                                )
                    : Enumerable.Empty<Tuple<Type, Type>>()
                )
                .SelectMany(_ => _)
                .Where(_ => _.Item1.IsGenericParameter)
                .Distinct()
                .ToDictionary(_ => _.Item1, _ => _.Item2);
        }

        public static IEnumerable<Type> GetConvertibleTypes(this Type type)
        {
            return EnumerableEx.Concat(
                EnumerableEx.Generate(type, t => t != null, t => t.BaseType, _ => _),
                type.GetInterfaces(),
                type.IsInterface ? EnumerableEx.Return(typeof(Object)) : Enumerable.Empty<Type>()
            ).If(_ => type.IsGenericType && !type.IsGenericTypeDefinition, _ =>
                _.Concat(type.GetGenericTypeDefinition().GetConvertibleTypes())
            ).Distinct();
        }

        public static Boolean IsParamArrayMethod(this MethodBase method)
        {
            return method.GetParameters()
                .Let(_ => _.Any() && Attribute.IsDefined(method.GetParameters().Last(), typeof(ParamArrayAttribute)));
        }

        public static Boolean HasExtensionMethods(this Type type)
        {
            return Attribute.IsDefined(type, typeof(ExtensionAttribute));
        }

        public static Boolean IsExtensionMethod(this MethodInfo method)
        {
            return Attribute.IsDefined(method, typeof(ExtensionAttribute));
        }

        public static IEnumerable<MethodInfo> GetExtensionMethods(this Type type)
        {
            return type.HasExtensionMethods()
                ? type.GetMethods().Where(IsExtensionMethod)
                : Enumerable.Empty<MethodInfo>();
        }

        public static Dictionary<Type, Type> GetGenericParameterMap(this Type type)
        {
            return type.GetGenericTypeDefinition().GetGenericArguments()
                .Zip(type.GetGenericArguments(), Tuple.Create)
                .Where(_ => _.Item1.IsGenericParameter)
                .ToDictionary(_ => _.Item1, _ => _.Item2);
        }

        public static Dictionary<Type, Type> GetGenericParameterMap(this MethodInfo method)
        {
            return method.GetGenericMethodDefinition().GetGenericArguments()
                .Zip(method.GetGenericArguments(), Tuple.Create)
                .Where(_ => _.Item1.IsGenericParameter)
                .ToDictionary(_ => _.Item1, _ => _.Item2);
        }

        public static Boolean IsAppropriate(Type parameter, Type argument)
        {
            return parameter.IsGenericParameter
                ? argument.GetConvertibleTypes()
                      .Let(ts => parameter.GetGenericParameterConstraints().All(c => ts.Contains(c)))
                : argument.GetConvertibleTypes().If(
                      _ => parameter.ContainsGenericParameters,
                      _ => _.Select(t => t.IsGenericType ? t.GetGenericTypeDefinition() : t)
                  ).Let(_ => _.Contains(parameter) || parameter.IsGenericType && _.Contains(parameter.GetGenericTypeDefinition()));
        }
    }
}