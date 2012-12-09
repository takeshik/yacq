// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id: f11cdf26be2df17b9601745f738ca022ac231687 $
/* YACQ REPL
 *   REPL and remote code evaluating system provider of YACQ
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
 * All rights reserved.
 * 
 * This file is part of YACQ REPL.
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
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Repl
{
    internal static class ReplSymbols
    {
        [YacqSymbol(DispatchTypes.Method, "!exit")]
        public static Expression ExitRepl(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.TypeCandidate(typeof(Environment)).Method(s, "Exit",
                Expression.Constant(0)
            );
        }

        [YacqSymbol(DispatchTypes.Method, "!man")]
        public static Expression ShowManualPage(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.TypeCandidate(typeof(Process)).Method(s, "Start",
                Expression.Constant("http://yacq.net/")
            );
        }

        [YacqSymbol(DispatchTypes.Method, "!gc")]
        public static Expression CollectGarbage(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.TypeCandidate(typeof(GC)).Method(s, "Collect");
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
