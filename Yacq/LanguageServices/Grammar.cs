// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using System.Threading;
using Parseq;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Represents a dictionary of language grammar rules.
    /// </summary>
    public partial class Grammar
        : IDictionary<Grammar.RuleKey, Lazy<Parser<Char, YacqExpression>>>
    {
        private static readonly StandardGrammar _standard = new StandardGrammar();

        private static readonly AlternativeGrammar _alternative = new AlternativeGrammar();

        private readonly IDictionary<RuleKey, Lazy<Parser<Char, YacqExpression>>> _rules;

        /// <summary>
        /// Gets or sets the reference to the parser with specified rule key.
        /// </summary>
        /// <param name="key">The rule key to get or set the parser.</param>
        /// <value>The reference to the parser with specified rule key.</value>
        public virtual Lazy<Parser<Char, YacqExpression>> this[RuleKey key]
        {
            get
            {
                return this._rules[key];
            }
            set
            {
                this._rules[key] = value;
            }
        }

        /// <summary>
        /// Gets or sets the reference to the parser with specified rule key.
        /// </summary>
        /// <param name="category">The category to get or set the parser.</param>
        /// <param name="priority">The priority to get or set the parser.</param>
        /// <param name="id">The ID to get or set the parser.</param>
        /// <value>The reference to the parser with specified rule key.</value>
        public Lazy<Parser<Char, YacqExpression>> this[String category, Int32 priority, String id]
        {
            get
            {
                return this[new RuleKey(category, priority, id)];
            }
            set
            {
                this[new RuleKey(category, priority, id)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the reference to the parser with specified category and priority.
        /// </summary>
        /// <param name="category">The category to get or set the parser.</param>
        /// <param name="priority">The priority to get or set the parser.</param>
        /// <value>The reference to the parser with specified category and priority.</value>
        public Lazy<Parser<Char, YacqExpression>> this[String category, Int32 priority]
        {
            get
            {
                return this[this.GetKey(category, priority)];
            }
            set
            {
                this[this.GetKey(category, priority)] = value;
            }
        }

        /// <summary>
        /// Gets or sets the reference to the parser with specified category and ID.
        /// </summary>
        /// <param name="category">The category to get or set the parser.</param>
        /// <param name="id">The ID to get or set the parser.</param>
        /// <value>The reference to the parser with specified category and ID.</value>
        public Lazy<Parser<Char, YacqExpression>> this[String category, String id]
        {
            get
            {
                return this[this.GetKey(category, id)];
            }
            set
            {
                this[this.GetKey(category, id)] = value;
            }
        }

        /// <summary>
        /// Gets the sequence of references to the parser with specified category.
        /// </summary>
        /// <param name="category">The category to get the parser.</param>
        /// <value>The sequence of references to the parser with specified category.</value>
        public IEnumerable<Lazy<Parser<Char, YacqExpression>>> this[String category]
        {
            get
            {
                return this.Keys
                    .Where(k => k.Category == category)
                    .Select(k => this[k]);
            }
        }

        /// <summary>
        /// Gets the standard grammar.
        /// </summary>
        /// <value>The standard grammar.</value>
        public static StandardGrammar Standard
        {
            get
            {
                return _standard;
            }
        }

        /// <summary>
        /// Gets the alternative grammar.
        /// </summary>
        /// <value>The alternative grammar.</value>
        public static AlternativeGrammar Alternative
        {
            get
            {
                return _alternative;
            }
        }

        /// <summary>
        /// Gets the number of rules in this grammar.
        /// </summary>
        /// <value>The number of rules in this grammar.</value>
        public Int32 Count
        {
            get
            {
                return this._rules.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this grammar is read-only.
        /// </summary>
        /// <value><c>true</c> if this rule is read-only; otherwise, <c>false</c>.</value>
        public virtual Boolean IsReadOnly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a collection containing the rule keys in this grammar.
        /// </summary>
        /// <value>A collection containing the rule keys in this grammar.</value>
        public ICollection<RuleKey> Keys
        {
            get
            {
                return this._rules.Keys
#if SILVERLIGHT
                    .OrderBy(k => k)
                    .ToArray()
#endif
                    ;
            }
        }

        /// <summary>
        /// Gets a collection containing the references to parser in this grammar.
        /// </summary>
        /// <value>A collection containing the references to parser in this grammar.</value>
        public ICollection<Lazy<Parser<Char, YacqExpression>>> Values
        {
            get
            {
                return this._rules
#if SILVERLIGHT
                    .Keys
                    .OrderBy(k => k)
                    .Select(k => this[k])
                    .ToArray()
#else
                    .Values
#endif
                    ;
            }
        }

        /// <summary>
        /// Gets or sets the reference to the parser of the default rule.
        /// </summary>
        /// <value>The reference to the parser of the default rule.</value>
        public Lazy<Parser<Char, YacqExpression>> DefaultRule
        {
            get
            {
                return this[RuleKey.Default];
            }
            set
            {
                this[RuleKey.Default] = value;
            }
        }

        /// <summary>
        /// Gets the getter for this grammar.
        /// </summary>
        /// <value>The getter for this grammar.</value>
        public virtual RuleGetter Get
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the setter for this grammar.
        /// </summary>
        /// <value>The setter for this grammar.</value>
        public virtual RuleSetter Set
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grammar"/> class.
        /// </summary>
        public Grammar()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Grammar"/> class that contains rules copied from the specified grammar.
        /// </summary>
        /// <param name="rules">The rules whose elements are copied to the new grammar.</param>
        public Grammar(IDictionary<RuleKey, Lazy<Parser<Char, YacqExpression>>> rules)
        {
            this._rules = 
#if SILVERLIGHT
                new Dictionary<RuleKey, Lazy<Parser<Char, YacqExpression>>>
#else
                new SortedDictionary<RuleKey, Lazy<Parser<Char, YacqExpression>>>
#endif
                    (rules ?? new Dictionary<RuleKey, Lazy<Parser<Char, YacqExpression>>>());
            this.Get = new RuleGetter(this);
            this.Set = new RuleSetter(this);
        }

        /// <summary>
        /// Returns an enumerator that iterates through rules in this grammar.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through rules in this grammar.
        /// </returns>
        public IEnumerator<KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>>> GetEnumerator()
        {
            return this._rules
#if SILVERLIGHT
                .OrderBy(p => p.Key)
#endif
                .GetEnumerator();
        }

        /// <summary>
        /// Removes all rules from this grammar.
        /// </summary>
        public virtual void Clear()
        {
            this._rules.Clear();
        }

        /// <summary>
        /// Adds the rule to this grammar.
        /// </summary>
        /// <param name="key">The rule key to add.</param>
        /// <param name="value">The reference to the parser which defines the rule.</param>
        public virtual void Add(RuleKey key, Lazy<Parser<Char, YacqExpression>> value)
        {
            this._rules.Add(key, value);
        }

        /// <summary>
        /// Determines whether the specified rule is contained in this grammar.
        /// </summary>
        /// <param name="key">The rule key to locate in this grammar.</param>
        /// <returns><c>true</c> if this grammar contains a rule with the key; otherwise, <c>false</c>.</returns>
        public virtual Boolean ContainsKey(RuleKey key)
        {
            return this._rules.ContainsKey(key);
        }

        /// <summary>
        /// Removes the rule with the specified rule key from this grammar.
        /// </summary>
        /// <param name="key">The rule key to remove.</param>
        /// <returns>
        /// <c>true</c> if the rule is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if key was not found in the grammar.
        /// </returns>
        public virtual Boolean Remove(RuleKey key)
        {
            return this._rules.Remove(key);
        }

        /// <summary>
        /// Gets the reference to the parser associated with the specified rule key.
        /// </summary>
        /// <param name="key">The rule key to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified rule key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified rule key is contained in this grammar; otherwise, <c>false</c>.</returns>
        public virtual Boolean TryGetValue(RuleKey key, out Lazy<Parser<Char, YacqExpression>> value)
        {
            return this._rules.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection<KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>>>.Add(KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>> item)
        {
            this._rules.Add(item);
        }

        Boolean ICollection<KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>>>.Contains(KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>> item)
        {
            return this._rules.Contains(item);
        }

        void ICollection<KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>>>.CopyTo(KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>>[] array, Int32 arrayIndex)
        {
            this._rules.CopyTo(array, arrayIndex);
        }

        Boolean ICollection<KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>>>.Remove(KeyValuePair<RuleKey, Lazy<Parser<Char, YacqExpression>>> item)
        {
            return this._rules.Remove(item);
        }

        /// <summary>
        /// Adds the rule to this grammar.
        /// </summary>
        /// <param name="key">The rule key to add.</param>
        /// <param name="rule">The factory function of the parser, which has a parameter to the getter for this grammar.</param>
        public void Add(RuleKey key, Func<RuleGetter, Parser<Char, YacqExpression>> rule)
        {
            this.Add(key, this.MakeValue(rule));
        }

        /// <summary>
        /// Adds the rule to this grammar.
        /// </summary>
        /// <param name="category">The category of new rule to add.</param>
        /// <param name="priority">The priority of new rule to add.</param>
        /// <param name="id">The ID of new rule to add.</param>
        /// <param name="rule">The factory function of the parser, which has a parameter to the getter for this grammar.</param>
        public void Add(String category, Int32 priority, String id, Func<RuleGetter, Parser<Char, YacqExpression>> rule)
        {
            this.Add(new RuleKey(category, priority, id), rule);
        }

        /// <summary>
        /// Adds the rule to this grammar. The priority of new rule is computed automatically.
        /// </summary>
        /// <param name="category">The category of new rule to add.</param>
        /// <param name="id">The ID of new rule to add.</param>
        /// <param name="rule">The factory function of the parser, which has a parameter to the getter for this grammar.</param>
        public void Add(String category, String id, Func<RuleGetter, Parser<Char, YacqExpression>> rule)
        {
            this.Add(category, (this.Keys.Where(k => k.Category == category).Max(k => (Nullable<Int32>) k.Priority) ?? 0) + 100, id, rule);
        }

        /// <summary>
        /// Determines whether the specified rule is contained in this grammar.
        /// </summary>
        /// <param name="category">The category of the rule to locate.</param>
        /// <param name="priority">The priority of the rule to locate.</param>
        /// <param name="id">The ID of the rule to locate.</param>
        /// <returns><c>true</c> if this grammar contains a rule with the key; otherwise, <c>false</c>.</returns>
        public Boolean ContainsKey(String category, Int32 priority, String id)
        {
            return this.ContainsKey(new RuleKey(category, priority, id));
        }

        /// <summary>
        /// Determines whether the specified rule is contained in this grammar.
        /// </summary>
        /// <param name="category">The category of the rule to locate.</param>
        /// <param name="priority">The priority of the rule to locate.</param>
        /// <returns><c>true</c> if this grammar contains a rule with the key; otherwise, <c>false</c>.</returns>
        public Boolean ContainsKey(String category, Int32 priority)
        {
            return this.ContainsKey(this.GetKey(category, priority));
        }

        /// <summary>
        /// Determines whether the specified rule is contained in this grammar.
        /// </summary>
        /// <param name="category">The category of the rule to locate.</param>
        /// <param name="id">The ID of the rule to locate.</param>
        /// <returns><c>true</c> if this grammar contains a rule with the key; otherwise, <c>false</c>.</returns>
        public Boolean ContainsKey(String category, String id)
        {
            return this.ContainsKey(this.GetKey(category, id));
        }

        /// <summary>
        /// Removes the rule with the specified rule key from this grammar.
        /// </summary>
        /// <param name="category">The category of the rule to remove.</param>
        /// <param name="priority">The priority of the rule to remove.</param>
        /// <param name="id">The ID of the rule to remove.</param>
        /// <returns><c>true</c> if the rule is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if key was not found in the grammar.</returns>
        public Boolean Remove(String category, Int32 priority, String id)
        {
            return this.ContainsKey(new RuleKey(category, priority, id));
        }

        /// <summary>
        /// Removes the rule with the specified rule key from this grammar.
        /// </summary>
        /// <param name="category">The category of the rule to remove.</param>
        /// <param name="priority">The priority of the rule to remove.</param>
        /// <returns><c>true</c> if the rule is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if key was not found in the grammar.</returns>
        public Boolean Remove(String category, Int32 priority)
        {
            return this.ContainsKey(this.GetKey(category, priority));
        }

        /// <summary>
        /// Removes the rule with the specified rule key from this grammar.
        /// </summary>
        /// <param name="category">The category of the rule to remove.</param>
        /// <param name="id">The ID of the rule to remove.</param>
        /// <returns><c>true</c> if the rule is successfully removed; otherwise, <c>false</c>. This method also returns <c>false</c> if key was not found in the grammar.</returns>
        public Boolean Remove(String category, String id)
        {
            return this.ContainsKey(this.GetKey(category, id));
        }

        /// <summary>
        /// Gets the reference to the parser associated with the specified rule key.
        /// </summary>
        /// <param name="category">The category of the rule to get.</param>
        /// <param name="priority">The priority of the rule to get.</param>
        /// <param name="id">The ID of the rule to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified rule key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified rule key is contained in this grammar; otherwise, <c>false</c>.</returns>
        public Boolean TryGetValue(String category, Int32 priority, String id, out Lazy<Parser<Char, YacqExpression>> value)
        {
            return this.TryGetValue(new RuleKey(category, priority, id), out value);
        }

        /// <summary>
        /// Gets the reference to the parser associated with the specified rule key.
        /// </summary>
        /// <param name="category">The category of the rule to get.</param>
        /// <param name="priority">The priority of the rule to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified rule key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified rule key is contained in this grammar; otherwise, <c>false</c>.</returns>
        public Boolean TryGetValue(String category, Int32 priority, out Lazy<Parser<Char, YacqExpression>> value)
        {
            return this.TryGetValue(this.GetKey(category, priority), out value);
        }

        /// <summary>
        /// Gets the reference to the parser associated with the specified rule key.
        /// </summary>
        /// <param name="category">The category of the rule to get.</param>
        /// <param name="id">The ID of the rule to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified rule key, if the key is found;
        /// otherwise, <c>null</c>. This parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the specified rule key is contained in this grammar; otherwise, <c>false</c>.</returns>
        public Boolean TryGetValue(String category, String id, out Lazy<Parser<Char, YacqExpression>> value)
        {
            return this.TryGetValue(this.GetKey(category, id), out value);
        }

        /// <summary>
        /// Gets the rule key with specified parameters.
        /// </summary>
        /// <param name="category">The category of the rule to get.</param>
        /// <param name="priority">The priority of the rule to get.</param>
        /// <returns>The rule key with specified parameters, if the key is found; otherwise, <c>null</c>.</returns>
        public RuleKey GetKey(String category, Int32 priority)
        {
            var key = this.Keys.SingleOrDefault(k => k.Category == category && k.Priority == priority);
            if (key.Equals(RuleKey.Default))
            {
                throw new KeyNotFoundException("Specified key was not found: category = " + category + ", priority = " + priority);
            }
            return key;
        }

        /// <summary>
        /// Gets the rule key with specified parameters.
        /// </summary>
        /// <param name="category">The category of the rule to get.</param>
        /// <param name="id">The ID of the rule to get.</param>
        /// <returns>The rule key with specified parameters, if the key is found; otherwise, <c>null</c>.</returns>
        public RuleKey GetKey(String category, String id)
        {
            var key = this.Keys.SingleOrDefault(k => k.Category == category && k.Id == id);
            if (key.Equals(RuleKey.Default))
            {
                throw new KeyNotFoundException("Specified key was not found: category = " + category + ", id = " + id);
            }
            return key;
        }

        /// <summary>
        /// Creates a modifiable clone of this grammar.
        /// </summary>
        /// <returns>A modifiable clone of this grammar.</returns>
        public Grammar Clone()
        {
            return new Grammar(this._rules);
        }

        private Lazy<Parser<Char, YacqExpression>> MakeValue(Func<RuleGetter, Parser<Char, YacqExpression>> rule)
        {
            return new Lazy<Parser<Char, YacqExpression>>(() => rule(this.Get), LazyThreadSafetyMode.PublicationOnly);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
