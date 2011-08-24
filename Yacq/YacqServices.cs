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
    public static class YacqServices
    {
        public static Expression[] ParseAll(SymbolTable symbols, String code)
        {
            return new Reader(code)
                .Read()
                .ReduceAll(symbols)
                .ToArray();
        }

        public static Expression Parse(SymbolTable symbols, String code)
        {
            return ParseAll(symbols, code).Last();
        }

        public static LambdaExpression ParseLambda(SymbolTable symbols, String code, params AmbiguousParameterExpression[] parameters)
        {
            return (LambdaExpression) YacqExpression.AmbiguousLambda(
                new Reader(code).Read().Single(),
                parameters
            ).Reduce(symbols);
        }

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

        public static LambdaExpression ParseLambda(SymbolTable symbols, Type itType, String code)
        {
            return ParseLambda(symbols, code, YacqExpression.AmbiguousParameter(itType, "it"));
        }

        public static Expression<Func<TReturn>> ParseLambda<TReturn>(SymbolTable symbols, String code)
        {
            return (Expression<Func<TReturn>>) ParseLambda(symbols, code);
        }

        public static Expression<Func<T, TReturn>> ParseLambda<T, TReturn>(SymbolTable symbols, String code)
        {
            return (Expression<Func<T, TReturn>>) ParseLambda(symbols, typeof(T), code);
        }

        public static Expression[] ParseAll(String code)
        {
            return ParseAll(null, code);
        }

        public static Expression Parse(String code)
        {
            return Parse(null, code);
        }

        public static LambdaExpression ParseLambda(String code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(null, code, parameters);
        }

        public static Expression<TDelegate> ParseLambda<TDelegate>(String code, params String[] parameterNames)
        {
            return ParseLambda<TDelegate>(null, code, parameterNames);
        }

        public static LambdaExpression ParseLambda(Type itType, String code)
        {
            return ParseLambda(null, itType, code);
        }

        public static Expression<Func<TReturn>> ParseLambda<TReturn>(String code)
        {
            return ParseLambda<TReturn>(default(SymbolTable), code);
        }

        public static Expression<Func<T, TReturn>> ParseLambda<T, TReturn>(String code)
        {
            return ParseLambda<T, TReturn>(null, code);
        }
    }
}