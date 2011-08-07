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
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    public static class YacqExtension
    {
        public static Expression Reduce(this Expression expr, SymbolTable symbols)
        {
            return expr != null
                ? expr is YacqExpression
                      ? ((YacqExpression) expr).Reduce(symbols)
                      : expr.Reduce()
                : null;
        }

        public static Expression TryReduce(this Expression expr, SymbolTable symbols = null)
        {
            try
            {
                return expr.Reduce(symbols);
            }
            catch
            {
                return null;
            }
        }

        public static IEnumerable<Expression> ReduceAll(this IEnumerable<Expression> expressions, SymbolTable symbols = null)
        {
            return expressions.Select(_ => _.Reduce(symbols));
        }

        internal static IEnumerable<Expression> List(this Expression expr, String head)
        {
            return (expr as ListExpression).If(
                l => l != null && (l[0] as IdentifierExpression).Null(_ => _.Name) == head,
                l => l.Elements.Skip(1),
                l => null
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
    }
}
