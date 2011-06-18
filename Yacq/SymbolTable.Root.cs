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
using System.Text.RegularExpressions;
using XSpect.Yacq.Expressions;
using System.Reactive.Linq;
using System.Reflection;

namespace XSpect.Yacq
{
    public partial class SymbolTable
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
            AddFlowOperators();
            AddSystemOperators();
            AddTypes();
            AddLiterals();
        }

        private static void AddArithmeticOperators()
        {
            Root.Add("+", ((Transformer) ((e, s) => e.Elements.Count == 2
                ? (Expression) Expression.UnaryPlus(e[1].Reduce(s))
                : e.Elements.Count == 3
                      ? Expression.Add(e[1].Reduce(s), e[2].Reduce(s))
                      : Expression.Add(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(YacqExpression.Identifier("+"))).Reduce(s))
            )));
            Root.Add("-", ((Transformer) ((e, s) => e.Elements.Count == 2
                ? (Expression) Expression.Negate(e[1].Reduce(s))
                : e.Elements.Count == 3
                      ? Expression.Subtract(e[1].Reduce(s), e[2].Reduce(s))
                      : Expression.Subtract(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(YacqExpression.Identifier("+"))).Reduce(s))
            )));
            Root.Add("*", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.Multiply(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("+"))).Reduce(s))
                : Expression.Multiply(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("/", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.Divide(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("+"))).Reduce(s))
                : Expression.Divide(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("**", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.Power(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("**"))).Reduce(s))
                : Expression.Power(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("%", ((Transformer) ((e, s) =>
                Expression.Modulo(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("<<", ((Transformer) ((e, s) =>
                Expression.LeftShift(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add(">>", ((Transformer) ((e, s) =>
                Expression.RightShift(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("++", ((Transformer) ((e, s) =>
                Expression.Increment(e[1].Reduce(s))
            )));
            Root.Add("--", ((Transformer) ((e, s) =>
                Expression.Decrement(e[1].Reduce(s))
            )));
            Root.Add("!", ((Transformer) ((e, s) =>
                Expression.Not(e[1].Reduce(s))
            )));
            Root.Add("~", ((Transformer) ((e, s) =>
                Expression.OnesComplement(e[1].Reduce(s))
            )));
            Root.Add("<", ((Transformer) ((e, s) =>
                Expression.LessThan(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("<=", ((Transformer) ((e, s) =>
                Expression.LessThanOrEqual(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add(">", ((Transformer) ((e, s) =>
                Expression.GreaterThan(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add(">=", ((Transformer) ((e, s) =>
                Expression.GreaterThanOrEqual(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("==", ((Transformer) ((e, s) =>
                Expression.Equal(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("!=", ((Transformer) ((e, s) =>
                Expression.NotEqual(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("===", ((Transformer) ((e, s) =>
                Expression.ReferenceEqual(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("!==", ((Transformer) ((e, s) =>
                Expression.ReferenceNotEqual(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("&", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.And(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("&"))).Reduce(s))
                : Expression.And(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("^", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.ExclusiveOr(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("^"))).Reduce(s))
                : Expression.ExclusiveOr(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("|", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.Or(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("|"))).Reduce(s))
                : Expression.Or(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("&&", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.AndAlso(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("&&"))).Reduce(s))
                : Expression.AndAlso(e[1].Reduce(s), e[2].Reduce(s))
            )));
            Root.Add("||", ((Transformer) ((e, s) => e.Elements.Count > 3
                ? Expression.OrElse(e[1].Reduce(s), YacqExpression.List(e.Elements.Skip(2).StartWith(Expression.Constant("||"))).Reduce(s))
                : Expression.OrElse(e[1].Reduce(s), e[2].Reduce(s))
            )));
        }

        private static void AddFlowOperators()
        {
            Root.Add("\\", ((Transformer) ((e, s) => e[1] is VectorExpression
                ? ((VectorExpression) e[1]).Elements
                      .Select(p => p.List(":").If(
                          _ => _ != null,
                          _ => Expression.Parameter(((TypeCandidateExpression) _.Last().Reduce(s)).Candidates.Single(), _.First().Id()),
                          _ => Expression.Parameter(null, _.First().Id())
                      ))
                      .ToArray()
                      .Let(ps => (Expression) Expression.Lambda(
                          e.Elements.Count > 3
                              ? Expression.Block(e.Elements.Skip(2).Select(_ => _.Reduce(new SymbolTable(s, ps.ToDictionary(p => p.Name, p => (Object) p)))))
                              : e.Elements[2].Reduce(new SymbolTable(s, ps.ToDictionary(p => p.Name, p => (Object) p))),
                          ps
                      )
                  )
                : Expression.Block(e.Elements.Skip(1).Select(_ => _.Reduce(s)))
            )));
            Root.Add(".", ((Transformer) ((e, s) => (e[1].Reduce(s).Let(e1 =>
                e1 is TypeCandidateExpression && e[2] is VectorExpression
                    ? (Expression) YacqExpression.TypeCandidate(((TypeCandidateExpression) e1).Candidates
                          .Single(t => t.GetGenericArguments().Length == ((VectorExpression) e[2]).Elements.Count)
                          .MakeGenericType(((VectorExpression) e[2]).Elements
                              .Select(_ => _.Reduce(s))
                              .OfType<TypeCandidateExpression>()
                              .Select(_ => _.Candidates.Single())
                              .ToArray()
                          )
                      )
                    : e[2] is ListExpression
                          ? (Expression) ((ListExpression) e[2]).Let(e2 =>
                                e2[0].List(".").If(
                                    _ => _ == null,
                                    _ => e1 is TypeCandidateExpression
                                        ? YacqExpression.MethodDispatch(
                                              ((TypeCandidateExpression) e1).Candidates
                                                  .SelectMany(t => t.GetMethods().Where(m => m.Name == e2[0].Id())),
                                              null,
                                              e2.Elements.Skip(1).Select(a => a.Reduce(s)).ToArray()
                                          )
                                        : YacqExpression.MethodDispatch(
                                              e1.Reduce(s),
                                              e1.Type.GetMethods()
                                                  .Concat(s.AllValues
                                                      .OfType<TypeCandidateExpression>()
                                                      .SelectMany(c => c.Candidates)
                                                      .SelectMany(t => t.GetExtensionMethods())
                                                  )
                                                  .Where(m => m.Name == e2[0].Id()),
                                              null,
                                              e2.Elements.Skip(1).Select(a => a.Reduce(s)).ToArray()
                                          ),
                                    _ => ((VectorExpression) _.ElementAt(2)).Elements
                                        .Cast<TypeCandidateExpression>()
                                        .Select(t => t.Candidates.Single())
                                        .ToArray()
                                        .Let(ta => e1 is TypeCandidateExpression
                                            ? YacqExpression.MethodDispatch(
                                                  ((TypeCandidateExpression) e1).Candidates
                                                      .SelectMany(t => t.GetMethods().Where(m => m.Name == _.ElementAt(1).Id())),
                                                  ta,
                                                  e2.Elements.Skip(1).Select(a => a.Reduce(s)).ToArray()
                                              )
                                            : YacqExpression.MethodDispatch(
                                                  e1.Reduce(s),
                                                  e1.Type.GetMethods()
                                                      .Concat(s.AllValues
                                                          .OfType<TypeCandidateExpression>()
                                                          .SelectMany(c => c.Candidates)
                                                          .SelectMany(t => t.GetExtensionMethods())
                                                      )
                                                      .Where(m => m.Name == _.ElementAt(1).Id()),
                                                  ta,
                                                  e2.Elements.Skip(1).Select(a => a.Reduce(s)).ToArray()
                                              )
                                        )
                                )
                            )
                          : e[2] is VectorExpression
                                ? e1 is TypeCandidateExpression
                                      ? YacqExpression.MemberDispatch(
                                            ((TypeCandidateExpression) e1).Candidates
                                                .SelectMany(t => t.GetDefaultMembers().OfType<PropertyInfo>()),
                                            ((VectorExpression) e[2]).Elements.Select(a => a.Reduce(s)).ToArray()
                                        )
                                      : YacqExpression.MemberDispatch(
                                            e1.Reduce(s),
                                            e1.Type.GetDefaultMembers().OfType<PropertyInfo>(),
                                            ((VectorExpression) e[2]).Elements.Select(a => a.Reduce(s)).ToArray()
                                        )
                                : e1 is TypeCandidateExpression
                                      ? YacqExpression.MemberDispatch(
                                            ((TypeCandidateExpression) e1).Candidates
                                                .SelectMany(t => t.GetMembers(BindingFlags.Public | BindingFlags.Static))
                                                .Where(m => m.Name == e[2].Id())
                                        )
                                      : YacqExpression.MemberDispatch(
                                            e1.Reduce(s),
                                            e1.Type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
                                                .Where(m => m.Name == e[2].Id())
                                        )
            )))));
        }

        private static void AddSystemOperators()
        {
            Root.Add("import-type", ((Transformer) ((e, s) =>
                YacqExpression.TypeCandidate(AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType(e[2].Reduce(s).Const<String>()))
                    .First(t => t != null)
                ).Apply(c => Root.Add(e[1].Id(), c)).Let(_ => default(Expression))
            )));
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

        private static void AddLiterals()
        {
            Root.Add("true", Expression.Constant(true));
            Root.Add("false", Expression.Constant(false));
            Root.Add("null", Expression.Constant(null));
        }
    }
}