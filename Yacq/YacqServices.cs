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
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;
using XSpect.Yacq.Linq;
#if !__MonoCS__
using System.Reactive.Linq;
using XSpect.Yacq.SystemObjects;

#endif

namespace XSpect.Yacq
{
    /// <summary>
    /// Provides language service static methods, generate expression trees from code strings with (optional) data.
    /// </summary>
    public static class YacqServices
    {
        /// <summary>
        /// Gets the version of this YACQ library.
        /// </summary>
        /// <returns>The version of this YACQ library.</returns>
        public static Version Version
        {
            get
            {
                return typeof(YacqServices).Assembly.GetName().Version;
            }
        }

        /// <summary>
        /// Read code string and generate expressions without reducing.
        /// </summary>
        /// <param name="code">Code string to read.</param>
        /// <returns>All expressions without reducing, generated from the code.</returns>
        public static YacqExpression[] ReadAll(String code)
        {
            return new Reader(new Tokenizer(code)).Read();
        }

        /// <summary>
        /// Read code string and generate expressions without reducing.
        /// </summary>
        /// <param name="code">Code string to read.</param>
        /// <returns>All expressions without reducing, generated from the code.</returns>
        public static YacqExpression Read(String code)
        {
            return ReadAll(code).Last();
        }

        /// <summary>
        /// Parse code string and generate expressions.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse.</param>
        /// <returns>All expressions generated from the code.</returns>
        public static Expression[] ParseAll(SymbolTable symbols, String code)
        {
            return ReadAll(code)
#if SILVERLIGHT
                .Cast<Expression>()
#endif
                .ReduceAll((symbols ?? new SymbolTable())
                    .If(s => !s.ExistsKey("$global"),
                        s => s.Add("$global", Expression.Constant(symbols))
                    )
                    .If(s => !s.ExistsKey("*assembly*"),
                        s => s.Add("*assembly*", Expression.Constant(new YacqAssembly("YacqGeneratedTypes")))
                    )
                )
                .ToArray();
        }

