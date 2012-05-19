// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2012 linerlock <x.linerlock@gmail.com>
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
using XSpect.Yacq.Expressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Generates pre-evaluating <see cref="YacqExpression"/> by supplied rules from code string sequence.
    /// </summary>
    public partial class Reader
    {
        /// <summary>
        /// Gets the parser of Yacq.
        /// </summary>
        /// <value>The parser of Yacq.</value>
        public Parser<Char, IEnumerable<YacqExpression>> Parser
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="parser">The sequence of additional rules.</param>
        public Reader(Parser<Char, IEnumerable<YacqExpression>> parser)
        {
            this.Parser = parser;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        public Reader()
            : this(null)
        {
            this.Parser = Defaults.Yacq;
        }

        /// <summary>
        /// Reads the code string and generates expressions.
        /// </summary>
        /// <param name="input">The code string to read.</param>
        /// <returns>Generated expressions.</returns>
        public YacqExpression[] Read(String input){
            var stream = new ReaderStream(input);
            Reply<Char,IEnumerable<YacqExpression>> reply;
            IEnumerable<YacqExpression> result;
            ErrorMessage message;
            switch ((reply = this.Parser(stream))
                .TryGetValue(out result, out message)
            )
            {
                case ReplyStatus.Success:
                    return result.ToArray();
                case ReplyStatus.Failure:
                    throw new ParseException("Syntax Error", reply.Stream.Position, reply.Stream.Position);
                default:
                    throw new ParseException(message.MessageDetails, message.Beginning, message.End);
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
