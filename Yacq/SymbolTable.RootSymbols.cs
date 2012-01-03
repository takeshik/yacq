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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using XSpect.Yacq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;
using XSpect.Yacq.SystemObjects;
#if !__MonoCS__
using System.Reactive;
using System.Reactive.Linq;
#endif

namespace XSpect.Yacq
{
    partial class SymbolTable
    {
        /// <summary>
        /// Returns the root <see cref="SymbolTable"/>, the common symbols for all <see cref="YacqExpression"/>.
        /// </summary>
        /// <value>
        /// The root <see cref="SymbolTable"/>, the common symbols for all <see cref="YacqExpression"/>.
        /// </value>
        public static SymbolTable Root
        {
            get;
            private set;
        }

        static SymbolTable()
        {
            Root = new SymbolTable()
                .Apply(s => s.Import(typeof(RootSymbols)));
        }

        private static class RootSymbols
        {
            /* The Guideline of Reduce(SymbolTable) Method:
             *   * If you are creating YacqExpression objects, you should not Reduce(s) arguments of
             *     factory methods and returned YacqExpression.
             *   * However, if you are creating Expression (non-YacqExpression) objects, you MUST
             *     Reduce(s) ALL arguments of factory methods, however argument is YacqExpression.
             *     All arguments in Expression factory methods MUST keep they are not need to call
             *     Reduce(s). The returned Expression may not Reduce(s) since it is not YacqExpression.
             *   * In YacqExpression-derived classes, all YacqExpression objects they have may be
             *     called Reduce(symbols), or not be called, by their needs. You MUST NOT
             *     Reduce(this.Symbols) in constructors because of symbols argument in
             *     Reduce(SymbolTable) method.
             */
    
            #region Function - Arithmetic

            [YacqSymbol(DispatchTypes.Method, "=")]
            public static Expression Assign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Assign(e.Arguments[0].Reduce(s), e.Arguments.Last().Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "=", e.Arguments
                              .Skip(1)
                              .SkipLast(1)
                              .Concat(new [] { _, })
                          )
                        : _
                    );
            }

