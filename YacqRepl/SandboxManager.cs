// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ REPL
 *   REPL and remote code evaluating system provider of YACQ
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using System.Net;
using System.Security;
using System.Security.Policy;
using XSpect.Yacq.Expressions;
using BitArray = XSpect.Collections.BitArray;

namespace XSpect.Yacq.Repl
{
    public class SandboxManager
        : MarshalByRefObject,
          IEnumerable<ISandbox>
    {
        private readonly Dictionary<Guid, ISandbox> _sandboxes;

        private readonly Dictionary<String, IReplInterface> _replInterfaces;

        public SandboxManager()
        {
            this._sandboxes = new Dictionary<Guid, ISandbox>();
            this._replInterfaces = new Dictionary<String, IReplInterface>();
        }

        public IEnumerator<ISandbox> GetEnumerator()
        {
            return this._sandboxes.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerable<ISandbox> this[String prefix]
        {
            get
            {
                return this._sandboxes.Keys
                    .Where(k => k.ToString("d").StartsWith(prefix))
                    .Select(k => this._sandboxes[k]);
            }
        }

        public IEnumerable<ISandbox> this[IPAddress address, Int32 prefix]
        {
            get
            {
                return Enumerable.Repeat(true, prefix)
                    .Concat(Enumerable.Repeat(false, address.GetAddressBytes().Length - prefix))
                    .ToArray()
                    .Apply(mask => EnumerableEx.Repeat(8)
                        .Scan(0, (a, i) => a + i)
                        .TakeWhile(i => i < mask.Length)
                        .ForEach((i, e) => Array.Reverse(mask, i, 8))
                    )
                    .Let(mask => (new BitArray(address.GetAddressBytes()) & mask)
                        .Let(net => this._sandboxes.Values
                            .Where(s => s.RemoteAddress
                                .Let(a => a.AddressFamily == address.AddressFamily
                                    && (new BitArray(a.GetAddressBytes()) & mask).Equals(net)
                                )
                            )
                        )
                    );
            }
        }

        public ISandbox CreateSandbox(Evidence evidence = null, IPAddress remoteAddress = null, CultureInfo culture = null)
        {
            return Guid.NewGuid()
                .Apply(k => this._sandboxes.Add(k, Sandbox.Create(
                    k,
                    evidence ?? new Evidence().Apply(e => e.AddHostEvidence(new Zone(SecurityZone.Internet))),
                    remoteAddress ?? IPAddress.None,
                    culture ?? CultureInfo.InvariantCulture
                )))
                .Let(k => this._sandboxes[k]);
        }

        public void Unload(ISandbox sandbox)
        {
            sandbox.Id.Apply(
                i => AppDomain.Unload(sandbox.Domain),
                i => this._sandboxes.Remove(i)
            );
        }

        public void Unload(String prefix)
        {
            this.Unload(this[prefix].Single());
        }

        public void AddReplInterface(String key, IReplInterface replInterface)
        {
            this._replInterfaces.Add(key, replInterface);
        }

        public Boolean AddReplInterface(String key)
        {
            return this._replInterfaces.Remove(key);
        }

        public void Run()
        {
            this.AddReplInterface("console", new ConsoleReplInterface());
            new FileInfo("replrc.yacq").If(f => f.Exists, f =>
                YacqServices.Parse(File.ReadLines(f.FullName).SelectMany(l => l))
                    .Evaluate()
                );
            this._replInterfaces.Values.ForEach(i => i.Initialize(this));
            this._replInterfaces.Values.ForEach(i => i.Run());
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
