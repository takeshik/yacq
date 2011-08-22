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
    public class LambdaListExpression
        : YacqExpression
    {
        public ReadOnlyCollection<Expression> Elements
        {
            get;
            private set;
        }

        public ReadOnlyCollection<AmbiguousParameterExpression> Parameters
        {
            get;
            private set;
        }

        public override String ToString()
        {
            return "{" + String.Join(" ", this.Elements.Select(e => e.ToString())) + "}";
        }

        protected override Expression ReduceImpl(SymbolTable symbols)
        {
            return Enumerable.Range(0, this.Elements
                .SelectMany(e => e.GetDescendants())
                .Max(e =>
                {
                    Int32 value = -1;
                    return e is IdentifierExpression && ((IdentifierExpression) e).Name.Let(s =>
                        s.StartsWith("$") && Int32.TryParse(s.Substring(1), out value)
                    )
                        ? value
                        : -1;
                }) + 1
            )
                .Select(i => AmbiguousParameter(symbols, "$" + i))
                .ToArray()
                .Let(ps => AmbiguousLambda(symbols, List(symbols, this.Elements), ps));
        }

        internal LambdaListExpression(
            SymbolTable symbols,
            IList<Expression> elements,
            IList<AmbiguousParameterExpression> parameters
        )
            : base(symbols)
        {
            this.Elements = new ReadOnlyCollection<Expression>(elements);
            this.Parameters = new ReadOnlyCollection<AmbiguousParameterExpression>(parameters);
        }
    }

    partial class YacqExpression
    {
        public static LambdaListExpression LambdaList(
            SymbolTable symbols,
            Expression[] elements,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return new LambdaListExpression(symbols, elements, parameters);
        }

        public static LambdaListExpression LambdaList(
            SymbolTable symbols,
            IEnumerable<Expression> elements,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return LambdaList(symbols, elements.ToArray(), parameters.ToArray());
        }

        public static LambdaListExpression LambdaList(
            SymbolTable symbols,
            IEnumerable<Expression> elements
        )
        {
            return LambdaList(symbols, elements.ToArray(), Enumerable.Empty<AmbiguousParameterExpression>());
        }

        public static LambdaListExpression LambdaList(
            Expression[] elements,
            params AmbiguousParameterExpression[] parameters
        )
        {
            return LambdaList(null, elements, parameters);
        }

        public static LambdaListExpression LambdaList(
            IEnumerable<Expression> elements,
            IEnumerable<AmbiguousParameterExpression> parameters
        )
        {
            return LambdaList(null, elements.ToArray(), parameters.ToArray());
        }

        public static LambdaListExpression LambdaList(
            IEnumerable<Expression> elements
        )
        {
            return LambdaList(null, elements.ToArray(), Enumerable.Empty<AmbiguousParameterExpression>());
        }
    }
}
