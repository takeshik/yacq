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
    /// Represents an macro expression, which is invocable like lambdas, but they are reduced into another expression in pre-evaluate time.
    /// </summary>
    public class MacroExpression
        : YacqExpression
    {
        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        /// <returns><c>true</c> if the node can be reduced, otherwise <c>false</c>.</returns>
        public override Boolean CanReduce
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the body expression in this expression.
        /// </summary>
        /// <value>The body expression in this expression.</value>
        public Expression Body
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parameters of this expression.
        /// </summary>
        /// <value>The parameters of this expression.</value>
        /// <remarks>All parameters must be typed as <see cref="Expression"/>. Unfixed parameter means to accept <see cref="Expression"/>.</remarks>
        public ReadOnlyCollection<AmbiguousParameterExpression> Parameters
        {
            get;
            private set;
        }

        internal MacroExpression(SymbolTable symbols, Expression body, IList<AmbiguousParameterExpression> parameters)
            : base(symbols)
        {
            if (parameters.Any(p => p.Type(symbols) != null && !typeof(Expression).IsAppropriate(p.Type)))
            {
                throw new ArgumentException("All parameters of macro must be Expression", "parameters");
            }
            this.Parameters = new ReadOnlyCollection<AmbiguousParameterExpression>(parameters ?? new AmbiguousParameterExpression[0]);
            this.Body = body ?? Empty();
            this.SetPosition(this.Parameters.EndWith(this.Body));
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return null;
        }

        /// <summary>
        /// Evaluates this macro expression.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="arguments">The arguments of this macro expression.</param>
        /// <returns>Result expression of applying this macro expression.</returns>
        public Expression Evaluate(SymbolTable symbols, IEnumerable<Expression> arguments)
        {
            return this.Parameters
                .Zip(arguments ?? new Expression[0], (p, a) => p.Type() == null || p.Type.IsAppropriate(a.Type)
                    ? Tuple.Create(p.Name, a)
                    : null
                )
                .ToArray()
                .Let(ps => ps.Any(_ => _ == null)
                    ? null
                    : this.Body.Reduce(new SymbolTable(symbols).Apply(s =>
                          ps.ForEach(p => s.Add(p.Item1, p.Item2.If(
                              e => !e.TryType(s).Null(t => typeof(Expression).IsAppropriate(t)),
                              _ => Quote(s, p.Item2)
                          )))
                      ))
                )
                .Null(_ => _.Method(symbols, "eval"));
        }

        /// <summary>
        /// Evaluates this macro expression.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="arguments">The arguments of this macro expression.</param>
        /// <returns>Result expression of applying this macro expression.</returns>
        public Expression Evaluate(SymbolTable symbols, params Expression[] arguments)
        {
            return this.Evaluate(symbols, (IEnumerable<Expression>) arguments);
        }

        /// <summary>
        /// Evaluates this macro expression.
        /// </summary>
        /// <param name="arguments">The arguments of this macro expression.</param>
        /// <returns>Result expression of applying this macro expression.</returns>
        public Expression Evaluate(IEnumerable<Expression> arguments)
        {
            return this.Evaluate(null, arguments);
        }

        /// <summary>
        /// Evaluates this macro expression.
        /// </summary>
        /// <param name="arguments">The arguments of this macro expression.</param>
        /// <returns>Result expression of applying this macro expression.</returns>
        public Expression Evaluate(params Expression[] arguments)
        {
            return this.Evaluate(null, arguments);
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="MacroExpression"/> that represents the macro, a pre-evaluate time expression preprocessor.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>A <see cref="MacroExpression"/> that has the properties set to the specified values.</returns>
        public static MacroExpression Macro(
            SymbolTable symbols,
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return new MacroExpression(symbols, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="MacroExpression"/> that represents the macro, a pre-evaluate time expression preprocessor.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>A <see cref="MacroExpression"/> that has the properties set to the specified values.</returns>
        public static MacroExpression Macro(
            SymbolTable symbols,
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return Macro(
                symbols,
                body,
                parameters != null ? parameters.ToArray() : null
            );
        }

        /// <summary>
        /// Creates a <see cref="MacroExpression"/> that represents the macro, a pre-evaluate time expression preprocessor.
        /// </summary>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">An array that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>A <see cref="MacroExpression"/> that has the properties set to the specified values.</returns>
        public static MacroExpression Macro(
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return Macro(null, body, parameters);
        }

        /// <summary>
        /// Creates a <see cref="MacroExpression"/> that represents the macro, a pre-evaluate time expression preprocessor.
        /// </summary>
        /// <param name="body">>An <see cref="Expression"/>  that represents the body of this expression.</param>
        /// <param name="parameters">A sequence that contains <see cref="AmbiguousParameterExpression"/> objects to use to populate the <see cref="AmbiguousLambdaExpression.Parameters"/> collection.</param>
        /// <returns>A <see cref="MacroExpression"/> that has the properties set to the specified values.</returns>
        public static MacroExpression Macro(
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return Macro(null, body, parameters);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
