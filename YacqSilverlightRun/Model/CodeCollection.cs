// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ Runner
 *   Runner and Compiler frontend of YACQ
 * Copyright © 2011-2012 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
 * All rights reserved.
 * 
 * This file is part of YACQ Runner.
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
using System.Collections.ObjectModel;
using System.Net;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Xml.Linq;
using AsynchronousExtensions;

namespace XSpect.Yacq.Runner.Model
{
    public class CodeCollection
        : ObservableCollection<Code>
    {
        public void Fetch()
        {
            var sourceUri = new Uri(Application.Current.Host.Source.AbsoluteUri);
            var sourceBase =
                "http://" +
                sourceUri.Host +
                String.Join("/", sourceUri.AbsolutePath.Split('/').SkipLast(1)) +
                "/";
            var query = "?_=" + (DateTime.UtcNow - new DateTime(1970, 1, 1)).Seconds;
            WebRequest.CreateHttp(sourceBase + "codes.xml" + query)
                .DownloadStringAsync()
                .Subscribe(s => XDocument.Parse(s).Root
                    .Elements("code")
                    .Select(xc =>
                        xc.Attribute("id").Value.Let(id =>
                        new Code()
                        {
                            Id = id,
                            Title = xc.Attribute("title").Value,
                            Description = xc.Element("desc").Let(x =>
                                new Lazy<String>(String.IsNullOrEmpty(x.Value) && x.Attribute("see") != null
                                    ? ((Func<String>) (() => WebRequest.Create(sourceBase + id + ".desc" + query)
                                          .DownloadStringAsync()
                                          .First()
                                      ))
                                    : () => x.Value
                                )
                            ),
                            OriginalBody = xc.Element("body").Let(x =>
                                new Lazy<String>(String.IsNullOrEmpty(x.Value) && x.Attribute("see") != null
                                    ? ((Func<String>) (() => WebRequest.Create(sourceBase + id + ".yacq" + query)
                                          .DownloadStringAsync()
                                          .First()
                                      ))
                                    : () => x.Value
                                )
                            ),
                        })
                    )
                    .ForEach(this.Add)
                );
        }
    }
}