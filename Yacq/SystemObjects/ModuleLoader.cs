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
using System.Reflection;
using System.Text.RegularExpressions;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.Symbols;

#if SILVERLIGHT
using System.Reactive.Linq;
using System.Xml.Linq;
#endif

namespace XSpect.Yacq.SystemObjects
{
    /// <summary>
    /// Loads YACQ text scripts, binary scripts, and libraries.
    /// </summary>
    public class ModuleLoader
    {
        /// <summary>
        /// Represents the prefix to load symbols from the filesystem. This field is constant.
        /// </summary>
        public const String FilePrefix = "file:";

        /// <summary>
        /// Represents the prefix to load symbols from Yacq assembly resources. This field is constant.
        /// </summary>
        public const String ResourcePrefix = "res:";

        /// <summary>
        /// Represents the prefix to load symbols from the CTS (Common Type System) tree (namespace import). This field is constant.
        /// </summary>
        public const String CtsPrefix = "cts:";

        internal const String LoadedModules = ".loadedModules";

        private const String _resourcePrefix = "XSpect.Yacq.Libraries.";

        private readonly Assembly _assembly = typeof(YacqServices).Assembly;

#if SILVERLIGHT
        internal static readonly Assembly[] Assemblies = new Type[]
        {
            typeof(Object),
            typeof(Uri),
            typeof(Enumerable),
            typeof(EnumerableEx),
            typeof(QueryableEx),
            typeof(Observable),
            typeof(Qbservable),
            typeof(XDocument),
            typeof(YacqServices),
        }
            .Select(t => t.Assembly)
            .ToArray();
#endif

        private static readonly String[] _extensions = new []
        {
            ".dll",
            ".yacb",
            ".yacq",
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
        /// Loads the file and apply to specified <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="symbols">The symbol table as the applying target.</param>
        /// <param name="name">Name of loading file. File extension can be omitted.</param>
        /// <returns>The return value expression of the loaded file.</returns>
        public Expression Load(SymbolTable symbols, String name)
        {
            return this.Get(symbols, name)
                .Null(s => Load(symbols, s.Item1, s.Item2))
                ?? Expression.Default(typeof(Object));
        }

        /// <summary>
        /// Adds symbols in specified <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="symbols">The symbol table as the adding target.</param>
        /// <param name="sourceSymbols">The symbol table as the source to add.</param>
        /// <returns><see cref="Expression.Empty()"/>.</returns>
        public Expression Load(SymbolTable symbols, SymbolTable sourceSymbols)
        {
            sourceSymbols.ForEach(p => symbols[p.Key] = p.Value);
            return Expression.Empty();
        }

        /// <summary>
        /// Loads the file, apply to new <see cref="SymbolTable"/> and add the symbol with specified name, which refers to it in specified <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="symbols">The symbol table to add the reference to the applied symbols.</param>
        /// <param name="name">Name of loading file. File extension can be omitted.</param>
        /// <param name="symbolName">Name of the symbol which refers to the applied symbols.</param>
        /// <returns>The return value expression of the loaded file.</returns>
        public Expression Import(SymbolTable symbols, String name, String symbolName)
        {
            return this.Get(symbols, name)
                .Null(_ => CreatePathSymbols(
                    symbols,
                    (symbolName.Contains(":")
                        ? symbolName.Substring(symbolName.IndexOf(':') + 1)
                        : symbolName
                    ).Split('.')
                ).Let(s => Load(s, _.Item1, _.Item2)));
        }

        /// <summary>
        /// Loads the file, apply to new <see cref="SymbolTable"/> and add the symbol named as file name, which refers to it in specified <see cref="SymbolTable"/>.
        /// </summary>
        /// <param name="symbols">The symbol table to add the reference to the applied symbols.</param>
        /// <param name="name">Name of loading file. File extension can be omitted.</param>
        /// <returns>The return value expression of the loaded file.</returns>
        public Expression Import(SymbolTable symbols, String name)
        {
            return this.Import(symbols, name, name);
        }

        private Tuple<Object, String> Get(SymbolTable symbols, String name)
        {
            if (!symbols.ContainsKey(LoadedModules))
            {
                symbols.Add(LoadedModules, Expression.Constant(new HashSet<String>(StringComparer.CurrentCultureIgnoreCase)));
            }
            switch (Regex.Match(name, @"(^[^:]+:)").Value)
            {
                case FilePrefix:
                    return GetFromFile(symbols, name.Substring(5));
                case ResourcePrefix:
                    return GetFromResource(symbols, name.Substring(4));
                case CtsPrefix:
                    return GetFromNamespace(symbols, name.Substring(4));
                default:
                    return GetFromFile(symbols, name)
                        ?? GetFromResource(symbols, name);
            }
        }

        private Tuple<Object, String> GetFromFile(SymbolTable symbols, String name)
        {
            var l = symbols[LoadedModules].Const<ICollection<String>>();
            return this.SearchPaths
                .Where(d => d.Exists)
                .SelectMany(d => _extensions
                    .SelectMany(_ => d.EnumerateFiles(name.Replace('.', Path.DirectorySeparatorChar) + _))
                 )
                .FirstOrDefault(f => f.Exists)
                .Null(f => l.Contains(FilePrefix + f.FullName)
                    ? null
                    : Tuple.Create(
                          (Object) f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                          (FilePrefix + f.FullName).Apply(l.Add)
                      )
                );
        }

        private Tuple<Object, String> GetFromResource(SymbolTable symbols, String name)
        {
            var l = symbols[LoadedModules].Const<ICollection<String>>();
            return _assembly.GetManifestResourceNames()
                .Select(n => n.Substring(_resourcePrefix.Length))
                .Where(n => Path.GetFileNameWithoutExtension(n) == name)
                .OrderBy(n => Array.IndexOf(_extensions, Path.GetExtension(n))
                    .Let(i => i < 0 ? Int32.MaxValue : i)
                )
                .FirstOrDefault()
                .Null(n => l.Contains(ResourcePrefix + n)
                    ? null
                    : Tuple.Create(
                          (Object) _assembly.GetManifestResourceStream(_resourcePrefix + n),
                          (ResourcePrefix + n).Apply(l.Add)
                      )
                );
        }

        private static Tuple<Object, String> GetFromNamespace(SymbolTable symbols, String name)
        {
            var l = symbols[LoadedModules].Const<ICollection<String>>();
            return Tuple.Create(
                (Object)
#if SILVERLIGHT
                Assemblies
#else
                AppDomain.CurrentDomain
                    .GetAssemblies()
#endif
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.IsPublic && !t.IsNested && t.Namespace == name)
                    .GroupBy(t => t.Name.Contains("`")
                        ? t.Name.Remove(t.Name.IndexOf('`'))
                        : t.Name
                    )
                    .ToArray(),
                (CtsPrefix + name).Apply(l.Add)
            );
        }

