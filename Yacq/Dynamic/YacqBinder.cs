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
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Dynamic
{
    /// <summary>
    /// Contains factory methods to create dynamic call site binders.
    /// </summary>
    public static class YacqBinder
    {
        /// <summary>
        /// Initializes a new binary operation binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="operation">The binary operation kind.</param>
        /// <returns>A new binary operation binder.</returns>
        public static BinaryOperationBinder BinaryOperation(SymbolTable symbols, ExpressionType operation)
        {
            return new YacqBinaryOperationBinder(symbols, operation);
        }

        /// <summary>
        /// Initializes a new binary operation binder.
        /// </summary>
        /// <param name="operation">The binary operation kind.</param>
        /// <returns>A new binary operation binder.</returns>
        public static BinaryOperationBinder BinaryOperation(ExpressionType operation)
        {
            return BinaryOperation(null, operation);
        }

        /// <summary>
        /// Initializes a new convert binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="type">The type to convert to.</param>
        /// <returns>A new convert binder.</returns>
        public static ConvertBinder Convert(SymbolTable symbols, Type type)
        {
            return new YacqConvertBinder(symbols, type, false);
        }

        /// <summary>
        /// Initializes a new convert binder.
        /// </summary>
        /// <param name="type">The type to convert to.</param>
        /// <returns>A new convert binder.</returns>
        public static ConvertBinder Convert(Type type)
        {
            return Convert(null, type);
        }

        /// <summary>
        /// Initializes a new get index binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new get index binder.</returns>
        public static GetIndexBinder GetIndex(SymbolTable symbols, params String[] argumentNames)
        {
            return new YacqGetIndexBinder(symbols, new CallInfo(argumentNames.Length, argumentNames));
        }

        /// <summary>
        /// Initializes a new get index binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new get index binder.</returns>
        public static GetIndexBinder GetIndex(SymbolTable symbols, IEnumerable<String> argumentNames)
        {
            return GetIndex(symbols, argumentNames.ToArray());
        }

        /// <summary>
        /// Initializes a new get index binder.
        /// </summary>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new get index binder.</returns>
        public static GetIndexBinder GetIndex(params String[] argumentNames)
        {
            return GetIndex(null, argumentNames);
        }

        /// <summary>
        /// Initializes a new get index binder.
        /// </summary>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new get index binder.</returns>
        public static GetIndexBinder GetIndex(IEnumerable<String> argumentNames)
        {
            return GetIndex(null, argumentNames);
        }

        /// <summary>
        /// Initializes a new get member binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="name">The name of the member to get.</param>
        /// <returns>A new get member binder.</returns>
        public static GetMemberBinder GetMember(SymbolTable symbols, String name)
        {
            return new YacqGetMemberBinder(symbols, name, false);
        }

        /// <summary>
        /// Initializes a new get member binder.
        /// </summary>
        /// <param name="name">The name of the member to get.</param>
        /// <returns>A new get member binder.</returns>
        public static GetMemberBinder GetMember(String name)
        {
            return GetMember(null, name);
        }

        /// <summary>
        /// Initializes a new invoke binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new invoke binder.</returns>
        public static InvokeBinder Invoke(SymbolTable symbols, params String[] argumentNames)
        {
            return new YacqInvokeBinder(symbols, new CallInfo(argumentNames.Length, argumentNames));
        }

        /// <summary>
        /// Initializes a new invoke binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new invoke binder.</returns>
        public static InvokeBinder Invoke(SymbolTable symbols, IEnumerable<String> argumentNames)
        {
            return Invoke(symbols, argumentNames.ToArray());
        }

        /// <summary>
        /// Initializes a new invoke binder.
        /// </summary>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new invoke binder.</returns>
        public static InvokeBinder Invoke(params String[] argumentNames)
        {
            return Invoke(null, argumentNames);
        }

        /// <summary>
        /// Initializes a new invoke binder.
        /// </summary>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new invoke binder.</returns>
        public static InvokeBinder Invoke(IEnumerable<String> argumentNames)
        {
            return Invoke(null, argumentNames);
        }

        /// <summary>
        /// Initializes a new invoke member binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="name">The name of the member to invoke.</param>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new invoke member binder.</returns>
        public static InvokeMemberBinder InvokeMember(SymbolTable symbols, String name, params String[] argumentNames)
        {
            return new YacqInvokeMemberBinder(symbols, name, false, new CallInfo(argumentNames.Length, argumentNames));
        }

        /// <summary>
        /// Initializes a new invoke member binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="name">The name of the member to invoke.</param>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new invoke member binder.</returns>
        public static InvokeMemberBinder InvokeMember(SymbolTable symbols, String name, IEnumerable<String> argumentNames)
        {
            return InvokeMember(symbols, name, argumentNames.ToArray());
        }

        /// <summary>
        /// Initializes a new invoke member binder.
        /// </summary>
        /// <param name="name">The name of the member to invoke.</param>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new invoke member binder.</returns>
        public static InvokeMemberBinder InvokeMember(String name, params String[] argumentNames)
        {
            return InvokeMember(null, name, argumentNames);
        }

        /// <summary>
        /// Initializes a new invoke member binder.
        /// </summary>
        /// <param name="name">The name of the member to invoke.</param>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new invoke member binder.</returns>
        public static InvokeMemberBinder InvokeMember(String name, IEnumerable<String> argumentNames)
        {
            return InvokeMember(null, name, argumentNames);
        }

        /// <summary>
        /// Initializes a new set index binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new set index binder.</returns>
        public static SetIndexBinder SetIndex(SymbolTable symbols, params String[] argumentNames)
        {
            return new YacqSetIndexBinder(symbols, new CallInfo(argumentNames.Length, argumentNames));
        }

        /// <summary>
        /// Initializes a new set index binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new set index binder.</returns>
        public static SetIndexBinder SetIndex(SymbolTable symbols, IEnumerable<String> argumentNames)
        {
            return SetIndex(symbols, argumentNames.ToArray());
        }

        /// <summary>
        /// Initializes a new set index binder.
        /// </summary>
        /// <param name="argumentNames">The array of argument names for this operation.</param>
        /// <returns>A new set index binder.</returns>
        public static SetIndexBinder SetIndex(params String[] argumentNames)
        {
            return SetIndex(null, argumentNames);
        }

        /// <summary>
        /// Initializes a new set index binder.
        /// </summary>
        /// <param name="argumentNames">The sequence of argument names for this operation.</param>
        /// <returns>A new set index binder.</returns>
        public static SetIndexBinder SetIndex(IEnumerable<String> argumentNames)
        {
            return SetIndex(null, argumentNames);
        }

        /// <summary>
        /// Initializes a new set member binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="name">The name of the member to set.</param>
        /// <returns>A new set member binder.</returns>
        public static SetMemberBinder SetMember(SymbolTable symbols, String name)
        {
            return new YacqSetMemberBinder(symbols, name, false);
        }

        /// <summary>
        /// Initializes a new set member binder.
        /// </summary>
        /// <param name="name">The name of the member to set.</param>
        /// <returns>A new set member binder.</returns>
        public static SetMemberBinder SetMember(String name)
        {
            return SetMember(null, name);
        }

        /// <summary>
        /// Initializes a new unary operation binder.
        /// </summary>
        /// <param name="symbols">The symbol table for the binder.</param>
        /// <param name="operation">The unary operation kind.</param>
        /// <returns>A new unary operation binder.</returns>
        public static UnaryOperationBinder UnaryOperation(SymbolTable symbols, ExpressionType operation)
        {
            return new YacqUnaryOperationBinder(symbols, operation);
        }

        /// <summary>
        /// Initializes a new unary operation binder.
        /// </summary>
        /// <param name="operation">The unary operation kind.</param>
        /// <returns>A new unary operation binder.</returns>
        public static UnaryOperationBinder UnaryOperation(ExpressionType operation)
        {
            return UnaryOperation(null, operation);
        }

        internal static Boolean IsInDynamicContext(SymbolTable symbols, Expression expression)
        {
            return expression is DynamicExpression
                || (expression as ContextfulExpression
                       ?? (expression as YacqExpression)
                              .Null(e => e.ReduceScan(symbols)
                                  .Choose(re => re as ContextfulExpression)
                                  .FirstOrDefault()
                              )
                   ).Null(ce => ce.ContextType == ContextType.Dynamic);
        }

        internal static Boolean IsInDynamicContext(SymbolTable symbols, IEnumerable<Expression> expressions)
        {
            return expressions.Any(e => IsInDynamicContext(symbols, e));
        }

        internal static Boolean IsInDynamicContext(SymbolTable symbols, params Expression[] expressions)
        {
            return IsInDynamicContext(symbols, (IEnumerable<Expression>) expressions);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
