﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    public class IdentifierExpression
        : YacqExpression
    {
        public String Name
        {
            get;
            private set;
        }

        internal IdentifierExpression(String name)
        {
            this.Name = name;
        }

        public override String ToString()
        {
            return this.Name;
        }

        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            dynamic value;
            if (symbols.TryResolve(this.Name, out value) && value is Expression)
            {
                return value;
            }
            else
            {
                throw new InvalidOperationException("Unknown identifier: " + this.Name);
            }
        }

        public dynamic Resolve(SymbolTable symbols)
        {
            return symbols.Resolve(this.Name);
        }

        public Boolean TryResolve(SymbolTable symbols, out dynamic value)
        {
            return symbols.TryResolve(this.Name, out value);
        }
    }

    partial class YacqExpression
    {
        public static IdentifierExpression Identifier(String name)
        {
            return new IdentifierExpression(name);
        }
    }
}