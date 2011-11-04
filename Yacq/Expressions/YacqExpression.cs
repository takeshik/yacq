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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides the base class from which the classes that represent YACQ expression tree nodes are derived.
    /// It also contains static factory methods to create the various node types. This is an abstract class.
    /// </summary>
    public abstract partial class YacqExpression
        : Expression
    {
        private Boolean _canReduce;

        private readonly Dictionary<Int32, Expression> _reducedExpressions;

        /// <summary>
        /// Gets the node type of this expression.
        /// </summary>
        /// <returns>One of the <see cref="ExpressionType"/> values.</returns>
        public override ExpressionType NodeType
        {
            get
            {
                return ExpressionType.Extension;
            }
        }

        /// <summary>
        /// Indicates that the node can be reduced to a simpler node. If this returns true, Reduce() can be called to produce the reduced form.
        /// </summary>
        /// <returns><c>true</c> if the node can be reduced, otherwise <c>false</c>.</returns>
        public override Boolean CanReduce
        {
            get
            {
                return this._canReduce;
            }
        }

        /// <summary>
        /// Gets the static type of the expression that this expression represents.
        /// </summary>
        /// <returns>The <see cref="System.Type"/> that represents the static type of the expression.</returns>
        public override Type Type
        {
            get
            {
                return this.CanReduce || this._reducedExpressions.ContainsKey(new SymbolTable(this.Symbols).AllHash)
                    ? this.Reduce().Null(e => e.Type)
                    : null;
            }
        }

        /// <summary>
        /// Gets the symbol table linked with this expression.
        /// </summary>
        /// <value>
        /// The symbol table linked with this expression.
        /// </value>
        public SymbolTable Symbols
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a new instance of <see cref="YacqExpression"/>.
        /// </summary>
        /// <param name="symbols">The symbol table linked with this expression.</param>
        protected YacqExpression(SymbolTable symbols)
        {
            this._canReduce = true;
            this._reducedExpressions = new Dictionary<Int32, Expression>();
            this.Symbols = symbols ?? new SymbolTable();
        }

        /// <summary>
        /// Reduces this node to a simpler expression. If <see cref="CanReduce"/> returns <c>true</c>, this should return a valid expression.
        /// This method can return another node which itself must be reduced.
        /// </summary>
        /// <returns>
        /// The reduced expression.
        /// </returns>
        public override Expression Reduce()
        {
            return this.Reduce(null);
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables. Reducing is continued while the reduced expression is not <see cref="YacqExpression"/>.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        public Expression Reduce(SymbolTable symbols, Type expectedType)
        {
            symbols = this.Symbols.Any()
                ? new SymbolTable(this.Symbols, symbols).Apply(
                      s => s.Add("$", Constant(s))
                  )
                : symbols;
            var hash = symbols.AllHash
                ^ (expectedType != null ? expectedType.GetHashCode() : 0);
            if (this._reducedExpressions.ContainsKey(hash))
            {
                return this._reducedExpressions[hash];
            }
            else
            {
                var expression = this.ForceReduce(symbols, expectedType);
                this._canReduce = false;
                if (expression != this)
                {
                    this._reducedExpressions[hash] = expression;
                }
                return expression;
            }
        }

        internal Expression ForceReduce(SymbolTable symbols, Type expectedType)
        {
            var expression = this.ReduceImpl(symbols, expectedType) ?? this;
            return expression == this
                ? expression
                : expression is YacqExpression
                      ? ((YacqExpression) expression).Reduce(symbols, expectedType)
                      : ImplicitConvert(expression, expectedType);
        }

        internal void ClearCache()
        {
            this._reducedExpressions.Clear();
        }

        /// <summary>
        /// When implemented in a derived class, reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected abstract Expression ReduceImpl(SymbolTable symbols, Type expectedType);

        internal static Expression ImplicitConvert(Expression expression, Type expectedType)
        {
            if (expectedType == null)
            {
                return expression;
            }
            if (expectedType.IsGenericParameter)
            {
                if (!expectedType.IsAppropriate(expression.Type))
                {
                    return null;
                }
            }
            else
            {
                if (expectedType.IsAppropriate(expression.Type) && expression.Type.IsValueType && !expectedType.IsValueType)
                {
                    return Convert(expression, expectedType);
                }
                if (!expectedType.IsAppropriate(expression.Type))
                {
                    if (TestNumericConversion(expression.Type, expectedType))
                    {
                        return Convert(expression, expectedType);
                    }
                    return null;
                }
            }
            return expression;
        }

        private static Boolean TestNumericConversion(Type expressionType, Type expectedType)
        {
            switch (Type.GetTypeCode(expressionType))
            {
                case TypeCode.Byte:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Char:
                        case TypeCode.UInt16:
                        case TypeCode.Int16:
                        case TypeCode.UInt32:
                        case TypeCode.Int32:
                        case TypeCode.UInt64:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.SByte:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Char:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int16:
                        case TypeCode.UInt32:
                        case TypeCode.Int32:
                        case TypeCode.UInt64:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.UInt16:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt32:
                        case TypeCode.Int32:
                        case TypeCode.UInt64:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int16:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.UInt32:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int32:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.UInt64:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Int64:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Decimal:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                case TypeCode.Single:
                    switch (Type.GetTypeCode(expectedType))
                    {
                        case TypeCode.Double:
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }
    }
}
