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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace XSpect.Yacq.LanguageServices
{
    /// <summary>
    /// Provides bidirectional character access for reading code string sequence.
    /// </summary>
    public class ReaderCursor
    {
        private LinkedListNode<Char> _node;

        /// <summary>
        /// Gets the belonging <see cref="Reader"/>.
        /// </summary>
        /// <value>The belonging <see cref="Reader"/>.</value>
        public Reader Reader
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current position of this cursor.
        /// </summary>
        /// <value>The current position of this cursor.</value>
        public TextPosition Position
        {
            get;
            private set;
        }

        internal ReaderCursor(Reader reader, String input)
            : this(reader, new LinkedList<Char>(input + "\0").First)
        {
            this.Position = new TextPosition(0, 1, 1);
        }

        private ReaderCursor(Reader reader, LinkedListNode<Char> node)
        {
            this.Reader = reader;
            this._node = node;
        }

        private IEnumerable<LinkedListNode<Char>> EnumerateForwardNodes()
        {
            return EnumerableEx.Generate(
                this._node,
                n => n != null && n.Value != '\0',
                n => n.Next,
                _ => _
            );
        }

        private IEnumerable<LinkedListNode<Char>> EnumerateBackwardNodes()
        {
            return EnumerableEx.Generate(
                this._node,
                n => n != null,
                n => n.Previous,
                _ => _
            )
                // Skip current Value
                .Skip(1);
        }

        /// <summary>
        /// Creates new <see cref="ReaderCursor"/> instance which has same states as this instance.
        /// </summary>
        /// <returns>New <see cref="ReaderCursor"/> instance which has same states as this instance.</returns>
        public ReaderCursor Clone()
        {
            return new ReaderCursor(this.Reader, this._node)
            {
                Position = this.Position,
            };
        }

        /// <summary>
        /// Enumerates characters from the character at current position to forward direction.
        /// </summary>
        /// <returns>A sequence to enumerate from the character at current position to forward direction.</returns>
        public IEnumerable<Char> EnumerateForward()
        {
            return this.EnumerateForwardNodes()
                .Select(n => n.Value);
        }

        /// <summary>
        /// Enumerates characters from the previous character of current position to backward direction.
        /// </summary>
        /// <returns>A sequence to enumerate from the previous character of current position to backward direction.</returns>
        public IEnumerable<Char> EnumerateBackward()
        {
            return this.EnumerateBackwardNodes()
                .Select(n => n.Value);
        }

        /// <summary>
        /// Peeks a character at specified offset in forward direction.
        /// </summary>
        /// <param name="offset">Offset of the character to peek.</param>
        /// <returns>A character at specified offset in forward direction.</returns>
        public Char PeekCharForward(Int32 offset)
        {
            return this.EnumerateForward().ElementAtOrDefault(offset);
        }

        /// <summary>
        /// Peeks a character at specified offset in backward direction.
        /// </summary>
        /// <param name="offset">Offset of the character to peek.</param>
        /// <returns>A character at specified offset in backward direction.</returns>
        public Char PeekCharBackward(Int32 offset)
        {
            return this.EnumerateBackward().ElementAtOrDefault(offset);
        }

        /// <summary>
        /// Peeks specified number of characters as a sequence in forward direction.
        /// </summary>
        /// <param name="count">The numbers of characters to peek.</param>
        /// <returns>A sequence of peeked characters in forward direction.</returns>
        public IEnumerable<Char> PeekForward(Int32 count)
        {
            return this.EnumerateForward()
                .Take(count);
        }

        /// <summary>
        /// Peeks specified number of characters as a string in forward direction.
        /// </summary>
        /// <param name="count">The numbers of characters to peek.</param>
        /// <returns>A string of peeked characters in forward direction.</returns>
        public String PeekStringForward(Int32 count)
        {
            return new String(this.PeekForward(count)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks specified number of characters as a sequence in backward direction.
        /// </summary>
        /// <param name="count">The numbers of characters to peek.</param>
        /// <returns>A sequence of peeked characters in backward direction.</returns>
        public IEnumerable<Char> PeekBackward(Int32 count)
        {
            return this.EnumerateBackward()
                .Take(count);
        }

        /// <summary>
        /// Peeks specified number of characters as a string in backward direction.
        /// </summary>
        /// <param name="count">The numbers of characters to peek.</param>
        /// <returns>A string of peeked characters in backward direction.</returns>
        public String PeekStringBackward(Int32 count)
        {
            return new String(this.PeekBackward(count)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks specified number of characters as a sequence in ordered backward direction.
        /// </summary>
        /// <param name="count">The numbers of characters to peek.</param>
        /// <returns>A sequence of peeked characters in ordered backward direction.</returns>
        public IEnumerable<Char> PeekOrderedBackward(Int32 count)
        {
            return this.EnumerateBackward()
                .Take(count)
                .Reverse();
        }

        /// <summary>
        /// Peeks specified number of characters as a string in ordered backward direction.
        /// </summary>
        /// <param name="count">The numbers of characters to peek.</param>
        /// <returns>A string of peeked characters in ordered backward direction.</returns>
        public String PeekStringOrderedBackward(Int32 count)
        {
            return new String(this.PeekOrderedBackward(count)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks characters as a string as long as a specified condition is <c>true</c>, in forward direction.
        /// </summary>
        /// <param name="predicate">A function to test each character for a condition.</param>
        /// <returns>A string of peeked characters in forward direction.</returns>
        public String PeekWhileStringForward(Func<Char, Boolean> predicate)
        {
            return new String(this.EnumerateForward()
                .TakeWhile(predicate)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks characters as a string as long as a specified condition is <c>true</c>, in forward direction. The character's index is used in the logic of the predicate function.
        /// </summary>
        /// <param name="predicate">A function to test each character for a condition; the second parameter of the function represents the index of the source character.</param>
        /// <returns>A string of peeked characters in forward direction.</returns>
        public String PeekWhileStringForward(Func<Char, Int32, Boolean> predicate)
        {
            return new String(this.EnumerateForward()
                .TakeWhile(predicate)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks characters as a string as long as a specified condition is <c>true</c>, in backward direction.
        /// </summary>
        /// <param name="predicate">A function to test each character for a condition.</param>
        /// <returns>A string of peeked characters in backward direction.</returns>
        public String PeekWhileStringBackward(Func<Char, Boolean> predicate)
        {
            return new String(this.EnumerateBackward()
                .TakeWhile(predicate)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks characters as a string as long as a specified condition is <c>true</c>, in backward direction. The character's index is used in the logic of the predicate function.
        /// </summary>
        /// <param name="predicate">A function to test each character for a condition; the second parameter of the function represents the index of the source character.</param>
        /// <returns>A string of peeked characters in backward direction.</returns>
        public String PeekWhileStringBackward(Func<Char, Int32, Boolean> predicate)
        {
            return new String(this.EnumerateBackward()
                .TakeWhile(predicate)
                .ToArray()
            );
        }

        /// <summary>
        /// Peeks characters as a string while specified character sequence is encountered, in forward direction.
        /// </summary>
        /// <param name="chars">A character sequence to search the terminator of peeking.</param>
        /// <returns>A string of peeked characters in forward direction.</returns>
        public String PeekWhileStringForward(IEnumerable<Char> chars)
        {
            return this.PeekStringForward(this.EnumerateForward()
                .Buffer(chars.Count(), 1)
                .Select((c, i) => Tuple.Create(c, i))
                .FirstOrDefault(_ => _.Item1.SequenceEqual(chars))
                .Null(_ => _.Item2)
            );
        }

        /// <summary>
        /// Peeks characters as a string while specified character sequence is encountered, in forward direction.
        /// </summary>
        /// <param name="chars">A character sequence to search the terminator of peeking.</param>
        /// <returns>A string of peeked characters in backward direction.</returns>
        public String PeekWhileStringBackward(IEnumerable<Char> chars)
        {
            return this.PeekStringForward(this.EnumerateBackward()
                .Buffer(chars.Count(), 1)
                .Select((c, i) => Tuple.Create(c, i))
                .FirstOrDefault(_ => _.Item1.SequenceEqual(chars))
                .Null(_ => _.Item2)
            );
        }

        /// <summary>
        /// Peeks characters as a string while specified character sequence is encountered, in forward direction.
        /// </summary>
        /// <param name="chars">A character sequence to search the terminator of peeking.</param>
        /// <returns>A string of peeked characters in ordered backward direction.</returns>
        public String PeekWhileStringOrderedBackward(IEnumerable<Char> chars)
        {
            chars = chars.Reverse();
            return this.PeekStringForward(this.EnumerateBackward()
                .Buffer(chars.Count(), 1)
                .Select((c, i) => Tuple.Create(c, i))
                .FirstOrDefault(_ => _.Item1.SequenceEqual(chars))
                .Null(_ => _.Item2)
            );
        }

        /// <summary>
        /// Moves this cursor to specified characters in forward direction.
        /// </summary>
        /// <param name="count">The number to move this cursor.</param>
        public void MoveForward(Int32 count)
        {
            if (count <= 0)
            {
                return;
            }
            else
            {
                this.PeekStringForward(count).Apply(
                    s => this._node = this.EnumerateForwardNodes().ElementAtOrDefault(count) ?? this._node.List.Last,
                    s => this.Position = this._node.Next != null
                        ? new TextPosition(
                              this.Position.Index + count,
                              this.Position.Line + Regex.Matches(s, "\r\n|\r|\n").Count,
                              s.LastIndexOfAny(new[] { '\r', '\n', }).Let(i => i > 0
                                  ? count - i
                                  : this.Position.Column + count
                              )
                          )
                        : new TextPosition(
                              this._node.List.Count - 1,
                              Regex.Matches(s, "\r\n|\r|\n").Count + 1,
                              s.LastIndexOfAny(new[] { '\r', '\n', }).Let(i => i > 0
                                  ? count - i
                                  : this.Position.Column + count
                              )
                          )
                );
            }
        }

        /// <summary>
        /// Moves this cursor to specified absolute index.
        /// </summary>
        /// <param name="index">The index number to move.</param>
        public void MoveTo(Int32 index)
        {
            this._node = this._node.List.First;
            this.Position = new TextPosition(0, 1, 1);
            this.MoveForward(index);
        }

        /// <summary>
        /// Determines whether the characters in forward direction and specified character sequence are equal.
        /// </summary>
        /// <param name="chars">A character sequence to compare.</param>
        /// <returns><c>true</c> if the characters in forward direction is equals to specified character sequence; otherwise, <c>false</c>.</returns>
        public Boolean MatchForward(IEnumerable<Char> chars)
        {
            return this.PeekForward(chars.Count())
                .SequenceEqual(chars);
        }

        /// <summary>
        /// Determines whether the characters in backward direction and specified character sequence are equal.
        /// </summary>
        /// <param name="chars">A character sequence to compare.</param>
        /// <returns><c>true</c> if the characters in backward direction is equals to specified character sequence; otherwise, <c>false</c>.</returns>
        public Boolean MatchBackward(IEnumerable<Char> chars)
        {
            return this.PeekBackward(chars.Count())
                .SequenceEqual(chars);
        }

        /// <summary>
        /// Determines whether the characters in ordered backward direction and specified character sequence are equal.
        /// </summary>
        /// <param name="chars">A character sequence to compare.</param>
        /// <returns><c>true</c> if the characters in ordered backward direction is equals to specified character sequence; otherwise, <c>false</c>.</returns>
        public Boolean MatchOrderedBackward(IEnumerable<Char> chars)
        {
            return this.PeekOrderedBackward(chars.Count())
                .SequenceEqual(chars);
        }

        /// <summary>
        /// Inserts specified character sequence after the current position in forward direction.
        /// </summary>
        /// <param name="chars">A character sequence to insert.</param>
        public void InsertForward(IEnumerable<Char> chars)
        {
            if (this._node.Previous != null)
            {
                chars.ForEach(c => this._node.List.AddAfter(this._node.Previous, c));
            }
            else
            {
                chars.Reverse().ForEach(c => this._node.List.AddFirst(c));
            }
        }

        /// <summary>
        /// Inserts specified character sequence before the current position in backward direction.
        /// </summary>
        /// <param name="chars">A character sequence to insert.</param>
        public void InsertBackward(IEnumerable<Char> chars)
        {
            chars.Reverse().ForEach(c => this._node.List.AddBefore(this._node, c));
        }

        /// <summary>
        /// Inserts specified character sequence before the current position in ordered backward direction.
        /// </summary>
        /// <param name="chars">A character sequence to insert.</param>
        public void InsertOrderedBackward(IEnumerable<Char> chars)
        {
            chars.ForEach(c => this._node.List.AddBefore(this._node, c));
        }

        /// <summary>
        /// Replaces the characters from the current position to specified character sequence in forward direction.
        /// </summary>
        /// <param name="chars">A character sequence to replace.</param>
        public void ReplaceForward(IEnumerable<Char> chars)
        {
            this.EnumerateForwardNodes()
                .Take(chars.Count())
                .Zip(chars, Tuple.Create)
                .ForEach(_ => _.Item1.Value = _.Item2);
        }

        /// <summary>
        /// Replaces the characters from the current position to specified character sequence in backward direction.
        /// </summary>
        /// <param name="chars">A character sequence to replace.</param>
        public void ReplaceBackward(IEnumerable<Char> chars)
        {
            this.EnumerateBackwardNodes()
                .Take(chars.Count())
                .Zip(chars, Tuple.Create)
                .ForEach(_ => _.Item1.Value = _.Item2);
        }

        /// <summary>
        /// Replaces the characters from the current position to specified character sequence in ordered backward direction.
        /// </summary>
        /// <param name="chars">A character sequence to replace.</param>
        public void ReplaceOrderedBackward(IEnumerable<Char> chars)
        {
            this.EnumerateBackwardNodes()
                .Take(chars.Count())
                .Reverse()
                .Zip(chars, Tuple.Create)
                .ForEach(_ => _.Item1.Value = _.Item2);
        }
    }
}