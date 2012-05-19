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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.SystemObjects
{
    /// <summary>
    /// Represents a XML document file to search by its name, member, or expression.
    /// </summary>
    public class DocumentSet
    {
        private readonly XDocument _xml;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSet"/> class.
        /// </summary>
        /// <param name="xmlFile">The XML file to read.</param>
        public DocumentSet(FileInfo xmlFile)
        {
            this._xml = XDocument.Load(xmlFile.FullName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSet"/> class.
        /// </summary>
        /// <param name="stream">The stream to read the XML file.</param>
        public DocumentSet(Stream stream)
        {
            using (stream)
            {
                this._xml = XDocument.Load(stream);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSet"/> class.
        /// </summary>
        /// <param name="xml">The XML string to read.</param>
        public DocumentSet(String xml)
        {
            this._xml = XDocument.Parse(xml);
        }

        /// <summary>
        /// Gets the document which is related with specified member.
        /// </summary>
        /// <param name="member">The member to get the related document.</param>
        /// <returns>The document XML elements which is related with <paramref name="member"/>.</returns>
        public XElement[] GetDocument(MemberInfo member)
        {
            return this.GetDocument(GetXmlDocumentName(member));
        }

        /// <summary>
        /// Gets the document which has specified name.
        /// </summary>
        /// <param name="name">The document name to search.</param>
        /// <returns>The document XML elements which is named as <paramref name="name"/>.</returns>
        public XElement[] GetDocument(String name)
        {
            return this._xml.Descendants("member")
                .Concat(this._xml.Descendants("article"))
                .FirstOrDefault(xm => xm.Attribute("name").Value == name)
                .Elements()
                .ToArray();
        }

        /// <summary>
        /// Gets the formatted string which represents specified member in name attribute of XML code documents.
        /// </summary>
        /// <param name="member">Member to get the name.</param>
        /// <returns>The formatted string which represents specified member in name attribute of XML code documents.</returns>
        public static String GetXmlDocumentName(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    {
                        var method = (MethodBase) member;
                        return "M:" + Format(method.DeclaringType, true)
                            + "."
                            + method.Name.Replace('.', '#')
                            + (method.IsGenericMethod
                                  ? "``" + method.GetGenericArguments().Length
                                  : ""
                              )
                            + (method.GetParameters().Any()
                                  ? "("
                                        + String.Join(",", method.GetParameters()
                                              .Select(p => Format(p.ParameterType, false))
                                          )
                                        + ")"
                                  : ""
                              );
                    }
                case MemberTypes.Event:
                    {
                        return "E:" + Format(member.DeclaringType, true) + "." + member.Name;
                    }
                case MemberTypes.Field:
                    {
                        return "F:" + Format(member.DeclaringType, true) + "." + member.Name;
                    }
                case MemberTypes.Property:
                    {
                        var property = (PropertyInfo) member;
                        return "P:" + Format(property.DeclaringType, true)
                            + "."
                            + property.Name
                            + (property.GetIndexParameters().Any()
                                  ? ("("
                                        + String.Join(",", property.GetIndexParameters().Select(p => Format(p.ParameterType, false)))
                                        + ")"
                                    )
                                  : ""
                              );
                    }
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    {
                        var type = (Type) member;
                        return "T:" + Format(type, true);
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the formatted string which represents specified symbol entry in name attribute of XML code documents.
        /// </summary>
        /// <param name="key">Symbol entry to get the name.</param>
        /// <returns>The formatted string which represents specified symbol entry in name attribute of XML code documents.</returns>
        public static String GetXmlDocumentName(SymbolEntry key)
        {
            return "Y:" +
                (key.LeftType != null
                    ? key.LeftType.TryGetGenericTypeDefinition() == typeof(Static<>)
                          ? "[" + GetXmlDocumentName(key.LeftType.GetGenericArguments()[0]).Substring(2) + "]."
                          : GetXmlDocumentName(key.LeftType).Substring(2) + "."
                    : ""
                ) +
                (key.DispatchType.HasFlag(DispatchTypes.Method)
                    ? "(" + key.Name + ")"
                    : key.Name
                );
        }

        private static String Format(Type type, Boolean getDefinition)
        {
            if (type.IsGenericParameter)
            {
                return (type.DeclaringMethod != null
                    ? "``"
                    : "`"
                ) + type.GenericParameterPosition;
            }
            else if (type.IsGenericType)
            {
                var d = type.GetGenericTypeDefinition().FullName.Replace('+', '.');
                return getDefinition
                    ? d
                    : d.Remove(d.LastIndexOf('`'))
                          + "{"
                          + String.Join(",", type.GetGenericArguments().Select(t => Format(t, false)))
                          + "}";
            }
            else if (type.IsArray)
            {
                return Format(type.GetElementType(), getDefinition) + "[]";
            }
            else if (type.IsByRef)
            {
                return Format(type.GetElementType(), getDefinition) + "@";
            }
            else
            {
                return type.FullName.Replace('+', '.');
            }
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
