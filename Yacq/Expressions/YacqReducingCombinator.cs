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
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    public class YacqReducingCombinator
    {
        private readonly Parser<Expression, Expression> _parser;

        private readonly SymbolTable _symbols;

        private readonly Type _expectedType;

        internal YacqReducingCombinator(Parser<Expression, Expression> parser, SymbolTable symbols, Type expectedType)
        {
            this._parser = parser;
            this._symbols = symbols;
            this._expectedType = expectedType;
        }

        #region Satisfy / Any

        public Parser<Expression, Expression> Satisfy(Func<Expression, Boolean> predicate)
        {
            return this.AndAlso(YacqCombinators.Satisfy(predicate));
        }

        public Parser<Expression, TExpression> Satisfy<TExpression>(Func<TExpression, Boolean> predicate)
            where TExpression : Expression
        {
            return this.Satisfy(e => (e as TExpression).Null(predicate)).Select(e => (TExpression) e);
        }

        public Parser<Expression, TExpression> Satisfy<TExpression>()
            where TExpression : Expression
        {
            return this.Satisfy<TExpression>(e => true);
        }

        public Parser<Expression, Expression> Any()
        {
            return this.Satisfy(e => true);
        }

        #endregion

        #region Is

        public Parser<Expression, Expression> Is(Func<Type, Boolean> typePredicate)
        {
            return this.Satisfy(e => typePredicate(e.Type()));
        }

        public Parser<Expression, Expression> Is(Type type)
        {
            return this.Is(type.IsAssignableFrom);
        }

        public Parser<Expression, Expression> Is<T>()
        {
            return this.Is(typeof(T));
        }

        #endregion

        #region TypeCandidate

        public Parser<Expression, TypeCandidateExpression> TypeCandidate(Func<Type, Boolean> typePredicate)
        {
            return this.Satisfy<TypeCandidateExpression>(e => typePredicate(e.ElectedType));
        }

        public Parser<Expression, TypeCandidateExpression> TypeCandidate(Type type)
        {
            return this.TypeCandidate(type.IsAssignableFrom);
        }

        public Parser<Expression, TypeCandidateExpression> TypeCandidate<T>()
        {
            return this.TypeCandidate(typeof(T));
        }

        public Parser<Expression, TypeCandidateExpression> TypeCandidate()
        {
            return this.Satisfy<TypeCandidateExpression>();
        }

        #endregion

        private Parser<Expression, Expression> AndAlso(Parser<Expression, Expression> parser)
        {
            return this._parser.AndAlso(parser, e => e.Reduce(this._symbols, this._expectedType));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
