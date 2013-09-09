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
using System.Runtime.CompilerServices;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq
{
    internal static class Extensions
    {
        internal static Expression TryConvert(this Expression expr, Type type)
        {
            return type == null || expr.Type == type
                ? expr
                : Expression.Convert(expr, type);
        }

        internal static Expression ReduceAndTryConvert(this Expression expr, SymbolTable symbols, Type expectedType)
        {
            return expr.Reduce(symbols, expectedType)
                ?? expr.Reduce(symbols).TryConvert(expectedType);
        }

        internal static IEnumerable<Type> GetConvertibleTypes(this Type type)
        {
            return type != null
                ? EnumerableEx.Concat(
                      type.Generate(t => t.BaseType, t => t != null),
                      type.GetInterfaces(),
                      type.IsInterface ? EnumerableEx.Return(typeof(Object)) : Enumerable.Empty<Type>()
                  ).If(_ => type.IsGenericType && !type.IsGenericTypeDefinition, _ =>
                      _.Concat(type.GetGenericTypeDefinition().GetConvertibleTypes())
                  ).Distinct()
                : Enumerable.Empty<Type>();
        }

        internal static MemberAccessibilities GetAccessibility(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    return (MemberAccessibilities) (((MethodBase) member).Attributes & MethodAttributes.MemberAccessMask);
                case MemberTypes.Event:
                    return ((EventInfo) member).Let(e =>
#if SILVERLIGHT || __MonoCS__
                        Enumerable.Empty<MethodInfo>()
#else
                        e.GetOtherMethods(true)
#endif
                            .StartWith(
                                e.GetAddMethod(true),
                                e.GetRemoveMethod(true),
                                e.GetRaiseMethod(true)
                            )
                            .Where(m => m != null)
                            .Select(m => m.GetAccessibility())
                            .Max(m => m)
                        );
                case MemberTypes.Field:
                    return (MemberAccessibilities) (((FieldInfo) member).Attributes & FieldAttributes.FieldAccessMask);
                case MemberTypes.Property:
                    return ((PropertyInfo) member).GetAccessors(true)
                        .Select(m => m.GetAccessibility())
                        .Max(m => m);
                case MemberTypes.NestedType:
                    switch (((Type) member).Attributes & TypeAttributes.VisibilityMask)
                    {
                        case TypeAttributes.NotPublic:
                        case TypeAttributes.NestedAssembly:
                            return MemberAccessibilities.Assembly;
                        case TypeAttributes.Public:
                        case TypeAttributes.NestedPublic:
                            return MemberAccessibilities.Public;
                        case TypeAttributes.NestedPrivate:
                            return MemberAccessibilities.Private;
                        case TypeAttributes.NestedFamily:
                            return MemberAccessibilities.Family;
                        case TypeAttributes.NestedFamANDAssem:
                            return MemberAccessibilities.FamANDAssem;
                        case TypeAttributes.NestedFamORAssem:
                            return MemberAccessibilities.FamORAssem;
                        default:
                            return MemberAccessibilities.Unknown;
                    }
                default:
                    return MemberAccessibilities.Unknown;
            }
        }

        internal static Boolean IsSpecialName(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    return ((MethodBase) member).IsSpecialName;
                case MemberTypes.Event:
                    return ((EventInfo) member).IsSpecialName;
                case MemberTypes.Field:
                    return ((FieldInfo) member).IsSpecialName;
                case MemberTypes.Property:
                    return ((PropertyInfo) member).IsSpecialName;
                case MemberTypes.NestedType:
                    return ((Type) member).IsSpecialName;
                default:
                    return false;
            }
        }

        internal static Boolean HasExtensionMethods(this Type type)
        {
            return Attribute.IsDefined(type, typeof(ExtensionAttribute));
        }

        internal static Boolean IsExtensionMethod(this MethodInfo method)
        {
            return Attribute.IsDefined(method, typeof(ExtensionAttribute));
        }

        internal static Type TryGetGenericTypeDefinition(this Type type)
        {
            return type != null && type.IsGenericType && !type.IsGenericTypeDefinition
                ? type.GetGenericTypeDefinition()
                : type;
        }

        internal static MethodInfo TryGetGenericMethodDefinition(this MethodInfo method)
        {
            return method != null && method.IsGenericMethod && !method.IsGenericMethodDefinition
                ? method.GetGenericMethodDefinition()
                : method;
        }

        internal static Type[] ToArgumentArray(this IDictionary<Type, Type> argumentMap)
        {
            return argumentMap
                .OrderBy(p => p.Key.GenericParameterPosition)
                .Select(p => p.Value)
                .ToArray();
        }

        internal static IEnumerable<Type> GetAppearingTypes(this Type type)
        {
            return type != null
                ? EnumerableEx.Return(type).Expand(t => t.GetGenericArguments())
                : Enumerable.Empty<Type>();
        }

        internal static Type GetCorrespondingType(this Type type, Type targetType)
        {
            return targetType.TryGetGenericTypeDefinition()
                .Let(d => type.GetConvertibleTypes()
                    .FirstOrDefault(t => t.TryGetGenericTypeDefinition() == d)
                );
        }

        internal static Boolean IsAppropriate(this Type type, Type target)
        {
            return type.IsAssignableFrom(target)
                || ((type.ContainsGenericParameters || target.ContainsGenericParameters) &&
                       ((((type.IsArray && target.IsArray) || (type.IsByRef && target.IsByRef) || (type.IsPointer && target.IsPointer))
                           && type.GetElementType().IsAppropriate(target.GetElementType())
                       ) || target.GetConvertibleTypes()
                                .Select(t => t.TryGetGenericTypeDefinition())
                                .Contains(type.TryGetGenericTypeDefinition())
                   ))
                || (type.IsGenericParameter && (
                       (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
                           && target.GetConstructor(Type.EmptyTypes) != null
                       ) ||
                       (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
                           && target.IsValueType
                           || target.TryGetGenericTypeDefinition() != typeof(Nullable<>)
                       ) ||
                       (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
                           && !target.IsValueType
                       )
                   ) &&
                   target.GetConvertibleTypes().Let(cs =>
                       type.GetGenericParameterConstraints().All(cs.Contains)
                   ))
                || (typeof(LambdaExpression).IsAssignableFrom(type)
                       && typeof(Delegate).IsAssignableFrom(target)
                       && type.GetDelegateSignature() == target.GetDelegateSignature()
                   );
        }

        internal static MethodInfo GetDelegateSignature(this Type type)
        {
            return (typeof(Delegate).IsAssignableFrom(type)
                ? type
                : type.TryGetGenericTypeDefinition() == typeof(Expression<>)
                      ? type.GetGenericArguments().Single()
                      : null
            ).Null(t => t.GetMethod("Invoke"));
        }

        internal static Type GetDelegateType(this MethodInfo method)
        {
            return Expression.GetDelegateType(method.GetParameters()
                .Select(p => p.ParameterType)
                .EndWith(method.ReturnType)
                .ToArray()
            );
        }

        internal static Type GetEnumerableElementType(this Type type)
        {
            return (type.IsInterface && type.TryGetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : type.GetInterface("IEnumerable`1", false)
            ).GetGenericArguments()[0];
        }

        internal static Type GetCommonType(this IEnumerable<Type> types, Boolean contravariant = false)
        {
            types = types.ToArray();
            return types
                .SelectMany(t => t.GetConvertibleTypes())
                .Distinct()
                .Except(EnumerableEx.Return(typeof(Object)))
                .OrderByDescending(t => t.IsInterface
                    ? t.GetInterfaces().Length
                    : t.Generate(_ => _.BaseType, _ => _ != null)
                          .Count()
                )
                .EndWith(typeof(Object))
                .ToArray()
                .If(_ => contravariant,
                    ts => ts.First(t => ts.All(t_ => t.IsAppropriate(t_) || t_.IsAppropriate(t))),
                    ts => ts.First(t => types.All(t.IsAppropriate))
                )
                .If(ct => ct.IsGenericTypeDefinition, ct => ct.GetGenericArguments()
                    .Let(ps => types.SelectMany(t => ps.Zip(
                        t.GetConvertibleTypes().First(x => x.TryGetGenericTypeDefinition() == ct).GetGenericArguments(),
                        Tuple.Create
                    )))
                    .ToLookup(_ => _.Item1, _ => _.Item2)
                    .Select(ts => (ts.Key.GenericParameterAttributes & GenericParameterAttributes.VarianceMask).Let(f =>
                        f == GenericParameterAttributes.Contravariant
                            ? ts.GetCommonType(true)
                            : f == GenericParameterAttributes.Covariant
                                  ? ts.GetCommonType()
                                  : ts.First()
                        )
                    )
                    .Let(ts => ct.MakeGenericType(ts.ToArray()))
                );
        }

        internal static Type ReplaceGenericArguments(this Type type, IDictionary<Type, Type> typeArgumentMap)
        {
            return type.IsGenericType
                ? type.GetGenericTypeDefinition()
                      .MakeGenericType(
                          type.GetGenericArguments()
                              .Select(t => (typeArgumentMap.GetValue(t) ?? t)
                                  .ReplaceGenericArguments(typeArgumentMap)
                              )
                              .ToArray()
                      )
                : type;
        }

        internal static Int32 GetParameterCount(this Expression expr)
        {
            return expr is AmbiguousLambdaExpression
                ? ((AmbiguousLambdaExpression) expr).Parameters.Count
                : expr is LambdaExpression
                      ? ((LambdaExpression) expr).Parameters.Count
                      : expr.Type.GetDelegateSignature().Null(m => m.GetParameters().Length);
        }

        internal static IEnumerable<ParameterInfo> GetAllParameters(this MethodInfo method)
        {
            return method.GetParameters().EndWith(method.ReturnParameter);
        }

        internal static Type GetNominalType(this Type type)
        {
            return type
                .Generate(t => t.BaseType, t => t != null)
                .First(t => t.IsPublic);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
