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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml;
using XSpect.Yacq.Expressions;
using XSpect.Yacq.LanguageServices;
using XSpect.Yacq.Linq;
using XSpect.Yacq.Symbols;
using XSpect.Yacq.SystemObjects;
using System.Reactive.Linq;

namespace XSpect.Yacq
{
    /// <summary>
    /// Provides language service static methods, generate expression trees from code strings with (optional) data.
    /// </summary>
    public static class YacqServices
    {
        /// <summary>
        /// Gets the version of this YACQ library.
        /// </summary>
        /// <returns>The version of this YACQ library.</returns>
        public static Version Version
        {
            get
            {
#if SILVERLIGHT
                return Version.Parse(typeof(YacqServices).Assembly
                    .GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false)
                    .OfType<AssemblyFileVersionAttribute>()
                    .First()
                    .Version
                );
#else
                return typeof(YacqServices).Assembly.GetName().Version;
#endif
            }
        }

        #region ReadAll

        /// <summary>
        /// Read code string and generate expressions without reducing.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read.</param>
        /// <returns>All expressions without reducing, generated from the code.</returns>
        public static YacqExpression[] ReadAll(Reader reader, IEnumerable<Char> code)
        {
            return (reader ?? new Reader()).Read(code);
        }

        /// <summary>
        /// Read code string and generate expressions without reducing.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read.</param>
        /// <returns>All expressions without reducing, generated from the code.</returns>
        public static YacqExpression[] ReadAll(IEnumerable<Char> code)
        {
            return ReadAll(null, code);
        }

        #endregion

        #region Read

        /// <summary>
        /// Read code string and generate expressions without reducing.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read.</param>
        /// <returns>All expressions without reducing, generated from the code.</returns>
        public static YacqExpression Read(Reader reader, IEnumerable<Char> code)
        {
            return ReadAll(reader, code).LastOrDefault();
        }

        /// <summary>
        /// Read code string and generate expressions without reducing.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read.</param>
        /// <returns>All expressions without reducing, generated from the code.</returns>
        public static YacqExpression Read(IEnumerable<Char> code)
        {
            return Read(null, code);
        }

        #endregion

        #region ParseAll

        /// <summary>
        /// Parse code string and generate expressions.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse.</param>
        /// <returns>All expressions generated from the code.</returns>
        public static Expression[] ParseAll(SymbolTable symbols, IEnumerable<Char> code)
        {
            return CreateSymbolTable(symbols).Let(s =>
                ReadAll(s.Resolve("*reader*").Const<Reader>(), code)
#if SILVERLIGHT
                .Cast<Expression>()
#endif
                .ReduceAll(s)
            ).ToArray();
        }

        /// <summary>
        /// Parse code string and generate expressions.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse.</param>
        /// <returns>All expressions generated from the code.</returns>
        public static Expression[] ParseAll(IEnumerable<Char> code)
        {
            return ParseAll(null, code);
        }

        #endregion

        #region Parse

        /// <summary>
        /// Parse code string and generate expressions, only return the last expression.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse.</param>
        /// <returns>The last expressions generated from the code.</returns>
        public static Expression Parse(SymbolTable symbols, IEnumerable<Char> code)
        {
            return ParseAll(symbols, code).Last();
        }

        /// <summary>
        /// Parse code string and generate expressions, only return the last expression.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse.</param>
        /// <returns>The last expressions generated from the code.</returns>
        public static Expression Parse(IEnumerable<Char> code)
        {
            return Parse(null, code);
        }

        #endregion

