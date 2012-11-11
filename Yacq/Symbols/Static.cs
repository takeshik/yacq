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

namespace XSpect.Yacq.Symbols
{
    /// <summary>
    /// Specify the symbol is for static call, not instance call. Use only for <see cref="SymbolEntry"/>.
    /// </summary>
    /// <typeparam name="T">The type which specify for the symbol's LeftType.</typeparam>
    public class Static<T>
    {
        /// <summary>
        /// Returns a <see cref="String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return "[" + typeof(T).FullName + "]";
        }
    }

    /// <summary>
    /// Supports to create and inspect <see cref="Static{T}"/> objects.
    /// </summary>
    public static class Static
    {
        /// <summary>
        /// Gets the target type of static calls on <see cref="Static{T}"/> type.
        /// </summary>
        /// <param name="type">The type to get the target type of static calls.</param>
        /// <returns>The target type of static calls.</returns>
        public static Type GetTargetType(Type type)
        {
            return type.TryGetGenericTypeDefinition() == typeof(Static<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        /// <summary>
        /// Gets the target type of static calls on <see cref="Static{T}"/> object.
        /// </summary>
        /// <param name="obj">The object to get the target type of static calls.</param>
        /// <returns>The target type of static calls.</returns>
        public static Type GetTargetType(Object obj)
        {
            return GetTargetType(obj.GetType());
        }

        /// <summary>
        /// Gets <see cref="Static{T}"/> type from target type of static calls.
        /// </summary>
        /// <param name="type">The target type of static calls.</param>
        /// <returns>The <see cref="Static{T}"/> type with specified target type of static calls.</returns>
        public static Type Type(Type type)
        {
            return typeof(Static<>).MakeGenericType(type);
        }

        /// <summary>
        /// Gets <see cref="Static{T}"/> type from target type of static calls.
        /// </summary>
        /// <typeparam name="T">The target type of static calls.</typeparam>
        /// <returns>The <see cref="Static{T}"/> type with specified target type of static calls.</returns>
        public static Type Type<T>()
        {
            return Type(typeof(T));
        }

        /// <summary>
        /// Creates new <see cref="Static{T}"/> with specified target type of static calls.
        /// </summary>
        /// <param name="type">The target type of static calls.</param>
        /// <returns>New <see cref="Static{T}"/> object with specified target type of static calls.</returns>
        public static Object Value(Type type)
        {
            return Activator.CreateInstance(Type(type));
        }

        /// <summary>
        /// Creates new <see cref="Static{T}"/> with specified target type of static calls.
        /// </summary>
        /// <typeparam name="T">The target type of static calls.</typeparam>
        /// <returns>New <see cref="Static{T}"/> object with specified target type of static calls.</returns>
        public static Static<T> Value<T>()
        {
            return (Static<T>) Activator.CreateInstance(Type<T>());
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