        /// <summary>
        /// Parse code string and generate expressions, only return the last expression.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse.</param>
        /// <returns>The last expressions generated from the code.</returns>
        public static Expression Parse(SymbolTable symbols, String code)
        {
            return ParseAll(symbols, code).Last();
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type returnType, String code, params AmbiguousParameterExpression[] parameters)
        {
            var expressions = ReadAll(code);
            return (LambdaExpression) YacqExpression.AmbiguousLambda(
                symbols,
                returnType,
                expressions.Length == 1
                    ? expressions.Single()
                    : YacqExpression.List(symbols, expressions
#if SILVERLIGHT
                          .Cast<Expression>()
#endif
                      ),
                parameters
            ).Reduce();
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, String code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(symbols, null, code, parameters);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type itType, Type returnType, String code)
        {
            return ParseLambda(symbols, returnType, code, YacqExpression.AmbiguousParameter(itType, "it"));
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type itType, String code)
        {
            return ParseLambda(symbols, itType, null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with specified parameter names.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate that the generating lambda expression represents.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameterNames">Parameter names of the function. Their types are inferred by <typeparamref name="TDelegate"/>.</param>
        /// <returns>The type-explicit lambda expression generated from the code and specified parameter names.</returns>
        public static Expression<TDelegate> ParseLambda<TDelegate>(SymbolTable symbols, String code, params String[] parameterNames)
        {
            return (Expression<TDelegate>) ParseLambda(
                symbols,
                typeof(TDelegate)
                    .GetDelegateSignature()
                    .ReturnType,
                code,
                typeof(TDelegate)
                    .GetDelegateSignature()
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .Zip(parameterNames, YacqExpression.AmbiguousParameter)
                    .ToArray()
            );
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and no parameters.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Action> ParseAction(SymbolTable symbols, String code)
        {
            return (Expression<Action>) ParseLambda(symbols, typeof(void), code, new AmbiguousParameterExpression[0]);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Action<T>> ParseAction<T>(SymbolTable symbols, String code)
        {
            return (Expression<Action<T>>) ParseLambda(symbols, typeof(T), typeof(void), code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and no parameters.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Func<TReturn>> ParseFunc<TReturn>(SymbolTable symbols, String code)
        {
            return (Expression<Func<TReturn>>) ParseLambda(symbols, typeof(TReturn), code, new AmbiguousParameterExpression[0]);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Func<T, TReturn>> ParseFunc<T, TReturn>(SymbolTable symbols, String code)
        {
            return (Expression<Func<T, TReturn>>) ParseLambda(symbols, typeof(T), typeof(TReturn), code);
        }

        /// <summary>
        /// Parse code string and generate expressions.
        /// </summary>
        /// <param name="code">Code string to parse.</param>
        /// <returns>All expressions generated from the code.</returns>
        public static Expression[] ParseAll(String code)
        {
            return ParseAll(null, code);
        }

        /// <summary>
        /// Parse code string and generate expressions, only return the last expression.
        /// </summary>
        /// <param name="code">Code string to parse.</param>
        /// <returns>The last expressions generated from the code.</returns>
        public static Expression Parse(String code)
        {
            return Parse(null, code);
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(Type returnType, String code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(null, returnType, code, parameters);
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(String code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(default(SymbolTable), code, parameters);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(Type itType, Type returnType, String code)
        {
            return ParseLambda(null, itType, returnType, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(Type itType, String code)
        {
            return ParseLambda(default(SymbolTable), itType, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with specified parameter names.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate that the generating lambda expression represents.</typeparam>
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameterNames">Parameter names of the function. Their types are inferred by <typeparamref name="TDelegate"/>.</param>
        /// <returns>The type-explicit lambda expression generated from the code and specified parameter names.</returns>
        public static Expression<TDelegate> ParseLambda<TDelegate>(String code, params String[] parameterNames)
        {
            return ParseLambda<TDelegate>(null, code, parameterNames);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and no parameters.
        /// </summary>
        /// <param name="code">Code string to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Action> ParseAction(String code)
        {
            return ParseAction(null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Action<T>> ParseAction<T>(String code)
        {
            return ParseAction<T>(null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and no parameters.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="code">Code string to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Func<TReturn>> ParseFunc<TReturn>(String code)
        {
            return ParseFunc<TReturn>(null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Func<T, TReturn>> ParseFunc<T, TReturn>(String code)
        {
            return ParseFunc<T, TReturn>(null, code);
        }

        #region Exension Methods

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IEnumerable source, SymbolTable symbols)
        {
            return new YacqQueryable(symbols, source.AsQueryable());
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IEnumerable<TSource> source, SymbolTable symbols)
        {
            return new YacqQueryable<TSource>(symbols, source.AsQueryable());
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQueryable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IQueryable source, SymbolTable symbols)
        {
            return new YacqQueryable(symbols, source);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IQueryable<TSource> source, SymbolTable symbols)
        {
            return new YacqQueryable<TSource>(symbols, source);
        }

#if !__MonoCS__
        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IObservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IObservable<TSource> source, SymbolTable symbols)
        {
            return new YacqQbservable<TSource>(symbols, source.AsQbservable());
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQbservable" /> to convert to a <see cref="YacqQbservable" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQbservable" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable Yacq(this IQbservable source, SymbolTable symbols)
        {
            return new YacqQbservable(symbols, source);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQbservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IQbservable<TSource> source, SymbolTable symbols)
        {
            return new YacqQbservable<TSource>(symbols, source);
        }
#endif

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IEnumerable source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IEnumerable<TSource> source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQueryable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IQueryable source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IQueryable<TSource> source)
        {
            return source.Yacq(null);
        }

#if !__MonoCS__
        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IObservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IObservable<TSource> source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQbservable" /> to convert to a <see cref="YacqQbservable" />.</param>
        /// <returns>The source as a <see cref="YacqQbservable" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable Yacq(this IQbservable source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQbservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IQbservable<TSource> source)
        {
            return source.Yacq(null);
        }
#endif

        #endregion
    }
}