            [YacqSymbol(DispatchTypes.Method, "+")]
            public static Expression Plus(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any(a => a.Type(s) == typeof(String))
                    ? YacqExpression.TypeCandidate(typeof(String)).Method(s, "Concat", e.Arguments)
                    : e.Arguments.Count == 1
                          ? Expression.UnaryPlus(e.Arguments[0].Reduce(s))
                          : Expression.Add(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                                .Let(_ => e.Arguments.Count > 2
                                    ? (Expression) YacqExpression.Function(s, "+", e.Arguments
                                          .Skip(2)
                                          .StartWith(_)
                                      )
                                    : _
                                );
            }

            [YacqSymbol(DispatchTypes.Method, "+=")]
            public static Expression PlusAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any(a => a.Type(s) == typeof(String))
                    ? Expression.Assign(
                          e.Arguments[0].Reduce(s),
                          YacqExpression.TypeCandidate(typeof(String)).Method(s, "Concat", e.Arguments)
                      )
                    : Expression.AddAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                          ? e.Arguments[1].Reduce(s)
                          : YacqExpression.Function(s, "+", e.Arguments.Skip(1)).Reduce(s)
                      );
            }

            [YacqSymbol(DispatchTypes.Method, "-")]
            public static Expression Minus(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 1
                    ? Expression.Negate(e.Arguments[0].Reduce(s))
                    : Expression.Subtract(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                          .Let(_ => e.Arguments.Count > 2
                              ? (Expression) YacqExpression.Function(s, "-", e.Arguments
                                    .Skip(2)
                                    .StartWith(_)
                                )
                              : _
                          );
            }

            [YacqSymbol(DispatchTypes.Method, "-=")]
            public static Expression MinusAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.SubtractAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "+", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "*")]
            public static Expression Multiply(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Multiply(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "*", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }

            [YacqSymbol(DispatchTypes.Method, "*=")]
            public static Expression MultiplyAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.MultiplyAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "*", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "/")]
            public static Expression Divide(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Divide(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "/", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "/=")]
            public static Expression DivideAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.DivideAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "*", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "**")]
            public static Expression Power(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Power(e.Arguments[0].Reduce(s, typeof(Double)), e.Arguments[1].Reduce(s, typeof(Double)))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "**", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "**=")]
            public static Expression PowerAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.PowerAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s, typeof(Double))
                    : YacqExpression.Function(s, "**", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "%")]
            public static Expression Modulo(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Modulo(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "%", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "%=")]
            public static Expression ModuloAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? Expression.ModuloAssign(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : Expression.Assign(
                          e.Arguments[0].Reduce(s),
                          YacqExpression.Function(s, "%", e.Arguments).Reduce(s)
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<<")]
            public static Expression LeftShift(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.LeftShift(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "<<", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<<=")]
            public static Expression LeftShiftAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? Expression.LeftShiftAssign(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : Expression.Assign(
                          e.Arguments[0].Reduce(s),
                          YacqExpression.Function(s, "<<", e.Arguments).Reduce(s)
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">>")]
            public static Expression RightShift(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.RightShift(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, ">>", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">>=")]
            public static Expression RightShiftAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? Expression.RightShiftAssign(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : Expression.Assign(
                          e.Arguments[0].Reduce(s),
                          YacqExpression.Function(s, ">>", e.Arguments).Reduce(s)
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "++")]
            public static Expression Increment(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.Increment(e.Arguments[0].Reduce(s));
            }
            
            [YacqSymbol(DispatchTypes.Method, "++=")]
            public static Expression PreIncrementAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.PreIncrementAssign(e.Arguments[0].Reduce(s));
            }
            
            [YacqSymbol(DispatchTypes.Method, "=++")]
            public static Expression PostIncrementAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.PostIncrementAssign(e.Arguments[0].Reduce(s));
            }
            
            [YacqSymbol(DispatchTypes.Method, "--")]
            public static Expression Decrement(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.Decrement(e.Arguments[0].Reduce(s));
            }
            
            [YacqSymbol(DispatchTypes.Method, "--=")]
            public static Expression PreDecrementAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.PreDecrementAssign(e.Arguments[0].Reduce(s));
            }
            
            [YacqSymbol(DispatchTypes.Method, "=--")]
            public static Expression PostDecrementAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.PostDecrementAssign(e.Arguments[0].Reduce(s));
            }
            
            #endregion

            #region Function - Logical

            [YacqSymbol(DispatchTypes.Method, "!")]
            public static Expression Not(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Not(e.Arguments[0].Reduce(s));
            }

            [YacqSymbol(DispatchTypes.Method, "~")]
            public static Expression OnesComplement(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.OnesComplement(e.Arguments[0].Reduce(s));
            }
            
            [YacqSymbol(DispatchTypes.Method, "<")]
            public static Expression LessThan(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.LessThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, "<", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, "<", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<=")]
            public static Expression LessThanOrEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.LessThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, "<=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, "<=", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">")]
            public static Expression GreaterThan(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.GreaterThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, ">", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, ">", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">=")]
            public static Expression GreaterThanOrEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.GreaterThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, ">=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, ">=", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<=>")]
            public static Expression Compare(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments[0].Method(s, "CompareTo", e.Arguments[1]);
            }
            
            [YacqSymbol(DispatchTypes.Method, "==")]
            public static Expression Equal(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.Equal(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, "==", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, "==", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "!=")]
            public static Expression NotEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.NotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, "!=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, "!=", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "===")]
            public static Expression ReferenceEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.ReferenceEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, "===", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, "===", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "!==")]
            public static Expression ReferenceNotEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.ReferenceNotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Function(s, "&&",
                          YacqExpression.Function(s, "!==", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Function(s, "!==", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "&")]
            public static Expression And(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.And(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "&", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "&=")]
            public static Expression AndAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.AndAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "&", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "!&")]
            public static Expression NotAnd(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Not(YacqExpression.Function(s, "&", e.Arguments));
            }

            [YacqSymbol(DispatchTypes.Method, "!&=")]
            public static Expression NotAndAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Assign(
                    e.Arguments[0].Reduce(s),
                    YacqExpression.Function(s, "!&", e.Arguments)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "|")]
            public static Expression Or(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Or(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "|", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "|=")]
            public static Expression OrAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.OrAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "|", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "!|")]
            public static Expression NotOr(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Not(YacqExpression.Function(s, "|", e.Arguments));
            }

            [YacqSymbol(DispatchTypes.Method, "!|=")]
            public static Expression NotOrAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Assign(
                    e.Arguments[0].Reduce(s),
                    YacqExpression.Function(s, "!|", e.Arguments)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "^")]
            public static Expression ExclusiveOr(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.ExclusiveOr(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "^", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }
            
            [YacqSymbol(DispatchTypes.Method, "^=")]
            public static Expression ExclusiveOrAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.ExclusiveOrAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "^", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "&&")]
            public static Expression AndAlso(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.AndAlso(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Function(s, "&&", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "!&&")]
            public static Expression NotAndAlso(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Not(YacqExpression.Function(s, "&&", e.Arguments));
            }
            
            [YacqSymbol(DispatchTypes.Method, "!||")]
            public static Expression NotOrElse(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Not(YacqExpression.Function(s, "||", e.Arguments));
            }
            
            #endregion

            #region Function - Null Testing

            [YacqSymbol(DispatchTypes.Method, "??")]
            public static Expression Coalesce(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Coalesce(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    .Let(_ => e.Arguments.Count > 2
                        ? (Expression) YacqExpression.Function(s, "??", e.Arguments
                              .Skip(2)
                              .StartWith(_)
                          )
                        : _
                    );
            }

            [YacqSymbol(DispatchTypes.Method, "?")]
            public static Expression IsNotNull(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count > 2
                    ? YacqExpression.Function(s, "&&",
                          e.Arguments
                              .Select(a => YacqExpression.Function(s, "!==",
                                  a,
                                  Expression.Constant(null)
                              ))
#if SILVERLIGHT
                              .Cast<Expression>()
#endif
                      )
                    : YacqExpression.Function(s, "!==",
                          e.Arguments[0],
                          Expression.Constant(null)
                     );
            }

            [YacqSymbol(DispatchTypes.Method, "!?")]
            public static Expression IsNull(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count > 2
                    ? YacqExpression.Function(s, "||",
                          e.Arguments
                              .Select(a => YacqExpression.Function(s, "===",
                                  a,
                                  Expression.Constant(null)
                              ))
#if SILVERLIGHT
                              .Cast<Expression>()
#endif

                      )
                    : YacqExpression.Function(s, "===",
                          e.Arguments[0],
                          Expression.Constant(null)
                     );
            }

            #endregion

            #region Function - Flow

            [YacqSymbol(DispatchTypes.Method, ".")]
            public static Expression Dot(DispatchExpression e, SymbolTable s, Type t)
            {
                var a0 = e.Arguments[0].Reduce(s);
                s = a0 is SymbolTableExpression
                    ? ((SymbolTableExpression) a0).Symbols
                    : s;
                if (a0 is TypeCandidateExpression && e.Arguments[1] is VectorExpression)
                {
                    // T.[foo bar]
                    return YacqExpression.TypeCandidate(s, ((TypeCandidateExpression) a0).Candidates
                        .Single(c => c.GetGenericArguments().Length == ((VectorExpression) e.Arguments[1]).Elements.Count)
                            .MakeGenericType(((VectorExpression) e.Arguments[1]).Elements
                                .ReduceAll(s)
                                .OfType<TypeCandidateExpression>()
                                .Select(_ => _.Candidates.Single())
                                .ToArray()
                            )
                        );
                }
                else if (e.Arguments[1] is ListExpression)
                {
                    var a1 = ((ListExpression) e.Arguments[1]);
                    var a1e0l = a1.Elements.ElementAtOrDefault(0).List(".");
                    return a1e0l != null
                        ? YacqExpression.Dispatch(
                              s,
                              DispatchTypes.Method,
                              e.Arguments[0],
                              a1e0l.First().Id(),
                              ((VectorExpression) a1e0l.Last()).Elements
                                  .ReduceAll(s)
                                  .Cast<TypeCandidateExpression>()
                                  .Select(_ => _.ElectedType),
                              a1.Elements.Skip(1)
                          )
                        : e.Arguments[0].Method(s, a1.Elements.ElementAtOrDefault(0).Null(_ => _.Id(), ""), a1.Elements.Skip(1));
                }
                else if (e.Arguments[1] is VectorExpression)
                {
                    return a0.Type.IsArray
                        ? (Expression) Expression.ArrayAccess(
                              a0,
                              ((VectorExpression) e.Arguments[1])
                                  .Elements
                                  .ReduceAll(s)
                          )
                        : YacqExpression.Dispatch(
                              s,
                              DispatchTypes.Member,
                              e.Arguments[0],
                              null,
                              ((VectorExpression) e.Arguments[1]).Elements
                          );
                }
                else
                {
                    return e.Arguments[0].Member(s, e.Arguments[1].Id());
                }
            }

            [YacqSymbol(DispatchTypes.Method, "let")]
            [YacqSymbol(DispatchTypes.Method, "$")]
            public static Expression Let(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any()
                    ? e.Arguments[0] is VectorExpression
                          ? (Expression) new SymbolTable(s).Let(s_ => ((VectorExpression) e.Arguments[0]).Elements
                                .SelectMany(_ => _.List(":").Let(l => l != null
                                    ? new[] { l.First(), Expression.Default(((TypeCandidateExpression) l.Last().Reduce(s)).ElectedType), }
                                    : EnumerableEx.Return(_)
                                ))
                                .Share(_ => _.Zip(_, (i, v) => i.Id().Let(n =>
                                    v.Reduce(s_).Apply(r => s_.Add(n, Expression.Variable(r.Type, n)))
                                )))
                                .ToArray()
                                .Let(_ => Expression.Block(
                                    s_.Literals.Values
                                        .OfType<ParameterExpression>(),
                                    (e.Arguments.Count > 1
                                        ? e.Arguments
                                              .Skip(1)
                                              .ReduceAll(s_)
                                        : EnumerableEx.Return(
#if SILVERLIGHT
                                              (Expression)
#endif
                                              Expression.Empty()
                                          )
                                    )
                                        .StartWith(s_.Literals.Values
                                            .OfType<ParameterExpression>()
                                            .Zip(_, Expression.Assign).ToArray()
                                        )
                                ))
                            )
                          : Expression.Block(e.Arguments.ReduceAll(s))
                    : Expression.Empty();
            }

            [YacqSymbol(DispatchTypes.Method, "use")]
            public static Expression Use(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any()
                    ? e.Arguments[0] is VectorExpression
                          ? (Expression) new SymbolTable(s).Let(s_ => ((VectorExpression) e.Arguments[0]).Elements
                                .SelectMany(_ => _.List(":").Let(l => l != null
                                    ? new[] { l.First(), Expression.Default(((TypeCandidateExpression) l.Last().Reduce(s)).ElectedType), }
                                    : EnumerableEx.Return(_)
                                ))
                                .Share(_ => _.Zip(_, (i, v) => i.Id().Let(n =>
                                    v.Reduce(s_).Apply(r => s_.Add(n, Expression.Variable(r.Type, n)))
                                )))
                                .ToArray()
                                .Let(_ => Expression.Block(
                                    s_.Literals.Values
                                        .OfType<ParameterExpression>(),
                                    EnumerableEx.Return(
                                        (Expression) Expression.Block(
                                            e.Arguments.Count > 1
                                                ? e.Arguments
                                                      .Skip(1)
                                                      .ReduceAll(s_)
                                                : EnumerableEx.Return(
#if SILVERLIGHT
                                                      (Expression)
#endif
                                                      Expression.Empty()
                                                  )
                                        )
                                        .Method(s, "finally",
                                            s_.Literals.Values
                                                .OfType<ParameterExpression>()
                                                .Where(p => typeof(IDisposable).IsAssignableFrom(p.Type))
                                                .Select(p => YacqExpression.Function(s, "?", p)
                                                    .Method(s, "then",
                                                        Expression.Convert(p, typeof(IDisposable))
                                                            .Method(s, "Dispose")
                                                    )
                                                )
#if SILVERLIGHT
                                                .Cast<Expression>()
#endif
                                        )
                                        .Reduce(s)
                                    )
                                        .StartWith(s_.Literals.Values
                                            .OfType<ParameterExpression>()
                                            .Zip(_, Expression.Assign).ToArray()
                                        )
                                ))
                            )
                          : Expression.Block(e.Arguments.ReduceAll(s))
                    : Expression.Empty();
            }

            [YacqSymbol(DispatchTypes.Method, "fun")]
            [YacqSymbol(DispatchTypes.Method, "\\")]
            public static Expression Function(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any()
                    ? e.Arguments[0] is VectorExpression
                          ? ((VectorExpression) e.Arguments[0]).Elements
                                .Select(p => p.List(":").If(_ => _ != null,
                                    _ => YacqExpression.AmbiguousParameter(
                                        s,
                                        ((TypeCandidateExpression) _.Last().Reduce(s)).ElectedType,
                                        _.First().Id()
                                    ),
                                    _ => YacqExpression.AmbiguousParameter(
                                        s,
                                        p.Id()
                                    )
                                ))
                                .ToArray()
                                .Let(p => YacqExpression.AmbiguousLambda(s, e.Arguments.Skip(1), p))
                          : YacqExpression.AmbiguousLambda(s, e.Arguments)
                    : (Expression) Expression.Lambda(Expression.Empty());
            }

            [YacqSymbol(DispatchTypes.Method, "alias")]
            public static Expression Alias(DispatchExpression e, SymbolTable s, Type t)
            {
                return new SymbolTable(s).Let(s_ => ((VectorExpression) e.Arguments[0]).Elements
                    .Share(_ => _.Zip(_, (i, v) => v.Reduce(s_)
                        .Apply(r => s_.Add(i.Id(), r))
                    ))
                    .ToArray()
                    .Let(_ => e.Arguments.Count > 2
                        ? Expression.Block(e.Arguments.Skip(1).ReduceAll(s_))
                        : e.Arguments.Last().Reduce(s_)
                    )
                );
            }

            [YacqSymbol(DispatchTypes.Method, "ignore")]
            [YacqSymbol(DispatchTypes.Method, "!-")]
            public static Expression Ignore(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.ReduceAll(s).ToArray().Let(_ => YacqExpression.Ignored(s));
            }

            [YacqSymbol(DispatchTypes.Method, "throw")]
            public static Expression Rethrow(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Rethrow();
            }

            [YacqSymbol(DispatchTypes.Method, "...")]
            public static Expression ErrorImmediately(DispatchExpression e, SymbolTable s, Type t)
            {
                throw new Exception();
            }

            [YacqSymbol(DispatchTypes.Method, ">_<")]
            public static Expression BreakImmediately(DispatchExpression e, SymbolTable s, Type t)
            {
                return Debugger.IsAttached
                    ? Expression.Constant(false).Apply(_ => Debugger.Break())
                    : Expression.Constant(true).Apply(_ => Debugger.Launch());
            }

            #endregion

            #region Function - General

            [YacqSymbol(DispatchTypes.Method, "tuple")]
            public static Expression CreateTuple(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(typeof(Tuple)).Method(s, "Create", e.Arguments);
            }

            #endregion

            #region Function - Input / Output

            [YacqSymbol(DispatchTypes.Method, "input")]
            public static Expression Input(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any()
                    ? YacqExpression.Function(s, "$",
                          e.Arguments[0].Method(s, "printn"),
                          YacqExpression.Function(s, "input")
                      )
                    : YacqExpression.TypeCandidate(typeof(Console)).Method(s, "ReadLine");
            }
            
            #endregion
            
            #region Function - Type Handling

            [YacqSymbol(DispatchTypes.Method, "type")]
            public static Expression GetType(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(
#if SILVERLIGHT
                    ModuleLoader.Assemblies
#else
                    AppDomain.CurrentDomain.GetAssemblies()
#endif
                        .Choose(a => a.GetType(e.Arguments[0].Reduce(s).Const<String>()))
                        .First()
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "typeof")]
            public static Expression TypeObjectOf(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Constant(
#if SILVERLIGHT
                    Type.GetType(e.Arguments[0].Reduce(s).Const<String>())
#else
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Choose(a => a.GetType(e.Arguments[0].Reduce(s).Const<String>()))
                        .First()
#endif
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "assembly")]
            public static Expression LoadAssembly(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Constant(
#if SILVERLIGHT
                    Assembly.Load(e.Arguments[0].Reduce(s).Const<String>())
#else
#pragma warning disable 618
                    Assembly.LoadWithPartialName(e.Arguments[0].Reduce(s).Const<String>())
#pragma warning restore
#endif
                );
            }

            [YacqSymbol(DispatchTypes.Method, "new")]
            public static Expression CreateAnonymousInstance(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments
                    .Share(_ => _.Zip(_, (i, v) => Tuple.Create(i.Id(), v.Reduce(s))))
                    .Let(ms => s.Resolve("*assembly*").Const<YacqAssembly>()
                        .TryDefineType(ms.ToDictionary(_ => _.Item1, _ => _.Item2.Type))
                        .Create(s)
                        .Let(nt => Expression.New(
                            nt.GetConstructors()[0],
                            ms.Select(_ => _.Item2),
                            ms.Select(_ => nt.GetProperty(_.Item1))
#if SILVERLIGHT
                                .Cast<MemberInfo>()
#endif
                        ))
                    );
            }

            [YacqSymbol(DispatchTypes.Method, "newtype")]
            public static Expression CreateType(DispatchExpression e, SymbolTable s, Type t)
            {
                var type = (e.Arguments[0].List(":") ?? new [] { e.Arguments[0], YacqExpression.TypeCandidate(typeof(Object)), })
                    .Let(es => s.Resolve("*assembly*").Const<YacqAssembly>().DefineType(
                        es.First().Id(),
                        (es.Last() is VectorExpression
                            ? ((VectorExpression) es.Last()).Elements
                            : EnumerableEx.Return(es.Last())
                        )
                            .ReduceAll(s)
                            .Cast<TypeCandidateExpression>()
                            .Select(_ => _.ElectedType)
                            .ToArray()
                    ));
                e.Arguments
                    .Skip(1)
                    .OfType<ListExpression>()
                    .ForEach(l =>
                    {
                        var rest = l.Elements
                            .SkipWhile(_ => _.Id().Let(i => i != "member" && i != "method"))
                            .Skip(1)
                            .ToArray();
                        var attributes = l.Elements
                            .SkipLast(rest.Length)
                            .OfType<IdentifierExpression>()
                            .Select(_ => _.Name)
                            .ToArray()
                            .Let(_ =>
                                (_.Last() == "method"
                                    ? MemberTypes.Method
                                    : rest
                                          .OfType<IdentifierExpression>()
                                          .Any(i => i.Name == "get" || i.Name == "set")
                                              ? MemberTypes.Property
                                              : MemberTypes.Field
                                )
                                .Let(mt => Tuple.Create(mt, _.Any()
                                    ? Enum.Parse(
                                          mt == MemberTypes.Field
                                              ? typeof(FieldAttributes)
                                              : typeof(MethodAttributes),
                                          String.Join(",", _.SkipLast(1)),
                                          true
                                      )
                                    : null
                                ))
                            )
                            .If(
                                _ => _.Item1 == MemberTypes.Method && rest.First().Id() == "new",
                                _ => Tuple.Create(MemberTypes.Constructor, _.Item2)
                            );
                        switch (attributes.Item1)
                        {
                            case MemberTypes.Field:
                                rest[0].List(":").Let(es => type.DefineField(
                                    es.First().Id(),
                                    ((TypeCandidateExpression) es.Last().Reduce(s)).ElectedType,
                                    (FieldAttributes) (attributes.Item2 ?? FieldAttributes.Public),
                                    rest.ElementAtOrDefault(1)
                                ));
                                break;
                            case MemberTypes.Property:
                                rest[0].List(":").Let(es => type.DefineProperty(
                                    es.First().Id(),
                                    ((TypeCandidateExpression) es.Last().Reduce(s)).ElectedType,
                                    (MethodAttributes) (attributes.Item2 ?? MethodAttributes.Public),
                                    rest[1].Id().Let(i => i != "get" && i != "set") ? rest[1] : null,
                                    rest.Any(_ => _.Id() == "get")
                                        ? rest
                                              .SkipWhile(_ => _.Id() != "get")
                                              .Skip(1)
                                              .If(
                                                  _ => _.Any() && _.First().Id() != "set",
                                                  _ => _.First(),
                                                  _ => YacqExpression.Ignored()
                                              )
                                        : null,
                                    rest.Any(_ => _.Id() == "set")
                                        ? rest
                                              .SkipWhile(_ => _.Id() != "set")
                                              .Skip(1)
                                              .If(
                                                  _ => _.Any() && _.First().Id() != "get",
                                                  _ => _.First(),
                                                  _ => YacqExpression.Ignored()
                                              )
                                        : null
                                ));
                                break;
                            case MemberTypes.Method:
                                (rest[0].List(":") ?? new [] { rest[0], YacqExpression.TypeCandidate(typeof(void)), })
                                    .Let(es => type.DefineMethod(
                                        es.First().Id(),
                                        (MethodAttributes) (attributes.Item2 ?? MethodAttributes.Public),
                                        ((TypeCandidateExpression) es.Last().Reduce(s)).ElectedType,
                                        ((VectorExpression) rest[1]).Elements
                                            .ReduceAll(s)
                                            .OfType<TypeCandidateExpression>()
                                            .Select(_ => _.ElectedType)
                                            .ToArray(),
                                        rest[2]
                                    ));
                                break;
                            case MemberTypes.Constructor:
                                type.DefineConstructor(
                                    (MethodAttributes) (attributes.Item2 ?? MethodAttributes.Public),
                                    ((VectorExpression) rest[1]).Elements
                                        .ReduceAll(s)
                                        .OfType<TypeCandidateExpression>()
                                        .Select(_ => _.ElectedType)
                                        .ToArray(),
                                    rest[2]
                                );
                                break;
                        }
                    });
                return YacqExpression.TypeCandidate(type.Create(s));
            }

            #endregion
            
            #region Function - Symbol Handling

            [YacqSymbol(DispatchTypes.Method, "def")]
            public static Expression Define(DispatchExpression e, SymbolTable s, Type t)
            {
                return s.Resolve(DispatchTypes.Member, "$here")(e, s, t)
                    .Method(s, "def", e.Arguments);
            }
            
            [YacqSymbol(DispatchTypes.Method, "def!")]
            public static Expression ForceDefine(DispatchExpression e, SymbolTable s, Type t)
            {
                return s.Resolve(DispatchTypes.Member, "$here")(e, s, t)
                    .Method(s, "def!", e.Arguments);
            }
            
            [YacqSymbol(DispatchTypes.Method, "undef")]
            public static Expression Undefine(DispatchExpression e, SymbolTable s, Type t)
            {
                return s.Resolve(DispatchTypes.Member, "$here")(e, s, t)
                    .Method(s, "undef", e.Arguments);
            }
            
            [YacqSymbol(DispatchTypes.Method, "load")]
            public static Expression Load(DispatchExpression e, SymbolTable s, Type t)
            {
                return s.Resolve(DispatchTypes.Member, "$here")(e, s, t)
                    .Method(s, "load", e.Arguments);
            }

            [YacqSymbol(DispatchTypes.Method, "import")]
            public static Expression Import(DispatchExpression e, SymbolTable s, Type t)
            {
                return s.Resolve(DispatchTypes.Member, "$here")(e, s, t)
                    .Method(s, "import", e.Arguments);
            }

            #endregion

            #region Method - Flow

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "")]
            public static Expression Void(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Left.Reduce(s).If(_ => _.Type != typeof(void),
                    _ => Expression.Block(typeof(void), _)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "let")]
            public static Expression LetObject(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Function(s, "let",
                    e.Arguments
                        .Skip(1)
                        .StartWith(YacqExpression.Vector(
                            s,
                            e.Arguments[0],
                            e.Left
                        ))
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "use")]
            public static Expression UseObject(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Function(s, "use",
                    e.Arguments
                        .Skip(1)
                        .StartWith(YacqExpression.Vector(
                            s,
                            e.Arguments[0],
                            e.Left
                        ))
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "alias")]
            public static Expression AliasObject(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Function(s, "alias",
                    e.Arguments
                        .Skip(1)
                        .StartWith(YacqExpression.Vector(
                            s,
                            e.Arguments[0],
                            e.Left
                        ))
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(Boolean), "cond")]
            public static Expression Condition(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Condition(
                    e.Left.Reduce(s),
                    e.Arguments[0].Reduce(s),
                    e.Arguments[1].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Boolean), "if")]
            public static Expression If(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.IfThenElse(
                    e.Left.Reduce(s),
                    e.Arguments[0].Reduce(s),
                    e.Arguments[1].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Boolean), "unless")]
            public static Expression Unless(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.IfThenElse(
                    e.Left.Reduce(s),
                    e.Arguments[1].Reduce(s),
                    e.Arguments[0].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Boolean), "then")]
            public static Expression Then(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.IfThen(
                    e.Left.Reduce(s),
                    e.Arguments.Count > 1
                        ? Expression.Block(e.Arguments.ReduceAll(s))
                        : e.Arguments[0].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Boolean), "else")]
            public static Expression Else(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.IfThen(
                    Expression.Not(e.Left.Reduce(s)),
                    e.Arguments.Count > 1
                        ? Expression.Block(e.Arguments.ReduceAll(s))
                        : e.Arguments[0].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "case")]
            public static Expression Switch(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Switch(
                    e.Left.Reduce(s),
                    e.Arguments.Count % 2 == 1
                        ? e.Arguments.Last().Reduce(s)
                        : null,
                    null,
                    e.Arguments
                        .SkipLast(e.Arguments.Count % 2)
                        .Buffer(2)
                        .Select(_ => Expression.SwitchCase(
                            _[1].Reduce(s),
                            _[0] is VectorExpression
                                ? ((VectorExpression) _[0]).Elements.ReduceAll(s)
                                : EnumerableEx.Return(_[0].Reduce(s))
                        ))
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "switch")]
            public static Expression SwitchVoid(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Switch(
                    typeof(void),
                    e.Left.Reduce(s),
                    e.Arguments.Count % 2 == 1
                        ? e.Arguments.Last().Reduce(s)
                        : null,
                    null,
                    e.Arguments
                        .SkipLast(e.Arguments.Count % 2)
                        .Buffer(2)
                        .Select(_ => Expression.SwitchCase(
                            _[1].Reduce(s),
                            _[0] is VectorExpression
                                ? ((VectorExpression) _[0]).Elements.ReduceAll(s)
                                : EnumerableEx.Return(_[0].Reduce(s))
                        ))
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Exception), "throw")]
            public static Expression Throw(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Throw(e.Left.Reduce(s));
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "except")]
            public static Expression Catch(DispatchExpression e, SymbolTable s, Type t)
            {
                return _Try(e, s, t, null);
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "catch")]
            public static Expression CatchVoid(DispatchExpression e, SymbolTable s, Type t)
            {
                return _Try(e, s, t, typeof(void));
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "fault")]
            public static Expression Fault(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.TryFault(
                    e.Left.Reduce(s),
                    e.Arguments.Count != 1
                        ? YacqExpression.Function(s, "let", e.Arguments)
                        : e.Arguments[0].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "finally")]
            public static Expression Finally(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.TryFinally(
                    e.Left.Reduce(s),
                    e.Arguments.Count != 1
                        ? YacqExpression.Function(s, "let", e.Arguments)
                        : e.Arguments[0].Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "lock")]
            public static Expression Lock(DispatchExpression e, SymbolTable s, Type t)
            {
                // TODO: This generates not canonical "lock" code now
                return YacqExpression.Identifier(s, ".lock-" + Guid.NewGuid().ToString("n").Substring(0, 8)).Let(i =>
                    YacqExpression.Function(s, "let",
                        YacqExpression.Vector(s, i, e.Arguments[0]),
                        YacqExpression.Function(s, "let",
                            YacqExpression.TypeCandidate(typeof(Monitor))
                                .Method(s, "Enter", i),
                            e.Left
                        )
                            .Method(s, "finally",
                                YacqExpression.TypeCandidate(typeof(Monitor))
                                    .Method(s, "Exit", i)
                            )
                    )
                );
            }

            #endregion

            #region Method - Input / Output

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "print")]
            public static Expression Print(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(typeof(Console)).Method(s, "WriteLine", e.Left);
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "printn")]
            public static Expression PrintWithoutNewLine(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(typeof(Console)).Method(s, "Write", e.Left);
            }

            #endregion

            #region Method - Type Handling

            [YacqSymbol(DispatchTypes.Method, typeof(Static<Object>), "new")]
            public static Expression CreateInstance(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Constructor,
                    e.Left,
                    null,
                    e.Arguments
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "with")]
            public static Expression InitializeInstance(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Left.Reduce(s).Let(l => l is NewExpression
                    ? (Expression) Expression.MemberInit(
                          (NewExpression) l,
                          e.Arguments
                              .Share(_ => _
                                  .Zip(_, (k, v) => Expression.Bind(
                                      l.Type.GetMember(
                                          k.Id(),
                                          BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy
                                      ).Single(),
                                      v.Reduce(s)
                                  ))
                              )
#if SILVERLIGHT
                              .Cast<MemberBinding>()
#endif
                      )
                    : e
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(Object), "to")]
            public static Expression Convert(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Convert(
                    e.Left.Reduce(s),
                    ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(Object), "as")]
            public static Expression TypeAs(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.TypeAs(
                    e.Left.Reduce(s),
                    ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(Object), "is")]
            public static Expression TypeIs(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.TypeIs(
                    e.Left.Reduce(s),
                    ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                );
            }
            
            #endregion

            #region Method - Symbol Handling

            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "def")]
            public static Expression DefineIn(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ =>
                    e.Left.Reduce(s).Const<SymbolTable>().Add(
                        e.Arguments[0].Id(),
                        e.Arguments[1].Reduce(s)
                    )
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "def!")]
            public static Expression ForceDefineIn(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ =>
                    e.Left.Reduce(s).Const<SymbolTable>()[e.Arguments[0].Id()]
                        = e.Arguments[1].Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "undef")]
            public static Expression UndefineIn(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ =>
                    e.Left.Reduce(s).Const<SymbolTable>().Remove(e.Arguments[0].Id())
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "load")]
            public static Expression LoadTo(DispatchExpression e, SymbolTable s, Type t)
            {
                return Root["*modules*"].Const<ModuleLoader>().Load(
                    e.Left.Reduce(s).Const<SymbolTable>(),
                    e.Arguments[0].Reduce(s).Const<String>()
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "import")]
            public static Expression ImportTo(DispatchExpression e, SymbolTable s, Type t)
            {
                return Root["*modules*"].Const<ModuleLoader>().Import(
                    e.Left.Reduce(s).Const<SymbolTable>(),
                    e.Arguments[0].Reduce(s).Const<String>(),
                    e.Arguments[e.Arguments.Count > 1 ? 1 : 0].Reduce(s).Const<String>()
                );
            }

            #endregion

            #region Method - Expressions

            [YacqSymbol(DispatchTypes.Method, typeof(Expression), "reduce")]
            public static Expression Reduce(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(typeof(YacqExtensions)).Method(s, "Reduce",
                    e.Left,
                    Expression.Constant(s),
                    Expression.Default(typeof(Type))
                );
            }

            #endregion

            #region Variable - Flow

            [YacqSymbol("...")]
            public static Expression NotImplementedError
                = Expression.Throw(Expression.Constant(new NotImplementedException()));

            [YacqSymbol(DispatchTypes.Member, ">_<")]
            public static Expression Break(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(typeof(Debugger))
                    .Member(s, "IsAttached")
                    .Method(s, "cond",
                        YacqExpression.Function(s, "let",
                            YacqExpression.TypeCandidate(typeof(Debugger)).Method("Break"),
                            Expression.Constant(false)
                        ),
                        YacqExpression.Function(s, "let",
                            YacqExpression.TypeCandidate(typeof(Debugger)).Method("Launch"),
                            Expression.Constant(true)
                        )
                    );
            }

            #endregion

            #region Variable - General

            [YacqSymbol("true")]
            public static Expression True
                = Expression.Constant(true);

            [YacqSymbol("false")]
            public static Expression False
                = Expression.Constant(false);

            [YacqSymbol(DispatchTypes.Member, "nil")]
            public static Expression Nothing(DispatchExpression e, SymbolTable s, Type t)
            {
                return t == null || t.IsGenericParameter
                    ? Expression.Constant(null)
                    : t == typeof(void)
                          ? (Expression) Expression.Empty()
                          : Expression.Default(t);
            }

            #endregion

            #region Variable - Symbol Handling

            [YacqSymbol(DispatchTypes.Member, "?")]
            public static Expression GetGlobalSymbols(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Constant(
                    s.AllKeys
                        .Where(_ => _.LeftType == null)
                        .OrderBy(_ => _.DispatchType.HasFlag(DispatchTypes.Member)
                            ? 0
                            : 1
                        )
                        .ThenBy(m => m.Name)
                        .Select(_ => _.DispatchType.HasFlag(DispatchTypes.Member)
                            ? _.Name
                            : "(" + _.Name + ")"
                        )
                );
            }

            [YacqSymbol(DispatchTypes.Member, "$here")]
            public static Expression HereSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return s.Resolve("$global");
            }

            [YacqSymbol(DispatchTypes.Member, "here")]
            public static Expression Here(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.SymbolTable(HereSymbol(e, s, t).Const<SymbolTable>());
            }

            [YacqSymbol(DispatchTypes.Member, "global")]
            public static Expression Global(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.SymbolTable(s.Resolve("$global").Const<SymbolTable>());
            }

            #endregion

            #region Variable - System Object

            [YacqSymbol("*modules*")]
            public static Expression Modules
                = Expression.Constant(new ModuleLoader(
#if !SILVERLIGHT
                      new DirectoryInfo("yacq_lib"),
                      new DirectoryInfo("lib"),
                      new DirectoryInfo(".")
#endif
                  ));

            [YacqSymbol("*docs*")]
            public static Expression Documents
                = Expression.Constant(new DocumentRepository(
#if !SILVERLIGHT
                      new DirectoryInfo("yacq_doc"),
                      new DirectoryInfo("doc"),
                      new DirectoryInfo(Path.Combine(
                          Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                          @"Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
                      )),
                      new DirectoryInfo(".")
#endif
                  ));

            #endregion

            #region Variable - Type

            // System, Data Types
            [YacqSymbol("Object")]
            public static Expression ObjectType = YacqExpression.TypeCandidate(typeof(Object));

            [YacqSymbol("Boolean")]
            public static Expression BooleanType = YacqExpression.TypeCandidate(typeof(Boolean));
            
            [YacqSymbol("Char")]
            public static Expression CharType = YacqExpression.TypeCandidate(typeof(Char));
            
            [YacqSymbol("String")]
            public static Expression StringType = YacqExpression.TypeCandidate(typeof(String));
            
            [YacqSymbol("SByte")]
            public static Expression SByteType = YacqExpression.TypeCandidate(typeof(SByte));
            
            [YacqSymbol("Byte")]
            public static Expression ByteType = YacqExpression.TypeCandidate(typeof(Byte));
            
            [YacqSymbol("Int16")]
            public static Expression Int16Type = YacqExpression.TypeCandidate(typeof(Int16));
            
            [YacqSymbol("UInt16")]
            public static Expression UInt16Type = YacqExpression.TypeCandidate(typeof(UInt16));
            
            [YacqSymbol("Int32")]
            public static Expression Int32Type = YacqExpression.TypeCandidate(typeof(Int32));
            
            [YacqSymbol("UInt32")]
            public static Expression UInt32Type = YacqExpression.TypeCandidate(typeof(UInt32));
            
            [YacqSymbol("Int64")]
            public static Expression Int64Type = YacqExpression.TypeCandidate(typeof(Int64));
            
            [YacqSymbol("UInt64")]
            public static Expression UInt64Type = YacqExpression.TypeCandidate(typeof(UInt64));
            
            [YacqSymbol("IntPtr")]
            public static Expression IntPtrType = YacqExpression.TypeCandidate(typeof(IntPtr));
            
            [YacqSymbol("UIntPtr")]
            public static Expression UIntPtrType = YacqExpression.TypeCandidate(typeof(UIntPtr));
            
            [YacqSymbol("Single")]
            public static Expression SingleType = YacqExpression.TypeCandidate(typeof(Single));
            
            [YacqSymbol("Double")]
            public static Expression DoubleType = YacqExpression.TypeCandidate(typeof(Double));
            
            [YacqSymbol("Decimal")]
            public static Expression DecimalType = YacqExpression.TypeCandidate(typeof(Decimal));
            
            [YacqSymbol("DateTime")]
            public static Expression DateTimeType = YacqExpression.TypeCandidate(typeof(DateTime));
            
            [YacqSymbol("DateTimeOffset")]
            public static Expression DateTimeOffsetType = YacqExpression.TypeCandidate(typeof(DateTimeOffset));
            
            [YacqSymbol("TimeSpan")]
            public static Expression TimeSpanType = YacqExpression.TypeCandidate(typeof(TimeSpan));
            
            [YacqSymbol("Guid")]
            public static Expression GuidType = YacqExpression.TypeCandidate(typeof(Guid));
            
            [YacqSymbol("Uri")]
            public static Expression UriType = YacqExpression.TypeCandidate(typeof(Uri));
            
            // System, Utility Classes
            
            [YacqSymbol("Convert")]
            public static Expression ConvertType = YacqExpression.TypeCandidate(typeof(Convert));
            
            [YacqSymbol("Math")]
            public static Expression MathType = YacqExpression.TypeCandidate(typeof(Math));
            
            [YacqSymbol("Nullable")]
            public static Expression NullableType = YacqExpression.TypeCandidate(typeof(Nullable));
            
            [YacqSymbol("Random")]
            public static Expression RandomType = YacqExpression.TypeCandidate(typeof(Random));
            
            // System.Collections.*
            
            [YacqSymbol("Dictionary")]
            public static Expression DictionaryType = YacqExpression.TypeCandidate(typeof(Dictionary<,>));
            
            [YacqSymbol("HashSet")]
            public static Expression HashSetType = YacqExpression.TypeCandidate(typeof(HashSet<>));
            
            [YacqSymbol("LinkedList")]
            public static Expression LinkedListType = YacqExpression.TypeCandidate(typeof(LinkedList<>));
            
            [YacqSymbol("List")]
            public static Expression ListType
                = YacqExpression.TypeCandidate(typeof(List<>));
            
            [YacqSymbol("Queue")]
            public static Expression QueueType = YacqExpression.TypeCandidate(typeof(Queue<>));
            
            [YacqSymbol("Stack")]
            public static Expression StackType = YacqExpression.TypeCandidate(typeof(Stack<>));
            
            // System.IO
            
            [YacqSymbol("Directory")]
            public static Expression DirectoryType = YacqExpression.TypeCandidate(typeof(Directory));
            
            [YacqSymbol("DirectoryInfo")]
            public static Expression DirectoryInfoType = YacqExpression.TypeCandidate(typeof(DirectoryInfo));
            
            [YacqSymbol("File")]
            public static Expression FileType = YacqExpression.TypeCandidate(typeof(File));
            
            [YacqSymbol("FileInfo")]
            public static Expression FileInfoType = YacqExpression.TypeCandidate(typeof(FileInfo));
            
            [YacqSymbol("Path")]
            public static Expression PathType = YacqExpression.TypeCandidate(typeof(Path));
            
            // System.Text.*
            
            [YacqSymbol("Encoding")]
            public static Expression EncodingType = YacqExpression.TypeCandidate(typeof(Encoding));
            
            [YacqSymbol("Regex")]
            public static Expression RegexType = YacqExpression.TypeCandidate(typeof(Regex));
            
            [YacqSymbol("StringBuilder")]
            public static Expression StringBuilderType = YacqExpression.TypeCandidate(typeof(StringBuilder));
            
            // LINQ Types
            
            [YacqSymbol("Enumerable")]
            public static Expression EnumerableType = YacqExpression.TypeCandidate(typeof(Enumerable));
            
            [YacqSymbol("EnumerableEx")]
            public static Expression EnumerableExType = YacqExpression.TypeCandidate(typeof(EnumerableEx));
            
            [YacqSymbol("Queryable")]
            public static Expression QueryableType = YacqExpression.TypeCandidate(typeof(Queryable));
            
            [YacqSymbol("QueryableEx")]
            public static Expression QueryableExType = YacqExpression.TypeCandidate(typeof(QueryableEx));

