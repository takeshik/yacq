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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace XSpect.Yacq
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
            return this._xml
                .Descendants("member")
                .FirstOrDefault(xm => xm.Attribute("name").Value == name)
                .Elements()
                .ToArray();
        }

        private static String GetXmlDocumentName(MemberInfo member)
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
                            + "("
                            + String.Join(",", method.GetParameters().Select(p => Format(p.ParameterType, false)))
                            + ")";
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
                var d = type.GetGenericTypeDefinition().FullName;
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
                return type.FullName;
            }
        }
    }
}