        private static Expression Load(SymbolTable symbols, Object obj, String name)
        {
            if (name.StartsWithInvariant(CtsPrefix))
            {
                return Expression.Constant(((IEnumerable<IGrouping<String, Type>>) obj)
                    .Select(g => g.Key.Apply(k => symbols[k] = YacqExpression.TypeCandidate(symbols, g)))
                    .ToArray()
                );
            }
            else
            {
                var stream = (Stream) obj;
                switch (Path.GetExtension(name).ToLowerInvariant())
                {
                    case ".dll":
#if SILVERLIGHT
                        throw new NotImplementedException("Loading DLL is not implemented in Sliverlight environment.");
#else
                        return Expression.Constant(new Byte[stream.Length].Apply(b => stream.Read(b, 0, b.Length))
                            .Let(Assembly.Load)
                            .Apply(a => a.GetTypes()
                                .Where(t => t.IsPublic)
                                .ForEach(symbols.Import)
                            )
                        );
#endif
                    case ".yacb":
                        throw new NotImplementedException("YACQ Binary code is not implemented.");
                    default:
                        return new StreamReader(stream, true)
                            .Dispose(r => YacqServices.Parse(symbols, r.ReadToEnd()));
                }
            }
        }

        internal static SymbolTable CreatePathSymbols(SymbolTable symbols, IEnumerable<String> fragments)
        {
            return ((SymbolTableExpression) EnumerableEx.Generate(
                Tuple.Create(fragments, symbols.ResolveModule()),
                _ => _.Item1.Any(),
                _ => Tuple.Create(_.Item1.Skip(1), _.Item1.First()
                    .Let(f => _.Item2.ExistsKey(f) && _.Item2.Resolve(f) is SymbolTableExpression
                        ? ((SymbolTableExpression) _.Item2.Resolve(f)).Symbols
                        : new SymbolTable().Apply(
                              s => s.MarkAsModule(),
                              s => _.Item2[f] = YacqExpression.SymbolTable(s)
                          )
                )),
                _ => _.Item2
            ).Last().Resolve(fragments.Last())).Symbols;
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
