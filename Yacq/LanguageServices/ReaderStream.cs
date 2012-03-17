using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Parseq;

namespace XSpect.Yacq.LanguageServices
{
    public class ReaderStream
        : Stream<Char>
    {
        public override Position Position {
            get { return this._position; }
        }

        public String Source {
            get { return this._source; }
        }

        public ReaderStream(String source, Position position){
            this._position = position;
            this._source = source;
            this._next = this._prev = null;
        }

        public ReaderStream(String source)
            : this(source, new Position(1, 1, 0))
        {

        }

        public override Boolean CanNext() {
            return this.Source.Length > this.Position.Index;
        }

        public override Boolean CanRewind(){
            return this.Position.Index > 0;
        }

        public override Stream<Char> Next(){
            if (this.CanNext()){
                return _next ?? (_next = new ReaderStream(this.Source,
                    _regex.Match(this.Source, this.Position.Index)
                        .Let(m => this.Position.Let(p => m.Success
                            ? new Position(1, p.Line + 1, p.Index + m.Length)
                            : new Position(p.Column + 1, p.Line, p.Index + 1)))))
                                .Apply(stream => this._prev = stream);
            }
            else
                throw new InvalidOperationException();
        }

        public override Stream<Char> Rewind(){
            if (this.CanRewind())
                return _prev;
            else
                throw new InvalidOperationException();
        }

        public override Char Perform(){
            if (this.Source.Length > this.Position.Index)
                return this.Source[this.Position.Index];
            else
                throw new InvalidOperationException();
        }

        public override Boolean TryGetValue(out Char value){
            if (this.Source.Length > this.Position.Index){
                value = this.Source[this.Position.Index];
                return true;
            }
            else {
                value = default(Char);
                return false;
            }
        }

        public override void Dispose(){
            if (_next != null)
                _next.Dispose();
            if (_prev != null)
                _prev.Dispose();
        }

        private readonly Position _position;
        private readonly String _source;
        private Stream<Char> _next;
        private Stream<Char> _prev;
        private readonly static Regex _regex = new Regex("^(\r\n|\r|\n)");
    }
}
