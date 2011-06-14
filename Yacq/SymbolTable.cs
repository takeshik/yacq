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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XSpect.Yacq
{
    public partial class SymbolTable
        : IDictionary<String, Object>
    {
        private readonly IDictionary<String, Object> _symbols;

        public IEnumerator<KeyValuePair<String, Object>> GetEnumerator()
        {
            return this._symbols.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection<KeyValuePair<String, Object>>.Add(KeyValuePair<String, Object> item)
        {
            ((ICollection<KeyValuePair<String, Object>>) this._symbols).Contains(item);
        }

        public void Clear()
        {
            this._symbols.Clear();
        }

        Boolean ICollection<KeyValuePair<String, Object>>.Contains(KeyValuePair<String, Object> item)
        {
            return ((ICollection<KeyValuePair<String, Object>>) this._symbols).Contains(item);
        }

        public void CopyTo(KeyValuePair<String, Object>[] array, Int32 arrayIndex)
        {
            ((ICollection<KeyValuePair<String, Object>>) this._symbols).CopyTo(array, arrayIndex);
        }

        public Boolean Remove(KeyValuePair<String, Object> item)
        {
            return ((ICollection<KeyValuePair<String, Object>>) this._symbols).Remove(item);
        }

        public Int32 Count
        {
            get
            {
                return this._symbols.Count;
            }
        }

        public Boolean IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public Boolean ContainsKey(String key)
        {
            return this._symbols.ContainsKey(key);
        }

        public void Add(String key, Object value)
        {
            this._symbols.Add(key, value);
        }

        public Boolean Remove(String key)
        {
            return this._symbols.Remove(key);
        }

        public Boolean TryGetValue(String key, out Object value)
        {
            return this._symbols.TryGetValue(key, out value);
        }

        Object IDictionary<String, Object>.this[String key]
        {
            get
            {
                return this._symbols[key];
            }
            set
            {
                this._symbols[key] = value;
            }
        }

        dynamic this[String key]
        {
            get
            {
                return this._symbols[key];
            }
            set
            {
                this._symbols[key] = value;
            }
        }

        public ICollection<String> Keys
        {
            get
            {
                return this._symbols.Keys;
            }
        }

        public ICollection<Object> Values
        {
            get
            {
                return this._symbols.Values;
            }
        }

        public SymbolTable Parent
        {
            get;
            private set;
        }

        public IEnumerable<SymbolTable> Chain
        {
            get
            {
                return EnumerableEx.Generate(this, t => t != null, t => t.Parent, _ => _);
            }
        }

        public IEnumerable<String> AllKeys
        {
            get
            {
                return this.Chain.SelectMany(_ => _.Keys).Distinct();
            }
        }

        public IEnumerable<Object> AllValues
        {
            get
            {
                return this.AllKeys.Select(this.Resolve);
            }
        }

        public SymbolTable(SymbolTable parent = null, IDictionary<String, dynamic> entries = null)
        {
            this._symbols = entries ?? new Dictionary<String, Object>();
            this.Parent = parent ?? Root;
        }

        public Boolean ExistsKey(String key)
        {
            return this.Chain.Any(_ => _.ContainsKey(key));
        }

        public dynamic Resolve(String key)
        {
            return this.Chain.First(_ => _.ContainsKey(key))[key];
        }

        public Boolean TryResolve(String key, out dynamic value)
        {
            if (this.ExistsKey(key))
            {
                value = this.Resolve(key);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}