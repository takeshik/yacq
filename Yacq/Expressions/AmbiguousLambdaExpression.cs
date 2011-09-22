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

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an ambiguous lambda expression, a lambda expression with <see cref="AmbiguousParameterExpression"/>.
    /// </summary>
    public class AmbiguousLambdaExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets the body of this expression.
        /// </summary>
        /// <value>The body of this expression.</value>
        public Expression Body
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the ambiguous parameters of this expression.
        /// </summary>
        /// <value>The ambiguous parameters of this expression.</value>
        public ReadOnlyCollection<AmbiguousParameterExpression> Parameters
        {
            get;
            private set;
        }

        /// <summary>
        /// Get the ambiguous parameters of this expression which is not fixed its type.
        /// </summary>
        /// <value>The ambiguous parameters of this expression which is not fixed its type.</value>
        public IEnumerable<AmbiguousParameterExpression> UnfixedParameters
        {
            get
            {
                return this.Parameters.Where(p => p.IsUnfixed);
            }
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return String.Join(", ", this.Parameters.Select(p => p.ToString()))
                + " => "
                + this.Body;
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols)
        {
            return this.UnfixedParameters.Any()
                ? null
                : this.Parameters
                      .Select(p => p.Reduce(symbols))
                      .Cast<ParameterExpression>()
                      .ToArray()
                      .Let(ps => Lambda(
                          this.Body.Reduce(
                              new SymbolTable(symbols, ps.ToDictionary(
                                  p => new SymbolEntry(DispatchTypes.Member, null, p.Name),
                                  p => (SymbolDefinition) ((e, s) => p)
                              ))
                          ),
                          ps
                      ));
        }

        internal AmbiguousLambdaExpression(
            SymbolTable symbols,
            Expression body,
            IList<AmbiguousParameterExpression> parameters
        )
            : base(symbols)
        {
            this.Body = body;
            this.Parameters = new ReadOnlyCollection<AmbiguousParameterExpression>(parameters);
        }

        /// <summary>
        /// Create new <see cref="AmbiguousLambdaExpression"/> with specified type argument map.
        /// </summary>
        /// <param name="typeArgumentMap">The type argument map for parameters of new <see cref="AmbiguousLambdaExpression"/>.</param>
        /// <returns>The new <see cref="AmbiguousLambdaExpression"/> with parameters which is specified type.</returns>
        public AmbiguousLambdaExpression ApplyTypeArguments(IDictionary<Type, Type> typeArgumentMap)
        {
            return AmbiguousLambda(
                this.Symbols,
                this.Body,
                this.Parameters
                    .Select(p => AmbiguousParameter(
                        p.Symbols,
                        p.Type != null && typeArgumentMap.ContainsKey(p.Type)
                            ? typeArgumentMap[p.Type]
                            : p.Type,
                        p.Name
                    ))
            );
        }

        /// <summary>
        /// Create new <see cref="AmbiguousLambdaExpression"/> with specified parameter types.
        /// </summary>
        /// <param name="types">The types for paramaters of new <see cref="AmbiguousLambdaExpression"/>.</param>
        /// <returns>The new <see cref="AmbiguousLambdaExpression"/> with parameters which is specified type.</returns>
        public AmbiguousLambdaExpression ApplyTypeArguments(IEnumerable<Type> types)
        {
            return AmbiguousLambda(
                this.Symbols,
                this.Body,
                this.Parameters
                    .Zip(types, (p, t) => AmbiguousParameter(
                        p.Symbols,
                        t,
                        p.Name
                    ))
            );
        }

        /// <summary>
        /// Create new <see cref="AmbiguousLambdaExpression"/> to match specified delegate type.
        /// </summary>
        /// <param name="delegateType">The delegate type which is matched for new <see cref="AmbiguousLambdaExpression"/>.</param>
        /// <returns>The new <see cref="AmbiguousLambdaExpression"/> which is matched with <paramref name="delegateType"/>.</returns>
        public AmbiguousLambdaExpression ApplyTypeArguments(Type delegateType)
        {
            return this.ApplyTypeArguments(delegateType.GetDelegateSignature()
                .GetParameters()
                .Select(p => p.ParameterType)
            );
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="body">>An <see cref="Expression"/> to set the <see cref="AmbiguousLambdaExpression.Body"/> property equal to.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return new AmbiguousLambdaExpression(symbols, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="body">>An <see cref="Expression"/> to set the <see cref="AmbiguousLambdaExpression.Body"/> property equal to.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(symbols, body, parameters.ToArray());
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="body">>An <see cref="Expression"/> to set the <see cref="AmbiguousLambdaExpression.Body"/> property equal to.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(null, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="body">>An <see cref="Expression"/> to set the <see cref="AmbiguousLambdaExpression.Body"/> property equal to.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(null, body, parameters.ToArray());
        }
    }
}
