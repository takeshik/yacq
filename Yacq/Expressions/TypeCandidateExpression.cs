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

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression which is a type candidate, a symbol of types and indicates static references.
    /// </summary>
    public class TypeCandidateExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets the candidate types of this expression.
        /// </summary>
        /// <value>The candidate types of this expression.</value>
        public ReadOnlyCollection<Type> Candidates
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="Type"/> object which indicates the most appropriate type of <see cref="Candidates"/>.
        /// </summary>
        /// <value>The <see cref="Type"/> object which indicates the most appropriate type of <see cref="Candidates"/>,
        /// or <c>null</c> if no appropriate types in this expression</value>
        public Type ElectedType
        {
            get
            {
                return this.Candidates.Count == 1
                    ? this.Candidates.Single()
                    : this.Candidates.Where(t => t.GetGenericArguments().Length == 0)
                          .Let(_ => _.Count() == 1 ? _.Single() : null);
            }
        }

        internal TypeCandidateExpression(
            SymbolTable symbols,
            IList<Type> candidates
        )
            : base(symbols)
        {
            this.Candidates = new ReadOnlyCollection<Type>(candidates ?? new Type[0]);
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return this.ElectedType != null
                ? this.ElectedType.Name
                : this.Candidates.First().Name + "[+" + (this.Candidates.Count - 1) + "]";
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
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="TypeCandidateExpression"/> that represents the type candidates, a symbol of type(s).
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="candidates">An array of <see cref="Type"/> objects that represents the candidate types of the expression.</param>
        /// <returns>An <see cref="TypeCandidateExpression"/> that has specified candidate types.</returns>
        public static TypeCandidateExpression TypeCandidate(SymbolTable symbols, params Type[] candidates)
        {
            return new TypeCandidateExpression(symbols, candidates);
        }

        /// <summary>
        /// Creates a <see cref="TypeCandidateExpression"/> that represents the type candidates, a symbol of type(s).
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="candidates">A sequence of <see cref="Type"/> objects that represents the candidate types of the expression.</param>
        /// <returns>An <see cref="TypeCandidateExpression"/> that has specified candidate types.</returns>
        public static TypeCandidateExpression TypeCandidate(SymbolTable symbols, IEnumerable<Type> candidates)
        {
            return TypeCandidate(
                symbols,
                candidates != null ? candidates.ToArray() : null
            );
        }

        /// <summary>
        /// Creates a <see cref="TypeCandidateExpression"/> that represents the type candidates, a symbol of type(s).
        /// </summary>
        /// <param name="candidates">An array of <see cref="Type"/> objects that represents the candidate types of the expression.</param>
        /// <returns>An <see cref="TypeCandidateExpression"/> that has specified candidate types.</returns>
        public static TypeCandidateExpression TypeCandidate(params Type[] candidates)
        {
            return TypeCandidate(null, candidates);
        }

        /// <summary>
        /// Creates a <see cref="TypeCandidateExpression"/> that represents the type candidates, a symbol of type(s).
        /// </summary>
        /// <param name="candidates">A sequence of <see cref="Type"/> objects that represents the candidate types of the expression.</param>
        /// <returns>An <see cref="TypeCandidateExpression"/> that has specified candidate types.</returns>
        public static TypeCandidateExpression TypeCandidate(IEnumerable<Type> candidates)
        {
            return TypeCandidate(null, candidates);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
