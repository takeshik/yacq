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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace XSpect.Yacq.Expressions
{
    public partial class DispatchExpression
        : YacqExpression
    {
        private Expression _left;

        public DispatchType DispatchType
        {
            get;
            private set;
        }

        public Expression Left
        {
            get;
            private set;
        }

        public String Name
        {
            get;
            private set;
        }

        public ReadOnlyCollection<Type> TypeArguments
        {
            get;
            private set;
        }

        public ReadOnlyCollection<Expression> Arguments
        {
            get;
            private set;
        }

        internal DispatchExpression(
            SymbolTable symbols,
            DispatchType dispatchType,
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
            this.TypeArguments = new ReadOnlyCollection<Type>(typeArguments);
            this.Arguments = new ReadOnlyCollection<Expression>(arguments);
        }

        public override String ToString()
        {
            switch (this.DispatchType & DispatchType.TargetMask)
            {
                case DispatchType.Member:
                    return this.Arguments.Any()
                        ? this.Left + "[" + String.Join(", ", this.Arguments.Select(e => e.ToString())) + "]"
                        : (this.Left != null ? this.Left + "." : "") + this.Name;
                case DispatchType.Method:
                    return (this.Left != null ? this.Left + "." : "")
                        + this.Name
                        + (this.TypeArguments.Any() ? "<" + String.Join(", ", this.TypeArguments.Select(t => t.Name)) + ">" : "")
                        + "(" + String.Join(", ", this.Arguments.Select(e => e.ToString())) + ")";
                case DispatchType.Constructor:
                    return this.Left + "(" + String.Join(", ", this.Arguments.Select(e => e.ToString())) + ")";
                default:
                    return "Dispatch(?)";
            }
        }

        // this._left and Candidate.Arguments is already reduced with symbols.

        protected override Expression ReduceImpl(SymbolTable symbols)
        {
            this._left = this.Left.Reduce(symbols);
            return symbols.ResolveMatch(this).If(
                d => d != null,
                d => d(this, symbols),
                d => this.GetMembers(symbols)
                    .Select(m => m is MethodInfo && ((MethodInfo) m).IsExtensionMethod()
                        ? new Candidate(
                              null,
                              m,
                              this.TypeArguments,
                              this.Arguments
                                  .ReduceAll(symbols)
                                  .StartWith(this._left)
                                  .ToArray()
                          )
                        : new Candidate(
                              this._left is TypeCandidateExpression ? null : this._left,
                              m,
                              this.TypeArguments,
                              this.Arguments
                                  .ReduceAll(symbols)
                                  .ToArray()
                          )
                    )
                    .Where(c => c.Arguments.Count == c.Parameters.Count
                        || (c.Parameters.IsParamArrayMethod() && c.Arguments.Count >= c.Parameters.Count - 1)
                        && c.Parameters.Select(p => typeof(Delegate).IsAssignableFrom(p.ParameterType)
                               ? p.ParameterType.GetDelegateSignature().GetParameters().Length
                               : 0
                           ).SequenceEqual(c.Arguments.Select(a => a is AmbiguousLambdaExpression
                               ? ((AmbiguousLambdaExpression) a).Parameters.Count
                               : a is LambdaExpression
                                     ? ((LambdaExpression) a).Parameters.Count
                                     : 0
                           ))
                    )
                    .Choose(c => InferTypeArguments(c, c.TypeArgumentMap, symbols))
                    .Choose(CheckAndFixArguments)
                    .OrderBy(c => c)
                    .FirstOrDefault()
                    .Null(c =>
                    {
                        switch (this.DispatchType)
                        {
                            case DispatchType.Constructor:
                                return New(c.Constructor, c.Arguments);
                            case DispatchType.Member:
                                return c.Property != null
                                    ? c.Arguments.Any()
                                          ? (Expression) Property(c.Instance, c.Property, c.Arguments)
                                          : Property(c.Instance, c.Property)
                                    : Field(c.Instance, c.Field);
                            default: // case DispatchType.Method:
                                return Call(c.Instance, c.Method, c.Arguments);
                        }
                    })
            ) ?? this.DispatchMissing(symbols);
        }

        public IEnumerable<MemberInfo> GetMembers(SymbolTable symbols)
        {
            switch (this.DispatchType)
            {
                case DispatchType.Constructor:
                    return ((TypeCandidateExpression) this._left).ElectedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                case DispatchType.Member:
                    return String.IsNullOrEmpty(this.Name)
                        // Default members must be instance properties.
                        ? this._left.Type.GetDefaultMembers()
                        : (this._left is TypeCandidateExpression
                              ? ((TypeCandidateExpression) this._left).ElectedType.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                              : this._left.Type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                          ).Where(m => m.Name == this.Name && (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property));
                case DispatchType.Method:
                    return this._left is TypeCandidateExpression
                        ? ((IEnumerable<MethodInfo>) ((TypeCandidateExpression) this._left).ElectedType
                              .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                          )
                              .If(_ => ((TypeCandidateExpression) this._left).ElectedType.IsInterface, _ =>
                                  _.Concat(typeof(Object).GetMethods(BindingFlags.Public | BindingFlags.Static))
                              )
                              .Where(m => m.Name == this.Name)
                        : ((IEnumerable<MethodInfo>) this._left.Type
                              .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                          )
                              .If(_ => this._left.Type.IsInterface, _ =>
                                  _.Concat(typeof(Object).GetMethods(BindingFlags.Public | BindingFlags.Instance))
                              )
                              .Where(m => m.Name == this.Name)
                              .Concat(symbols.AllLiterals.Values
                                  .OfType<TypeCandidateExpression>()
                                  .SelectMany(e => e.Candidates)
                                  .Where(t => t.HasExtensionMethods())
                                  .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                                  .Where(m => m.Name == this.Name && m.IsExtensionMethod())
                              );
                default:
                    throw new NotSupportedException("Dispatcher doesn't support: " + this.DispatchType);
            }
        }

        private Candidate InferTypeArguments(Candidate candidate, IDictionary<Type, Type> typeArgumentMap, SymbolTable symbols)
        {
            return this.DispatchType != DispatchType.Method
                ? candidate
                : new Dictionary<Type, Type>(typeArgumentMap).Let(map =>
                  {
                      if (map.Count == candidate.Method.GetGenericArguments().Length)
                      {
                          return new Candidate(
                              candidate.Instance,
                              candidate.Method != null && candidate.Method.IsGenericMethodDefinition
                                  ? candidate.Method.MakeGenericMethod(typeArgumentMap.ToArgumentArray())
                                  : candidate.Member,
                              map,
                              candidate.ParameterMap
                                  .Select(_ => _.Item2 is AmbiguousLambdaExpression && _.Item1.GetDelegateSignature() != null
                                      ? ((AmbiguousLambdaExpression) _.Item2)
                                            .ApplyTypeArguments(_.Item1)
                                            .ApplyTypeArguments(map)
                                            .Reduce(symbols)
                                      : _.Item2
                                  )
                                  .ToArray()
                          );
                      }
                      else
                      {
                          candidate.ParameterMap
                              .Where(_ => (_.Item1.IsGenericParameter
                                  ? EnumerableEx.Return(_.Item1)
                                  : _.Item1.GetGenericArguments()
                              )
                                  .Any(t => !map.ContainsKey(t))
                              )
                              .ForEach(_ =>
                              {
                                  if (_.Item2 is AmbiguousLambdaExpression && _.Item1.GetDelegateSignature() != null)
                                  {
                                      if (_.Item1.GetDelegateSignature().ReturnType.Let(r =>
                                          r.IsGenericParameter && !map.ContainsKey(r)
                                      ))
                                      {
                                          _.Item1.GetDelegateSignature().GetParameters()
                                              .Select(p => p.ParameterType)
                                              .Where(t => t.IsGenericParameter)
                                              .Select(t => map.ContainsKey(t) ? map[t] : null)
                                              .If(ts => ts.All(t => t != null), ts =>
                                                  map[_.Item1.GetDelegateSignature().ReturnType] = ((AmbiguousLambdaExpression) _.Item2)
                                                      .ApplyTypeArguments(ts)
                                                      .Type(symbols)
                                                      .GetDelegateSignature()
                                                      .ReturnType
                                              );
                                      }
                                  }
                                  else if (_.Item1.ContainsGenericParameters)
                                  {
                                      (_.Item1.IsGenericParameter
                                          ? EnumerableEx.Return(Tuple.Create(_.Item1, _.Item2.Type))
                                          : _.Item1.GetAppearingTypes()
                                                .Zip(_.Item2.Type.GetCorrespondingType(_.Item1).GetAppearingTypes(), Tuple.Create)
                                                .Where(t => t.Item1.IsGenericParameter)
                                      ).ForEach(t => map[t.Item1] = t.Item2);
                                  }
                              });
                          return map.Keys.All(typeArgumentMap.ContainsKey)
                              ? null
                              : this.InferTypeArguments(candidate, map, symbols);
                      }
                  });
        }

        private static Candidate CheckAndFixArguments(Candidate candidate)
        {
            return candidate.TypeArgumentMap.All(p => p.Key.IsAppropriate(p.Value))
                && candidate.ParameterMap.All(_ => _.Item1.IsAppropriate(_.Item2.Type))
                ? new Candidate(
                      candidate.Instance,
                      candidate.Member,
                      candidate.TypeArgumentMap,
                      candidate.ParameterMap
                          .Select(_ => typeof(LambdaExpression).IsAssignableFrom(_.Item1)
                              ? Quote(_.Item2)
                              : !_.Item1.IsValueType && _.Item2.Type.IsValueType
                                    ? Convert(_.Item2, _.Item1)
                                    : _.Item2
                          )
                          .If(_ => candidate.Parameters.IsParamArrayMethod(), _ =>
                              _.Take(candidate.Parameters.Count - 1)
                                  .Concat(EnumerableEx.Return(NewArrayInit(
                                      candidate.Parameters.Last().ParameterType.GetElementType(),
                                      _.Skip(candidate.Parameters.Count - 1)
                                  )))
                          )
                          .ToArray()
                  )
                : null;
        }

        private Expression DispatchMissing(SymbolTable symbols)
        {
            // Cast Operation
            if (this.DispatchType.HasFlag(DispatchType.Constructor)
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
                throw new InvalidOperationException("Dispatch failed: " + this);
            }
        }
    }

    partial class YacqExpression
    {
        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
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
                (typeArguments ?? Enumerable.Empty<Type>()).ToArray(),
                arguments
            );
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            Expression left,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, dispatchType, left, name, Enumerable.Empty<Type>(), arguments);
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, dispatchType, null, name, (typeArguments ?? Enumerable.Empty<Type>()).ToArray(), arguments);
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(symbols, dispatchType, null, name, arguments);
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, left, name, typeArguments, arguments.ToArray());
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            Expression left,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, left, name, arguments.ToArray());
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, name, typeArguments, arguments.ToArray());
        }

        public static DispatchExpression Dispatch(
            SymbolTable symbols,
            DispatchType dispatchType,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(symbols, dispatchType, name, arguments.ToArray());
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, typeArguments, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            Expression left,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, name, typeArguments, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            String name,
            params Expression[] arguments
        )
        {
            return Dispatch(null, dispatchType, name, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, typeArguments, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            Expression left,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, left, name, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            String name,
            IEnumerable<Type> typeArguments,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, name, typeArguments, arguments);
        }

        public static DispatchExpression Dispatch(
            DispatchType dispatchType,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return Dispatch(null, dispatchType, name, arguments);
        }
    }
}
