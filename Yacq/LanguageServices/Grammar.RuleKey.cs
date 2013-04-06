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
using System.Linq;

namespace XSpect.Yacq.LanguageServices
{
    partial class Grammar
    {
        /// <summary>
        /// Represents the key of rules in <see cref="Grammar" />,
        /// </summary>
        public struct RuleKey
            : IComparable<RuleKey>,
              IEquatable<RuleKey>
        {
            private static readonly RuleKey _default = new RuleKey();

            /// <summary>
            /// Gets the rule key of the default rule.
            /// </summary>
            /// <value>The rule key of the default rule.</value>
            public static RuleKey Default
            {
                get
                {
                    return _default;
                }
            }

            /// <summary>
            /// Gets the category of this rule.
            /// </summary>
            /// <value>The category of this rule.</value>
            public String Category
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the priority of this rule.
            /// </summary>
            /// <value>The priority of this rule.</value>
            /// <remarks>Rules are ordered by the priority value ascendingly.</remarks>
            public Int32 Priority
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the ID of this rule.
            /// </summary>
            /// <value>The ID of this rule.</value>
            public String Id
            {
                get;
                private set;
            }

            /// <summary>
            /// Initializes a new instance of <see cref="RuleKey"/>.
            /// </summary>
            /// <param name="category">The category name of this rule.</param>
            /// <param name="priority">The priority of this rule.</param>
            /// <param name="id">The ID of this rule.</param>
            public RuleKey(String category, Int32 priority, String id)
                : this()
            {
                this.Category = category;
                this.Priority = priority;
                this.Id = id;
            }

            /// <summary>
            /// Indicates whether this instance and a specified object are equal.
            /// </summary>
            /// <param name="obj">Another object to compare to.</param>
            /// <returns>
            /// <c>true</c> if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, <c>false</c>.
            /// </returns>
            public override Boolean Equals(Object obj)
            {
                return obj is RuleKey && this.Equals((RuleKey) obj);
            }

            /// <summary>
            /// Returns the hash code for this instance.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer that is the hash code for this instance.
            /// </returns>
            public override Int32 GetHashCode()
            {
                return unchecked(
                    (this.Category != null ? this.Category.GetHashCode() : 0) ^
                    this.Priority.GetHashCode() ^
                    (this.Id != null ? this.Id.GetHashCode() : 0)
                );
            }

            /// <summary>
            /// Returns the fully qualified type name of this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="String"/> containing a fully qualified type name.
            /// </returns>
            public override String ToString()
            {
                return this.Equals(Default)
                    ? "DEFAULT"
                    : (this.Category ?? "(default)") + ":" + this.Priority + "[" + (this.Id ?? "(default)") + "]";
            }

            /// <summary>
            /// Compares this rule key with another rule key.
            /// </summary>
            /// <param name="other">A rule key to compare with this rule key.</param>
            /// <returns>
            /// A value that indicates the relative order of the rule keys being compared. The return value has the following meanings:
            /// Less than zero - This rule key is less than the <paramref name="other"/>.
            /// Zero - This rule key is equal to <paramref name="other"/>.
            /// Greater than zero - This rule key is greater than <paramref name="other"/>.
            /// </returns>
            public Int32 CompareTo(RuleKey other)
            {
                Int32 ret;
                return (ret = String.CompareOrdinal(this.Category, other.Category)) != 0
                    ? ret
                    : String.CompareOrdinal(this.Id, other.Id) == 0
                          ? 0
                          : this.Priority.CompareTo(other.Priority);
            }

            /// <summary>
            /// Indicates whether this rule and a specified rule are equal.
            /// </summary>
            /// <param name="other">Another rule to compare to.</param>
            /// <returns>
            /// <c>true</c> if <paramref name="other"/> and this rule are the same value; otherwise, <c>false</c>.
            /// </returns>
            public Boolean Equals(RuleKey other)
            {
                return this.Category == other.Category && (this.Priority == other.Priority || this.Id == other.Id);
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
