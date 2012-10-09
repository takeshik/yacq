// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Parseq;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Provides the base class from which the classes that represent YACQ expression tree nodes are derived.
    /// It also contains static factory methods to create the various node types. This is an abstract class.
    /// </summary>
    public abstract partial class YacqExpression
        : Expression
    {
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
                return true;
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
                var type = this.Type();
                if (type == null)
                {
                    throw new InvalidOperationException("Failed to reduce the expression. Use Type() extension method instead.");
                }
                return type;
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
        /// Gets the start position in the source for this expression.
        /// </summary>
        /// <value>The start position in the source for this expression.</value>
        public Position StartPosition
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the end position in the source for this expression.
        /// </summary>
        /// <value>The end position in the source for this expression.</value>
        public Position EndPosition
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
        public Expression Reduce(SymbolTable symbols, Type expectedType = null)
        {
            return this.ReduceScan(symbols, expectedType)
                .FirstOrLast(e => !(e is YacqExpression));
        }

        /// <summary>
        /// Reduces this node to a simpler expression with additional symbol tables. This method can return another node which itself must be reduced.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        public Expression ReduceOnce(SymbolTable symbols = null, Type expectedType = null)
        {
            symbols = this.CreateSymbolTable(symbols);
            var hash = symbols.AllHash
                ^ (expectedType != null ? expectedType.GetHashCode() : 0);
            if (this._reducedExpressions.ContainsKey(hash))
            {
                return this._reducedExpressions[hash];
            }
            else
            {
                var expression = (this.ReduceImpl(symbols, expectedType) ?? this).If(
                    e => e != this && !(e is YacqExpression),
                    e => ImplicitConvert(e, expectedType)
                );
                if (expression != this)
                {
                    this._reducedExpressions[hash] = expression;
                }
                return expression;
            }
        }

        internal Boolean IsCached(SymbolTable symbols, Type expectedType)
        {
            return this._reducedExpressions.ContainsKey(this.CreateSymbolTable(symbols).AllHash
                ^ (expectedType != null ? expectedType.GetHashCode() : 0)
            );
        }

        internal void ClearCache()
        {
            this._reducedExpressions.Clear();
        }

        internal SymbolTable CreateSymbolTable(SymbolTable symbols)
        {
            return this.Symbols.Any()
                ? new SymbolTable(this.Symbols, symbols)
                : symbols ?? new SymbolTable();
        }

        /// <summary>
        /// When implemented in a derived class, reduces this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced expression.</returns>
        protected abstract Expression ReduceImpl(SymbolTable symbols, Type expectedType);

        internal void SetPosition(Position start, Position end)
        {
            this.StartPosition = start;
            this.EndPosition = end;
        }

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
                if ((expectedType.IsAppropriate(expression.Type) && expression.Type.IsValueType && !expectedType.IsValueType)
                    || expectedType.GetMethod("op_Implicit", BindingFlags.Public | BindingFlags.Static, null, new [] { expression.Type, }, null) != null
                )
                {
                    return Convert(expression, expectedType);
                }
                if (!expectedType.IsAppropriate(expression.Type))
                {
                    if (Nullable.GetUnderlyingType(expectedType).Let(t => t != null && t == expression.Type))
                    {
                        return Convert(expression, typeof(Nullable<>).MakeGenericType(expression.Type));
                    }
                    else if (Nullable.GetUnderlyingType(expression.Type).Let(t => t != null && t == expectedType))
                    {
                        return Convert(expression, Nullable.GetUnderlyingType(expression.Type));
                    }
                    return ConvertNumericType(expression.Type, expectedType)
                        .Null(t => Convert(expression, t));
                }
            }
            return expression;
        }

        internal static Type ConvertNumericType(Type expressionType, Type expectedType)
        {
            var expectedType_ = Type.GetTypeCode(Nullable.GetUnderlyingType(expectedType) ?? expectedType);
            switch (Type.GetTypeCode(Nullable.GetUnderlyingType(expressionType) ?? expressionType))
            {
                case TypeCode.Char:
                    switch (expectedType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.SByte:
                    switch (expectedType_)
                    {
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.Byte:
                    switch (expectedType_)
                    {
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.Int16:
                    switch (expectedType_)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.UInt16:
                    switch (expectedType_)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.Int32:
                    switch (expectedType_)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.UInt32:
                    switch (expectedType_)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.Int64:
                    switch (expectedType_)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.UInt64:
                    switch (expectedType_)
                    {
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.Single:
                    switch (expectedType_)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return expectedType;
                        default:
                            return null;
                    }
                case TypeCode.Double:
                    switch (expectedType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return expectedType;
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }

        internal static Type ConvertNumericTypeForAlithmetics(Type type, Boolean needsSigned)
        {
            var isNullable = Nullable.GetUnderlyingType(type) != null;
            switch (Type.GetTypeCode(isNullable ? Nullable.GetUnderlyingType(type) : type))
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    return isNullable
                        ? typeof(Nullable<Int32>)
                        : typeof(Int32);
                case TypeCode.UInt32:
                    return needsSigned
                        ? isNullable
                              ? typeof(Nullable<Int64>)
                              : typeof(Int64)
                        : isNullable
                              ? typeof(Nullable<UInt32>)
                              : typeof(UInt32);
                case TypeCode.Int64:
                    return isNullable
                        ? typeof(Nullable<Int64>)
                        : typeof(Int64);
                case TypeCode.UInt64:
                    return needsSigned
                        ? null
                        : isNullable
                              ? typeof(Nullable<UInt64>)
                              : typeof(UInt64);
                default:
                    return null;
            }
        }

        internal static Type ConvertNumericTypeForAlithmetics(Type leftType, Type rightType)
        {
            var isNullable = Nullable.GetUnderlyingType(leftType) != null || Nullable.GetUnderlyingType(rightType) != null;
            var rightType_ = Type.GetTypeCode(Nullable.GetUnderlyingType(rightType) ?? rightType);
            switch (Type.GetTypeCode(Nullable.GetUnderlyingType(leftType) ?? leftType))
            {
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                            return isNullable
                                ? typeof(Nullable<Int32>)
                                : typeof(Int32);
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return isNullable
                                ? typeof(Nullable<Int64>)
                                : typeof(Int64);
                        case TypeCode.Single:
                            return isNullable
                                ? typeof(Nullable<Single>)
                                : typeof(Single);
                        case TypeCode.Double:
                            return isNullable
                                ? typeof(Nullable<Double>)
                                : typeof(Double);
                        case TypeCode.Decimal:
                            return isNullable
                                ? typeof(Nullable<Decimal>)
                                : typeof(Decimal);
                        default:
                            return null;
                    }
                case TypeCode.UInt32:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                            return isNullable
                                ? typeof(Nullable<UInt32>)
                                : typeof(UInt32);
                        case TypeCode.SByte:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                            return isNullable
                                ? typeof(Nullable<Int64>)
                                : typeof(Int64);
                        case TypeCode.UInt64:
                            return isNullable
                                ? typeof(Nullable<UInt64>)
                                : typeof(UInt64);
                        case TypeCode.Single:
                            return isNullable
                                ? typeof(Nullable<Single>)
                                : typeof(Single);
                        case TypeCode.Double:
                            return isNullable
                                ? typeof(Nullable<Double>)
                                : typeof(Double);
                        case TypeCode.Decimal:
                            return isNullable
                                ? typeof(Nullable<Decimal>)
                                : typeof(Decimal);
                        default:
                            return null;
                    }
                case TypeCode.Int64:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                            return isNullable
                                ? typeof(Nullable<Int64>)
                                : typeof(Int64);
                        case TypeCode.Single:
                            return isNullable
                                ? typeof(Nullable<Single>)
                                : typeof(Single);
                        case TypeCode.Double:
                            return isNullable
                                ? typeof(Nullable<Double>)
                                : typeof(Double);
                        case TypeCode.Decimal:
                            return isNullable
                                ? typeof(Nullable<Decimal>)
                                : typeof(Decimal);
                        default:
                            return null;
                    }
                case TypeCode.UInt64:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.Byte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                            return isNullable
                                ? typeof(Nullable<UInt64>)
                                : typeof(UInt64);
                        case TypeCode.Single:
                            return isNullable
                                ? typeof(Nullable<Single>)
                                : typeof(Single);
                        case TypeCode.Double:
                            return isNullable
                                ? typeof(Nullable<Double>)
                                : typeof(Double);
                        case TypeCode.Decimal:
                            return isNullable
                                ? typeof(Nullable<Decimal>)
                                : typeof(Decimal);
                        default:
                            return null;
                    }
                case TypeCode.Single:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                            return isNullable
                                ? typeof(Nullable<Single>)
                                : typeof(Single);
                        case TypeCode.Double:
                            return isNullable
                                ? typeof(Nullable<Double>)
                                : typeof(Double);
                        default:
                            return null;
                    }
                case TypeCode.Double:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                            return isNullable
                                ? typeof(Nullable<Double>)
                                : typeof(Double);
                        default:
                            return null;
                    }
                case TypeCode.Decimal:
                    switch (rightType_)
                    {
                        case TypeCode.Char:
                        case TypeCode.SByte:
                        case TypeCode.Byte:
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Decimal:
                            return isNullable
                                ? typeof(Nullable<Decimal>)
                                : typeof(Decimal);
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
