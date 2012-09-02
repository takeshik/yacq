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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using XSpect.Yacq.Serialization;
using XSpect.Yacq.Symbols;

namespace XSpect.Yacq.Expressions
{
    /// <summary>
    /// Represents a serialized expression, an expression that has object graph to reconstruct an expression.
    /// </summary>
    public class SerializedExpression
        : YacqExpression
    {
        private readonly Node _value;

        internal SerializedExpression(
            SymbolTable symbols,
            Node value
        )
            : base(symbols)
        {
            this._value = value;
        }

        /// <summary>
        /// Returns a <see cref="String"/> that represents this expression.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> that represents this expression.
        /// </returns>
        public override String ToString()
        {
            return "(Serialized)";
        }

        /// <summary>
        /// Reduces (means deserialize) this node to a simpler expression with additional symbol tables.
        /// </summary>
        /// <param name="symbols">The additional symbol table for reducing.</param>
        /// <param name="expectedType">The type which is expected as the type of reduced expression.</param>
        /// <returns>The reduced (or deserialized) expression.</returns>
        protected override Expression ReduceImpl(SymbolTable symbols, Type expectedType)
        {
            return this.Deserialize();
        }

        /// <summary>
        /// Gets the deserialized expression of this expression.
        /// </summary>
        /// <returns>The deserialized expression of this expression.</returns>
        public Expression Deserialize()
        {
            return this._value.Deserialize();
        }

        /// <summary>
        /// Saves the object graph which represents this expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public void Save(XmlDictionaryWriter writer)
        {
            new DataContractSerializer(typeof(Node), "expression", Node.Namespace)
                .WriteObject(writer, this._value);
            writer.Flush();
        }

        /// <summary>
        /// Saves the object graph which represents this expression as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public void SaveText(Stream stream)
        {
            this.Save(XmlDictionaryWriter.CreateTextWriter(stream));
        }

        /// <summary>
        /// Saves the object graph which represents this expression as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="file">The file to write the output to.</param>
        public void SaveText(FileInfo file)
        {
            file.OpenWrite()
                .Dispose(this.SaveText);
        }

        /// <summary>
        /// Gets the object graph which represents this expression as Data Contract XML data.
        /// </summary>
        /// <returns>The Data Contract XML data that represents this expression.</returns>
        public String SaveText()
        {
            var data = new MemoryStream()
                .Dispose(this.SaveText)
                .ToArray();
            return Encoding.UTF8.GetString(
                data
#if SILVERLIGHT
                , 0, data.Length
#endif
            );
        }

        /// <summary>
        /// Saves the object graph which represents this expression as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="stream">The destination stream.</param>
        public void SaveBinary(Stream stream)
        {
            this.Save(XmlDictionaryWriter.CreateBinaryWriter(stream));
        }

        /// <summary>
        /// Saves the object graph which represents this expression as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="file">The file to write the output to.</param>
        public void SaveBinary(FileInfo file)
        {
            file.OpenWrite()
                .Dispose(this.SaveBinary);
        }

        /// <summary>
        /// Gets the object graph which represents this expression as Data Contract binary data.
        /// </summary>
        /// <returns>The Data Contract binary data that represents this expression.</returns>
        public Byte[] SaveBinary()
        {
            return new MemoryStream()
                .Dispose(this.SaveBinary)
                .ToArray();
        }
    }

    partial class YacqExpression
    {
        #region Serialize

        /// <summary>
        /// Creates a <see cref="SerializedExpression"/> to get object graph which represents specified expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the object graph which represents specified expression.</returns>
        public static SerializedExpression Serialize(SymbolTable symbols, Expression expression)
        {
            return new SerializedExpression(symbols, Node.Serialize(expression));
        }

        /// <summary>
        /// Creates a <see cref="SerializedExpression"/> to get object graph which represents specified expression.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the object graph which represents specified expression.</returns>
        public static SerializedExpression Serialize(Expression expression)
        {
            return Serialize(null, expression);
        }

        #endregion

        #region Load

        /// <summary>
        /// Loads the object graph and creates a <see cref="SerializedExpression"/> which represents an expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="reader">An <see cref="XmlDictionaryReader"/> used to read the object graph.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression Load(SymbolTable symbols, XmlDictionaryReader reader)
        {
            return new SerializedExpression(
                symbols,
                (Node) new DataContractSerializer(typeof(Node), "expression", Node.Namespace)
                    .ReadObject(reader)
            );
        }

        /// <summary>
        /// Loads the object graph and creates a <see cref="SerializedExpression"/> which represents an expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlDictionaryReader"/> used to read the object graph.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression Load(XmlDictionaryReader reader)
        {
            return Load(null, reader);
        }

        #endregion

        #region LoadText

        /// <summary>
        /// Loads the object graph as Data Contract XML data from an input stream and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadText(SymbolTable symbols, Stream stream)
        {
            return Load(
                symbols,
                XmlDictionaryReader.CreateTextReader(stream, XmlDictionaryReaderQuotas.Max)
            );
        }

        /// <summary>
        /// Loads the object graph as Data Contract XML data from specified file and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadText(SymbolTable symbols, FileInfo file)
        {
            return file.OpenRead()
                .Dispose(s => LoadText(symbols, s));
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract XML data and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="data">The Data Contract XML data to use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadText(SymbolTable symbols, String data)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(data), false)
                .Dispose(s => LoadText(symbols, s));
        }

        /// <summary>
        /// Loads the object graph as Data Contract XML data from an input stream and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadText(Stream stream)
        {
            return LoadText(null, stream);
        }

        /// <summary>
        /// Loads the object graph as Data Contract XML data from specified file and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadText(FileInfo file)
        {
            return LoadText(null, file);
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract XML data and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="data">The Data Contract XML data to use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadText(String data)
        {
            return LoadText(null, data);
        }

        #endregion

        #region LoadBinary

        /// <summary>
        /// Loads the object graph as Data Contract binary data into an output stream and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadBinary(SymbolTable symbols, Stream stream)
        {
            return Load(
                symbols,
                XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max)
            );
        }

        /// <summary>
        /// Loads the object graph as Data Contract binary data from specified file and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadBinary(SymbolTable symbols, FileInfo file)
        {
            return file.OpenRead()
                .Dispose(s => LoadBinary(symbols, s));
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract binary data and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="data">The Data Contract binary data to use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadBinary(SymbolTable symbols, Byte[] data)
        {
            return new MemoryStream(data, false)
                .Dispose(s => LoadBinary(symbols, s));
        }

        /// <summary>
        /// Loads the object graph as Data Contract binary data into an output stream and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadBinary(Stream stream)
        {
            return LoadBinary(null, stream);
        }

        /// <summary>
        /// Loads the object graph as Data Contract binary data from specified file and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadBinary(FileInfo file)
        {
            return LoadBinary(null, file);
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract binary data and creates a <see cref="SerializedExpression"/> which represents an expression.
        /// </summary>
        /// <param name="data">The Data Contract binary data to use as source.</param>
        /// <returns>A <see cref="SerializedExpression"/> that has the loaded object graph and represents an expression.</returns>
        public static SerializedExpression LoadBinary(Byte[] data)
        {
            return LoadBinary(null, data);
        }

        #endregion
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
