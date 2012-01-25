// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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

namespace XSpect.Yacq
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

    internal static class Static
    {
        public static Type GetTargetType(Type type)
        {
            return type.TryGetGenericTypeDefinition() == typeof(Static<>)
                ? type.GetGenericArguments()[0]
                : null;
        }

        public static Type MakeType(Type type)
        {
            return typeof(Static<>).MakeGenericType(type);
        }

        public static Type MakeType<T>()
        {
            return MakeType(typeof(T));
        }

        public static Object Value(Type type)
        {
            return Activator.CreateInstance(MakeType(type));
        }

        public static Object Value<T>()
        {
            return Activator.CreateInstance(MakeType<T>());
        }
    }
}