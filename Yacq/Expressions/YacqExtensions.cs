// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides a set of static methods for working with specific kinds of <see cref="YacqExpression"/> and other instances.
    /// </summary>
    public static class YacqExtensions
    {
        /// <summary>
        /// Reduces this node to a simpler expression, with (if possible) additional symbol tables.
        /// </summary>
        /// <param name="expression">The reducing expression.</param>
        /// <param name="symbols">The additional symbol table for reducing. If <paramref name="expression"/> is not <see cref="YacqExpression"/>, this parameter is ignored.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        public static Expression Reduce(this Expression expression, SymbolTable symbols, Type expectedType = null)
        {
            return expression != null
                ? expression is YacqExpression
                      ? ((YacqExpression) expression).Reduce(symbols, expectedType)
                      : YacqExpression.ImplicitConvert(expression, expectedType)
                : null;
        }

        /// <summary>
        /// Generates a sequence of expressions by reducing this expression and enumerates its progress until the expression is not reducible.
        /// </summary>
        /// <param name="expression">The reducing expression.</param>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>A sequence of the reduced expressions.</returns>
        public static IEnumerable<Expression> ReduceScan(this Expression expression, SymbolTable symbols = null, Type expectedType = null)
        {
            Expression result = (expression as YacqExpression).Null(
                e => e.ReduceOnce(symbols, expectedType),
                () => expression.Reduce(symbols, expectedType)
            );
            expression = null;
            while (expression != result)
            {
                yield return expression = result;
                if (expression is YacqExpression)
                {
                    result = ((YacqExpression) expression).ReduceOnce(symbols, expectedType);
                }
                else
                {
                    result = expression.Reduce();
                }
            }
        }

        /// <summary>
        /// Reduces this node to a simpler expression, with (if possible) additional symbol tables. Any errors are ignored and returns <c>null</c>.
        /// </summary>
        /// <param name="expression">The reducing expression.</param>
        /// <param name="symbols">The additional symbol table for reducing. If <paramref name="expression"/> is not <see cref="YacqExpression"/>, this parameter is ignored.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression, or <c>null</c> if reducing was failed.</returns>
        public static Expression TryReduce(this Expression expression, SymbolTable symbols = null, Type expectedType = null)
        {
            try
            {
                return expression.Reduce(symbols, expectedType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Reduces all node in this sequence to a simpler expression, with (if possible) additional symbol tables.
        /// </summary>
        /// <param name="expressions">The sequence which contains reducing expressions.</param>
        /// <param name="symbols">The additional symbol table for reducing. If <paramref name="expressions"/> contains expression which is not <see cref="YacqExpression"/>, this parameter is ignored in them.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The sequence which contains reduced expression.</returns>
        public static IEnumerable<Expression> ReduceAll(this IEnumerable<Expression> expressions, SymbolTable symbols = null, Type expectedType = null)
        {
            return expressions
                .Select(_ => _.Reduce(symbols, expectedType))
                .Where(_ => !(_ is IgnoredExpression));
        }

        /// <summary>
        /// Gets the static type of the expression (which with reduced with additional symbol tables, if possible) that this <see cref="Expression"/> represents.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="symbols">The additional symbol table for reducing. If <paramref name="expression"/> is not <see cref="YacqExpression"/>, this parameter is ignored.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The static type of the expression, or reduced expression if <paramref name="expression"/> is <see cref="YacqExpression"/>.</returns>
        public static Type Type(this Expression expression, SymbolTable symbols = null, Type expectedType = null)
        {
            return expression.Null(expr_ => (expr_ as YacqExpression).Null(e => e.CanReduce
                ? e.Reduce(symbols, expectedType)
                      .If(_ => _ is YacqExpression, _ => null)
                      .Null(_ => _.Type)
                : null,
                () => expression.Type
            ));
        }

        /// <summary>
        /// Gets the static type of the expression (which with reduced with additional symbol tables, if possible) that this <see cref="Expression"/> represents. Any errors are ignored and returns <c>null</c>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="symbols">The additional symbol table for reducing. If <paramref name="expression"/> is not <see cref="YacqExpression"/>, this parameter is ignored.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The static type of the expression, or reduced expression if <paramref name="expression"/> is <see cref="YacqExpression"/>, or <c>null</c> if reducing was failed.</returns>
        public static Type TryType(this Expression expression, SymbolTable symbols = null, Type expectedType = null)
        {
            try
            {
                return expression.Type(symbols, expectedType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the member-reference dispatching.
        /// </summary>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <returns>A chained member-reference <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Member(
            this Expression left,
            SymbolTable symbols,
            String name
        )
        {
            return YacqExpression.Dispatch(symbols, DispatchTypes.Member, left, name);
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the member-reference dispatching.
        /// </summary>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <returns>A chained member-reference <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Member(
            this Expression left,
            String name
        )
        {
            return YacqExpression.Dispatch(DispatchTypes.Member, left, name);
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the method-call dispatching.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A chained method-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Method(
            this Expression left,
            SymbolTable symbols,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return YacqExpression.Dispatch(symbols, DispatchTypes.Method, left, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the method-call dispatching.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A chained method-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Method(
            this Expression left,
            SymbolTable symbols,
            String name,
            params Expression[] arguments
        )
        {
            return YacqExpression.Dispatch(symbols, DispatchTypes.Method, left, name, arguments);
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the method-call dispatching.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A chained method-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Method(
            this Expression left,
            SymbolTable symbols,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return YacqExpression.Dispatch(symbols, DispatchTypes.Method, left, name, arguments);
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the method-call dispatching.
        /// </summary>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="typeArguments">A sequence of <see cref="Type"/> objects that represents the type arguments for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A chained method-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Method(
            this Expression left,
            String name,
            IEnumerable<Type> typeArguments,
            params Expression[] arguments
        )
        {
            return YacqExpression.Dispatch(DispatchTypes.Method, left, name, typeArguments, arguments);
        }

        /// <summary>
        /// Creates a chained <see cref="DispatchExpression"/> that represents the method-call dispatching.
        /// </summary>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">An array of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A chained method-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Method(
            this Expression left,
            String name,
            params Expression[] arguments
        )
        {
            return YacqExpression.Dispatch(DispatchTypes.Method, left, name, arguments);
        }

        /// <summary>
        /// Creates a <see cref="DispatchExpression"/> that represents the method-call dispatching.
        /// </summary>
        /// <param name="left">An <see cref="Expression"/> that representts the receiver or static reference for dispatching.</param>
        /// <param name="name">The name to use for dispatching.</param>
        /// <param name="arguments">A sequence of <see cref="Expression"/> objects that represents the arguments for dispatching.</param>
        /// <returns>A chained method-call <see cref="DispatchExpression"/> that has the properties set to the specified values.</returns>
        public static DispatchExpression Method(
            this Expression left,
            String name,
            IEnumerable<Expression> arguments
        )
        {
            return YacqExpression.Dispatch(DispatchTypes.Method, left, name, arguments);
        }

        /// <summary>
        /// Compiles and evaluates an expression as a parameterless lambda expression.
        /// </summary>
        /// <param name="expression">An <see cref="Expression"/> to evaluate.</param>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="args">An array to pass to the compiled expression as arguments.</param>
        /// <returns>The return value of the compiled expression.</returns>
        /// <remarks>This method may reduce the performance.</remarks>
        public static Object Evaluate(this Expression expression, SymbolTable symbols = null, params Object[] args)
        {
            return expression.Reduce(symbols).If(
                    e => e is ConstantExpression,
                    e => ((ConstantExpression) e).Value,
                    e => (e as LambdaExpression ?? Expression.Lambda(e))
                        .Compile()
                        .DynamicInvoke(args)
            );
        }

        /// <summary>
        /// Compiles and evaluates an expression as a parameterless lambda expression.
        /// </summary>
        /// <typeparam name="T">The type of the return value.</typeparam>
        /// <param name="expression">An <see cref="Expression"/> to evaluate.</param>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="args">An array to pass to the compiled expression as arguments.</param>
        /// <returns>The return value of the compiled expression as the specified type.</returns>
        /// <remarks>This method may reduce the performance.</remarks>
        public static T Evaluate<T>(this Expression expression, SymbolTable symbols = null, params Object[] args)
        {
            return (T) expression.Evaluate(symbols, args);
        }

        internal static IEnumerable<Expression> List(this Expression expression, String head = null)
        {
            return (expression as ListExpression).Let(l =>
                l != null
                    ? head == null
                          ? l.Elements
                          : (l.Elements.Any() ? l[0] as IdentifierExpression : null).Null(_ => _.Name) == head
                                ? l.Elements.Skip(1)
                                : null
                    : null
            );
        }

        internal static T Const<T>(this Object self)
            where T : class
        {
            return self is T
                ? (T) self
                : self is ConstantExpression && ((Expression) self).Type.GetConvertibleTypes().Contains(typeof(T))
                      ? (T) ((ConstantExpression) self).Value
                      : null;
        }

        internal static String Id(this Object self)
        {
            return self is IdentifierExpression ? ((IdentifierExpression) self).Name : null;
        }

        internal static IDictionary<SymbolEntry, SymbolDefinition> ToSymbols(this IEnumerable<ParameterExpression> parameters)
        {
            return parameters.ToDictionary(
                p => new SymbolEntry(p.Name),
                p => (SymbolDefinition) ((e, s, t) => p)
            );
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
