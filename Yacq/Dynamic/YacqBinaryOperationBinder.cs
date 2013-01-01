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
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Dynamic
{
    internal class YacqBinaryOperationBinder
        : BinaryOperationBinder
    {
        private readonly SymbolTable _symbols;

        public YacqBinaryOperationBinder(SymbolTable symbols, ExpressionType operation)
            : base(operation)
        {
            this._symbols = symbols;
        }

        public override DynamicMetaObject FallbackBinaryOperation(DynamicMetaObject target, DynamicMetaObject arg, DynamicMetaObject errorSuggestion)
        {
            try
            {
                return new DynamicMetaObject(
                    YacqExpression.Function(this._symbols, GetSymbolName(this.Operation),
                        target.Expression.Reduce(_symbols).TryConvert(target.RuntimeType),
                        arg.Expression.Reduce(_symbols).TryConvert(target.RuntimeType)
                    ).Reduce(this._symbols).TryConvert(typeof(Object)),
                    target.Restrictions.Merge(arg.Restrictions)
                );
            }
            catch (Exception ex)
            {
                return errorSuggestion
                    ?? new DynamicMetaObject(
                           Expression.Throw(Expression.Constant(ex), typeof(Object)),
                           BindingRestrictions.Empty
                       );
            }
        }

        private static String GetSymbolName(ExpressionType operation)
        {
            switch (operation)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "&&";
                case ExpressionType.Coalesce:
                    return "??";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "==";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LeftShift:
                    return "<<";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.MultiplyChecked:
                    return "*";
                case ExpressionType.NotEqual:
                    return "!=";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "|";
                case ExpressionType.Power:
                    return "**";
                case ExpressionType.RightShift:
                    return ">>";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Assign:
                    return "=";
                case ExpressionType.AddAssign:
                    return "+=";
                case ExpressionType.AndAssign:
                    return "&=";
                case ExpressionType.DivideAssign:
                    return "/=";
                case ExpressionType.ExclusiveOrAssign:
                    return "^=";
                case ExpressionType.LeftShiftAssign:
                    return "<<=";
                case ExpressionType.ModuloAssign:
                    return "&=";
                case ExpressionType.MultiplyAssign:
                    return "*=";
                case ExpressionType.OrAssign:
                    return "|=";
                case ExpressionType.PowerAssign:
                    return "**=";
                case ExpressionType.RightShiftAssign:
                    return ">>=";
                case ExpressionType.SubtractAssign:
                    return "-=";
                case ExpressionType.AddAssignChecked:
                    return "+=";
                case ExpressionType.MultiplyAssignChecked:
                    return "*=";
                case ExpressionType.SubtractAssignChecked:
                    return "-=";
            }
            throw new NotSupportedException("YacqBinaryOperationBinder does not supports " + operation);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
