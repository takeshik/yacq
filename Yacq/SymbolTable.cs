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
    /// <summary>
    /// Represents a dictionary of symbols, the mechanism to hook all member references and method calls.
    /// </summary>
    public partial class SymbolTable
        : IDictionary<SymbolEntry, SymbolDefinition>
    {
        private readonly IDictionary<SymbolEntry, SymbolDefinition> _symbols;

        private Nullable<Int32> _hash;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<SymbolEntry, SymbolDefinition>> GetEnumerator()
        {
            return this._symbols.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection<KeyValuePair<SymbolEntry, SymbolDefinition>>.Add(KeyValuePair<SymbolEntry, SymbolDefinition> item)
        {
            this.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all symbols from this symbol table..
        /// </summary>
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

        /// <summary>
        /// Gets the number of elements contained in the symbol table.
        /// </summary>
        /// <value>The number of elements contained in the symbol table</value>
        public Int32 Count
        {
            get
            {
                return this._symbols.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this symbol table is read-only.
        /// </summary>
        /// <value><c>true</c> if this symbol table is read-only; otherwise, <c>false</c>.</value>
        public Boolean IsReadOnly
        {
            get
            {
                // Modifying Root SymbolTable is permitted only in static constructor.
                return this.Parent == null && Root != null;
            }
        }

        /// <summary>
        /// Determines whether this symbol table contains an element with the specified symbol key.
        /// </summary>
        /// <param name="key">The symbol key to locate in this symbol table.</param>
        /// <returns>
        /// <c>true</c> if this symbol table contains an element with the symbol key; otherwise, <c>false</c>.
        /// </returns>
        public Boolean ContainsKey(SymbolEntry key)
        {
            return this._symbols.ContainsKey(key);
        }

        /// <summary>
        /// Adds a symbol with the provided symbol key and value to this symbol table.
        /// </summary>
        /// <param name="key">The symbol key to add.</param>
        /// <param name="value">The symbol value to add.</param>
        public void Add(SymbolEntry key, SymbolDefinition value)
        {
            this.PrepareModify();
            this._symbols.Add(key, value);
        }

        /// <summary>
        /// Removes the symbol with the specified symbol key from this symbol table.
        /// </summary>
        /// <param name="key">The symbol key to remove.</param>
        /// <returns>
        /// <c>true</c> if the symbol is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if key was not found in the symbol table.
        /// </returns>
        public Boolean Remove(SymbolEntry key)
        {
            this.PrepareModify();
            return this._symbols.Remove(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The symbol key to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified symbol key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified symbol key is contained in this symbol table; otherwise, <c>false</c>.</returns>
        public Boolean TryGetValue(SymbolEntry key, out SymbolDefinition value)
        {
            return this._symbols.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets or sets the symbol value with the specified symbol key.
        /// </summary>
        /// <returns>The symbol value with the specified symbol key.</returns>
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

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the symbol keys of this symbol table.
        /// </summary>
        /// <value>An <see cref="ICollection{T}"/> containing the symbol keys of this symbol table.</value>
        public ICollection<SymbolEntry> Keys
        {
            get
            {
                return this._symbols.Keys;
            }
        }

        /// <summary>
        /// Gets an <see cref="ICollection{T}"/> containing the symbol values of this symbol table.
        /// </summary>
        /// <value>An <see cref="ICollection{T}"/> containing the symbol values of this symbol table.</value>
        public ICollection<SymbolDefinition> Values
        {
            get
            {
                return this._symbols.Values;
            }
        }

        /// <summary>
        /// Gets the parent <see cref="SymbolTable"/> of this symbol table.
        /// </summary>
        /// <value>
        /// The parent <see cref="SymbolTable"/> of this symbol table.
        /// </value>
        public SymbolTable Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the sequence of <see cref="Parent"/> symbols, from this instance to the <see cref="Root"/>.
        /// </summary>
        /// <value>
        /// The sequence of <see cref="Parent"/> symbols, from this instance to the <see cref="Root"/>.
        /// </value>
        public IEnumerable<SymbolTable> Chain
        {
            get
            {
                return EnumerableEx.Generate(this, t => t != null, t => t.Parent, _ => _);
            }
        }

        /// <summary>
        /// Gets all symbol keys in this instance's <see cref="Chain"/>,
        /// </summary>
        /// <value>
        /// All symbol keys in this instance's <see cref="Chain"/>,
        /// </value>
        public IEnumerable<SymbolEntry> AllKeys
        {
            get
            {
                return this.Chain.SelectMany(_ => _.Keys).Distinct();
            }
        }

        /// <summary>
        /// Gets all symbol values in this instance's <see cref="Chain"/>,
        /// </summary>
        /// <value>
        /// All symbol values in this instance's <see cref="Chain"/>,
        /// </value>
        public IEnumerable<SymbolDefinition> AllValues
        {
            get
            {
                return this.AllKeys.Select(this.Resolve);
            }
        }

        /// <summary>
        /// Gets literal values of symbols which is marked as <see cref="DispatchTypes.Literal"/>.
        /// </summary>
        /// <value>
        /// Literal values of symbols which is marked as <see cref="DispatchTypes.Literal"/>.
        /// </value>
        public IDictionary<String, Expression> Literals
        {
            get
            {
                return this
                    .Where(p => p.Key.DispatchType.HasFlag(DispatchTypes.Literal))
                    .ToDictionary(p => p.Key.Name, p => p.Value(null, null));
            }
        }

        /// <summary>
        /// Gets all literal values in this instance's <see cref="Chain"/>,
        /// </summary>
        /// <value>
        /// All literal values in this instance's <see cref="Chain"/>,
        /// </value>
        public IDictionary<String, Expression> AllLiterals
        {
            get
            {
                return this.AllKeys
                    .Where(k => k.DispatchType.HasFlag(DispatchTypes.Literal))
                    .ToDictionary(k => k.Name, k => this.Resolve(k)(null, null));
            }
        }

        /// <summary>
        /// Gets the dictionary which contains all symbols in this instance's <see cref="Chain"/>.
        /// </summary>
        /// <value>
        /// The dictionary which contains all symbols in this instance's <see cref="Chain"/>.
        /// </value>
        public IDictionary<SymbolEntry, SymbolDefinition> Flatten
        {
            get
            {
                return this.AllKeys.ToDictionary(k => k, this.Resolve);
            }
        }

        /// <summary>
        /// Gets the hash value of <see cref="AllKeys"/>.
        /// </summary>
        /// <value>
        /// The hash value of <see cref="AllKeys"/>.
        /// </value>>
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

        /// <summary>
        /// Gets or sets the symbol with the specified symbol key properties.
        /// </summary>
        /// <param name="dispatchType">The key's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The key's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The key's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns>The symbol value with the specified symbol key properties.</returns>
        public SymbolDefinition this[DispatchTypes dispatchType, Type leftType, String name]
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

        /// <summary>
        /// Gets or sets the literal symbol with the name.
        /// </summary>
        /// <param name="name">The literal's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns>The literal value of the specified name.</returns>
        public Expression this[String name]
        {
            get
            {
                return this[DispatchTypes.Member | DispatchTypes.Literal, null, name](null, null);
            }
            set
            {
                this[DispatchTypes.Member | DispatchTypes.Literal, null, name] = (e, s) => value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SymbolTable"/> class.
        /// </summary>
        /// <param name="parent">Parent <see cref="SymbolTable"/> of this symbol table.</param>
        /// <param name="entries">Initial entries of this symbol table.</param>
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

        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return String.Format(
                "Depth {0}: Count = {1} ({2})",
                this.Chain.Count() - 1,
                this.Count,
                this.AllKeys.Count()
            );
        }

        /// <summary>
        /// Adds the symbol to this symbol table.
        /// </summary>
        /// <param name="dispatchType">The symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The symbol's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="definition">The <see cref="SymbolDefinition"/> of the symbol.</param>
        public void Add(DispatchTypes dispatchType, Type leftType, String name, SymbolDefinition definition)
        {
            this.Add(new SymbolEntry(dispatchType, leftType, name), definition);
        }

        /// <summary>
        /// Adds the symbol to this symbol table.
        /// </summary>
        /// <param name="dispatchType">The symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="name">The symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="definition">The <see cref="SymbolDefinition"/> of the symbol.</param>
        public void Add(DispatchTypes dispatchType, String name, SymbolDefinition definition)
        {
            this.Add(dispatchType, null, name, definition);
        }

        /// <summary>
        /// Add the literal symbol to this symbol table.
        /// </summary>
        /// <param name="name">The literal symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="expression">The symbol's literal value.</param>
        public void Add(String name, Expression expression)
        {
            this.Add(DispatchTypes.Member | DispatchTypes.Literal, name, (e, s) => expression);
        }

        /// <summary>
        /// Adds the alias symbol to this symbol table.
        /// </summary>
        /// <param name="dispatchType">The symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The symbol's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="targetDispatchType">The target symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="targetLeftType">The target symbol's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="targetName">The target symbol's <see cref="SymbolEntry.Name"/>.</param>
        public void Add(DispatchTypes dispatchType, Type leftType, String name, DispatchTypes targetDispatchType, Type targetLeftType, String targetName)
        {
            this.Add(dispatchType, leftType, name, this[targetDispatchType, targetLeftType, targetName]);
        }

        /// <summary>
        /// Adds the alias symbol to this symbol table.
        /// </summary>
        /// <param name="dispatchType">The symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="name">The symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="targetDispatchType">The target symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="targetLeftType">The target symbol's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="targetName">The target symbol's <see cref="SymbolEntry.Name"/>.</param>
        public void Add(DispatchTypes dispatchType, String name, DispatchTypes targetDispatchType, Type targetLeftType, String targetName)
        {
            this.Add(dispatchType, name, this[targetDispatchType, targetLeftType, targetName]);
        }

        /// <summary>
        /// Adds the alias symbol to this symbol table.
        /// </summary>
        /// <param name="dispatchType">The symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The symbol's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="targetDispatchType">The target symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="targetName">The target symbol's <see cref="SymbolEntry.Name"/>.</param>
        public void Add(DispatchTypes dispatchType, Type leftType, String name, DispatchTypes targetDispatchType, String targetName)
        {
            this.Add(dispatchType, leftType, name, this[targetDispatchType, null, targetName]);
        }

        /// <summary>
        /// Adds the alias symbol to this symbol table.
        /// </summary>
        /// <param name="dispatchType">The symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="name">The symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="targetDispatchType">The target symbol's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="targetName">The target symbol's <see cref="SymbolEntry.Name"/>.</param>
        public void Add(DispatchTypes dispatchType, String name, DispatchTypes targetDispatchType, String targetName)
        {
            this.Add(dispatchType, name, this[targetDispatchType, null, targetName]);
        }

        /// <summary>
        /// Adds the alias literal symbol to this symbol table.
        /// </summary>
        /// <param name="name">The literal symbol's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="targetName">The target literal symbol's <see cref="SymbolEntry.Name"/>.</param>
        public void Add(String name, String targetName)
        {
            this.Add(name, this[targetName]);
        }

        /// <summary>
        /// Determines whether the specified symbol key is contained in this symbol table's <see cref="Chain"/>.
        /// </summary>
        /// <param name="key">The symbol key to locate in this symbol table.</param>
        /// <returns><c>true</c> if the specified symbol key is contained in this symbol table's <see cref="Chain"/>; otherwise, <c>false</c>.</returns>
        public Boolean ExistsKey(SymbolEntry key)
        {
            return this.Chain.Any(_ => _.ContainsKey(key));
        }

        /// <summary>
        /// Determines whether the specified symbol key is contained in this symbol table's <see cref="Chain"/>.
        /// </summary>
        /// <param name="dispatchType">The key's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The key's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The key's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns><c>true</c> the specified symbol key is contained in this symbol table's <see cref="Chain"/>; otherwise, <c>false</c>.</returns>
        public Boolean ExistsKey(DispatchTypes dispatchType, Type leftType, String name)
        {
            return this.ExistsKey(new SymbolEntry(dispatchType, leftType, name));
        }

        /// <summary>
        /// Gets the symbol from this symbol table's <see cref="Chain"/> with the specified symbol key.
        /// </summary>
        /// <param name="key">The symbol key to get.</param>
        /// <returns>The symbol value with the specified symbol key.</returns>
        public SymbolDefinition Resolve(SymbolEntry key)
        {
            return this.Chain.First(_ => _.ContainsKey(key))[key];
        }

        /// <summary>
        /// Gets the symbol from this symbol table's <see cref="Chain"/> with the specified symbol key properties.
        /// </summary>
        /// <param name="dispatchType">The key's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The key's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The key's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns>The symbol value with the specified symbol key properties.</returns>
        public SymbolDefinition Resolve(DispatchTypes dispatchType, Type leftType, String name)
        {
            return this.Resolve(new SymbolEntry(dispatchType, leftType, name));
        }

        /// <summary>
        /// Gets the literal symbol from this symbol table's <see cref="Chain"/> with the name.
        /// </summary>
        /// <param name="name">The literal's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns>The literal value of the specified name.</returns>
        public Expression Resolve(String name)
        {
            return this.Resolve(DispatchTypes.Member | DispatchTypes.Literal, null, name)(null, null);
        }

        /// <summary>
        /// Gets the symbol from this symbol table's <see cref="Chain"/> with the specified symbol key.
        /// </summary>
        /// <param name="key">The symbol key to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified symbol key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified symbol key is contained in this symbol table's <see cref="Chain"/>; otherwise, <c>false</c>.</returns>
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

        /// <summary>
        /// Gets the symbol from this symbol table's <see cref="Chain"/> with the specified symbol key properties.
        /// </summary>
        /// <param name="dispatchType">The key's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The key's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The key's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified symbol key properties, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified symbol key is contained in this symbol table's <see cref="Chain"/>; otherwise, <c>false</c>.</returns>
        public Boolean TryResolve(DispatchTypes dispatchType, Type leftType, String name, out SymbolDefinition value)
        {
            return this.TryResolve(new SymbolEntry(dispatchType, leftType, name), out value);
        }

        /// <summary>
        /// Gets the literal symbol from this symbol table's <see cref="Chain"/> with the name.
        /// </summary>
        /// <param name="name">The symbol literal's <see cref="SymbolEntry.Name"/>.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified literal symbol key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified literal symbol key is contained in this symbol table's <see cref="Chain"/>; otherwise, <c>false</c>.</returns>
        public Boolean TryResolve(String name, out Expression value)
        {
            SymbolDefinition literal;
            if (this.TryResolve(DispatchTypes.Member | DispatchTypes.Literal, null, name, out literal))
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

        /// <summary>
        /// Gets the most appropriate symbol with the specified symbol key.
        /// </summary>
        /// <param name="key">The symbol key to get.</param>
        /// <returns>The symbol value with the most appropriate to the specified symbol key.</returns>
        public SymbolDefinition Match(SymbolEntry key)
        {
            return this
                .Where(p =>
                    (p.Key.DispatchType == DispatchTypes.Unknown || p.Key.DispatchType.HasFlag(key.DispatchType)) &&
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

        /// <summary>
        /// Gets the most appropriate symbol with the specified symbol key properties.
        /// </summary>
        /// <param name="dispatchType">The key's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The key's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The key's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns>The symbol value with the most appropriate to the specified symbol key properties.</returns>
        public SymbolDefinition Match(DispatchTypes dispatchType, Type leftType, String name)
        {
            return this.Match(new SymbolEntry(dispatchType, leftType, name));
        }

        /// <summary>
        /// Gets the most appropriate symbol with the specified <see cref="DispatchExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="DispatchExpression"/> to use as symbol key properties.</param>
        /// <returns>The symbol value with the most appropriate to the specified <see cref="DispatchExpression"/>.</returns>
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

        /// <summary>
        /// Gets the most appropriate symbol from this symbol table's <see cref="Chain"/> with the specified symbol key.
        /// </summary>
        /// <param name="key">The symbol key to get.</param>
        /// <returns>The symbol value with the most appropriate to the specified symbol key.</returns>
        public SymbolDefinition ResolveMatch(SymbolEntry key)
        {
            return this.Chain.Select(_ => _.Match(key)).FirstOrDefault(_ => _ != null);
        }

        /// <summary>
        /// Gets the most appropriate symbol from this symbol table's <see cref="Chain"/> with the specified symbol key properties.
        /// </summary>
        /// <param name="dispatchType">The key's <see cref="SymbolEntry.DispatchType"/>.</param>
        /// <param name="leftType">The key's <see cref="SymbolEntry.LeftType"/>.</param>
        /// <param name="name">The key's <see cref="SymbolEntry.Name"/>.</param>
        /// <returns>The symbol value with the most appropriate to the specified symbol key properties.</returns>
        public SymbolDefinition ResolveMatch(DispatchTypes dispatchType, Type leftType, String name)
        {
            return this.ResolveMatch(new SymbolEntry(dispatchType, leftType, name));
        }

        /// <summary>
        /// Gets the most appropriate symbol from this symbol table's <see cref="Chain"/> with the specified <see cref="DispatchExpression"/>.
        /// </summary>
        /// <param name="expression">The <see cref="DispatchExpression"/> to use as symbol key properties.</param>
        /// <returns>The symbol value with the most appropriate to the specified <see cref="DispatchExpression"/>.</returns>
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