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
using Parseq;
using Parseq.Combinators;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    internal static class ParserExtensions
    {
        internal static Parser<TToken, TResult> Do<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            params Action<TResult>[] actions
        )
        {
            return parser.Select(e => e.Apply(actions));
        }

        internal static Parser<Char, YacqExpression> SetPosition(this Parser<Char, YacqExpression> parser)
        {
            Parser<Char, Position> pos = stream => Reply.Success(stream, stream.Position);
            return pos.SelectMany(s =>
                parser.SelectMany(p =>
                    pos.Select(e =>
                        p.Apply(_ => _.SetPosition(s, new Position(e.Line, e.Column - 1, e.Index - 1)))
                    )
                )
            );
        }

        internal static Parser<TToken, TResult> EnterContext<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            String name
        )
        {
            return ((Parser<TToken, Position>) (stream => Reply.Success(stream, stream.Position)))
                .SelectMany(p => parser.Do(_ => Reader.State.Current.Null(s => s.EnterContext(name, p))));
        }

        internal static Parser<TToken, TResult> LeaveContext<TToken, TResult>(
            this Parser<TToken, TResult> parser,
            String name
        )
        {
            return ((Parser<TToken, Position>) (stream => Reply.Success(stream, stream.Position)))
                .SelectMany(p => parser.Do(_ => Reader.State.Current.Null(s => s.LeaveContext(name))));
        }
    }
}
