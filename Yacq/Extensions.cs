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

namespace XSpect.Yacq
{
    internal static class Extensions
    {
        internal static TReturn Let<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
        {
            return func(self);
        }

        internal static TReceiver Apply<TReceiver>(this TReceiver self, params Action<TReceiver>[] actions)
        {
            Array.ForEach(actions, a => a(self));
            return self;
        }

        internal static TReturn Dispose<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
            where TReceiver : IDisposable
        {
            using (self)
            {
                return func(self);
            }
        }

        internal static void Dispose<TReceiver>(this TReceiver self, Action<TReceiver> func)
            where TReceiver : IDisposable
        {
            using (self)
            {
                func(self);
            }
        }

        internal static TReturn Default<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, Func<TReturn> funcIfDefault)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : funcIfDefault();
        }

        internal static TReturn Default<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfDefault)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : valueIfDefault;
        }

        internal static TReturn Default<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : default(TReturn);
        }

        internal static TReceiver Default<TReceiver>(this TReceiver self, Action<TReceiver> action, Func<TReceiver> funcIfDefault)
        {
            if (EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver)))
            {
                action(self);
                return self;
            }
            else
            {
                return funcIfDefault();
            }
        }

        internal static TReceiver Default<TReceiver>(this TReceiver self, Action<TReceiver> action, TReceiver valueIfDefault)
        {
            if (EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver)))
            {
                action(self);
                return self;
            }
            else
            {
                return valueIfDefault;
            }
        }

        internal static TReceiver Default<TReceiver>(this TReceiver self, Action<TReceiver> action)
        {
            if (EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver)))
            {
                action(self);
                return self;
            }
            else
            {
                return default(TReceiver);
            }
        }

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, Func<TReturn> funcIfNull)
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : funcIfNull();
        }

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfNull)
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : valueIfNull;
        }

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : default(TReturn);
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action, Func<TReceiver> funcIfNull)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return funcIfNull();
            }
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action, TReceiver valueIfNull)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return valueIfNull;
            }
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return default(TReceiver);
            }
        }

        internal static Nullable<TReturn> Nullable<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, Func<TReturn> funcIfDefault)
            where TReturn : struct
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : funcIfDefault();
        }

        internal static Nullable<TReturn> Nullable<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfDefault)
            where TReturn : struct
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : valueIfDefault;
        }

        internal static Nullable<TReturn> Nullable<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
            where TReturn : struct
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : default(Nullable<TReturn>);
        }

        internal static TReturn If<TReceiver, TReturn>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            TReturn then,
            TReturn otherwise
        )
        {
            return predicate(self)
                ? then
                : otherwise;
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            TReceiver then
        )
        {
            return predicate(self)
                ? then
                : self;
        }

        internal static TReturn If<TReceiver, TReturn>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Func<TReceiver, TReturn> then,
            Func<TReceiver, TReturn> otherwise
        )
        {
            return predicate(self)
                ? then(self)
                : otherwise(self);
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Func<TReceiver, TReceiver> then
        )
        {
            return predicate(self)
                ? then(self)
                : self;
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Action<TReceiver> then,
            Action<TReceiver> otherwise
        )
        {
            return predicate(self)
                ? self.Apply(then)
                : self.Apply(otherwise);
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Action<TReceiver> then
        )
        {
            return predicate(self)
                ? self.Apply(then)
                : self;
        }

        internal static Boolean StartsWithInvariant(this String str, String value)
        {
            return str.StartsWith(value, StringComparison.InvariantCulture);
        }

        internal static Boolean EndsWithInvariant(this String str, String value)
        {
            return str.EndsWith(value, StringComparison.InvariantCulture);
        }

        internal static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TSource : class
        {
            return source.Select(selector).Where(_ => _ != null);
        }

        internal static IEnumerable<IList<TSource>> PartitionBy<TSource>(this IEnumerable<TSource> source, Func<TSource, Boolean> predicate)
        {
            var list = new List<TSource>();
            var isInSplitter = false;
            foreach (var e in source)
            {
                if (predicate(e) != isInSplitter)
                {
                    isInSplitter = !isInSplitter;
                    if (list.Any())
                    {
                        yield return list;
                        list = new List<TSource>();
                    }
                }
                list.Add(e);
            }
            if (list.Any())
            {
                yield return list;
            }
        }

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

        internal static IEnumerable<Expression> GetDescendants(this Expression self)
        {
            return self.GetType().GetConvertibleTypes()
#if SILVERLIGHT
                .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => 
                        (typeof(Expression).IsAssignableFrom(p.PropertyType)
                            || p.PropertyType.GetInterfaces()
                                   .Any(_ => _.TryGetGenericTypeDefinition() == typeof(IEnumerable<>)
                                       && typeof(Expression).IsAssignableFrom(_.GetGenericArguments()[0])
                                   )
                        ) && p.GetIndexParameters().IsEmpty()
                    )
                )
                .Select(p => p.GetValue(self, null))
#else
                .SelectMany(t => t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                .Select(p => p.GetValue(self))
#endif
                .SelectMany(_ => _ as IEnumerable<Expression>
                    ?? (_ is Expression
                           ? EnumerableEx.Return((Expression) _)
                           : Enumerable.Empty<Expression>()
                       )
                )
                .SelectMany(GetDescendants)
                .StartWith(self);
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
#if SILVERLIGHT
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
                .Concat(EnumerableEx.Return(method.ReturnType))
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
                .Concat(EnumerableEx.Return(typeof(Object)))
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
                              .Select(t => (typeArgumentMap.ContainsKey(t)
                                  ? typeArgumentMap[t]
                                  : t
                              ).ReplaceGenericArguments(typeArgumentMap))
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
            return method.GetParameters().Concat(EnumerableEx.Return(method.ReturnParameter));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
