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
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;

namespace XSpect.Yacq
{
    /// <summary>
    /// Provides language service static methods, generate expression trees from code strings with (optional) data.
    /// </summary>
    public static class YacqServices
    {
        /// <summary>
        /// Parse code string and generate expressions.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse.</param>
        /// <returns>All expressions generated from the code.</returns>
        public static Expression[] ParseAll(SymbolTable symbols, String code)
        {
            return new Reader(code)
                .Read()
#if SILVERLIGHT
                .Cast<Expression>()
#endif
                .ReduceAll(symbols)
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
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, String code, params AmbiguousParameterExpression[] parameters)
        {
            return (LambdaExpression) YacqExpression.AmbiguousLambda(
                new Reader(code).Read().Single(),
                parameters
            ).Reduce(symbols);
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
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type itType, String code)
        {
            return ParseLambda(symbols, code, YacqExpression.AmbiguousParameter(itType, "it"));
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no parameters.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Func<TReturn>> ParseLambda<TReturn>(SymbolTable symbols, String code)
        {
            return (Expression<Func<TReturn>>) ParseLambda(symbols, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Func<T, TReturn>> ParseLambda<T, TReturn>(SymbolTable symbols, String code)
        {
            return (Expression<Func<T, TReturn>>) ParseLambda(symbols, typeof(T), code);
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
        /// <param name="code">Code string to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(String code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(null, code, parameters);
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
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(Type itType, String code)
        {
            return ParseLambda(null, itType, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no parameters.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="code">Code string to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Func<TReturn>> ParseLambda<TReturn>(String code)
        {
            return ParseLambda<TReturn>(default(SymbolTable), code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="code">Code string to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Func<T, TReturn>> ParseLambda<T, TReturn>(String code)
        {
            return ParseLambda<T, TReturn>(null, code);
        }
    }
}