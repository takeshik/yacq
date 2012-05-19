// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2012 linerlock <x.linerlock@gmail.com>
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

#pragma warning disable 1591

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    public class ReaderStream
        : Stream<Char>
    {
        private readonly Position _position;

        private readonly String _source;
        
        private Stream<Char> _next;
        
        private Stream<Char> _prev;
        
        private readonly static Regex _regex = new Regex("^(\r\n|\r|\n)");
        
        public override Position Position
        {
            get
            {
                return this._position;
            }
        }

        public String Source
        {
            get
            {
                return this._source;
            }
        }

        public ReaderStream(String source, Position position)
        {
            this._position = position;
            this._source = source;
            this._next = this._prev = null;
        }

        public ReaderStream(String source)
            : this(source, new Position(1, 1, 0))
        {
        }

        ~ReaderStream()
        {
            this.Dispose(false);
        }

        public override Boolean CanNext()
        {
            return this.Source.Length > this.Position.Index;
        }

        public override Boolean CanRewind()
        {
            return this.Position.Index > 0;
        }

        public override Stream<Char> Next()
        {
            if (this.CanNext())
            {
                return _next ?? (_next = new ReaderStream(this.Source,
                    _regex.Match(this.Source, this.Position.Index)
                        .Let(m => this.Position.Let(p => m.Success
                            ? new Position(1, p.Line + 1, p.Index + m.Length)
                            : new Position(p.Column + 1, p.Line, p.Index + 1)
                        ))
                )).Apply(stream => this._prev = stream);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override Stream<Char> Rewind()
        {
            if (this.CanRewind())
            {
                return _prev;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override Char Perform()
        {
            if (this.Source.Length > this.Position.Index)
            {
                return this.Source[this.Position.Index];
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public override Boolean TryGetValue(out Char value)
        {
            if (this.Source.Length > this.Position.Index)
            {
                value = this.Source[this.Position.Index];
                return true;
            }
            else
            {
                value = default(Char);
                return false;
            }
        }

        public override sealed void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(Boolean disposing)
        {
            if (_next != null)
            {
                _next.Dispose();
            }
            if (_prev != null)
            {
                _prev.Dispose();
            }
        }
    }
}

#pragma warning restore 1591
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
