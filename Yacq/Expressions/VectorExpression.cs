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
using XSpect.Yacq.Collections;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents a vector, similar to <see cref="ListExpression"/> but this generates arrays when you reduce.
    /// </summary>
    public class VectorExpression
        : YacqSequenceExpression
    {
        internal VectorExpression(
            SymbolTable symbols,
            YacqList elements
        )
            : base(symbols, elements)
        {
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return "[" + String.Join(" ", this.Elements.Select(e => e.ToString())) + "]";
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return this.Elements.Any()
                // TODO: Use expectedType?
                ? this.Elements.Select(e => e.List(":"))
                      .ToArray()
                      .If(ps => ps.All(p => p != null),
                          ps => ps.Select(p => p.First().Reduce(symbols)).ToArray().Let(ks =>
                              ps.Select(p => p.Last().Reduce(symbols)).ToArray().Let(vs =>
                                  ks.Select(k => k.Type).GetCommonType().Let(kt =>
                                      vs.Select(v => v.Type).GetCommonType().Let(vt =>
                                          Dispatch(symbols, DispatchTypes.Constructor,
                                              TypeCandidate(typeof(Dictionary<,>).MakeGenericType(kt, vt)),
                                              null
                                          ).Method(symbols, "has", ks
                                              .Select(k => ImplicitConvert(k, kt))
                                              .Zip(vs.Select(v => ImplicitConvert(v, vt)),
                                                  (k, v) => Vector(symbols, k, v)
                                              )
#if SILVERLIGHT
                                              .Cast<Expression>()
#endif
                                          )
                                      )
                                  )
                              )
                          ),
                          _ => (Expression) this.Elements.ReduceAll(symbols)
                              .Let(es => es
                                  .Select(e => e.Type)
                                  .Distinct()
                                  .GetCommonType()
                                  .Let(t => NewArrayInit(t, es.Select(e => ImplicitConvert(e, t))))
                              )
                      )
                : NewArrayInit(expectedType.Null(t => t.GetElementType()) ?? typeof(Object));
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="VectorExpression"/> that represents the vector.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="VectorExpression"/> that has specified elements.</returns>
        public static VectorExpression Vector(SymbolTable symbols, YacqList elements)
        {
            return new VectorExpression(symbols, elements);
        }

        /// <summary>
        /// Creates a <see cref="VectorExpression"/> that represents the vector.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="VectorExpression"/> that has specified elements.</returns>
        public static VectorExpression Vector(SymbolTable symbols, IEnumerable<Expression> elements)
        {
            return Vector(symbols, YacqList.Create(elements));
        }

        /// <summary>
        /// Creates a <see cref="VectorExpression"/> that represents the vector.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="elements">An array of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="VectorExpression"/> that has specified elements.</returns>
        public static VectorExpression Vector(SymbolTable symbols, params Expression[] elements)
        {
            return Vector(symbols, (IEnumerable<Expression>) elements);
        }

        /// <summary>
        /// Creates a <see cref="VectorExpression"/> that represents the vector.
        /// </summary>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="VectorExpression"/> that has specified elements.</returns>
        public static VectorExpression Vector(YacqList elements)
        {
            return Vector(null, elements);
        }

        /// <summary>
        /// Creates a <see cref="VectorExpression"/> that represents the vector.
        /// </summary>
        /// <param name="elements">A sequence of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="VectorExpression"/> that has specified elements.</returns>
        public static VectorExpression Vector(IEnumerable<Expression> elements)
        {
            return Vector(null, elements);
        }

        /// <summary>
        /// Creates a <see cref="VectorExpression"/> that represents the vector.
        /// </summary>
        /// <param name="elements">An array of <see cref="Expression"/> objects that represents the elements of the expression.</param>
        /// <returns>A <see cref="VectorExpression"/> that has specified elements.</returns>
        public static VectorExpression Vector(params Expression[] elements)
        {
            return Vector(null, elements);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
