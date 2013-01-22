// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id$
/* YACQ <http://yacq.net/>
 *   Yet Another Compilable Query Language, based on Expression Trees API
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
using System.Reflection;
using System.Xml.Linq;

namespace XSpect.Yacq.SystemObjects
{
    /// <summary>
    /// Provides management system for XML document files.
    /// </summary>
    public class DocumentRepository
    {
        /// <summary>
        /// Gets the collection of loaded <see cref="DocumentSet"/>.
        /// </summary>
        /// <value>The collection of loaded <see cref="DocumentSet"/>.</value>
        public IDictionary<String, DocumentSet> DocumentSets
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the list to search paths for document XML file.
        /// </summary>
        /// <value>The list to search paths for document XML file.</value>
        public IList<DirectoryInfo> SearchPaths
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRepository"/> class.
        /// </summary>
        /// <param name="searchPaths">An array which contains search paths for document XML files.</param>
        public DocumentRepository(params DirectoryInfo[] searchPaths)
        {
            this.DocumentSets = new Dictionary<String, DocumentSet>();
            this.SearchPaths = new List<DirectoryInfo>(searchPaths);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentRepository"/> class.
        /// </summary>
        public DocumentRepository()
            : this(Arrays.Empty<DirectoryInfo>())
        {
        }

        /// <summary>
        /// Gets the document which has specified name.
        /// </summary>
        /// <param name="name">The full name of the document (documentSetName/documentKey).</param>
        /// <returns>The document XML elements which has specified name.</returns>
        public XElement[] GetDocument(String name)
        {
            return name.Split('/')
                .Let(_ => this.LoadDocumentSet(_[0])
                    .Null(s => s.GetDocument(_[1])
                )
            );
        }

        /// <summary>
        /// Gets the document which is related with specified member.
        /// </summary>
        /// <param name="member">The member to get the related document.</param>
        /// <returns>The document XML elements which is related with <paramref name="member"/>.</returns>
        public XElement[] GetDocument(MemberInfo member)
        {
            return this.LoadDocumentSet(member)
                .Null(s => s.GetDocument(member));
        }

        /// <summary>
        /// Gets the document which is related with specified expression.
        /// </summary>
        /// <param name="expression">The expression to get the related document.</param>
        /// <returns>The document XML elements which is related with <paramref name="expression"/>.</returns>
        public XElement[] GetDocument(Expression expression)
        {
            return expression is LambdaExpression
                ? GetDocument(((LambdaExpression) expression).Body)
                : expression is MemberExpression
                      ? GetDocument(((MemberExpression) expression).Member)
                      : expression is MethodCallExpression
                            ? GetDocument(((MethodCallExpression) expression).Method)
                            : null;
        }

        private DocumentSet LoadDocumentSet(String key)
        {
            return this.DocumentSets.GetValue(key)
                ?? (this.SearchPaths
                       .Where(d => d.Exists)
                       .SelectMany(d => d.EnumerateFiles(key + ".xml"))
                       .FirstOrDefault()
                       .Null(f => new DocumentSet(f))
                   ).Apply(s => this.DocumentSets.Add(key, s));
        }

        private DocumentSet LoadDocumentSet(MemberInfo member)
        {
            return this.LoadDocumentSet((member is Type
                ? (Type) member
                : member.DeclaringType
#if SILVERLIGHT
            ).Assembly.FullName.Split(',')[0]);
#else
            ).Assembly.GetName().Name);
#endif
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
