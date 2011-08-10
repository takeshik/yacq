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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;
using System.Text.RegularExpressions;

namespace XSpect.Yacq
{
    partial class SymbolTable
    {
        public static SymbolTable Root
        {
            get;
            private set;
        }

        static SymbolTable()
        {
            Root = new SymbolTable();
            AddArithmeticOperators();
            AddLogicalOperators();
            AddFlowOperators();
            AddLiterals();
            AddTypes();
            AddMacros();
        }

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

        private static void AddArithmeticOperators()
        {
            Root.Add(DispatchType.Method, "+", (e, s) =>
                e.Arguments.Any(a => a.Reduce(s).Type(s) == typeof(String))
                    ? YacqExpression.Dispatch(
                          s,
                          DispatchType.Method,
                          YacqExpression.TypeCandidate(typeof(String)),
                          "Concat",
                          e.Arguments
                      )
                    : e.Arguments.Count == 1
                          ? (Expression) Expression.UnaryPlus(e.Arguments[0].Reduce(s))
                          : Expression.Add(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                                ? e.Arguments[1].Reduce(s)
                                : YacqExpression.Dispatch(s, DispatchType.Method, "+", e.Arguments.Skip(1)).Reduce(s)
                            )
            );
            Root.Add(DispatchType.Method, "-", (e, s) =>
                e.Arguments.Count == 1
                    ? (Expression) Expression.Negate(e.Arguments[0].Reduce(s))
                    : Expression.Subtract(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                          ? e.Arguments[1].Reduce(s)
                          : YacqExpression.Dispatch(s, DispatchType.Method, "-", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "*", (e, s) =>
                Expression.Multiply(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "*", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "/", (e, s) =>
                Expression.Divide(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "/", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "**", (e, s) =>
                 Expression.Power(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchType.Method, "**", e.Arguments.Skip(1)).Reduce(s)
                 )
            );
            Root.Add(DispatchType.Method, "%", (e, s) =>
                 Expression.Modulo(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchType.Method, "%", e.Arguments.Skip(1)).Reduce(s)
                 )
            );
            Root.Add(DispatchType.Method, "<<", (e, s) =>
                 Expression.LeftShift(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchType.Method, "<<", e.Arguments.Skip(1)).Reduce(s)
                 )
            );
            Root.Add(DispatchType.Method, ">>", (e, s) =>
                 Expression.RightShift(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                     ? e.Arguments[1].Reduce(s)
                     : YacqExpression.Dispatch(s, DispatchType.Method, ">>", e.Arguments.Skip(1)).Reduce(s)
                 )
            );
            Root.Add(DispatchType.Method, "++", (e, s) =>
                 Expression.Increment(e.Arguments[0].Reduce(s))
            );
            Root.Add(DispatchType.Method, "--", (e, s) =>
                 Expression.Decrement(e.Arguments[0].Reduce(s))
            );
        }

        private static void AddLogicalOperators()
        {
            Root.Add(DispatchType.Method, "!", (e, s) =>
                Expression.Not(e.Arguments[0].Reduce(s))
            );
            Root.Add(DispatchType.Method, "~", (e, s) =>
                Expression.OnesComplement(e.Arguments[0].Reduce(s))
            );
            Root.Add(DispatchType.Method, "<", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.LessThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, "<", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, "<", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, "<=", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.LessThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, "<=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, "<=", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, ">", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.GreaterThan(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, ">", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, ">", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, ">=", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.GreaterThanOrEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, ">=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, ">=", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, "==", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.Equal(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, "==", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, "==", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, "!=", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.NotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, "!=", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, "!=", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, "===", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.ReferenceEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, "===", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, "===", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, "!==", (e, s) =>
                e.Arguments.Count == 2
                    ? (Expression) Expression.ReferenceNotEqual(e.Arguments[0].Reduce(s), e.Arguments[1].Reduce(s))
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&",
                          YacqExpression.Dispatch(s, DispatchType.Method, "!==", e.Arguments[0], e.Arguments[1]),
                          YacqExpression.Dispatch(s, DispatchType.Method, "!==", e.Arguments.Skip(1))
                      )
            );
            Root.Add(DispatchType.Method, "&", (e, s) =>
                Expression.And(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "|", (e, s) =>
                Expression.Or(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "|", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "^", (e, s) =>
                Expression.ExclusiveOr(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "^", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "&&", (e, s) =>
                Expression.AndAlso(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "&&", e.Arguments.Skip(1)).Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, "||", (e, s) =>
                Expression.OrElse(e.Arguments[0].Reduce(s), e.Arguments.Count == 2
                    ? e.Arguments[1].Reduce(s)
                    : YacqExpression.Dispatch(s, DispatchType.Method, "||", e.Arguments.Skip(1)).Reduce(s)
                )
            );
        }

        private static void AddFlowOperators()
        {
            Root.Add(DispatchType.Method, ".", (e, s) =>
            {
                if (e.Arguments[0] is TypeCandidateExpression && e.Arguments[1] is VectorExpression)
                {
                    // T.[foo bar]
                    return YacqExpression.TypeCandidate(s, ((TypeCandidateExpression) e.Arguments[0]).Candidates
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
                              DispatchType.Method,
                              e.Arguments[0],
                              ((IdentifierExpression) a1e0l.First()).Name,
                              ((VectorExpression) a1e0l.Last()).Elements
                                  .Cast<TypeCandidateExpression>()
                                  .Select(_ => _.ElectedType),
                              a1.Elements.Skip(1)
                          )
                        : YacqExpression.Dispatch(
                              s,
                              DispatchType.Method,
                              e.Arguments[0],
                              ((IdentifierExpression) a1[0]).Name,
                              a1.Elements.Skip(1)
                          );
                }
                else if (e.Arguments[1] is VectorExpression)
                {
                    return YacqExpression.Dispatch(
                        s,
                        DispatchType.Member,
                        e.Arguments[0],
                        null,
                        ((VectorExpression) e.Arguments[1]).Elements
                    );
                }
                else
                {
                    return YacqExpression.Dispatch(
                        s,
                        DispatchType.Member,
                        e.Arguments[0],
                        ((IdentifierExpression) e.Arguments[1]).Name
                    );
                }
            });
            Root.Add(DispatchType.Method, "\\", (e, s) =>
                 e.Arguments[0] is VectorExpression
                     ? ((VectorExpression) e.Arguments[0]).Elements
                           .Select(p => p.List(":").If(_ => _ != null,
                               _ => YacqExpression.AmbiguousParameter(
                                   s,
                                   ((TypeCandidateExpression) _.Last().Reduce(s)).ElectedType,
                                   ((IdentifierExpression) _.First()).Name
                               ),
                               _ => YacqExpression.AmbiguousParameter(
                                   s,
                                   ((IdentifierExpression) p).Name
                               )
                           ))
                           .ToArray()
                           .Let(p => YacqExpression.AmbiguousLambda(s, e.Arguments.Count == 1
                               ? Expression.Empty()
                               : e.Arguments.Count == 2
                                     ? e.Arguments[1]
                                     : Expression.Block(e.Arguments.Skip(1).ReduceAll(s)),
                               p
                           ))
                     : e.Arguments.Count == 0
                           ? Expression.Empty()
                           : e.Arguments.Count == 1
                                 ? e.Arguments[0]
                                 : Expression.Block(e.Arguments.ReduceAll(s))
             );
        }

        private static void AddLiterals()
        {
            Root.Add("true", Expression.Constant(true));
            Root.Add("false", Expression.Constant(false));
            Root.Add("nil", Expression.Constant(null));
        }

        private static void AddTypes()
        {
            Root.Add("Object", YacqExpression.TypeCandidate(typeof(Object)));
            Root.Add("Boolean", YacqExpression.TypeCandidate(typeof(Boolean)));
            Root.Add("Char", YacqExpression.TypeCandidate(typeof(Char)));
            Root.Add("String", YacqExpression.TypeCandidate(typeof(String)));
            Root.Add("SByte", YacqExpression.TypeCandidate(typeof(SByte)));
            Root.Add("Byte", YacqExpression.TypeCandidate(typeof(Byte)));
            Root.Add("Int16", YacqExpression.TypeCandidate(typeof(Int16)));
            Root.Add("UInt16", YacqExpression.TypeCandidate(typeof(UInt16)));
            Root.Add("Int32", YacqExpression.TypeCandidate(typeof(Int32)));
            Root.Add("UInt32", YacqExpression.TypeCandidate(typeof(UInt32)));
            Root.Add("Int64", YacqExpression.TypeCandidate(typeof(Int64)));
            Root.Add("UInt64", YacqExpression.TypeCandidate(typeof(UInt64)));
            Root.Add("IntPtr", YacqExpression.TypeCandidate(typeof(IntPtr)));
            Root.Add("Single", YacqExpression.TypeCandidate(typeof(Single)));
            Root.Add("Double", YacqExpression.TypeCandidate(typeof(Double)));
            Root.Add("Decimal", YacqExpression.TypeCandidate(typeof(Decimal)));
            Root.Add("DateTime", YacqExpression.TypeCandidate(typeof(DateTime)));
            Root.Add("DateTimeOffset", YacqExpression.TypeCandidate(typeof(DateTimeOffset)));
            Root.Add("TimeSpan", YacqExpression.TypeCandidate(typeof(TimeSpan)));
            Root.Add("Guid", YacqExpression.TypeCandidate(typeof(Guid)));
            Root.Add("Math", YacqExpression.TypeCandidate(typeof(Math)));
            Root.Add("Convert", YacqExpression.TypeCandidate(typeof(Convert)));
            Root.Add("Tuple", YacqExpression.TypeCandidate(typeof(Tuple)));
            Root.Add("Regex", YacqExpression.TypeCandidate(typeof(Regex)));
            Root.Add("Enumerable", YacqExpression.TypeCandidate(typeof(Enumerable)));
            Root.Add("EnumerableEx", YacqExpression.TypeCandidate(typeof(EnumerableEx)));
            Root.Add("Queryable", YacqExpression.TypeCandidate(typeof(Queryable)));
            Root.Add("Observable", YacqExpression.TypeCandidate(typeof(Observable)));
            Root.Add("Qbservable", YacqExpression.TypeCandidate(typeof(Qbservable)));
            Root.Add("Action", YacqExpression.TypeCandidate(
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
            ));
            Root.Add("Func", YacqExpression.TypeCandidate(
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
            ));
        }

        private static void AddMacros()
        {
            Root.Add(DispatchType.Method, typeof(Object), "as", (e, s) =>
                Expression.TypeAs(
                    e.Left.Reduce(s),
                    ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                )
            );
            Root.Add(DispatchType.Method, typeof(Object), "is", (e, s) =>
                Expression.TypeIs(
                    e.Left.Reduce(s),
                    ((TypeCandidateExpression) e.Arguments[0].Reduce(s)).ElectedType
                )
            );
            Root.Add(DispatchType.Method, typeof(Object), "let", (e, s) =>
                e.Arguments[1].Reduce(new SymbolTable(s).Apply(_ => _.Add(
                    ((IdentifierExpression) e.Arguments[0]).Name,
                    e.Left.Reduce(s)
                )))
            );
            Root.Add(DispatchType.Method, typeof(Object), "cond", (e, s) =>
                Expression.Condition(
                    e.Left.Reduce(s),
                    e.Arguments[0].Reduce(s),
                    e.Arguments[1].Reduce(s)
                )
            );
            Root.Add(DispatchType.Method, null, "input", (e, s) =>
                YacqExpression.Dispatch(
                    s,
                    DispatchType.Method,
                    YacqExpression.TypeCandidate(typeof(Console)),
                    "ReadLine"
                )
            );
            Root.Add(DispatchType.Method, typeof(Object), "print", (e, s) =>
                YacqExpression.Dispatch(
                    s,
                    DispatchType.Method,
                    YacqExpression.TypeCandidate(typeof(Console)),
                    "WriteLine",
                    e.Left
                )
            );
        }
    }
}