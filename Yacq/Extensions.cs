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

        internal static Boolean EqualsExact(this Expression self, Expression other)
        {
            return self.GetType() == other.GetType() &&
                self.GetType().GetConvertibleTypes()
                    .SelectMany(t => t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    .All(f => EqualsExact(f.GetValue(self), f.GetValue(other)));
        }

        private static Boolean EqualsExact(this Object self, Object other)
        {
            return self == other || (self != null && other != null &&
                self is Expression
                    ? EqualsExact((Expression) self, (Expression) other)
                    : self is IEnumerable<object>
                          ? ((IEnumerable<Object>) self).Zip((IEnumerable<Object>) other, EqualsExact).All(_ => _)
                          : self.Equals(other)
            );
        }

        internal static IEnumerable<Type> GetConvertibleTypes(this Type type)
        {
            return type != null
                ? EnumerableEx.Concat(
                      EnumerableEx.Generate(type, t => t != null, t => t.BaseType, _ => _),
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

        internal static IEnumerable<MethodInfo> GetExtensionMethods(this Type type)
        {
            return type.HasExtensionMethods()
                ? type.GetMethods().Where(IsExtensionMethod)
                : Enumerable.Empty<MethodInfo>();
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
            return ((type.IsArray && target.IsArray) || (type.IsByRef && target.IsByRef) || (type.IsPointer && target.IsPointer)
                && type.GetElementType().IsAppropriate(target.GetElementType())
            ) ||
                (type.IsGenericParameter &&
                    !(
                        (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
                            && target.GetConstructor(Type.EmptyTypes) == null
                        ) ||
                        (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint)
                            && target.IsClass
                            || target.TryGetGenericTypeDefinition() == typeof(Nullable<>)
                        ) ||
                        (type.GenericParameterAttributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint)
                            && target.IsValueType
                        )
                    ) &&
                    target.GetConvertibleTypes().Let(cs =>
                        type.GetGenericParameterConstraints().All(cs.Contains)
                    )
                ) ||
                    target.GetConvertibleTypes()
                        .Select(t => t.TryGetGenericTypeDefinition())
                        .Contains(type.TryGetGenericTypeDefinition())
                // Special matches:
                || (
                    typeof(LambdaExpression).IsAssignableFrom(type) &&
                    typeof(Delegate).IsAssignableFrom(target) &&
                    type.GetDelegateSignature() == target.GetDelegateSignature()
                )
                || (type.IsValueType && target == typeof(Object));
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

        internal static Type GetCommonType(this IEnumerable<Type> types)
        {
            return types
                .SelectMany(t => t.GetConvertibleTypes())
                .Distinct()
                .Except(EnumerableEx.Return(typeof(Object)))
                .OrderByDescending(t => EnumerableEx
                    .Generate(t, _ => _.BaseType != null, _ => _.BaseType, _ => _)
                    .Count()
                )
                .EndWith(typeof(Object))
                .First(t => types.All(t.IsAssignableFrom));
        }

        internal static ParameterInfo[] GetParameters(this MemberInfo member)
        {
            return member is MethodBase
                ? ((MethodBase) member).GetParameters()
                : member is PropertyInfo
                      ? ((PropertyInfo) member).GetIndexParameters()
                      : new ParameterInfo[0];
        }

        internal static Type ReplaceGenericArguments(this Type type, IDictionary<Type, Type> typeArgumentMap)
        {
            return type.IsGenericType
                ? type.GetGenericTypeDefinition()
                      .MakeGenericType(
                          type.GetGenericArguments()
                              .Select(t => (typeArgumentMap.TryGetValue(t) ?? t)
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
            return EnumerableEx.Generate(type, t => t != null, t => t.BaseType, _ => _)
                .First(t => t.IsPublic);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
