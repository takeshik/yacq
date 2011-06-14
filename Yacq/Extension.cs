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

namespace XSpect.Yacq
{
    internal static class Extension
    {
        internal static Boolean If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate)
        {
            return predicate(self);
        }

        internal static TResult If<TReceiver, TResult>(this TReceiver self, Func<TReceiver, Boolean> predicate, TResult valueIfTrue, TResult valueIfFalse)
        {
            if (self == null)
            {
                return default(TResult);
            }
            else if (self == null || predicate(self))
            {
                return valueIfTrue;
            }
            else
            {
                return valueIfFalse;
            }
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, TReceiver valueIfTrue)
        {
            return self.If(predicate, valueIfTrue, self);
        }

        internal static TResult If<TReceiver, TResult>(this TReceiver self, Func<TReceiver, Boolean> predicate, Func<TReceiver, TResult> funcIfTrue, Func<TReceiver, TResult> funcIfFalse)
        {
            if (predicate(self))
            {
                return funcIfTrue(self);
            }
            else
            {
                return funcIfFalse(self);
            }
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Func<TReceiver, TReceiver> funcIfTrue)
        {
            return self.If(predicate, funcIfTrue, _ => _);
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Action<TReceiver> actionIfTrue, Action<TReceiver> actionIfFalse)
        {
            if (predicate(self))
            {
                actionIfTrue(self);
            }
            else
            {
                actionIfFalse(self);
            }
            return self;
        }

        internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Action<TReceiver> actionIfTrue)
        {
            return self.If(predicate, actionIfTrue, _ =>
            {
            });
        }

        internal static TResult Null<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func, TResult valueIfNull)
            where TReceiver : class
        {
            if (self == null)
            {
                return valueIfNull;
            }
            else
            {
                return func(self);
            }
        }

        internal static TResult Null<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func)
            where TReceiver : class
        {
            return Null(self, func, default(TResult));
        }

        internal static void Null<TReceiver>(this TReceiver self, Action<TReceiver> action)
        {
            if (self != null)
            {
                action(self);
            }
        }

        internal static TResult Let<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func)
        {
            return func(self);
        }

        internal static TReceiver Apply<TReceiver>(this TReceiver self, params Action<TReceiver>[] actions)
        {
            return Apply(self, (IEnumerable<Action<TReceiver>>) actions);
        }

        internal static TReceiver Apply<TReceiver>(this TReceiver self, IEnumerable<Action<TReceiver>> actions)
        {
            foreach (var a in actions)
            {
                a(self);
            }
            return self;
        }

        internal static Expression TryConvert(this Expression expr, Type type)
        {
            return type == null || expr.Type == type
                ? expr
                : Expression.Convert(expr, type);
        }
    }
}