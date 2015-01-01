// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2015 Kimura Youichi <kim.upsilon@bucyou.net>
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
using System.Text;
using NUnit.Framework;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;

namespace XSpect.Yacq.Tests
{
    [TestFixture]
    public class AlternativeGrammerTest
    {
        private Reader readerAlternative = new Reader(Grammar.Alternative);

        [Test]
        public void DotInvokeTest()
        {
            var result = YacqServices.Read(readerAlternative, "Console.Beep()");

            var expected = YacqExpression.List(
                YacqExpression.Identifier("."),
                YacqExpression.Identifier("Console"),
                YacqExpression.List(
                    YacqExpression.Identifier("Beep")
                )
            );

            Assert.AreEqual(YacqExpression.Serialize(expected).SaveText(),
                YacqExpression.Serialize(result).SaveText());
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
