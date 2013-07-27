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
using System.Text.RegularExpressions;

namespace XSpect.Yacq.Expressions
{
    internal static class EscapeSequences
    {
        internal const String Pattern =
            @"\\\$\([^\(]*(((?<Open>\()[^\(\)]*)+((?<Close-Open>\))[^\(\)]*)+)*\)(?(Open)(?!))"
                + @"|\\M-\\C-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                + @"|\\C-\\M-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                + @"|\\C-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                + @"|\\M-(\\[0-7]{1,3}|\\x[0-9A-Fa-f]{1,2}|[ -~])"
                + @"|\\u[0-9A-Fa-f]{1,4}"
                + @"|\\U[0-9A-Fa-f]{1,8}"
                + @"|\\[0-7]{1,3}"
                + @"|\\x[0-9A-Fa-f]{1,2}"
                + @"|\\.";

        internal static String Parse(String str, IList<String> codes)
        {
            return Regex.Replace(str, Pattern, m => ParseFragment(m.Value, codes));
        }

        private static String ParseFragment(String str, IList<String> codes)
        {
            if (str[0] != '\\')
            {
                return str;
            }
            str = str.Substring(1);
            if (str.StartsWithInvariant("$("))
            {
                codes.Add(ParseFragment(str.Substring(1), codes));
                return "{" + (codes.Count - 1) + "}";
            }
            else if (str.StartsWithInvariant("M-\\C-") || str.StartsWithInvariant("C-\\M-"))
            {
                return ((Char) (ParseFragment(str.Substring(4), codes)[0] & 0x9f | 0x80)).ToString();
            }
            else if (str.StartsWithInvariant("C-"))
            {
                return ((Char) (ParseFragment(str.Substring(2), codes)[0] & 0x9f)).ToString();
            }
            else if (str.StartsWithInvariant("M-"))
            {
                return ((Char) (ParseFragment(str.Substring(2), codes)[0] & 0xff | 0x80)).ToString();
            }
            else if (str[0] == 'u' && str.Length > 1)
            {
                return ((Char) Convert.ToInt32(str.Substring(1), 16)).ToString();
            }
            else if (str[0] == 'U' && str.Length > 1)
            {
                return ((Char) Convert.ToInt32(str.Substring(1), 16)).ToString();
            }
            else if (Char.IsDigit(str, 0))
            {
                return ((Char) Convert.ToInt32(str, 8)).ToString();
            }
            else if (str[0] == 'x' && str.Length > 1)
            {
                return ((Char) Convert.ToInt32(str.Substring(1), 16)).ToString();
            }
            else
            {
                switch (str[0])
                {
                    case 'a':
                        return "\a";
                    case 'b':
                        return "\b";
                    case 'e':
                        return "\x1b";
                    case 'f':
                        return "\f";
                    case 'n':
                        return "\n";
                    case 'r':
                        return "\r";
                    case 's':
                        return " ";
                    case 't':
                        return "\t";
                    case 'v':
                        return "\v";
                    case 'N':
                        return Environment.NewLine;
                    default:
                        return str;
                }
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