#if !__MonoCS__
            [YacqSymbol("Observable")]
            public static Expression ObservableType = YacqExpression.TypeCandidate(typeof(Observable));
            
            [YacqSymbol("Observer")]
            public static Expression ObserverType = YacqExpression.TypeCandidate(typeof(Observer));
            
            [YacqSymbol("ObservableExtensions")]
            public static Expression ObservableExtensionsType = YacqExpression.TypeCandidate(typeof(ObservableExtensions));
            
            [YacqSymbol("Qbservable")]
            public static Expression QbservableType = YacqExpression.TypeCandidate(typeof(Qbservable));
#endif
            
            // Generic Delegate Types
            
            [YacqSymbol("Action")]
            public static Expression ActionType = YacqExpression.TypeCandidate(
                typeof(Action),
                typeof(Action<>),
                typeof(Action<,>),
                typeof(Action<,,>),
                typeof(Action<,,,>),
                typeof(Action<,,,,>),
                typeof(Action<,,,,,>),
                typeof(Action<,,,,,,>),
                typeof(Action<,,,,,,,>),
                typeof(Action<,,,,,,,,>),
                typeof(Action<,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,,,,>),
                typeof(Action<,,,,,,,,,,,,,,,>)
            );
            
            [YacqSymbol("Func")]
            public static Expression FuncType = YacqExpression.TypeCandidate(
                typeof(Func<>),
                typeof(Func<,>),
                typeof(Func<,,>),
                typeof(Func<,,,>),
                typeof(Func<,,,,>),
                typeof(Func<,,,,,>),
                typeof(Func<,,,,,,>),
                typeof(Func<,,,,,,,>),
                typeof(Func<,,,,,,,,>),
                typeof(Func<,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,,>),
                typeof(Func<,,,,,,,,,,,,,,,,>)
            );

            #endregion

            #region Member - Symbol Handling

            [YacqSymbol(DispatchTypes.Member, typeof(Object), "?")]
            public static Expression GetMembersAndSymbols(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Constant(
                    e.Left.Type(s).Let(lt =>
                        lt.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                            .Where(m => !(m is ConstructorInfo
                                || (m is MethodInfo && m.Name.StartsWith("get_") || m.Name.StartsWith("set_"))))
                            .Concat(s.AllLiterals.Values
                                .OfType<TypeCandidateExpression>()
                                .SelectMany(_ => _.Candidates)
                                .SelectMany(_ => _.GetExtensionMethods()
                                    .Where(m => m.GetParameters()[0].ParameterType.IsAppropriate(lt))
                                )
#if SILVERLIGHT
                                .Cast<MemberInfo>()
#endif
                            )
                            .OrderBy(m => m is MethodInfo
                                ? ((MethodInfo) m).IsExtensionMethod()
                                      ? 2
                                      : 1
                                : 0
                            )
                            .ThenBy(m => m.Name)
                            .Select(m => m is MethodBase
                                ? "("+ m.Name + ")"
                                : m.Name
                            )
                            .Concat(s.AllKeys
                                .Where(_ => _.TypeMatch(lt))
                                .OrderBy(_ => _.DispatchType.HasFlag(DispatchTypes.Member)
                                    ? 0
                                    : 1
                                )
                                .ThenBy(m => m.Name)
                                .Select(_ => _.DispatchType.HasFlag(DispatchTypes.Member)
                                    ? _.Name
                                    : "(" + _.Name + ")"
                                )
                            )
                            .Distinct()
                    )
                );
            }

            [YacqSymbol(DispatchTypes.Member, typeof(Static<Object>), "?")]
            public static Expression GetStaticMembersAndSymbols(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Constant(
                    ((TypeCandidateExpression) e.Left.Reduce(s)).ElectedType.Let(lt =>
                        lt.GetMembers(BindingFlags.Public | BindingFlags.Static)
                            .Where(m => !(m is MethodInfo && m.Name.StartsWith("get_") || m.Name.StartsWith("set_")))
                            .OrderBy(m => m is MethodInfo
                                ? ((MethodInfo) m).IsExtensionMethod()
                                      ? 2
                                      : 1
                                : 0
                            )
                            .ThenBy(m => m.Name)
                            .Select(m => m is MethodBase
                                ? "(" + m.Name + ")"
                                : m.Name
                            )
                            .Concat(s.AllKeys
                                .Where(_ => _.TypeMatch(typeof(Static<>).MakeGenericType(lt)))
                                .OrderBy(_ => _.DispatchType.HasFlag(DispatchTypes.Member)
                                    ? 0
                                    : 1
                                )
                                .ThenBy(m => m.Name)
                                .Select(_ => _.DispatchType.HasFlag(DispatchTypes.Member)
                                    ? _.Name
                                    : "(" + _.Name + ")"
                                )
                            )
                            .Distinct()
                    )
                );
            }

            #endregion

            #region Member - Type Handling

            [YacqSymbol(DispatchTypes.Member, typeof(Static<Object>), "array")]
            public static Expression MakeArrayType(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(
                    ((TypeCandidateExpression) e.Left.Reduce(s)).ElectedType.MakeArrayType()
                );
            }

            [YacqSymbol(DispatchTypes.Member, typeof(Static<Object>), "type")]
            public static Expression GetTypeObject(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Constant(
                    ((TypeCandidateExpression) e.Left.Reduce(s)).ElectedType
                );
            }

            #endregion

            #region Helpers

            private static Expression _Try(DispatchExpression e, SymbolTable s, Type t, Type returnType)
            {
                return Expression.MakeTry(
                    returnType,
                    e.Left.Reduce(s),
                    null,
                    null,
                    e.Arguments
                        .SkipLast(e.Arguments.Count % 2)
                        .Buffer(2)
                        .Select(c => (c.First() as VectorExpression)
                            .Null(v => (IList<Expression>) v.Elements, new[] { c.First(), })
                            .Let(ves => (ves[0].List(":").If(
                                    les => les != null,
                                    les => new SymbolTable(s)
                                    {
                                        {les.First().Id(), Expression.Parameter(
                                            ((TypeCandidateExpression) les.Last().Reduce(s)).ElectedType,
                                            les.First().Id()
                                        )}
                                    }
                                        .Let(ns => Expression.Catch(
                                            (ParameterExpression) ns.Literals.Values.Single(),
                                            c.Last().Reduce(ns),
                                            ves.Count > 1 ? ves[1].Reduce(ns) : null
                                        )),
                                    les => Expression.Catch(
                                        ((TypeCandidateExpression) ves[0].Reduce(s)).ElectedType,
                                        c.Last().Reduce(s),
                                        ves.Count > 1 ? ves[1].Reduce(s) : null
                                    )
                                )
                            ))
                        )
                        .If(_ => e.Arguments.Count % 2 == 1, _ =>
                            _.StartWith(Expression.Catch(
                                typeof(Exception),
                                e.Arguments.Last().Reduce(s))
                            )
                        )
                        .ToArray()
                );
            }

            #endregion
        }
    }
}