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
using Parseq.Combinators;
using XSpect.Yacq.Expressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Generates pre-evaluating <see cref="YacqExpression"/> by supplied rules from code string sequence.
    /// </summary>
    public class Reader
    {
        /// <summary>
        /// Gets the grammar definition to read.
        /// </summary>
        /// <value>The grammar definition to read.</value>
        public Grammar Grammar
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reader macro rules.
        /// </summary>
        /// <value>A sequence of the reader macro rules.</value>
        public IEnumerable<Parser<Char, YacqExpression>> Macros
        {
            get
            {
                this.CheckIfMacrosSupported();
                return this.Grammar.Get["term.ext"];
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        /// <param name="grammar">The grammar definition to read.</param>
        public Reader(Grammar grammar)
        {
            this.Grammar = grammar;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reader"/> class.
        /// </summary>
        public Reader()
            : this(Grammar.Standard)
        {
        }

        /// <summary>
        /// Reads the code string and generates expressions.
        /// </summary>
        /// <param name="input">The code string to read.</param>
        /// <returns>Generated expressions.</returns>
        public YacqExpression[] Read(IEnumerable<Char> input)
        {
            using (var stream = (input ?? "").AsStream())
            {
                IReply<Char, IEnumerable<YacqExpression>> reply;
                IEnumerable<YacqExpression> result;
                ErrorMessage message;
                switch ((reply = this.GetDefinitiveParser()(stream)).TryGetValue(out result, out message))
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

        /// <summary>
        /// Returns the reader macro rule with specified ID.
        /// </summary>
        /// <param name="id">The ID of the reader macro rule.</param>
        /// <returns>The reader macro rule with specified ID.</returns>
        public Parser<Char, YacqExpression> GetMacro(String id)
        {
            this.CheckIfMacrosSupported();
            return this.Grammar.Get["term.ext", id];
        }

        /// <summary>
        /// Determines whether the specified reader macro is defined.
        /// </summary>
        /// <param name="id">The ID of the reader macro to locate.</param>
        /// <returns><c>true</c> if the grammar contains a reader macro with the key; otherwise, <c>false</c>.</returns>
        public Boolean IsMacroDefined(String id)
        {
            this.CheckIfMacrosSupported();
            return this.Grammar.ContainsKey("term.ext", id);
        }

        /// <summary>
        /// Defines the reader macro rule with specified ID.
        /// </summary>
        /// <param name="id">The ID of new reader macro rule to define.</param>
        /// <param name="rule">The factory function of the parser, which has a parameter to the getter for the grammar.</param>
        public void DefineMacro(String id, Func<Grammar.RuleGetter, Parser<Char, YacqExpression>> rule)
        {
            this.CheckIfMacrosSupported();
            this.Grammar.Set["term.ext", id] = rule;
        }

        /// <summary>
        /// Undefines the reader macro rule with specified ID.
        /// </summary>
        /// <param name="id">The ID of reader macro rule to remove.</param>
        /// <returns><c>true</c> if the reader macro rule is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if key was not found in the grammar.</returns>
        public Boolean UndefineMacro(String id)
        {
            this.CheckIfMacrosSupported();
            return this.Grammar.Remove("term.ext", id);
        }

        private Parser<Char, IEnumerable<YacqExpression>> GetDefinitiveParser()
        {
            return this.Grammar.Get.Let(g => g.Default
                .Many()
                .Between(g["root", "ignore"], g["root", "ignore"])
                .Left(Errors.FollowedBy(Chars.Eof(), "Syntax Error: unexpected end of code"))
            );
        }

        private void CheckIfMacrosSupported()
        {
            if (!this.Grammar.ContainsKey("term", "ext"))
            {
                throw new NotSupportedException("The grammar of this reader does not support reader macros.");
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
