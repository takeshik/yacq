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

            Root = new SymbolTable()
            {
                #region Global Method: Arithmetics
                {DispatchTypes.Method, "=", (e, s) =>
                    Expression.Assign(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                },
                {DispatchTypes.Method, "+", (e, s) =>
                    e.Arguments.Any(a => a.Type(s) == typeof(String))
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
                                )
                },
                {DispatchTypes.Method, "+=", (e, s) =>
                    e.Arguments.Any(a => a.Type(s) == typeof(String))
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
                          )
                },
                {DispatchTypes.Method, "-", (e, s) =>
                    e.Arguments.Count == 1
                        ? (Expression) Expression.Negate(e.Arguments[0].Reduce(s))
                        : Expression.Subtract(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                              ? e.Arguments[1].Reduce(s)
                              : YacqExpression.Dispatch(s, DispatchTypes.Method, "-", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "-=", (e, s) =>
                    Expression.SubtractAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "-", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "*", (e, s) =>
                    Expression.Multiply(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "*", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "*=", (e, s) =>
                    Expression.MultiplyAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "*", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "/", (e, s) =>
                    Expression.Divide(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "/", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "/=", (e, s) =>
                    Expression.MultiplyAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "/", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "**", (e, s) =>
                     Expression.Power(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                         ? e.Arguments[1].Reduce(s)
                         : YacqExpression.Dispatch(s, DispatchTypes.Method, "**", e.Arguments.Skip(1)).Reduce(s)
                     )
                },
                {DispatchTypes.Method, "**=", (e, s) =>
                    Expression.PowerAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "**", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "%", (e, s) =>
                     Expression.Modulo(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                         ? e.Arguments[1].Reduce(s)
                         : YacqExpression.Dispatch(s, DispatchTypes.Method, "%", e.Arguments.Skip(1)).Reduce(s)
                     )
                },
                {DispatchTypes.Method, "%=", (e, s) =>
                    Expression.ModuloAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "%", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "<<", (e, s) =>
                     Expression.LeftShift(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                         ? e.Arguments[1].Reduce(s)
                         : YacqExpression.Dispatch(s, DispatchTypes.Method, "<<", e.Arguments.Skip(1)).Reduce(s)
                     )
                },
                {DispatchTypes.Method, "<<=", (e, s) =>
                    Expression.LeftShiftAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "<<", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, ">>", (e, s) =>
                     Expression.RightShift(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                         ? e.Arguments[1].Reduce(s)
                         : YacqExpression.Dispatch(s, DispatchTypes.Method, ">>", e.Arguments.Skip(1)).Reduce(s)
                     )
                },
                {DispatchTypes.Method, ">>=", (e, s) =>
                    Expression.LeftShiftAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, ">>", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "++", (e, s) =>
                     Expression.Increment(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "++=", (e, s) =>
                     Expression.PreIncrementAssign(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "=++", (e, s) =>
                     Expression.PostIncrementAssign(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "--", (e, s) =>
                     Expression.Decrement(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "--=", (e, s) =>
                     Expression.PreDecrementAssign(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "=--", (e, s) =>
                     Expression.PostDecrementAssign(e.Arguments[0].Reduce(s))
                },
                #endregion
                #region Global Method: Logicals
                {DispatchTypes.Method, "!", (e, s) =>
                    Expression.Not(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "~", (e, s) =>
                    Expression.OnesComplement(e.Arguments[0].Reduce(s))
                },
                {DispatchTypes.Method, "<", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.LessThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "<", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "<", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, "<=", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.LessThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "<=", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "<=", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, ">", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.GreaterThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, ">", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, ">", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, ">=", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.GreaterThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, ">=", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, ">=", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, "<=>", (e, s) =>
                    YacqExpression.Dispatch(DispatchTypes.Method, e.Arguments[0], "CompareTo", e.Arguments[1])
                },
                {DispatchTypes.Method, "==", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.Equal(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "==", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "==", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, "!=", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.NotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "!=", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "!=", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, "===", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.ReferenceEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "===", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "===", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, "!==", (e, s) =>
                    e.Arguments.Count == 2
                        ? (Expression) Expression.ReferenceNotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&",
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "!==", e.Arguments[0], e.Arguments[1]),
                              YacqExpression.Dispatch(s, DispatchTypes.Method, "!==", e.Arguments.Skip(1))
                          )
                },
                {DispatchTypes.Method, "&", (e, s) =>
                    Expression.And(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "&=", (e, s) =>
                    Expression.AndAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "|", (e, s) =>
                    Expression.Or(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "|", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "|=", (e, s) =>
                    Expression.OrAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "|", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "^", (e, s) =>
                    Expression.ExclusiveOr(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "^", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "^=", (e, s) =>
                    Expression.ExclusiveOrAssign(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "^", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "&&", (e, s) =>
                    Expression.AndAlso(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "&&", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                {DispatchTypes.Method, "||", (e, s) =>
                    Expression.OrElse(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                        ? e.Arguments[1].Reduce(s)
                        : YacqExpression.Dispatch(s, DispatchTypes.Method, "||", e.Arguments.Skip(1)).Reduce(s)
                    )
                },
                #endregion
                #region Global Method: Null Testings
                {DispatchTypes.Method, "??", (e, s) =>
                     Expression.Coalesce(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                         ? e.Arguments[1].Reduce(s)
                         : YacqExpression.Dispatch(s, DispatchTypes.Method, "??", e.Arguments.Skip(1)).Reduce(s)
                     )
                },
                {DispatchTypes.Method, "?", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        "!==",
                        e.Arguments[0],
                        Expression.Constant(null)
                    )
                },
                {DispatchTypes.Method, "!?", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        "===",
                        e.Arguments[0],
                        Expression.Constant(null)
                    )
                },
                #endregion
                #region Global Method: Flowings
                {DispatchTypes.Method, ".", (e, s) =>
                {
                    var a0 = e.Arguments[0].Reduce(s);
                    if (a0 is TypeCandidateExpression && e.Arguments[1] is VectorExpression)
                    {
                        // T.[foo bar]
                        return YacqExpression.TypeCandidate(s, ((TypeCandidateExpression) a0).Candidates
                            .Single(t => t.GetGenericArguments().Length == ((VectorExpression) e.Arguments[1]).Elements.Count)
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
                }},
                {DispatchTypes.Method, "let", (e, s) =>
                    e.Arguments.Any()
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
                                                  .StartWith(s_.Literals.Values.Zip(_, Expression.Assign).ToArray())
                                          )
                                        : (Expression) Expression.Empty()
                                    )
                                )
                              : Expression.Block(e.Arguments.ReduceAll(s))
                        : Expression.Empty()
                },
                {DispatchTypes.Method, "$", DispatchTypes.Method, "let"},
                {DispatchTypes.Method, "fun", (e, s) =>
                    e.Arguments.Any()
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
                        : (Expression) Expression.Lambda(Expression.Empty())
                },
                {DispatchTypes.Method, "\\", DispatchTypes.Method, "fun"},
                {DispatchTypes.Method, "alias", (e, s) =>
                    new SymbolTable(s).Let(s_ => ((VectorExpression) e.Arguments[0]).Elements
                        .Share(_ => _.Zip(_, (i, v) => v.Reduce(s_)
                            .Apply(r => s_.Add(i.Id(), r))
                        ))
                        .ToArray()
                        .Let(_ => e.Arguments.Count > 2
                            ? Expression.Block(e.Arguments.Skip(1).ReduceAll(s_))
                            : e.Arguments.Last().Reduce(s_)
                        )
                    )
                },
                #endregion
                #region Global Method: Generals
                {DispatchTypes.Method, "...", (e, s) =>
                {
                    throw new Exception();
                }},
                {DispatchTypes.Method, ">_<", (e, s) =>
                    Expression.Empty().Apply(_ => Debugger.Break())
                },
                {DispatchTypes.Method, "input", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        YacqExpression.TypeCandidate(typeof(Console)),
                        "ReadLine"
                    )
                },
                {DispatchTypes.Method, typeof(Object), "print", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        YacqExpression.TypeCandidate(typeof(Console)),
                        "WriteLine",
                        e.Left
                    )
                },
                {DispatchTypes.Method, typeof(Object), "printn", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        YacqExpression.TypeCandidate(typeof(Console)),
                        "Write",
                        e.Left
                    )
                },
                {DispatchTypes.Method, "typeof", (e, s) =>
                    Expression.Constant(
#if SILVERLIGHT
                        Type.GetType((String) ((TextExpression) e.Arguments[0]).Value)
#else
                        AppDomain.CurrentDomain.GetAssemblies()
                            .Choose(a => a.GetType(((String) ((TextExpression) e.Arguments[0]).Value)))
                            .First()
#endif
                    )
                },
                #endregion
                #region Global Method: Expressions
                {DispatchTypes.Method, "type", (e, s) =>
                    YacqExpression.TypeCandidate(
#if SILVERLIGHT
                        Type.GetType((String) ((TextExpression) e.Arguments[0]).Value)
#else
                        AppDomain.CurrentDomain.GetAssemblies()
                            .Choose(a => a.GetType((String) ((TextExpression) e.Arguments[0]).Value))
                            .First()
#endif
                    )
                },
                #endregion
                #region Global Method: Symbol Handlings
                {DispatchTypes.Method, "def", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        s.Resolve("$global"),
                        "def",
                        e.Arguments
                    )
                },
                {DispatchTypes.Method, "def!", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        s.Resolve("$global"),
                        "def!",
                        e.Arguments
                    )
                },
                {DispatchTypes.Method, "undef", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        s.Resolve("$global"),
                        "undef",
                        e.Arguments
                    )
                },
                {DispatchTypes.Method, "load", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        s.Resolve("$global"),
                        "load",
                        e.Arguments
                    )
                },
                #endregion
                #region Macro Method: Flowings
                {DispatchTypes.Method, typeof(Object), "let", (e, s) =>
                    e.Left.Reduce(s).Let(_ => Expression.Invoke(
                        YacqExpression.AmbiguousLambda(
                            s,
                            e.Arguments.Skip(1),
                            YacqExpression.AmbiguousParameter(s, _.Type, e.Arguments[0].Id())
                        ).Reduce(s),
                        _
                    ))
                },
                {DispatchTypes.Method, typeof(Object), "alias", (e, s) =>
                    new SymbolTable(s).Apply(s_ => s_.Add(
                        e.Arguments[0].Id(),
                        e.Left.Reduce(s)
                    )).Let(s_ => e.Arguments.Count > 2
                        ? Expression.Block(e.Arguments.Skip(1).ReduceAll(s_))
                        : e.Arguments[1].Reduce(s_)
                    )
                },
                {DispatchTypes.Method, typeof(Object), "cond", (e, s) =>
                    Expression.Condition(
                        e.Left.Reduce(s),
                        e.Arguments[0].Reduce(s),
                        e.Arguments[1].Reduce(s)
                    )
                },
                #endregion
                #region Macro Method: Type Handlings
                {DispatchTypes.Method, typeof(Static<Object>), "new", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Constructor,
                        e.Left,
                        null,
                        e.Arguments
                    )
                },
                {DispatchTypes.Method, typeof(Object), "with", (e, s) =>
                    e.Left.Reduce(s).Let(l => l is NewExpression
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
                    )
                },
                {DispatchTypes.Method, typeof(Object), "to", (e, s) =>
                    Expression.Convert(
                        e.Left.Reduce(s),
                        ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                    )
                },
                {DispatchTypes.Method, typeof(Object), "as", (e, s) =>
                    Expression.TypeAs(
                        e.Left.Reduce(s),
                        ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                    )
                },
                {DispatchTypes.Method, typeof(Object), "is", (e, s) =>
                    Expression.TypeIs(
                        e.Left.Reduce(s),
                        ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                    )
                },
                #endregion
                #region Macro Method: Symbol Handlings
                {DispatchTypes.Method, typeof(SymbolTable), "def", (e, s) =>
                    Expression.Empty().Apply(_ =>
                        e.Left.Reduce(s).Const<SymbolTable>().Add(
                            e.Arguments[0].Id(),
                            e.Arguments[1].Reduce(s)
                        )
                    )
                },
                {DispatchTypes.Method, typeof(SymbolTable), "def!", (e, s) =>
                    Expression.Empty().Apply(_ =>
                        e.Left.Reduce(s).Const<SymbolTable>()[e.Arguments[0].Id()]
                            = e.Arguments[1].Reduce(s)
                    )
                },
                {DispatchTypes.Method, typeof(SymbolTable), "undef", (e, s) =>
                    Expression.Empty().Apply(_ =>
                        e.Left.Reduce(s).Const<SymbolTable>().Remove(e.Arguments[0].Id())
                    )
                },
                {DispatchTypes.Method, typeof(SymbolTable), "load", (e, s) =>
                    Root["*libPath*"].Const<IList<DirectoryInfo>>()
                        .Where(d => d.Exists)
                        .SelectMany(d => new [] { ".yacq", "", }
                            .SelectMany(_ => d.EnumerateFiles(e.Arguments[0].Reduce(s).Const<String>() + _))
                         )
                        .First(f => f.Exists)
                        .Let(f => e.Left.Reduce(s).Const<SymbolTable>().Let(ts =>
                            ts[".loadedFiles"].Const<IList<String>>().Contains(f.FullName)
                                ? Expression.Constant(null)
                                : YacqServices.Parse(
                                      ts,
                                      File.ReadAllText(f.FullName
                                          .Apply(ts[".loadedFiles"].Const<IList<String>>().Add)
                                      )
                                  )
                        ))
                },
                #endregion
                #region Global Member: Generals
                {"...", Expression.Throw(Expression.Constant(new NotImplementedException()))},
                {"true", Expression.Constant(true)},
                {"false", Expression.Constant(false)},
                {"nil", Expression.Constant(null)},
                {DispatchTypes.Member, ">_<", (e, s) =>
                    YacqExpression.Dispatch(
                        s,
                        DispatchTypes.Method,
                        YacqExpression.TypeCandidate(typeof(Debugger)),
                        "Break"
                    )
                },
                #endregion
                #region Global Member: Configurations
                {"*libPath*", Expression.Constant(new List<DirectoryInfo>()
                {
#if !SILVERLIGHT
                    new DirectoryInfo("yacq_lib"),
                    new DirectoryInfo("lib"),
                    new DirectoryInfo("."),
#endif
                })},
                #endregion
                #region Global Member: Types
                // System, Data Types
                {"Object", YacqExpression.TypeCandidate(typeof(Object))},
                {"Boolean", YacqExpression.TypeCandidate(typeof(Boolean))},
                {"Char", YacqExpression.TypeCandidate(typeof(Char))},
                {"String", YacqExpression.TypeCandidate(typeof(String))},
                {"SByte", YacqExpression.TypeCandidate(typeof(SByte))},
                {"Byte", YacqExpression.TypeCandidate(typeof(Byte))},
                {"Int16", YacqExpression.TypeCandidate(typeof(Int16))},
                {"UInt16", YacqExpression.TypeCandidate(typeof(UInt16))},
                {"Int32", YacqExpression.TypeCandidate(typeof(Int32))},
                {"UInt32", YacqExpression.TypeCandidate(typeof(UInt32))},
                {"Int64", YacqExpression.TypeCandidate(typeof(Int64))},
                {"UInt64", YacqExpression.TypeCandidate(typeof(UInt64))},
                {"IntPtr", YacqExpression.TypeCandidate(typeof(IntPtr))},
                {"UIntPtr", YacqExpression.TypeCandidate(typeof(UIntPtr))},
                {"Single", YacqExpression.TypeCandidate(typeof(Single))},
                {"Double", YacqExpression.TypeCandidate(typeof(Double))},
                {"Decimal", YacqExpression.TypeCandidate(typeof(Decimal))},
                {"DateTime", YacqExpression.TypeCandidate(typeof(DateTime))},
                {"DateTimeOffset", YacqExpression.TypeCandidate(typeof(DateTimeOffset))},
                {"TimeSpan", YacqExpression.TypeCandidate(typeof(TimeSpan))},
                {"Guid", YacqExpression.TypeCandidate(typeof(Guid))},
                {"Uri", YacqExpression.TypeCandidate(typeof(Uri))},
                // System, Utility Classes
                {"Convert", YacqExpression.TypeCandidate(typeof(Convert))},
                {"Math", YacqExpression.TypeCandidate(typeof(Math))},
                {"Nullable", YacqExpression.TypeCandidate(typeof(Nullable))},
                {"Random", YacqExpression.TypeCandidate(typeof(Random))},
                {"Tuple", YacqExpression.TypeCandidate(typeof(Tuple))},
                // System.Collections.*
                {"Dictionary", YacqExpression.TypeCandidate(typeof(Dictionary<,>))},
                {"HashSet", YacqExpression.TypeCandidate(typeof(HashSet<>))},
                {"LinkedList", YacqExpression.TypeCandidate(typeof(LinkedList<>))},
                {"List", YacqExpression.TypeCandidate(typeof(List<>))},
                {"Queue", YacqExpression.TypeCandidate(typeof(LinkedList<>))},
                {"Stack", YacqExpression.TypeCandidate(typeof(Stack<>))},
                // System.IO
                {"Directory", YacqExpression.TypeCandidate(typeof(Directory))},
                {"DirectoryInfo", YacqExpression.TypeCandidate(typeof(DirectoryInfo))},
                {"File", YacqExpression.TypeCandidate(typeof(File))},
                {"FileInfo", YacqExpression.TypeCandidate(typeof(FileInfo))},
                {"Path", YacqExpression.TypeCandidate(typeof(Path))},
                // System.Text.*
                {"Encoding", YacqExpression.TypeCandidate(typeof(Encoding))},
                {"Regex", YacqExpression.TypeCandidate(typeof(Regex))},
                {"StringBuilder", YacqExpression.TypeCandidate(typeof(StringBuilder))},
                // LINQ Types
                {"Enumerable", YacqExpression.TypeCandidate(typeof(Enumerable))},
                {"EnumerableEx", YacqExpression.TypeCandidate(typeof(EnumerableEx))},
                {"Queryable", YacqExpression.TypeCandidate(typeof(Queryable))},
                {"QueryableEx", YacqExpression.TypeCandidate(typeof(QueryableEx))},
                {"Observable", YacqExpression.TypeCandidate(typeof(Observable))},
                {"Observer", YacqExpression.TypeCandidate(typeof(Observer))},
                {"ObservableExtensions", YacqExpression.TypeCandidate(typeof(ObservableExtensions))},
                {"Qbservable", YacqExpression.TypeCandidate(typeof(Qbservable))},
                // Generic Delegate Types
                {"Action", YacqExpression.TypeCandidate(
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
                )},
                {"Func", YacqExpression.TypeCandidate(
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
                )},
                #endregion
                #region Macro Member: Type Handlings
                {DispatchTypes.Member, typeof(Static<Object>), "array", (e, s) =>
                    YacqExpression.TypeCandidate(
                        ((TypeCandidateExpression) e.Left.Reduce(s)).ElectedType.MakeArrayType()
                    )
                },
                {DispatchTypes.Member, typeof(Static<Object>), "type", (e, s) =>
                    Expression.Constant(
                        ((TypeCandidateExpression) e.Left.Reduce(s)).ElectedType
                    )
                },
                #endregion
            };
        }
    }
}