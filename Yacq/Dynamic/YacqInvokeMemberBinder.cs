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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Dynamic
{
    internal class YacqInvokeMemberBinder
        : InvokeMemberBinder
    {
        private readonly SymbolTable _symbols;

        public YacqInvokeMemberBinder(SymbolTable symbols, String name, Boolean ignoreCase, CallInfo callInfo)
            : base(name, ignoreCase, callInfo)
        {
            this._symbols = symbols;
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            try
            {
                return new DynamicMetaObject(
                    (Static.GetTargetType(target.Value)
                        .Null(t => YacqExpression.TypeCandidate(this._symbols, t))
                        ?? target.Expression.Reduce(this._symbols).TryConvert(target.RuntimeType)
                    )
                        .Method(this._symbols, this.Name,
                            args.SelectAll(o => o.Expression.Reduce(this._symbols).TryConvert(o.RuntimeType))
                        )
                        .Reduce(this._symbols)
                        .If(e => e.Type == typeof(void), e =>
                            Expression.Block(e, Expression.Default(typeof(Object)))
                        )
                        .If(e => e.Type.IsValueType, e =>
                            e.TryConvert(typeof(Object))
                        ),
                    target.Restrictions
                );
            }
            catch (Exception ex)
            {
                return errorSuggestion
                    ?? new DynamicMetaObject(
                           Expression.Throw(Expression.Constant(ex), typeof(Object)),
                           BindingRestrictions.Empty
                       );
            }
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            return this.FallbackInvokeMember(target, args, errorSuggestion);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
