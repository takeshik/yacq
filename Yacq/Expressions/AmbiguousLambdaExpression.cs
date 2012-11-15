// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an ambiguous lambda expression, a lambda expression with <see cref="AmbiguousParameterExpression"/>.
    /// </summary>
    public class AmbiguousLambdaExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets the return type of this expression.
        /// </summary>
        /// <value>The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</value>
        public Type ReturnType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the body expressions in this expression.
        /// </summary>
        /// <value>The body expressions in this expression.</value>
        public ReadOnlyCollection<Expression> Bodies
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

        internal AmbiguousLambdaExpression(
            SymbolTable symbols,
            Type returnType,
            IList<Expression> bodies,
            IList<AmbiguousParameterExpression> parameters
        )
            : base(symbols)
        {
            this.ReturnType = returnType;
            this.Bodies = new ReadOnlyCollection<Expression>(bodies ?? new Expression[0]);
            this.Parameters = new ReadOnlyCollection<AmbiguousParameterExpression>(parameters ?? new AmbiguousParameterExpression[0]);
            this.SetPosition(this.Bodies.Concat(this.Parameters));
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
                + (this.Bodies.Count > 2 ? "{ ... }" : this.Bodies.Single().ToString());
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return this.UnfixedParameters.Any()
                ? expectedType != null && expectedType.GetDelegateSignature() != null
                      ? this.ApplyTypeArguments(expectedType)
                            .If(e => e.UnfixedParameters.IsEmpty(), e => e, e => null)
                      : null
                : this.Parameters
                      .Select(p => p.Reduce(symbols))
                      .Cast<ParameterExpression>()
                      .ToArray()
                      .Let(ps =>
                          new SymbolTable(symbols, ps.ToSymbols())
                              .Let(s => this.Bodies.Count == 0
                                  ? Empty()
                                  : this.Bodies.Count == 1
                                        ? this.Bodies.Single().Reduce(s)
                                        : Block(this.Bodies.ReduceAll(s))
                              )
                              .Let(e => this.ReturnType != null
                                  ? Lambda(GetDelegateType(this.Parameters
                                        .Select(p => p.Type)
                                        .EndWith(this.ReturnType)
                                        .ToArray()
                                    ), e, ps)
                                  : expectedType == null
                                        ? Lambda(e, ps)
                                        : expectedType.GetDelegateSignature()
                                              .GetParameters()
                                              .Let(eps => eps.Length == ps.Length && eps
                                                  .Zip(ps, (p, a) => p.ParameterType.IsAppropriate(a.Type))
                                                  .All(_ => _)
                                              )
                                                  ? expectedType
                                                        .If(et => et.ContainsGenericParameters, et =>
                                                            et.ReplaceGenericArguments(et
                                                                .GetDelegateSignature()
                                                                .GetAllParameters()
                                                                .Select(p => p.ParameterType)
                                                                .Zip(ps.Select(p => p.Type).EndWith(e.Type), Tuple.Create)
                                                                .Where(_ => _.Item1.IsGenericParameter)
                                                                .ToDictionary(_ => _.Item1, _ => _.Item2)
                                                            )
                                                        )
                                                        .Let(et => typeof(LambdaExpression).IsAssignableFrom(expectedType)
                                                            ? (Expression) Quote(Lambda(et.GetGenericArguments()[0], e, ps))
                                                            : Lambda(et, e, ps)
                                                        )
                                                  : null
                              )
                      );
        }

        /// <summary>
        /// Creates new <see cref="AmbiguousLambdaExpression"/> with specified parameter types.
        /// </summary>
        /// <param name="parameters">The types for paramaters of new <see cref="AmbiguousLambdaExpression"/>.</param>
        /// <returns>The new <see cref="AmbiguousLambdaExpression"/> with specified parameters.</returns>
        public AmbiguousLambdaExpression ApplyTypeArguments(IEnumerable<AmbiguousParameterExpression> parameters)
        {
            return AmbiguousLambda(
                this.Symbols,
                // HACK: Clear all cache in Bodies to suppress undefied ParameterExpression errors.
                this.Bodies.Apply(_ => _
                    .SelectMany(e => e.GetDescendants())
                    .OfType<YacqExpression>()
                    .ForEach(e => e.ClearCache())
                ),
                parameters
            );
        }

        /// <summary>
        /// Creates new <see cref="AmbiguousLambdaExpression"/> with specified type argument map.
        /// </summary>
        /// <param name="typeArgumentMap">The type argument map for parameters of new <see cref="AmbiguousLambdaExpression"/>.</param>
        /// <returns>The new <see cref="AmbiguousLambdaExpression"/> with parameters which is specified type.</returns>
        public AmbiguousLambdaExpression ApplyTypeArguments(IDictionary<Type, Type> typeArgumentMap)
        {
            return this.ApplyTypeArguments(this.Parameters
                .Select(p => AmbiguousParameter(
                    p.Symbols,
                    typeArgumentMap.TryGetValue(p.Type) ?? p.Type,
                    p.Name
                ))
            );
        }

        /// <summary>
        /// Creates new <see cref="AmbiguousLambdaExpression"/> with specified parameter types.
        /// </summary>
        /// <param name="types">The types for paramaters of new <see cref="AmbiguousLambdaExpression"/>.</param>
        /// <returns>The new <see cref="AmbiguousLambdaExpression"/> with parameters which is specified type.</returns>
        public AmbiguousLambdaExpression ApplyTypeArguments(IEnumerable<Type> types)
        {
            return this.ApplyTypeArguments(this.Parameters
                .Zip(types, (p, t) => AmbiguousParameter(
                    p.Symbols,
                    t,
                    p.Name
                ))
            );
        }

        /// <summary>
        /// Creates new <see cref="AmbiguousLambdaExpression"/> to match specified delegate type.
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
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Type returnType,
            IEnumerable<Expression> bodies,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return new AmbiguousLambdaExpression(
                symbols,
                returnType,
                bodies != null ? bodies.ToArray() : null,
                parameters
            );
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Type returnType,
            IEnumerable<Expression> bodies,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(
                symbols,
                returnType,
                bodies,
                parameters != null ? parameters.ToArray() : null
            );
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Type returnType,
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(
                symbols,
                returnType,
                body != null ? EnumerableEx.Return(body) : null,
                parameters
            );
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Type returnType,
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(
                symbols,
                returnType,
                body,
                parameters != null ? parameters.ToArray() : null
            );
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            IEnumerable<Expression> bodies,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(symbols, null, bodies, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            IEnumerable<Expression> bodies,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(symbols, null, bodies, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(symbols, null, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(symbols, null, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Type returnType,
            IEnumerable<Expression> bodies,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(null, returnType, bodies, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Type returnType,
            IEnumerable<Expression> bodies,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(null, returnType, bodies, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Type returnType,
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(null, returnType, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Type returnType,
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(null, returnType, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            IEnumerable<Expression> bodies,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(default(Type), bodies, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="bodies">>A sequence of <see cref="Expression"/> objects that represents the bodies of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            IEnumerable<Expression> bodies,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(default(Type), bodies, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(default(Type), body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="AmbiguousLambdaExpression"/> that represents the lambda expression with <see cref="AmbiguousParameterExpression"/>.
        /// </summary>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>An <see cref="AmbiguousLambdaExpression"/> that has the properties set to the specified values.</returns>
        public static AmbiguousLambdaExpression AmbiguousLambda(
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(default(Type), body, parameters);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
