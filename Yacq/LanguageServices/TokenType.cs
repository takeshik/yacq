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

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Specifies kinds of <see cref="Token"/>.
    /// </summary>
    public enum TokenType
        : byte
    {
        /// <summary>
        /// End of token sequence.
        /// </summary>
        End = 0,
        /// <summary>
        /// String which is composed by spaces, tabs and newlines.
        /// </summary>
        Whitespace,
        /// <summary>
        /// ' character (not used; reserved).
        /// </summary>
        Quote,
        /// <summary>
        /// ( character.
        /// </summary>
        LeftParenthesis,
        /// <summary>
        /// ) character.
        /// </summary>
        RightParenthesis,
        /// <summary>
        /// , character.
        /// </summary>
        Comma,
        /// <summary>
        /// . character.
        /// </summary>
        Period,
        /// <summary>
        /// : character.
        /// </summary>
        Colon,
        /// <summary>
        /// [ character.
        /// </summary>
        LeftBracket,
        /// <summary>
        /// ] character.
        /// </summary>
        RightBracket,
        /// <summary>
        /// { character.
        /// </summary>
        LeftBrace,
        /// <summary>
        /// } character.
        /// </summary>
        RightBrace,
        /// <summary>
        /// String which is marked as comment.
        /// </summary>
        Comment,
        /// <summary>
        /// String which is marked as string literal.
        /// </summary>
        StringLiteral,
        /// <summary>
        /// String which is marked as number literal.
        /// </summary>
        NumberLiteral,
        /// <summary>
        /// String which is marked as identifer.
        /// </summary>
        Identifier,
    }
}