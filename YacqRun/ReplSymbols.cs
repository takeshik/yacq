// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ Runner
 *   Runner and Compiler frontend of YACQ
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
 * All rights reserved.
 * 
 * This file is part of YACQ Runner.
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.Runner
{
    internal static class ReplSymbols
    {
        internal static Boolean ReplContinuous
        {
            get;
            set;
        }

        internal static Boolean ReplVerbose
        {
            get;
            set;
        }

        internal static readonly List<Tuple<String, Expression, Object>> ReplHistory
            = new List<Tuple<String, Expression, Object>>();

        [YacqSymbol("!history")]
        public static readonly Expression History
            = Expression.Constant(ReplHistory);

        [YacqSymbol("!continuous")]
        public static readonly Expression Continuous
            = Expression.Property(null, typeof(ReplSymbols), "ReplContinuous");

        [YacqSymbol("!verbose")]
        public static readonly Expression Verbose
            = Expression.Property(null, typeof(ReplSymbols), "ReplVerbose");

        static ReplSymbols()
        {
            ReplContinuous = false;
            ReplVerbose = true;
        }

        [YacqSymbol(DispatchTypes.Method, "!exit")]
        public static Expression ExitRepl(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.Dispatch(
                DispatchTypes.Method,
                YacqExpression.TypeCandidate(typeof(Environment)),
                "Exit",
                Expression.Constant(0)
            );
        }

        [YacqSymbol(DispatchTypes.Method, "!help")]
        public static Expression ShowHelp(DispatchExpression e, SymbolTable s, Type t)
        {
            Console.WriteLine(
                #region String
@"Commands:
  (!exit)
    Exit this program.
  (!help)
    Show this message.
  (!chelp)
    Show command-line option help.
  (!man)
    Open the reference manual web page.
  (!about)
    Show general and copyright description.
  (!gc)
    Run GC manually.
  !history
    Get history list (Tuples of (input string, parsed expression, result value).
  !inputs
    Get input string history list.
  !exprs
    Get parsed expression history list.
  !results
    Get result value history list.
  !continuous
    Boolean member. Get or set whether the symbols is continuous during REPL.
    ex) (= !continuous true) ; Default is false.
  !verbose
    Boolean member. Get or set whether REPL shows verbose messages.
    ex) (= !verbosed false) ; Default is true.
  CODE
    Run one-line CODE.
  <<INPUT [ENTER] CODES
    Run multi-line CODES while INPUT line was got (heredoc <<EOT).
  CODE
    Otherwise: Run one-line code."
                #endregion
);
            return Expression.Empty();
        }

        [YacqSymbol(DispatchTypes.Method, "!chelp")]
        public static Expression ShowCommandHelp(DispatchExpression e, SymbolTable s, Type t)
        {
            Console.WriteLine(
                #region String
@"Command-line Options:
  YacqRun
    Run in REPL mode.
  YacqRun PATH
    Run script file in PATH.
  YacqRun -c PATH
    Compile script file in PATH to Console Application EXE.
  YacqRun -cw PATH
    Compile script file in PATH to Windows Application EXE.
  YacqRun -cl PATH
    Compile script file in PATH to Library DLL.
  NOTE: Specify - in PATH means read script data from standard input."
                #endregion
);
            return Expression.Empty();
        }

        [YacqSymbol(DispatchTypes.Method, "!man")]
        public static Expression ShowManualPage(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.Dispatch(
                DispatchTypes.Method,
                YacqExpression.TypeCandidate(typeof(Process)),
                "Start",
                Expression.Constant("http://www.yacq.net/")
            );
        }

        [YacqSymbol(DispatchTypes.Method, "!about")]
        public static Expression ShowCopying(DispatchExpression e, SymbolTable s, Type t)
        {
            Console.WriteLine(
                #region String
@"YACQ <http://www.yacq.net/>
  Yet Another Compilable Query Language, based on Expression Trees API
  Language service is provided by Yacq.dll, the assembly name is:
    {0}
YACQ Runner (YACQRun) is part of YACQ
  Runner and Compiler of YACQ

Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
All rights reserved.

YACQ and YACQ Runner are Free Software; licensed under the MIT License.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the ""Software""), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE."
                #endregion
                , typeof(YacqServices).Assembly.GetName()
            );
            return Expression.Empty();
        }

        [YacqSymbol(DispatchTypes.Method, "!gc")]
        public static Expression CollectGarbage(DispatchExpression e, SymbolTable s, Type t)
        {
            return YacqExpression.Dispatch(
                DispatchTypes.Method,
                YacqExpression.TypeCandidate(typeof(GC)),
                "Collect"
            );
        }

        [YacqSymbol(DispatchTypes.Member, "!inputs")]
        public static Expression Inputs(DispatchExpression e, SymbolTable s, Type t)
        {
            return Expression.Constant(ReplHistory.Select(_ => _.Item1).ToArray());
        }

        [YacqSymbol(DispatchTypes.Member, "!exprs")]
        public static Expression Expressions(DispatchExpression e, SymbolTable s, Type t)
        {
            return Expression.Constant(ReplHistory.Select(_ => _.Item2).ToArray());
        }

        [YacqSymbol(DispatchTypes.Member, "!results")]
        public static Expression Results(DispatchExpression e, SymbolTable s, Type t)
        {
            return Expression.Constant(ReplHistory.Select(_ => _.Item3).ToArray());
        }
    }
}