        #region ParseLambda

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type returnType, IEnumerable<Char> code, params AmbiguousParameterExpression[] parameters)
        {
            symbols = CreateSymbolTable(symbols);
            var expressions = ReadAll(symbols.Resolve("*reader*").Const<Reader>(), code);
            return (LambdaExpression) YacqExpression.AmbiguousLambda(
                symbols,
                returnType,
                expressions.Length == 1
                    ? expressions.Single()
                    : YacqExpression.List(symbols, expressions
#if SILVERLIGHT
                          .Cast<Expression>()
#endif
                      ),
                parameters
            ).Reduce();
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, IEnumerable<Char> code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(symbols, null, code, parameters);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type itType, Type returnType, IEnumerable<Char> code)
        {
            return ParseLambda(symbols, returnType, code, YacqExpression.AmbiguousParameter(itType, "it"));
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(SymbolTable symbols, Type itType, IEnumerable<Char> code)
        {
            return ParseLambda(symbols, itType, null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with specified parameter names.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate that the generating lambda expression represents.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameterNames">Parameter names of the function. Their types are inferred by <typeparamref name="TDelegate"/>.</param>
        /// <returns>The type-explicit lambda expression generated from the code and specified parameter names.</returns>
        public static Expression<TDelegate> ParseLambda<TDelegate>(SymbolTable symbols, IEnumerable<Char> code, params String[] parameterNames)
        {
            return (Expression<TDelegate>) ParseLambda(
                symbols,
                typeof(TDelegate)
                    .GetDelegateSignature()
                    .ReturnType,
                code,
                typeof(TDelegate)
                    .GetDelegateSignature()
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .Zip(parameterNames ?? Arrays.Empty<String>(), YacqExpression.AmbiguousParameter)
                    .ToArray()
            );
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(Type returnType, IEnumerable<Char> code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(null, returnType, code, parameters);
        }

        /// <summary>
        /// Parse code string as the body of the function and generate lambda expression with specified parameters.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameters">Parameters of the function.</param>
        /// <returns>The lambda expressions generated from the code and specified parameters.</returns>
        public static LambdaExpression ParseLambda(IEnumerable<Char> code, params AmbiguousParameterExpression[] parameters)
        {
            return ParseLambda(default(SymbolTable), code, parameters);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="returnType">The return type of this expression; typeof(<see cref="Void"/>) indicates this expression doesn't return value, or <c>null</c> if undetermined.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(Type itType, Type returnType, IEnumerable<Char> code)
        {
            return ParseLambda(null, itType, returnType, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with only one parameter named "it".
        /// </summary>
        /// <param name="itType">The type of "it" parameter.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static LambdaExpression ParseLambda(Type itType, IEnumerable<Char> code)
        {
            return ParseLambda(default(SymbolTable), itType, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with specified parameter names.
        /// </summary>
        /// <typeparam name="TDelegate">The type of the delegate that the generating lambda expression represents.</typeparam>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain specified parameter symbol names.</param>
        /// <param name="parameterNames">Parameter names of the function. Their types are inferred by <typeparamref name="TDelegate"/>.</param>
        /// <returns>The type-explicit lambda expression generated from the code and specified parameter names.</returns>
        public static Expression<TDelegate> ParseLambda<TDelegate>(IEnumerable<Char> code, params String[] parameterNames)
        {
            return ParseLambda<TDelegate>(null, code, parameterNames);
        }

        #endregion

        #region ParseAction

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and no parameters.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Action> ParseAction(SymbolTable symbols, IEnumerable<Char> code)
        {
            return (Expression<Action>) ParseLambda(symbols, typeof(void), code, Arrays.Empty<AmbiguousParameterExpression>());
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Action<T>> ParseAction<T>(SymbolTable symbols, IEnumerable<Char> code)
        {
            return (Expression<Action<T>>) ParseLambda(symbols, typeof(T), typeof(void), code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and no parameters.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Action> ParseAction(IEnumerable<Char> code)
        {
            return ParseAction(null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with no return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Action<T>> ParseAction<T>(IEnumerable<Char> code)
        {
            return ParseAction<T>(null, code);
        }

        #endregion

        #region ParseFunc

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and no parameters.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Func<TReturn>> ParseFunc<TReturn>(SymbolTable symbols, IEnumerable<Char> code)
        {
            return (Expression<Func<TReturn>>) ParseLambda(symbols, typeof(TReturn), code, Arrays.Empty<AmbiguousParameterExpression>());
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Func<T, TReturn>> ParseFunc<T, TReturn>(SymbolTable symbols, IEnumerable<Char> code)
        {
            return (Expression<Func<T, TReturn>>) ParseLambda(symbols, typeof(T), typeof(TReturn), code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and no parameters.
        /// </summary>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="code">Code (character sequence or string) to parse as the body of function.</param>
        /// <returns>The lambda expression generated from the code.</returns>
        public static Expression<Func<TReturn>> ParseFunc<TReturn>(IEnumerable<Char> code)
        {
            return ParseFunc<TReturn>(null, code);
        }

        /// <summary>
        /// Parse code string as the body of function and generate lambda expression with return value and only one parameter named "it".
        /// </summary>
        /// <typeparam name="T">The type of "it" parameter.</typeparam>
        /// <typeparam name="TReturn">The return type of the generating lambda expression.</typeparam>
        /// <param name="code">Code (character sequence or string) to parse as the body of function. The code can contain the parameter symbol name (it).</param>
        /// <returns>The lambda expression generated from the code and the parameter.</returns>
        public static Expression<Func<T, TReturn>> ParseFunc<T, TReturn>(IEnumerable<Char> code)
        {
            return ParseFunc<T, TReturn>(null, code);
        }

        #endregion

        #region Save

        /// <summary>
        /// Saves the object graph which represents specified expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public static void Save(SymbolTable symbols, Expression expression, XmlDictionaryWriter writer)
        {
            YacqExpression.Serialize(symbols, expression).Save(writer);
        }

        /// <summary>
        /// Saves the object graph which represents specified expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public static void Save(Expression expression, XmlDictionaryWriter writer)
        {
            Save(null, expression, writer);
        }

        #endregion

        #region SaveText

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void SaveText(SymbolTable symbols, Expression expression, Stream stream)
        {
            YacqExpression.Serialize(symbols, expression).SaveText(stream);
        }

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void SaveText(SymbolTable symbols, Expression expression, FileInfo file)
        {
            YacqExpression.Serialize(symbols, expression).SaveText(file);
        }

        /// <summary>
        /// Gets the object graph which represents specified expression as Data Contract XML data.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <returns>The Data Contract XML data that represents this expression.</returns>
        public static String SaveText(SymbolTable symbols, Expression expression)
        {
            return YacqExpression.Serialize(symbols, expression).SaveText();
        }

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void SaveText(Expression expression, Stream stream)
        {
            SaveText(null, expression, stream);
        }

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void SaveText(Expression expression, FileInfo file)
        {
            SaveText(null, expression, file);
        }

        /// <summary>
        /// Gets the object graph which represents specified expression as Data Contract XML data.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <returns>The Data Contract XML data that represents this expression.</returns>
        public static String SaveText(Expression expression)
        {
            return SaveText(null, expression);
        }

        #endregion

        #region SaveBinary

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void SaveBinary(SymbolTable symbols, Expression expression, Stream stream)
        {
            YacqExpression.Serialize(symbols, expression).SaveBinary(stream);
        }

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void SaveBinary(SymbolTable symbols, Expression expression, FileInfo file)
        {
            YacqExpression.Serialize(symbols, expression).SaveBinary(file);
        }

        /// <summary>
        /// Gets the object graph which represents specified expression as Data Contract binary data.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="expression">The expression to serialize.</param>
        /// <returns>The Data Contract XML data that represents this expression.</returns>
        public static Byte[] SaveBinary(SymbolTable symbols, Expression expression)
        {
            return YacqExpression.Serialize(symbols, expression).SaveBinary();
        }

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void SaveBinary(Expression expression, Stream stream)
        {
            SaveBinary(null, expression, stream);
        }

        /// <summary>
        /// Saves the object graph which represents specified expression as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void SaveBinary(Expression expression, FileInfo file)
        {
            SaveBinary(null, expression, file);
        }

        /// <summary>
        /// Gets the object graph which represents specified expression as Data Contract binary data.
        /// </summary>
        /// <param name="expression">The expression to serialize.</param>
        /// <returns>The Data Contract XML data that represents this expression.</returns>
        public static Byte[] SaveBinary(Expression expression)
        {
            return SaveBinary(null, expression);
        }

        #endregion

        #region ReadAndSave

        /// <summary>
        /// Saves the object graph which represents the read expression from specified code using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public static void ReadAndSave(Reader reader, IEnumerable<Char> code, XmlDictionaryWriter writer)
        {
            Save(Read(reader, code), writer);
        }

        /// <summary>
        /// Saves the object graph which represents the read expression from specified code using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public static void ReadAndSave(IEnumerable<Char> code, XmlDictionaryWriter writer)
        {
            ReadAndSave(null, code, writer);
        }

        #endregion

        #region ReadAndSaveText

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ReadAndSaveText(Reader reader, IEnumerable<Char> code, Stream stream)
        {
            SaveText(Read(reader, code), stream);
        }

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ReadAndSaveText(Reader reader, IEnumerable<Char> code, FileInfo file)
        {
            SaveText(Read(reader, code), file);
        }

        /// <summary>
        /// Gets the object graph which represents read expression from specified code as Data Contract XML data.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <returns>The Data Contract XML data that represents the parsed expression.</returns>
        public static String ReadAndSaveText(Reader reader, IEnumerable<Char> code)
        {
            return SaveText(Read(reader, code));
        }

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ReadAndSaveText(IEnumerable<Char> code, Stream stream)
        {
            ReadAndSaveText(null, code, stream);
        }

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ReadAndSaveText(IEnumerable<Char> code, FileInfo file)
        {
            ReadAndSaveText(null, code, file);
        }

        /// <summary>
        /// Gets the object graph which represents read expression from specified code as Data Contract XML data.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <returns>The Data Contract XML data that represents the parsed expression.</returns>
        public static String ReadAndSaveText(IEnumerable<Char> code)
        {
            return ReadAndSaveText(null, code);
        }

        #endregion

        #region ReadAndSaveBinary

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ReadAndSaveBinary(Reader reader, IEnumerable<Char> code, Stream stream)
        {
            SaveBinary(Read(reader, code), stream);
        }

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ReadAndSaveBinary(Reader reader, IEnumerable<Char> code, FileInfo file)
        {
            SaveBinary(Read(reader, code), file);
        }

        /// <summary>
        /// Gets the object graph which represents read expression from specified code as Data Contract binary data.
        /// </summary>
        /// <param name="reader">The <see cref="Reader"/> to read the code string.</param>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <returns>The Data Contract binary data that represents the parsed expression.</returns>
        public static Byte[] ReadAndSaveBinary(Reader reader, IEnumerable<Char> code)
        {
            return SaveBinary(Read(reader, code));
        }

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ReadAndSaveBinary(IEnumerable<Char> code, Stream stream)
        {
            ReadAndSaveBinary(null, code, stream);
        }

        /// <summary>
        /// Saves the object graph which represents read expression from specified code as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ReadAndSaveBinary(IEnumerable<Char> code, FileInfo file)
        {
            ReadAndSaveBinary(null, code, file);
        }

        /// <summary>
        /// Gets the object graph which represents read expression from specified code as Data Contract binary data.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to read and serialize.</param>
        /// <returns>The Data Contract binary data that represents the parsed expression.</returns>
        public static Byte[] ReadAndSaveBinary(IEnumerable<Char> code)
        {
            return ReadAndSaveBinary(null, code);
        }

        #endregion

        #region ParseAndSave

        /// <summary>
        /// Saves the object graph which represents the parsed expression from specified code using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public static void ParseAndSave(SymbolTable symbols, IEnumerable<Char> code, XmlDictionaryWriter writer)
        {
            Save(Parse(symbols, code), writer);
        }

        /// <summary>
        /// Saves the object graph which represents the parsed expression from specified code using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="writer">An <see cref="XmlDictionaryWriter"/> used to write the object graph.</param>
        public static void ParseAndSave(IEnumerable<Char> code, XmlDictionaryWriter writer)
        {
            ParseAndSave(null, code, writer);
        }

        #endregion

        #region ParseAndSaveText

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ParseAndSaveText(SymbolTable symbols, IEnumerable<Char> code, Stream stream)
        {
            SaveText(Parse(symbols, code), stream);
        }

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ParseAndSaveText(SymbolTable symbols, IEnumerable<Char> code, FileInfo file)
        {
            SaveText(Parse(symbols, code), file);
        }

        /// <summary>
        /// Gets the object graph which represents parsed expression from specified code as Data Contract XML data.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <returns>The Data Contract XML data that represents the parsed expression.</returns>
        public static String ParseAndSaveText(SymbolTable symbols, IEnumerable<Char> code)
        {
            return SaveText(Parse(symbols, code));
        }

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract XML data into an output stream.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ParseAndSaveText(IEnumerable<Char> code, Stream stream)
        {
            ParseAndSaveText(null, code, stream);
        }

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract XML data to specified file.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ParseAndSaveText(IEnumerable<Char> code, FileInfo file)
        {
            ParseAndSaveText(null, code, file);
        }

        /// <summary>
        /// Gets the object graph which represents parsed expression from specified code as Data Contract XML data.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <returns>The Data Contract XML data that represents the parsed expression.</returns>
        public static String ParseAndSaveText(IEnumerable<Char> code)
        {
            return ParseAndSaveText(null, code);
        }

        #endregion

        #region ParseAndSaveBinary

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ParseAndSaveBinary(SymbolTable symbols, IEnumerable<Char> code, Stream stream)
        {
            SaveBinary(Parse(symbols, code), stream);
        }

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ParseAndSaveBinary(SymbolTable symbols, IEnumerable<Char> code, FileInfo file)
        {
            SaveBinary(Parse(symbols, code), file);
        }

        /// <summary>
        /// Gets the object graph which represents parsed expression from specified code as Data Contract binary data.
        /// </summary>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <returns>The Data Contract binary data that represents the parsed expression.</returns>
        public static Byte[] ParseAndSaveBinary(SymbolTable symbols, IEnumerable<Char> code)
        {
            return SaveBinary(Parse(symbols, code));
        }

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract binary data into an output stream.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="stream">The destination stream.</param>
        public static void ParseAndSaveBinary(IEnumerable<Char> code, Stream stream)
        {
            ParseAndSaveBinary(null, code, stream);
        }

        /// <summary>
        /// Saves the object graph which represents parsed expression from specified code as Data Contract binary data to specified file.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <param name="file">The file to write the output to.</param>
        public static void ParseAndSaveBinary(IEnumerable<Char> code, FileInfo file)
        {
            ParseAndSaveBinary(null, code, file);
        }

        /// <summary>
        /// Gets the object graph which represents parsed expression from specified code as Data Contract binary data.
        /// </summary>
        /// <param name="code">Code (character sequence or string) to parse and serialize.</param>
        /// <returns>The Data Contract binary data that represents the parsed expression.</returns>
        public static Byte[] ParseAndSaveBinary(IEnumerable<Char> code)
        {
            return ParseAndSaveBinary(null, code);
        }

        #endregion

        #region Load

        /// <summary>
        /// Loads the object graph and deserializes to an <see cref="Expression"/> which represents the serialized expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="reader">An <see cref="XmlDictionaryReader"/> used to read the object graph.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression Load(SymbolTable symbols, XmlDictionaryReader reader)
        {
            return YacqExpression.Load(symbols, reader).Deserialize();
        }

        /// <summary>
        /// Loads the object graph and deserializes to an <see cref="Expression"/> which represents the serialized expression using the specified <see cref="XmlDictionaryWriter"/>.
        /// </summary>
        /// <param name="reader">An <see cref="XmlDictionaryReader"/> used to read the object graph.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression Load(XmlDictionaryReader reader)
        {
            return Load(null, reader);
        }

        #endregion

        #region LoadText

        /// <summary>
        /// Loads the object graph as Data Contract XML data from an input stream and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadText(SymbolTable symbols, Stream stream)
        {
            return YacqExpression.LoadText(symbols, stream).Deserialize();
        }

        /// <summary>
        /// Loads the object graph as Data Contract XML data from specified file and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadText(SymbolTable symbols, FileInfo file)
        {
            return YacqExpression.LoadText(symbols, file).Deserialize();
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract XML data and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="data">The Data Contract XML data to use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadText(SymbolTable symbols, String data)
        {
            return YacqExpression.LoadText(symbols, data).Deserialize();
        }

        /// <summary>
        /// Loads the object graph as Data Contract XML data from an input stream and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadText(Stream stream)
        {
            return LoadText(null, stream);
        }

        /// <summary>
        /// Loads the object graph as Data Contract XML data from specified file and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadText(FileInfo file)
        {
            return LoadText(null, file);
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract XML data and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="data">The Data Contract XML data to use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadText(String data)
        {
            return LoadText(null, data);
        }

        #endregion

        #region LoadBinary

        /// <summary>
        /// Loads the object graph as Data Contract binary data from an input stream and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadBinary(SymbolTable symbols, Stream stream)
        {
            return YacqExpression.LoadBinary(symbols, stream).Deserialize();
        }

        /// <summary>
        /// Loads the object graph as Data Contract binary data from specified file and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadBinary(SymbolTable symbols, FileInfo file)
        {
            return YacqExpression.LoadBinary(symbols, file).Deserialize();
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract binary data and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="symbols">The symbol table for the expression.</param>
        /// <param name="data">The Data Contract XML data to use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadBinary(SymbolTable symbols, Byte[] data)
        {
            return YacqExpression.LoadBinary(symbols, data).Deserialize();
        }

        /// <summary>
        /// Loads the object graph as Data Contract binary data from an input stream and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="stream">The stream to load as input.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadBinary(Stream stream)
        {
            return LoadBinary(null, stream);
        }

        /// <summary>
        /// Loads the object graph as Data Contract binary data from specified file and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="file">The file to load and use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadBinary(FileInfo file)
        {
            return LoadBinary(null, file);
        }

        /// <summary>
        /// Loads the object graph as specified Data Contract binary data and deserializes to an <see cref="Expression"/> which represents the serialized expression.
        /// </summary>
        /// <param name="data">The Data Contract XML data to use as source.</param>
        /// <returns>An <see cref="Expression"/> which was deserialized.</returns>
        public static Expression LoadBinary(Byte[] data)
        {
            return LoadBinary(null, data);
        }

        #endregion

        #region Exension Methods

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IEnumerable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IEnumerable source, SymbolTable symbols)
        {
            return new YacqQueryable(symbols, source.AsQueryable());
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IEnumerable<TSource> source, SymbolTable symbols)
        {
            return new YacqQueryable<TSource>(symbols, source.AsQueryable());
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQueryable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IQueryable source, SymbolTable symbols)
        {
            return new YacqQueryable(symbols, source);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IQueryable<TSource> source, SymbolTable symbols)
        {
            return new YacqQueryable<TSource>(symbols, source);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IObservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IObservable<TSource> source, SymbolTable symbols)
        {
            return new YacqQbservable<TSource>(symbols, source.AsQbservable());
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQbservable" /> to convert to a <see cref="YacqQbservable" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQbservable" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable Yacq(this IQbservable source, SymbolTable symbols)
        {
            return new YacqQbservable(symbols, source);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQbservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <param name="symbols">Additional <see cref="SymbolTable"/> for resolve symbols.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IQbservable<TSource> source, SymbolTable symbols)
        {
            return new YacqQbservable<TSource>(symbols, source);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IEnumerable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IEnumerable<TSource> source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQueryable" /> to convert to a <see cref="YacqQueryable" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable Yacq(this IQueryable source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQueryable{TSource}" /> to convert to a <see cref="YacqQueryable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQueryable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQueryable<TSource> Yacq<TSource>(this IQueryable<TSource> source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IObservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IObservable<TSource> source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <param name="source">An <see cref="IQbservable" /> to convert to a <see cref="YacqQbservable" />.</param>
        /// <returns>The source as a <see cref="YacqQbservable" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable Yacq(this IQbservable source)
        {
            return source.Yacq(null);
        }

        /// <summary>
        /// Enables querying with YACQ code strings.
        /// </summary>
        /// <typeparam name="TSource">The type of elements of <paramref name="source" />.</typeparam>
        /// <param name="source">An <see cref="IQbservable{TSource}" /> to convert to a <see cref="YacqQbservable{TSource}" />.</param>
        /// <returns>The source as a <see cref="YacqQbservable{TSource}" /> to access to YACQ query operator methods.</returns>
        public static YacqQbservable<TSource> Yacq<TSource>(this IQbservable<TSource> source)
        {
            return source.Yacq(null);
        }

        #endregion

        private static SymbolTable CreateSymbolTable(SymbolTable symbols)
        {
            return (symbols ?? new SymbolTable())
                .If(s => !s.ExistsKey("$global"),
                    s => s.Add("$global", Expression.Constant(s))
                )
                .If(s => !s.ExistsKey("*reader*"),
                    s => s.Add("*reader*", Expression.Constant(new Reader()))
                )
                .If(s => !s.ExistsKey("*assembly*"),
                    s => s.Add("*assembly*", Expression.Constant(new YacqAssembly("YacqGeneratedTypes")))
                );
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
