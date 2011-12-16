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
    internal static class Extensions
    {
        internal static Boolean If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate)
        {
            return predicate(self);
        }

        internal static TResult If<TReceiver, TResult>(this TReceiver self, Func<TReceiver, Boolean> predicate, TResult valueIfTrue, TResult valueIfFalse)
        {
            if (self == null)
            {
                return default(TResult);
            }
            else if (self == null || predicate(self))
            {
                return valueIfTrue;
            }
            else
            {
                return valueIfFalse;
            }
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, TReceiver valueIfTrue)
        {
            return self.If(predicate, valueIfTrue, self);
        }

        internal static TResult If<TReceiver, TResult>(this TReceiver self, Func<TReceiver, Boolean> predicate, Func<TReceiver, TResult> funcIfTrue, Func<TReceiver, TResult> funcIfFalse)
        {
            if (predicate(self))
            {
                return funcIfTrue(self);
            }
            else
            {
                return funcIfFalse(self);
            }
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Func<TReceiver, TReceiver> funcIfTrue)
        {
            return self.If(predicate, funcIfTrue, _ => _);
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Action<TReceiver> actionIfTrue, Action<TReceiver> actionIfFalse)
        {
            if (predicate(self))
            {
                actionIfTrue(self);
            }
            else
            {
                actionIfFalse(self);
            }
            return self;
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Action<TReceiver> actionIfTrue)
        {
            return self.If(predicate, actionIfTrue, _ =>
            {
            });
        }

        internal static TResult Null<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func, TResult valueIfNull)
            where TReceiver : class
        {
            if (self == null)
            {
                return valueIfNull;
            }
            else
            {
                return func(self);
            }
        }

        internal static TResult Null<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func)
            where TReceiver : class
        {
            return Null(self, func, default(TResult));
        }

        internal static void Null<TReceiver>(this TReceiver self, Action<TReceiver> action)
        {
            if (self != null)
            {
                action(self);
            }
        }

        internal static TResult Let<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func)
        {
            return func(self);
        }

        internal static TReceiver Apply<TReceiver>(this TReceiver self, params Action<TReceiver>[] actions)
        {
            return Apply(self, (IEnumerable<Action<TReceiver>>) actions);
        }

        internal static TReceiver Apply<TReceiver>(this TReceiver self, IEnumerable<Action<TReceiver>> actions)
        {
            foreach (var a in actions)
            {
                a(self);
            }
            return self;
        }

        internal static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TSource : class
        {
            return source.Select(selector).Where(_ => _ != null);
        }

        internal static Expression TryConvert(this Expression expr, Type type)
        {
            return type == null || expr.Type == type
                ? expr
                : Expression.Convert(expr, type);
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
                typeof(Expression).IsAssignableFrom(self.GetType())
                    ? EqualsExact((Expression) self, (Expression) other)
                    : typeof(IEnumerable<Object>).IsAssignableFrom(self.GetType())
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

        internal static Boolean IsParamArrayMethod(this IEnumerable<ParameterInfo> parameters)
        {
            return parameters.Any() && Attribute.IsDefined(parameters.Last(), typeof(ParamArrayAttribute));
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

        internal static Type TryGetGenericTypeDefinition(this Type t)
        {
            return t != null && t.IsGenericType && !t.IsGenericTypeDefinition ? t.GetGenericTypeDefinition() : t;
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
            return (type.IsGenericParameter &&
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

        internal static Type GetEnumerableElementType(this Type type)
        {
            return type
                .GetInterface("IEnumerable`1", false)
                .GetGenericArguments()[0];
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
                      : 0;
        }
    }
}