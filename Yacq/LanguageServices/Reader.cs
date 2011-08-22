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
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.LanguageServices
{
    public partial class Reader
    {
        private LinkedListNode<Token> _cursor;

        private readonly Stack<Scope> _stack;

        public Scope Current
        {
            get
            {
                return this._stack.Peek();
            }
        }

        public Reader(IEnumerable<Token> tokens)
        {
            this._cursor = new LinkedList<Token>(tokens).First;
            this._stack = new Stack<Scope>();
            this.EnterScope();
        }

        public Reader(String code)
            : this(new Tokenizer(code))
        {
        }

        private void EnterScope()
        {
            this._stack.Push(new Scope(this._cursor.Value));
        }

        public ICollection<Expression> LeaveScope()
        {
            var context = this._stack.Pop();
            if (!(
                (this._cursor.Value.Type == TokenType.RightParenthesis && context.Header.Type == TokenType.LeftParenthesis) ||
                (this._cursor.Value.Type == TokenType.RightBracket && context.Header.Type == TokenType.LeftBracket) ||
                (this._cursor.Value.Type == TokenType.RightBrace && context.Header.Type == TokenType.LeftBrace)
            ))
            {
                throw new InvalidOperationException("Invalid parenthesis match.");
            }
            else
            {
                return context.List;
            }
        }

        public ICollection<Expression> Read()
        {
            do
            {
                switch (this._cursor.Value.Type)
                {
                    case TokenType.LeftParenthesis:
                    case TokenType.LeftBracket:
                    case TokenType.LeftBrace:
                        this.EnterScope();
                        break;
                    case TokenType.RightParenthesis:
                        this.LeaveScope().Apply(l =>
                            this.Current.AddLast(YacqExpression.List(l))
                        );
                        break;
                    case TokenType.RightBracket:
                        this.LeaveScope().Apply(l =>
                            this.Current.AddLast(YacqExpression.Vector(l))
                        );
                        break;
                    case TokenType.RightBrace:
                        this.LeaveScope().Apply(l =>
                            this.Current.AddLast(YacqExpression.LambdaList(l))
                        );
                        break;
                    case TokenType.Period:
                        this.Current.RemoveLast().Apply(l =>
                            this.Current.Hook.Push(_ => this.Current.AddLast(YacqExpression.List(
                                YacqExpression.Identifier("."),
                                l,
                                _.RemoveLast()
                            )))
                        );
                        break;
                    case TokenType.Colon:
                        this.Current.RemoveLast().Apply(l =>
                            this.Current.Hook.Push(_ => this.Current.AddLast(YacqExpression.List(
                                YacqExpression.Identifier(":"),
                                l,
                                _.RemoveLast()
                            )))
                        );
                        break;
                    case TokenType.Identifier:
                        this.Current.AddLast(YacqExpression.Identifier(this._cursor.Value.Text));
                        break;
                    case TokenType.StringLiteral:
                        this.Current.AddLast(YacqExpression.Text(this._cursor.Value.Text));
                        break;
                    case TokenType.NumberLiteral:
                        this.Current.AddLast(YacqExpression.Number(this._cursor.Value.Text));
                        break;
                }
            } while ((this._cursor = this._cursor.Next).Value.Type != TokenType.End);
            return this._stack.Single().List.ToArray();
        }
    }
}