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
using System.Runtime.Serialization;
using XSpect.Yacq.LanguageServices;

namespace XSpect.Yacq
{
#if !SILVERLIGHT
    [Serializable()]
#endif
    public class ParseException
        : Exception
    {
        public Token Token
        {
            get;
            private set;
        }

        public ParseException(String message)
            : base(message)
        {
        }

        public ParseException(String message, Token token)
            : base(message + String.Format(
                  " ({0} at line {1}, column {2}, position {3})",
                  token.Type,
                  token.Line,
                  token.Column,
                  token.Position
              ))
        {
            this.Token = token;
        }

#if !SILVERLIGHT
        protected ParseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
#endif
    }
}