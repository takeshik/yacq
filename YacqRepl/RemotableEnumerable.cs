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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;
using Parseq;

namespace XSpect.Yacq.Repl
{
    internal static class RemotingEnumerable
    {
        internal class RemotableEnumerable<T>
            : MarshalByRefObject,
              IEnumerable<T>
        {
            private readonly IEnumerable<T> _enumerable;

            public RemotableEnumerable(IEnumerable<T> enumerable)
            {
                this._enumerable = enumerable;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new RemotableEnumerator<T>(this._enumerable.GetEnumerator());
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        internal class RemotableEnumerator<T>
            : MarshalByRefObject,
              IEnumerator<T>
        {
            private readonly IEnumerator<T> _enumerator;

            public RemotableEnumerator(IEnumerator<T> enumerator)
            {
                this._enumerator = enumerator;
            }

            public void Dispose()
            {
                this._enumerator.Dispose();
            }

            public Boolean MoveNext()
            {
                return this._enumerator.MoveNext();
            }

            public void Reset()
            {
                this._enumerator.Reset();
            }

            public T Current
            {
                get
                {
                    return this._enumerator.Current;
                }
            }

            Object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }
        }

        internal static IEnumerable<TSource> Remotable<TSource>(this IEnumerable<TSource> source)
        {
            return new RemotableEnumerable<TSource>(source);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
