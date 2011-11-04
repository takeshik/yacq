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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using XSpect.Yacq.Expressions;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Reactive;
using XSpect.Yacq.SystemObjects;

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
    
            #region Global Method: Arithmetics

            [YacqSymbol(DispatchTypes.Method, "=")]
            public static Expression Assign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Assign(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s));
            }

            [YacqSymbol(DispatchTypes.Method, "+")]
            public static Expression Plus(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any(a => a.Type(s) == typeof(String))
                    ? YacqExpression.Dispatch(
                          s,
                          DispatchTypes.Method,
                          YacqExpression.TypeCandidate(typeof(String)),
                          "Concat",
                          e.Arguments
                      )
                    : e.Arguments.Count == 1
                          ? (Expression) Expression.UnaryPlus(e.Arguments[0].Reduce(s))
                          : Expression.Add(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                                ? e.Arguments[1].Reduce(s)
                                : YacqExpression.Dispatch(s, DispatchTypes.Method, "+", e.Arguments.Skip(1)).Reduce(s)
                            );
            }

            [YacqSymbol(DispatchTypes.Method, "+=")]
            public static Expression PlusAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any(a => a.Type(s) == typeof(String))
                    ? Expression.Assign(
                          e.Arguments[0].Reduce(s),
                          YacqExpression.Dispatch(
                              s,
                              DispatchTypes.Method,
                              YacqExpression.TypeCandidate(typeof(String)),
                              "Concat",
                              e.Arguments
                          )
                      )
                    : Expression.AddAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                          ? e.Arguments[1].Reduce(s)
                          : YacqExpression.Dispatch(s, DispatchTypes.Method, "+", e.Arguments.Skip(1)).Reduce(s)
                      );
            }

            [YacqSymbol(DispatchTypes.Method, "-")]
            public static Expression Minus(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 1
                    ? (Expression) Expression.Negate(e.Arguments[0].Reduce(s))
                    : Expression.Subtract(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                          ? e.Arguments[1].Reduce(s)
                          : YacqExpression.Dispatch(s, DispatchTypes.Method, "-", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "-=")]
            public static Expression MinusAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.SubtractAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "-", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "*")]
            public static Expression Multiply(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Multiply(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "*", e.Arguments.Skip(1)).Reduce(s)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "*=")]
            public static Expression MultiplyAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.MultiplyAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "*", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "/")]
            public static Expression Divide(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Divide(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "/", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "/=")]
            public static Expression DivideAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.DivideAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "/", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "**")]
            public static Expression Power(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.Power(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchTypes.Method, "**", e.Arguments.Skip(1)).Reduce(s)
                 );
            }
            
            [YacqSymbol(DispatchTypes.Method, "**=")]
            public static Expression PowerAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.PowerAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "**", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "%")]
            public static Expression Modulo(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.Modulo(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchTypes.Method, "%", e.Arguments.Skip(1)).Reduce(s)
                 );
            }
            
            [YacqSymbol(DispatchTypes.Method, "%=")]
            public static Expression ModuloAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.ModuloAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "%", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<<")]
            public static Expression LeftShift(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.LeftShift(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchTypes.Method, "<<", e.Arguments.Skip(1)).Reduce(s)
                 );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<<=")]
            public static Expression LeftShiftAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.LeftShiftAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "<<", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">>")]
            public static Expression RightShift(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.RightShift(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchTypes.Method, ">>", e.Arguments.Skip(1)).Reduce(s)
                 );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">>=")]
            public static Expression RightShiftAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.RightShiftAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, ">>", e.Arguments.Skip(1)).Reduce(s)
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

            #region Global Method: Logicals

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
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "<", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "<", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<=")]
            public static Expression LessThanOrEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.LessThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "<=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "<=", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">")]
            public static Expression GreaterThan(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.GreaterThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, ">", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, ">", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, ">=")]
            public static Expression GreaterThanOrEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.GreaterThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, ">=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, ">=", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "<=>")]
            public static Expression Compare(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(s, DispatchTypes.Method, e.Arguments[0], "CompareTo", e.Arguments[1]);
            }
            
            [YacqSymbol(DispatchTypes.Method, "==")]
            public static Expression Equal(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.Equal(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "==", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "==", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "!=")]
            public static Expression NotEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.NotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "!=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "!=", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "===")]
            public static Expression ReferenceEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.ReferenceEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "===", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "===", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "!==")]
            public static Expression ReferenceNotEqual(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Count == 2
                    ? (Expression) Expression.ReferenceNotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "!==", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchTypes.Method, "!==", e.Arguments.Skip(1))
                      );
            }
            
            [YacqSymbol(DispatchTypes.Method, "&")]
            public static Expression And(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.And(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "&=")]
            public static Expression AndAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.AndAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "|")]
            public static Expression Or(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Or(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "|", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "|=")]
            public static Expression OrAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.OrAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "|", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "^")]
            public static Expression ExclusiveOr(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.ExclusiveOr(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "^", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "^=")]
            public static Expression ExclusiveOrAssign(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.ExclusiveOrAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "^", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "&&")]
            public static Expression AndAlso(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.AndAlso(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "||")]
            public static Expression OrElse(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.OrElse(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchTypes.Method, "||", e.Arguments.Skip(1)).Reduce(s)
                );
            }
            
            #endregion

            #region Global Method: Null Testings

            [YacqSymbol(DispatchTypes.Method, "??")]
            public static Expression Coalesce(DispatchExpression e, SymbolTable s, Type t)
            {
                 return Expression.Coalesce(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchTypes.Method, "??", e.Arguments.Skip(1)).Reduce(s)
                 );
            }

            [YacqSymbol(DispatchTypes.Method, "?")]
            public static Expression IsNotNull(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    "!==",
                    e.Arguments[0],
                    Expression.Constant(null)
                );
            }

            [YacqSymbol(DispatchTypes.Method, "!?")]
            public static Expression IsNull(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    "===",
                    e.Arguments[0],
                    Expression.Constant(null)
                );
            }

            #endregion

            #region Global Method: Flowings

            [YacqSymbol(DispatchTypes.Method, ".")]
            public static Expression Dot(DispatchExpression e, SymbolTable s, Type t)
            {
                var a0 = e.Arguments[0].Reduce(s);
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
                    var a1e0l = a1[0].List(".");
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
                        : YacqExpression.Dispatch(
                              s,
                              DispatchTypes.Method,
                              e.Arguments[0],
                              a1[0].Id(),
                              a1.Elements.Skip(1)
                          );
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
                    return YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Member,
                        e.Arguments[0],
                        e.Arguments[1].Id()
                    );
                }
            }

            [YacqSymbol(DispatchTypes.Method, "let")]
            [YacqSymbol(DispatchTypes.Method, "$")]
            public static Expression Let(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Arguments.Any()
                    ? e.Arguments[0] is VectorExpression
                          ? new SymbolTable(s).Let(s_ => ((VectorExpression) e.Arguments[0]).Elements
                                .SelectMany(_ => _.List(":").Let(l => l != null
                                    ? new [] { l.First(), Expression.Default(((TypeCandidateExpression) l.Last().Reduce(s)).ElectedType), }
                                    : EnumerableEx.Return(_)
                                ))
                                .Share(_ => _.Zip(_, (i, v) => i.Id().Let(n =>
                                    v.Reduce(s_).Apply(r => s_.Add(n, Expression.Variable(r.Type, n)))
                                )))
                                .ToArray()
                                .Let(_ => e.Arguments.Count > 1
                                    ? Expression.Block(
                                          s_.Literals.Values.OfType<ParameterExpression>(),
                                          e.Arguments
                                              .Skip(1)
                                              .ReduceAll(s_)
                                              .StartWith(s_.Literals.Values
                                                  .OfType<ParameterExpression>()
                                                  .Zip(_, Expression.Assign).ToArray()
                                              )
                                      )
                                    : (Expression) Expression.Empty()
                                )
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

            #endregion

            #region Global Method: Generals

            [YacqSymbol(DispatchTypes.Method, "...")]
            public static Expression ErrorImmediately(DispatchExpression e, SymbolTable s, Type t)
            {
                throw new Exception();
            }

            [YacqSymbol(DispatchTypes.Method, ">_<")]
            public static Expression BreakImmediately(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ => Debugger.Break());
            }

            [YacqSymbol(DispatchTypes.Method, "tuple")]
            public static Expression Tuple(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    YacqExpression.TypeCandidate(typeof(Tuple)),
                    "Create",
                    e.Arguments
                );
            }

            [YacqSymbol(DispatchTypes.Method, "input")]
            public static Expression Input(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    YacqExpression.TypeCandidate(typeof(Console)),
                    "ReadLine"
                );
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "print")]
            public static Expression Print(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    YacqExpression.TypeCandidate(typeof(Console)),
                    "WriteLine",
                    e.Left
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(Object), "printn")]
            public static Expression PrintWithoutNewLine(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    YacqExpression.TypeCandidate(typeof(Console)),
                    "Write",
                    e.Left
                );
            }
            
            #endregion
            
            #region Global Method: Type Handlings

            [YacqSymbol(DispatchTypes.Method, "type")]
            public static Expression GetType(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.TypeCandidate(
#if SILVERLIGHT
                    Type.GetType(e.Arguments[0].Reduce(s).Const<String>())
#else
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Choose(a => a.GetType(e.Arguments[0].Reduce(s).Const<String>()))
                        .First()
#endif
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
            
            #endregion
            
            #region Global Method: Symbol Handlings

            [YacqSymbol(DispatchTypes.Method, "def")]
            public static Expression DefineGlobalSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    s.Resolve("$global"),
                    "def",
                    e.Arguments
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "def!")]
            public static Expression ForceGlobalDefineSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    s.Resolve("$global"),
                    "def!",
                    e.Arguments
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "undef")]
            public static Expression UndefineGlobalSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    s.Resolve("$global"),
                    "undef",
                    e.Arguments
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, "load")]
            public static Expression LoadToGlobal(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    s.Resolve("$global"),
                    "load",
                    e.Arguments
                );
            }
            
            #endregion

            #region Macro Method: Flowings
            
            [YacqSymbol(DispatchTypes.Method, typeof(Object), "let")]
            public static Expression LetObject(DispatchExpression e, SymbolTable s, Type t)
            {
                return e.Left.Reduce(s).Let(_ => Expression.Invoke(
                    YacqExpression.AmbiguousLambda(
                        s,
                        e.Arguments.Skip(1),
                        YacqExpression.AmbiguousParameter(s, _.Type, e.Arguments[0].Id())
                    ).Reduce(s),
                    _
                ));
            }

            [YacqSymbol(DispatchTypes.Method, typeof(Object), "alias")]
            public static Expression AliasObject(DispatchExpression e, SymbolTable s, Type t)
            {
                return new SymbolTable(s).Apply(s_ => s_.Add(
                    e.Arguments[0].Id(),
                    e.Left.Reduce(s)
                )).Let(s_ => e.Arguments.Count > 2
                    ? Expression.Block(e.Arguments.Skip(1).ReduceAll(s_))
                    : e.Arguments[1].Reduce(s_)
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
            
            #endregion

            #region Macro Method: Type Handlings
            
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

            #region Macro Method: Symbol Handlings

            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "def")]
            public static Expression DefineSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ =>
                    e.Left.Reduce(s).Const<SymbolTable>().Add(
                        e.Arguments[0].Id(),
                        e.Arguments[1].Reduce(s)
                    )
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "def!")]
            public static Expression ForceDefineSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ =>
                    e.Left.Reduce(s).Const<SymbolTable>()[e.Arguments[0].Id()]
                        = e.Arguments[1].Reduce(s)
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "undef")]
            public static Expression UndefineSymbol(DispatchExpression e, SymbolTable s, Type t)
            {
                return Expression.Empty().Apply(_ =>
                    e.Left.Reduce(s).Const<SymbolTable>().Remove(e.Arguments[0].Id())
                );
            }
            
            [YacqSymbol(DispatchTypes.Method, typeof(SymbolTable), "load")]
            public static Expression Load(DispatchExpression e, SymbolTable s, Type t)
            {
                return Root["*modules*"].Const<ModuleLoader>().Load(
                    e.Left.Reduce(s).Const<SymbolTable>(),
                    e.Arguments[0].Reduce(s).Const<String>()
                );
            }
            
            #endregion
            
            #region Global Member: Generals

            [YacqSymbol("...")]
            public static Expression NotImplementedError
                = Expression.Throw(Expression.Constant(new NotImplementedException()));

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

            [YacqSymbol(DispatchTypes.Member, ">_<")]
            public static Expression Break(DispatchExpression e, SymbolTable s, Type t)
            {
                return YacqExpression.Dispatch(
                    s,
                    DispatchTypes.Method,
                    YacqExpression.TypeCandidate(typeof(Debugger)),
                    "Break"
                );
            }
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

            #endregion
            
            #region Global Member: Configurations and System Objects

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

            [YacqSymbol("*typegen*")]
            public static Expression TypeGenerator
                = Expression.Constant(new TypeGenerator("YacqGeneratedTypes"));

            #endregion

            #region Global Member: Types

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
            
            [YacqSymbol("Observable")]
            public static Expression ObservableType = YacqExpression.TypeCandidate(typeof(Observable));
            
            [YacqSymbol("Observer")]
            public static Expression ObserverType = YacqExpression.TypeCandidate(typeof(Observer));
            
            [YacqSymbol("ObservableExtensions")]
            public static Expression ObservableExtensionsType = YacqExpression.TypeCandidate(typeof(ObservableExtensions));
            
            [YacqSymbol("Qbservable")]
            public static Expression QbservableType = YacqExpression.TypeCandidate(typeof(Qbservable));
            
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

            #region Macro Member: Generals

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

            #region Macro Member: Type Handlings

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
        }
    }
}