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
    public class AmbiguousLambdaExpression
        : YacqExpression
    {
        public Expression Body
        {
            get;
            private set;
        }

        public ReadOnlyCollection<AmbiguousParameterExpression> Parameters
        {
            get;
            private set;
        }

        public IEnumerable<AmbiguousParameterExpression> UnfixedParameters
        {
            get
            {
                return this.Parameters.Where(p => p.IsUnfixed);
            }
        }

        public override String ToString()
        {
            return String.Join(", ", this.Parameters.Select(p => p.ToString()))
                + " => "
                + this.Body;
        }

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
                                  p => new SymbolEntry(DispatchType.Member, null, p.Name),
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

        public AmbiguousLambdaExpression ApplyTypeArguments(IEnumerable<Type> typeArguments)
        {
            return AmbiguousLambda(
                this.Symbols,
                this.Body,
                this.Parameters
                    .Zip(typeArguments, (p, t) => AmbiguousParameter(
                        p.Symbols,
                        t,
                        p.Name
                    ))
            );
        }

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
        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return new AmbiguousLambdaExpression(symbols, body, parameters);
        }

        public static AmbiguousLambdaExpression AmbiguousLambda(
            SymbolTable symbols,
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(symbols, body, parameters.ToArray());
        }

        public static AmbiguousLambdaExpression AmbiguousLambda(
            Expression body,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return AmbiguousLambda(null, body, parameters);
        }

        public static AmbiguousLambdaExpression AmbiguousLambda(
            Expression body,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return AmbiguousLambda(null, body, parameters.ToArray());
        }
    }
}
