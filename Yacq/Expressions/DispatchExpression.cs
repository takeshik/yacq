﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XSpect.Yacq.Dynamic;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an dispatch expression, an abstract layer to member references and method calls with <see cref="SymbolTable"/>.
    /// </summary>
    public partial class DispatchExpression
        : YacqExpression
    {
        private const BindingFlags _instanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        private const BindingFlags _staticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private Type _observableType = null;

        private Expression _left;

        /// <summary>
        /// Gets the type of dispatching of this expression.
        /// </summary>
        /// <value>The type of dispatching of this expression.</value>
        public DispatchTypes DispatchType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets an <see cref="Expression"/> that represents the receiver or static reference for dispatching.
        /// </summary>
        /// <value>An <see cref="Expression"/> that represents the receiver or static reference for dispatching.</value>
        public Expression Left
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name for dispatching.
        /// </summary>
        /// <value>The name for dispatching.</value>
        public String Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of types that represent type arguments for dispatching.
        /// </summary>
        /// <value>a collection of types that represent type arguments for dispatching.</value>
        public ReadOnlyCollection<Type> TypeArguments
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a collection of expressions that represent arguments for dispatching.
        /// </summary>
        /// <value>a collection of expressions that represent arguments for dispatching.</value>
        public ReadOnlyCollection<Expression> Arguments
        {
            get;
            private set;
        }

        internal DispatchExpression(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IList<Type> typeArguments,
            IList<Expression> arguments
        )
            : base(symbols)
        {
            this.DispatchType = dispatchType;
            this.Left = left;
            this.Name = name;
            this.TypeArguments = new ReadOnlyCollection<Type>(typeArguments ?? Type.EmptyTypes);
            this.Arguments = new ReadOnlyCollection<Expression>(arguments ?? Arrays.Empty<Expression>());
            this.SetPosition(arguments.StartWith(left));
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            switch (this.DispatchType & DispatchTypes.TargetMask)
            {
                case DispatchTypes.Member:
                    return this.Arguments.Any()
                        ? this.Left + "[" + this.Arguments.Stringify(", ") + "]"
                        : (this.Left != null ? this.Left + "." : "") + this.Name;
                case DispatchTypes.Method:
                    return (this.Left != null ? this.Left + "." : "")
                        + this.Name
                        + (this.TypeArguments.Any() ? "<" + this.TypeArguments.Stringify(t => t.Name, ", ") + ">" : "")
                        + "(" + this.Arguments.Stringify(", ") + ")";
                case DispatchTypes.Constructor:
                    return this.Left + "(" + this.Arguments.Stringify(", ") + ")";
                default:
                    return "Dispatch(?)";
            }
        }

        // this._left and Candidate.Arguments is already reduced with symbols.

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            this._left = this.Left.Reduce(symbols);
            return symbols.ResolveMatch(this)
                .If(d => d == null, d => symbols.Missing)
                (this, symbols, expectedType);
        }

        /// <summary>
        /// Default definition method of <see cref="SymbolTable.Missing"/>.
        /// </summary>
        /// <param name="e">The expression to be reduced.</param>
        /// <param name="s">The symbol table which this symbol (value) belongs.</param>
        /// <param name="t">The expected <see cref="Expression.Type"/> from the caller, or <c>null</c> if any type will be accepted.</param>
        /// <returns>The reduced expression.</returns>
        public static Expression DefaultMissing(DispatchExpression e, SymbolTable s, Type t)
        {
            return e._left is DynamicExpression || YacqBinder.IsInDynamicContext(s, e.Arguments.StartWith(e.Left))
                ? e.DispatchByDynamicBinder(s, t)
                : e.DispatchByTypeSystem(s, t);
        }

        /// <summary>
        /// Gets all members and extension methods in specified types.
        /// </summary>
        /// <param name="symbols">The symbol table for search extension methods.</param>
        /// <param name="types">Types to get members.</param>
        /// <returns>All members and extension methods in <paramref name="types"/>.</returns>
        public static IEnumerable<MemberInfo> GetMembers(SymbolTable symbols, params Type[] types)
        {
            return types
                .SelectMany(t => t.IsInterface
                    ? t.GetInterfaces().StartWith(t)
                    : Arrays.From(t)
                )
                .Distinct()
                .SelectMany(t => Static.GetTargetType(t)
                    .Null(st => st.GetMembers(_staticFlags))
                    ?? t.GetMembers(_instanceFlags)
                )
                .Concat(symbols.AllLiterals.Values
                    .OfType<TypeCandidateExpression>()
                    .SelectMany(e => e.Candidates)
                    .Where(ct => ct.HasExtensionMethods())
                    .SelectMany(ct => ct.GetMethods(_staticFlags))
                    .Where(m => m.IsExtensionMethod() && m.GetParameters()[0].ParameterType.Let(t => types.Any(t.IsAppropriate)))
#if SILVERLIGHT
                    .Cast<MemberInfo>()
#endif
                )
                .If(
                    _ => types.All(t => t.IsInterface),
                    _ => _.Concat(typeof(Object).GetMembers(_instanceFlags))
                );
        }

        /// <summary>
        /// Gets all members and extension methods in specified types.
        /// </summary>
        /// <param name="symbols">The symbol table for search extension methods.</param>
        /// <param name="types">Types to get members.</param>
        /// <returns>All members and extension methods in <paramref name="types"/>.</returns>
        public static IEnumerable<MemberInfo> GetMembers(SymbolTable symbols, IEnumerable<Type> types)
        {
            return GetMembers(symbols, types.ToArray());
        }

        private Expression DispatchByTypeSystem(SymbolTable symbols, Type expectedType)
        {
            return ImplicitConvert(this.GetMembers(symbols)
                .Select(CreateCandidate)
                .Choose(c => c.ParameterMap != null
                    ? c.ParameterMap
                          .Select(_ => _.Item2.Reduce(symbols, _.Item1))
                          .ToArray()
                          .If(
                              es => es.All(a => a != null),
                              es => c.Clone(argumentNames: new String[c.Parameters.Count], arguments: es),
                              es => null
                          )
                    : null
                )
                .Where(c => c.ParameterMap.Select(_ => _.Item1.GetDelegateSignature().Null(m => m.GetParameters().Length))
                    .SequenceEqual(c.ParameterMap.Select(_ =>
                        (_.Item2 as LambdaExpression).Null(e => e.Parameters.Count,
                            () => (_.Item2 as AmbiguousLambdaExpression).Null(e => e.Parameters.Count),
                            () => _.Item2.Type(symbols).GetDelegateSignature().Null(m => m.GetParameters().Length)
                        )
                    ))
                )
                .Choose(c => InferTypeArguments(c, c.TypeArgumentMap, symbols))
                .Where(c => !c.Arguments.Any(e => e == null || e is YacqExpression))
                .Choose(c => c.TypeArgumentMap.All(p => p.Key.IsAppropriate(p.Value))
                    ? c.Clone(arguments: (c.IsParamArrayContext
                          ? c.Arguments
                                .Take(c.Parameters.Count - 1)
                                .EndWith(Vector(symbols, c.Arguments.Skip(c.Parameters.Count - 1))
                                    .Reduce(symbols, c.Parameters.Last().ParameterType)
                                )
                          : c.ParameterMap.Select(_ => _.Item2)
                      ).ToArray())
                    : null
                )
                .OrderBy(c => c)
                .ThenBy(c => c.Arguments.Sum(e => e.GetParameterCount()))
                .ThenBy(c => c.Arguments.Sum(a => a.Generate(
                    _ => ((UnaryExpression) _).Operand,
                    _ => _ is UnaryExpression && _.NodeType == ExpressionType.Convert,
                    _ => _
                ).Count()))
                .FirstOrDefault()
                .Null(c => this.GetResultExpression(symbols, c))
                    ?? this.DispatchFailback(symbols),
                expectedType
            );
        }

        private Expression DispatchByDynamicBinder(SymbolTable symbols, Type expectedType)
        {
            switch (this.DispatchType & DispatchTypes.TargetMask)
            {
                case DispatchTypes.Member:
                    return Dynamic(
                        YacqBinder.GetMember(symbols, this.Name),
                        typeof(Object),
                        this.Arguments.ReduceAll(symbols)
                            .StartWith((this._left as TypeCandidateExpression)
                                .Null(tc => Constant(Static.Value(tc.ElectedType)))
                                ?? this._left
                            )
                    );
                case DispatchTypes.Method:
                    return Dynamic(
                        YacqBinder.InvokeMember(symbols, this.Name, Enumerable.Repeat("", this.Arguments.Count)),
                        typeof(Object),
                        this.Arguments.ReduceAll(symbols)
                            .StartWith((this._left as TypeCandidateExpression)
                                .Null(tc => Constant(Static.Value(tc.ElectedType)))
                                ?? this._left
                            )
                    );
                default:
                    throw new NotSupportedException("Dispatcher (in dynamic context) doesn't support: " + this.DispatchType);
            }
        }

        private IEnumerable<MemberInfo> GetMembers(SymbolTable symbols)
        {
            switch (this.DispatchType & DispatchTypes.TargetMask)
            {
                case DispatchTypes.Constructor:
                    return GetTypes((TypeCandidateExpression) this._left)
                        .Select(Static.GetTargetType)
                        .ToArray()
                        .Let(ts => GetMembers(symbols, ts)
                            .Where(m =>
                                m.MemberType == MemberTypes.Constructor &&
                                ts.Contains(m.DeclaringType) &&
                                m.GetAccessibility() == MemberAccessibilities.Public
                            )
                        );
                case DispatchTypes.Member:
                    return String.IsNullOrEmpty(this.Name)
                        // Default members must be instance properties.
                        ? this._left.Type.GetDefaultMembers()
                        : GetMembers(symbols, GetTypes(this._left))
                              .Where(m => m.Name == this.Name && (
                                  m.MemberType == MemberTypes.Field ||
                                  m.MemberType == MemberTypes.Property ||
                                  m.MemberType == MemberTypes.Event ||
                                  m.MemberType == MemberTypes.NestedType
                              ) && m.GetAccessibility() == MemberAccessibilities.Public);
                case DispatchTypes.Method:
                    return GetMembers(symbols, GetTypes(this._left))
                        .Where(m =>
                            m.MemberType == MemberTypes.Method &&
                            m.Name == this.Name &&
                            m.GetAccessibility() == MemberAccessibilities.Public
                        );
                default:
                    throw new NotSupportedException("Dispatcher doesn't support: " + this.DispatchType);
            }
        }

        private Candidate CreateCandidate(MemberInfo member)
        {
            return (member as MethodInfo)
                .Null(m => m.IsExtensionMethod() && !(this._left is TypeCandidateExpression))
                .Let(_ => new Candidate(
                    this._left is TypeCandidateExpression || _
                        ? null
                        : this._left,
                    member,
                    this.TypeArguments,
                    this.Arguments
                        .Select(e => e.List(":").Null(l => l.First().Id()))
                        .If(es => _, es => es.StartWith(new String[1]))
                        .ToArray(),
                    this.Arguments
                        .Select(e => e.List(":").Null(l => l.Last(), e))
                        .If(es => _, es => es.StartWith(this._left))
                        .ToArray()
                ));
        }

        private Candidate InferTypeArguments(Candidate candidate, IDictionary<Type, Type> typeArgumentMap, SymbolTable symbols)
        {
            return this.DispatchType != DispatchTypes.Method
                ? candidate
                : new Dictionary<Type, Type>(typeArgumentMap).Let(map =>
                  {
                      if (map.Count == candidate.Method.GetGenericArguments().Length)
                      {
                          return candidate.Clone(
                              member: candidate.Method != null && candidate.Method.IsGenericMethodDefinition
                                  ? candidate.Method.MakeGenericMethod(typeArgumentMap.ToArgumentArray())
                                  : candidate.Member,
                              arguments: candidate.ParameterMap
                                  .Select(_ => _.Item2 is AmbiguousLambdaExpression && _.Item1.GetDelegateSignature() != null
                                      ? ((AmbiguousLambdaExpression) _.Item2)
                                            .ApplyTypeArguments(_.Item1)
                                            .ApplyTypeArguments(map)
                                            .Reduce(symbols, _.Item1.ReplaceGenericArguments(map))
                                      : _.Item2
                                  )
                                  .ToArray()
                          );
                      }
                      else
                      {
                          candidate.ParameterMap
                              .Where(_ => !(_.Item1.IsGenericParameter
                                  ? Arrays.From(_.Item1)
                                  : _.Item1.GetGenericArguments()
                              ).All(map.ContainsKey))
                              .ForEach(_ =>
                              {
                                  if (_.Item1.GetDelegateSignature() != null)
                                  {
                                      if (_.Item2 is AmbiguousLambdaExpression)
                                      {
                                          var method = _.Item1.GetDelegateSignature();
                                          if (!method.ReturnType.Let(r =>
                                              (r.IsGenericParameter ? new [] { r, } : r.GetGenericArguments()).Let(ts => ts.All(map.ContainsKey))
                                          ))
                                          {
                                              method.GetParameters()
                                                  .Select(p => p.ParameterType.Let(t => t.IsGenericParameter
                                                      ? map.GetValue(t)
                                                      : t
                                                  ))
                                                  .If(ts => ts.All(t => t != null), ts =>
                                                      map = new TypeNode(method.ReturnType).Match(map, ((AmbiguousLambdaExpression) _.Item2)
                                                          .ApplyTypeArguments(ts)
                                                          .Type(symbols)
                                                          .GetDelegateSignature()
                                                          .ReturnType
                                                      )
                                                  );
                                          }
                                      }
                                      else
                                      {
                                          map = new TypeNode(_.Item1).Match(map, _.Item2.Type.If(
                                              t => _.Item2 is LambdaExpression,
                                              t => t.GetDelegateSignature().GetDelegateType()
                                          ));
                                      }
                                  }
                                  else if (_.Item1.ContainsGenericParameters)
                                  {
                                      (_.Item1.IsGenericParameter
                                          ? Arrays.From(Tuple.Create(_.Item1, _.Item2.Type))
                                          : _.Item1.GetAppearingTypes()
                                                .Zip(_.Item2.Type.GetCorrespondingType(_.Item1).GetAppearingTypes(), Tuple.Create)
                                                .Where(t => t.Item1.IsGenericParameter)
                                      ).ForEach(t => map = new TypeNode(t.Item1).Match(map, t.Item2));
                                  }
                              });
                          return map.Keys.All(typeArgumentMap.ContainsKey)
                              ? null
                              : this.InferTypeArguments(candidate, map, symbols);
                    }
                  });
        }

        private Expression GetResultExpression(SymbolTable symbols, Candidate c)
        {
            switch (this.DispatchType)
            {
                case DispatchTypes.Constructor:
                    return New(c.Constructor, c.Arguments);
                case DispatchTypes.Member:
                    switch (c.Member.MemberType)
                    {
                        case MemberTypes.Field:
                            return Field(c.Instance, c.Field);
                        case MemberTypes.Property:
                            return c.Arguments.Any()
                                ? (Expression) Property(c.Instance, c.Property, c.Arguments)
                                : Property(c.Instance, c.Property);
                        case MemberTypes.Event:
                            if (_observableType == null &&
                                (_observableType = Assembly.Load("System.Reactive.Linq")
                                    .Null(a => a.GetType("System.Reactive.Linq.Observable"))
                                ) == null
                            )
                            {
                                throw new NotSupportedException("Events are supported only if you load the assembly 'System.Reactive.Linq'.");
                            }
                            return
                                typeof(Action<>).MakeGenericType(c.Event.EventHandlerType).Let(t => Call(
                                    _observableType,
                                    "FromEventPattern",
                                    new []
                                    {
                                        c.Event.EventHandlerType,
                                        c.Event.EventHandlerType.GetMethod("Invoke").GetParameters()[1].ParameterType,
                                    },
                                    Convert(Call(
                                        typeof(Delegate),
                                        "CreateDelegate",
                                        Type.EmptyTypes,
                                        Constant(t),
                                        this._left is TypeCandidateExpression
                                            ? Default(typeof(Object))
                                            : this._left,
                                        Constant(c.Event.GetAddMethod())
                                    ), t),
                                    Convert(Call(
                                        typeof(Delegate),
                                        "CreateDelegate",
                                        Type.EmptyTypes,
                                        Constant(t),
                                        this._left is TypeCandidateExpression
                                            ? Default(typeof(Object))
                                            : this._left,
                                        Constant(c.Event.GetRemoveMethod())
                                    ), t)
                                ));
                        case MemberTypes.NestedType:
                            return TypeCandidate(symbols, c.Type);
                        default:
                            return null;
                    }
                default: // case DispatchType.Method:
                    return Call(c.Instance, c.Method, c.Arguments);
            }
        }

        private Expression DispatchFailback(SymbolTable symbols)
        {
            // Default constructor of value types
            if (this.DispatchType.HasFlag(DispatchTypes.Constructor)
                && ((TypeCandidateExpression) this._left).ElectedType.IsValueType
                && !this.Arguments.Any()
            )
            {
                return Default(((TypeCandidateExpression) this._left).ElectedType);
            }
            // Cast Operation
            else if (this.DispatchType.HasFlag(DispatchTypes.Constructor)
                && ((TypeCandidateExpression) this._left).ElectedType != null
                && this.Arguments.Count == 1)
            {
                return Convert(
                    this.Arguments
                        .Single()
                        .Reduce(symbols),
                    ((TypeCandidateExpression) this._left).ElectedType
                );
            }
            else
            {
                throw new ParseException("Dispatch failed: " + this, this);
            }
        }

        private static IEnumerable<Type> GetTypes(Expression expression)
        {
            return expression is TypeCandidateExpression
                ? GetTypes((TypeCandidateExpression) expression)
                : Arrays.From(expression.Type);
        }

        private static IEnumerable<Type> GetTypes(TypeCandidateExpression expression)
        {
            return expression.Candidates.Select(Static.Type);
        }
    }

    partial class YacqExpression
    {
        #region Dispatch

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return new DispatchExpression(
                symbols,
                dispatchType,
                left,
                name,
                typeArguments != null ? typeArguments.ToArray() : Type.EmptyTypes,
                arguments
            );
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            Expression left,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, dispatchType, left, name, Type.EmptyTypes, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, dispatchType, null, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, dispatchType, null, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(
                symbols,
                dispatchType,
                left,
                name,
                typeArguments,
                arguments != null ? arguments.ToArray() : Arrays.Empty<Expression>()
            );
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, left, name, null, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, null, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchTypes dispatchType,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, null, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            Expression left,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            Expression left,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the dispatching, member reference or method calls.
        /// </summary>
        /// <param name="dispatchType">The dispatching type.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>An <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Dispatch(
            DispatchTypes dispatchType,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, name, arguments);
        }

        #endregion

        #region Variable

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the member-reference dispatching, without the receiver.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <returns>A variable-reference <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Variable(
            SymbolTable symbols,
            String name
        )
        {
            return Dispatch(symbols, DispatchTypes.Member, name);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the member-reference dispatching, without the receiver.
        /// </summary>
        /// <param name="name">The name to use for dispatching.</param>
        /// <returns>A variable-reference <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Variable(
            String name
        )
        {
            return Dispatch(DispatchTypes.Member, name);
        }

        #endregion

        #region Function

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the function-call dispatching, without the receiver.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A function-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Function(
            SymbolTable symbols,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, DispatchTypes.Method, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the function-call dispatching, without the receiver.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A function-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Function(
            SymbolTable symbols,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, DispatchTypes.Method, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the function-call dispatching, without the receiver.
        /// </summary>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A function-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Function(
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(DispatchTypes.Method, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the function-call dispatching, without the receiver.
        /// </summary>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A function-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Function(
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(DispatchTypes.Method, name, arguments);
        }

        #endregion
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
