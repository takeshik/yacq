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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XSpect.Yacq.LanguageServices
{
    public class Tokenizer
        : Object,
          IEnumerable<Token>
    {
        private Int32 _position;

        private Int32 _line;

        private Int32 _column;

        public String Input
        {
            get;
            private set;
        }

        public Tokenizer(String input)
        {
            this._position = 0;
            this._line = 1;
            this._column = 1;
            this.Input = input;
        }

        public Char PeekChar(Int32 offset = 0)
        {
            return this._position + offset >= this.Input.Length
                ? '\0'
                : this.Input[this._position + offset];
        }

        public Char ReadChar()
        {
            var c = this.PeekChar();
            ++this._position;
            return c;
        }

        public String Peek(Int32 length, Int32 offset = 0)
        {
            var sb = new StringBuilder(length, length);
            Enumerable.Range(offset, length)
                .TakeWhile(i => this._position + i >= this.Input.Length)
                .ForEach(i => sb.Append(this.Input[this._position + i]));
            return sb.ToString();
        }

        public String Read(Int32 length)
        {
            var s = this.Peek(length);
            this._position += length;
            return s;
        }

        private Token CreateToken(TokenType type, String str)
        {
            return new Token(type, str, this._position, this._line, this._column);
        }

        public Token Peek()
        {
            Char c;
            switch (c = this.PeekChar())
            {
                case '\0':
                    return this.CreateToken(TokenType.End, "");
                case ' ':
                case '\t':
                case '\r':
                case '\n':
                    return this.CreateToken(TokenType.Whitespace, this.RegexSlice(@"[ \t\r\n]+"));
                case ';':
                    return this.CreateToken(TokenType.Comment, this.RegexSlice(@"[\r\n]+"));
                case '\'':
                    return this.CreateToken(TokenType.StringLiteral, this.RegexSlice(@"[^\\]'"));
                case '"':
                    return this.CreateToken(TokenType.StringLiteral, this.RegexSlice(@"[^\\]"""));
                case '`':
                    return this.CreateToken(TokenType.StringLiteral, this.RegexSlice(@"[^\\]`"));
                case '(':
                    return this.CreateToken(TokenType.LeftParenthesis, "(");
                case ')':
                    return this.CreateToken(TokenType.RightParenthesis, ")");
                case '[':
                    return this.CreateToken(TokenType.LeftBracket, "[");
                case ']':
                    return this.CreateToken(TokenType.RightBracket, "]");
                case '{':
                    return this.CreateToken(TokenType.LeftBrace, "{");
                case '}':
                    return this.CreateToken(TokenType.RightBrace, "}");
                case ',':
                    return this.CreateToken(TokenType.Comma, ",");
                case '.':
                    return this.CreateToken(TokenType.Period, ".");
                case ':':
                    return this.CreateToken(TokenType.Colon, ":");
                default:
                    return Char.IsNumber(c)
                        ? this.CreateToken(TokenType.NumberLiteral, this.RegexSlice("[0-9a-z_.]+"))
                        : this.CreateToken(TokenType.Identifier, this.RegexSlice(@"[^ \t\r\n""#\(\),.:;\[\]`\{\}]+"));
            }
        }

        private String RegexSlice(String pattern)
        {
            var match = Regex.Match(this.Input.Substring(this._position), pattern);
            return this.Input.Substring(this._position, match.Index + match.Length);
        }

        public Token Read()
        {
            Token t = this.Peek();
            if (t.Text.Contains("\r") || t.Text.Contains("\n"))
            {
                this._line += Regex.Matches(t.Text, "\r\n|\r|\n").Count;
                this._column = 1;
            }
            else
            {
                this._column += t.Text.Length;
            }
            this._position += t.Text.Length;
            return t;
        }

        public IEnumerator<Token> GetEnumerator()
        {
            Token t;
            do
            {
                t = this.Read();
                if (t.Type != TokenType.Whitespace && t.Type != TokenType.Comment)
                {
                    yield return t;
                }
            } while (t.Type != TokenType.End);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}