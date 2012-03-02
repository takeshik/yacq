// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents an expression which is a number.
    /// </summary>
    public class NumberExpression
        : YacqExpression
    {
        /// <summary>
        /// Gets the original string and source of constant number of this expression.
        /// </summary>
        /// <value>The original string and source of constant number of this expression.</value>
        public String SourceText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the constant number which this expression represents.
        /// </summary>
        /// <value>The constant number which this expression represents.</value>
        public Object Value
        {
            get;
            private set;
        }

        internal NumberExpression(
            SymbolTable symbols,
            String text
        )
            : base(symbols)
        {
            this.SourceText = text;
            this.Value = this.Parse();
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return this.SourceText;
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return Constant((expectedType != null
                ? ConvertNumericType(this.Value.GetType(), expectedType)
                      .Null(t => System.Convert.ChangeType(this.Value, Nullable.GetUnderlyingType(t) ?? t, CultureInfo.InvariantCulture))
                : null
            ) ?? this.Value);
        }

        private Object Parse()
        {
            var text = this.SourceText.Replace("_", "").ToUpper();
            var b = text[0] != '-'
                ? text.Length > 2
                      ? GetBase(text.Substring(0, 2))
                      : 10
                : text.Length > 3
                      ? GetBase(text.Substring(1, 3))
                      : 10;
            var suffix = text.Length > 1
                ? new String(text
                      .Substring(text.Length - 2)
                      .Where(b == 10
                          ? (_ => _ == 'D' || _ == 'F' || _ == 'M' || _ == 'L' || _ == 'U')
                          : ((Func<Char, Boolean>) (_ => _ == 'L' || _ == 'U'))
                      )
                      .ToArray()
                  )
                : "";
            text = text.Substring(0, text.Length - suffix.Length);
            if (b == 10 && suffix == "M")
            {
                return Decimal.Parse(text, NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            else if (b == 10 && (text.Contains(".") || suffix == "D" || suffix == "F"))
            {
                return suffix == "F"
                    ? (Object) Single.Parse(text, NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture)
                    : Double.Parse(text, NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture);
            }
            else
            {
                if (text[0] != '-')
                {
                    if (text[0] == '+')
                    {
                        text = text.Substring(1);
                    }
                    var value = b != 10
                        ? System.Convert.ToUInt64(text.Substring(2), b)
                        : UInt64.Parse(text, NumberStyles.AllowExponent | NumberStyles.Number, CultureInfo.InvariantCulture);
                    if (suffix == "UL" || suffix == "LU")
                    {
                        return value;
                    }
                    else if (suffix == "U")
                    {
                        return value <= UInt32.MaxValue
                            ? (UInt32) value
                            : (Object) value;
                    }
                    else if (suffix == "L")
                    {
                        return value <= Int64.MaxValue
                            ? (Int64) value
                            : (Object) value;
                    }
                    else
                    {
                        return value <= Int32.MaxValue
                            ? (Int32) value
                            : value <= UInt32.MaxValue
                                  ? (UInt32) value
                                  : (Object) value;
                    }
                }
                else
                {
                    var value = b != 10
                        ? System.Convert.ToInt64("-" + text.Substring(3), b)
                        : Int64.Parse(text, CultureInfo.InvariantCulture);
                    return suffix != "L" && value >= Int32.MinValue && value <= Int32.MaxValue
                        ? (Int32) value
                        : (Object) value;
                }
            }
        }

        private static Int32 GetBase(String b)
        {
            return b == "0B"
                ? 2
                : b == "0O"
                      ? 8
                      : b == "0X"
                            ? 16
                            : 10;
        }
    }

    partial class YacqExpression
    {
        /// <summary>
        /// Creates a <see cref="NumberExpression"/> that represents a number from specified source string.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="text">The source string of this expression.</param>
        /// <returns>An <see cref="NumberExpression"/> which generates a number from specified string.</returns>
        public static NumberExpression Number(SymbolTable symbols, String text)
        {
            return new NumberExpression(symbols, text);
        }

        /// <summary>
        /// Creates a <see cref="NumberExpression"/> that represents a number from specified source string.
        /// </summary>
        /// <param name="text">The source string of this expression.</param>
        /// <returns>An <see cref="NumberExpression"/> which generates a number from specified string.</returns>
        public static NumberExpression Number(String text)
        {
            return Number(null, text);
        }
    }
}
