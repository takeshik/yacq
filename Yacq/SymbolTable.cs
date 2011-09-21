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
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq
{
    public struct SymbolEntry
        : IEquatable<SymbolEntry>
    {
        public DispatchType DispatchType
        {
            get;
            private set;
        }

        public Type LeftType
        {
            get;
            private set;
        }

        public String Name
        {
            get;
            private set;
        }

        public SymbolEntry(DispatchType dispatchType, Type leftType, String name)
            : this()
        {
            this.DispatchType = dispatchType;
            this.LeftType = leftType;
            this.Name = name;
        }

        public override Boolean Equals(Object obj)
        {
            return obj is SymbolEntry && this.Equals((SymbolEntry) obj);
        }

        public override Int32 GetHashCode()
        {
            return unchecked(
                this.DispatchType.GetHashCode() ^
                (this.LeftType != null ? this.LeftType.GetHashCode() : 0) ^
                (this.Name != null ? this.Name.GetHashCode() : 0)
            );
        }

        public override String ToString()
        {
            return (this.DispatchType & DispatchType.TargetMask)
                + " " + (this.LeftType.Null(t => t.Name + ".") ?? "")
                + this.Name;
        }

        public Boolean Equals(SymbolEntry other)
        {
            return
                this.DispatchType == other.DispatchType &&
                this.LeftType == other.LeftType &&
                this.Name == other.Name;
        }
    }

    public delegate Expression SymbolDefinition(DispatchExpression expression, SymbolTable symbols);

    public partial class SymbolTable
        : IDictionary<SymbolEntry, SymbolDefinition>
    {
        private readonly IDictionary<SymbolEntry, SymbolDefinition> _symbols;

        private Nullable<Int32> _hash;

        public IEnumerator<KeyValuePair<SymbolEntry, SymbolDefinition>> GetEnumerator()
        {
            return this._symbols.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection<KeyValuePair<SymbolEntry, SymbolDefinition>>.Add(KeyValuePair<SymbolEntry, SymbolDefinition> item)
        {
            this.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            this.PrepareModify();
            this._symbols.Clear();
        }

        Boolean ICollection<KeyValuePair<SymbolEntry, SymbolDefinition>>.Contains(KeyValuePair<SymbolEntry, SymbolDefinition> item)
        {
            return this._symbols.Contains(item);
        }

        void ICollection<KeyValuePair<SymbolEntry, SymbolDefinition>>.CopyTo(KeyValuePair<SymbolEntry, SymbolDefinition>[] array, Int32 arrayIndex)
        {
            this._symbols.CopyTo(array, arrayIndex);
        }

        Boolean ICollection<KeyValuePair<SymbolEntry, SymbolDefinition>>.Remove(KeyValuePair<SymbolEntry, SymbolDefinition> item)
        {
            this.PrepareModify();
            return this._symbols.Remove(item);
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
                // Modifying Root SymbolTable is permitted only in static constructor.
                return this.Parent == null && Root != null;
            }
        }

        public Boolean ContainsKey(SymbolEntry key)
        {
            return this._symbols.ContainsKey(key);
        }

        public void Add(SymbolEntry key, SymbolDefinition value)
        {
            this.PrepareModify();
            this._symbols.Add(key, value);
        }

        public Boolean Remove(SymbolEntry key)
        {
            this.PrepareModify();
            return this._symbols.Remove(key);
        }

        public Boolean TryGetValue(SymbolEntry key, out SymbolDefinition value)
        {
            return this._symbols.TryGetValue(key, out value);
        }

        public SymbolDefinition this[SymbolEntry key]
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

        public ICollection<SymbolEntry> Keys
        {
            get
            {
                return this._symbols.Keys;
            }
        }

        public ICollection<SymbolDefinition> Values
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

        public SymbolDefinition Missing
        {
            get;
            set;
        }

        public IEnumerable<SymbolTable> Chain
        {
            get
            {
                return EnumerableEx.Generate(this, t => t != null, t => t.Parent, _ => _);
            }
        }

        public IEnumerable<SymbolEntry> AllKeys
        {
            get
            {
                return this.Chain.SelectMany(_ => _.Keys).Distinct();
            }
        }

        public IEnumerable<SymbolDefinition> AllValues
        {
            get
            {
                return this.AllKeys.Select(this.Resolve);
            }
        }

        public IDictionary<String, Expression> Literals
        {
            get
            {
                return this
                    .Where(p => p.Key.DispatchType.HasFlag(DispatchType.Literal))
                    .ToDictionary(p => p.Key.Name, p => p.Value(null, null));
            }
        }

        public IDictionary<String, Expression> AllLiterals
        {
            get
            {
                return this.AllKeys
                    .Where(k => k.DispatchType.HasFlag(DispatchType.Literal))
                    .ToDictionary(k => k.Name, k => this.Resolve(k)(null, null));
            }
        }

        public IDictionary<SymbolEntry, SymbolDefinition> Flatten
        {
            get
            {
                return this.AllKeys.ToDictionary(k => k, this.Resolve);
            }
        }

        public Int32 AllKeysHash
        {
            get
            {
                return this.Chain.Aggregate(0, (sa, s) =>
                    sa ^ s._hash ?? (Int32) (s._hash = s.Keys.Aggregate(0, (ka, k) =>
                        ka ^ k.GetHashCode()
                    ))
                );
            }
        }

        public SymbolDefinition this[DispatchType dispatchType, Type leftType, String name]
        {
            get
            {
                return this._symbols[new SymbolEntry(dispatchType, leftType, name)];
            }
            set
            {
                this._symbols[new SymbolEntry(dispatchType, leftType, name)] = value;
            }
        }

        public Expression this[String name]
        {
            get
            {
                return this[DispatchType.Member | DispatchType.Literal, null, name](null, null);
            }
        }

        public SymbolTable(SymbolTable parent = null, IDictionary<SymbolEntry, SymbolDefinition> entries = null)
        {
            this.Parent = parent ?? Root;
            this._symbols = entries != null
                ? entries
                      .Where(p => !(this.Parent.ExistsKey(p.Key) && this.Parent.Resolve(p.Key) == p.Value))
                      .ToDictionary(p => p.Key, p => p.Value)
                : new Dictionary<SymbolEntry, SymbolDefinition>();
            this._hash = null;
        }

        public override String ToString()
        {
            return String.Format(
                "Depth {0}: Count = {1} ({2})",
                this.Chain.Count() - 1,
                this.Count,
                this.AllKeys.Count()
            );
        }

        public void Add(DispatchType dispatchType, Type leftType, String name, SymbolDefinition definition)
        {
            this.Add(new SymbolEntry(dispatchType, leftType, name), definition);
        }

        public void Add(DispatchType dispatchType, String name, SymbolDefinition definition)
        {
            this.Add(dispatchType, null, name, definition);
        }

        public void Add(String name, Expression expression)
        {
            this.Add(DispatchType.Member | DispatchType.Literal, name, (e, s) => expression);
        }

        public Boolean ExistsKey(SymbolEntry key)
        {
            return this.Chain.Any(_ => _.ContainsKey(key));
        }

        public Boolean ExistsKey(DispatchType dispatchType, Type leftType, String name)
        {
            return this.ExistsKey(new SymbolEntry(dispatchType, leftType, name));
        }

        public SymbolDefinition Resolve(SymbolEntry key)
        {
            return this.Chain.First(_ => _.ContainsKey(key))[key];
        }

        public SymbolDefinition Resolve(DispatchType dispatchType, Type leftType, String name)
        {
            return this.Resolve(new SymbolEntry(dispatchType, leftType, name));
        }

        public Expression Resolve(String name)
        {
            return this.Resolve(DispatchType.Member | DispatchType.Literal, null, name)(null, null);
        }

        public Boolean TryResolve(SymbolEntry key, out SymbolDefinition value)
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

        public Boolean TryResolve(DispatchType dispatchType, Type leftType, String name, out SymbolDefinition value)
        {
            return this.TryResolve(new SymbolEntry(dispatchType, leftType, name), out value);
        }

        public Boolean TryResolve(String name, out Expression value)
        {
            SymbolDefinition literal;
            if (this.TryResolve(DispatchType.Member | DispatchType.Literal, null, name, out literal))
            {
                value = literal(null, null);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public SymbolDefinition Match(SymbolEntry key)
        {
            return this
                .Where(p =>
                    (p.Key.DispatchType == DispatchType.Unknown || p.Key.DispatchType.HasFlag(key.DispatchType)) &&
                    p.Key.Name == key.Name &&
                    (p.Key.LeftType == null || p.Key.LeftType == key.LeftType || p.Key.LeftType.IsAssignableFrom(key.LeftType) || (
                        p.Key.LeftType.TryGetGenericTypeDefinition() == typeof(Static<>) &&
                        key.LeftType.TryGetGenericTypeDefinition() == typeof(Static<>) &&
                        p.Key.LeftType.GetGenericArguments()[0].Let(t =>
                            key.LeftType.GetGenericArguments()[0].Let(kt =>
                                t == kt || t.IsAssignableFrom(kt)
                            )
                        )
                    ))
                )
                .OrderBy(p => p.Key.LeftType != null
                    ? key.LeftType.GetConvertibleTypes()
                          .TakeWhile(t => t == p.Key.LeftType)
                          .Count()
                    : Int32.MaxValue
                )
                .FirstOrDefault()
                .Value;
        }

        public SymbolDefinition Match(DispatchType dispatchType, Type leftType, String name)
        {
            return this.Match(new SymbolEntry(dispatchType, leftType, name));
        }

        public SymbolDefinition Match(DispatchExpression expression)
        {
            return this.Match(
                expression.DispatchType,
                expression.Left.Reduce(this)
                    .Null(e => e is TypeCandidateExpression
                        ? typeof(Static<>).MakeGenericType(((TypeCandidateExpression) e).ElectedType)
                        : e.Type
                    ),
                expression.Name
            );
        }

        public SymbolDefinition ResolveMatch(SymbolEntry key)
        {
            return this.Chain.Select(_ => _.Match(key)).FirstOrDefault(_ => _ != null);
        }

        public SymbolDefinition ResolveMatch(DispatchType dispatchType, Type leftType, String name)
        {
            return this.ResolveMatch(new SymbolEntry(dispatchType, leftType, name));
        }

        public SymbolDefinition ResolveMatch(DispatchExpression expression)
        {
            return this.ResolveMatch(
                expression.DispatchType,
                expression.Left.Reduce(this)
                    .Null(e => e is TypeCandidateExpression
                        ? typeof(Static<>).MakeGenericType(((TypeCandidateExpression) e).ElectedType)
                        : e.Type
                    ),
                expression.Name
            );
        }

        private void PrepareModify()
        {
            if(this.IsReadOnly)
            {
                throw new InvalidOperationException("This SymbolTable is read-only.");
            }
            this._hash = null;
        }
    }
}