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
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Resources;
using System.Xml.Linq;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.SystemObjects
{
    /// <summary>
    /// Loads YACQ text scripts, binary scripts, and libraries.
    /// </summary>
    public class ModuleLoader
    {
        internal const String LoadedFiles = ".loadedFiles";

        private const String _resourcePrefix = "XSpect.Yacq.Libraries.";

        private readonly Assembly _assembly = typeof(YacqServices).Assembly;

        /// <summary>
        /// Gets the extensions <see cref="ModuleLoader"/> can load.
        /// </summary>
        public static readonly String[] Extensions = new []
        {
            ".dll",
            ".yacb",
            ".yacq",
            "",
        };
            
        /// <summary>
        /// Gets the list to search paths for library file.
        /// </summary>
        /// <value>The list to search paths for library file.</value>
        public IList<DirectoryInfo> SearchPaths
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleLoader"/> class.
        /// </summary>
        /// <param name="searchPaths">An array which contains search paths for library files.</param>
        public ModuleLoader(params DirectoryInfo[] searchPaths)
        {
            this.SearchPaths = new List<DirectoryInfo>(searchPaths);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleLoader"/> class.
        /// </summary>
        public ModuleLoader()
            : this(new DirectoryInfo[0])
        {
        }

        /// <summary>
        /// Load the script and apply to specified <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="symbols">The symbol table as the applying target.</param>
        /// <param name="name">Name of loading script. File extension can be omitted.</param>
        /// <returns>The return value expression of the loaded script.</returns>
        public Expression Load(SymbolTable symbols, String name)
        {
            return this.Get(symbols, name)
                .Null(s => this.Load(symbols, s.Item1, s.Item2))
                ?? Expression.Default(typeof(Object));
        }

        private Expression Load(SymbolTable symbols, Stream stream, String extension)
        {
            switch (extension)
            {
                case ".dll":
                    return Expression.Constant(new Byte[stream.Length].Apply(b => stream.Read(b, 0, b.Length))
                        .Let(Assembly.Load)
                        .Apply(a => a.GetTypes()
                            .Where(t => t.IsPublic)
                            .ForEach(symbols.Import)
                        )
                    );
                case ".yacb":
                    throw new NotImplementedException("YACQ Binary code is not implemented.");
                default:
                    using (var reader = new StreamReader(stream, true))
                    {
                        return YacqServices.Parse(symbols, reader.ReadToEnd());
                    }
            }
        }

        private Tuple<Stream, String> Get(SymbolTable symbols, String name)
        {
            var l = symbols[LoadedFiles].Const<IList<String>>();
            return this.SearchPaths
                .Where(d => d.Exists)
                .SelectMany(d => Extensions
                    .SelectMany(_ => d.EnumerateFiles(name + _))
                 )
                .FirstOrDefault(f => f.Exists)
                .Null(f => l.Contains(f.FullName)
                    ? null
                    : Tuple.Create(
                          (Stream) f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                              .Apply(_ => l.Add(f.FullName)),
                          f.Extension
                      )
                )
                    ?? _assembly.GetManifestResourceNames()
                        .Select(n => n.Substring(_resourcePrefix.Length))
                        .Where(n => Path.GetFileNameWithoutExtension(n) == name)
                        .OrderBy(n => Array.IndexOf(Extensions, Path.GetExtension(n))
                            .Let(i => i < 0 ? Int32.MaxValue : i)
                        )
                        .FirstOrDefault()
                        .Null(n => l.Contains("res:" + n)
                            ? null
                            : Tuple.Create(
                                  _assembly.GetManifestResourceStream(_resourcePrefix
                                      + n.Apply(l.Add)
                                  ),
                                  Path.GetExtension(n)
                              )
                        );
        }
    }
}