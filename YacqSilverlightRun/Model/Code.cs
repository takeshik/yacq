// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
// $Id$
/* YACQ Runner
 *   Runner and Compiler frontend of YACQ
 * Copyright © 2011 Takeshi KIRIYA (aka takeshik) <takeshik@users.sf.net>
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
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using XSpect.Yacq.Expressions;

namespace XSpect.Yacq.Runner.Model
{
    public class Code
        : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private String _id;

        public String Id
        {
            get
            {
                return this._id;
            }
            set
            {
                this._id = value;
                this.OnPropertyChanged("Id");
            }
        }

        private String _title;

        public String Title
        {
            get
            {
                return this._title;
            }
            set
            {
                this._title = value;
                this.OnPropertyChanged("Title");
            }
        }

        private Lazy<String> _description;

        public Lazy<String> Description
        {
            get
            {
                return this._description;
            }
            set
            {
                this._description = value;
                this.OnPropertyChanged("Description");
            }
        }

        private String _body;

        public String Body
        {
            get
            {
                return this._body ?? this.OriginalBody.Value;
            }
            set
            {
                this._body = value;
                this.OnPropertyChanged("Body");
            }
        }

        private Lazy<String> _originalBody;

        public Lazy<String> OriginalBody
        {
            get
            {
                return this._originalBody;
            }
            set
            {
                this._originalBody = value;
                this.OnPropertyChanged("OriginalBody");
            }
        }

        private String _output;

        public String Output
        {
            get
            {
                return this._output;
            }
            set
            {
                this._output = value;
                this.OnPropertyChanged("Output");
            }
        }

        public void Run()
        {
            this.Output = "";
            Observable.Start(() =>
            {
                try
                {
                    this.WriteOutput("Started.");
                    var expr = YacqServices.Parse(
                        new SymbolTable()
                        {
                            {DispatchTypes.Method, typeof(Object), "print", (e, s, t) =>
                                YacqExpression.Dispatch(
                                    s,
                                    DispatchTypes.Method,
                                    Expression.Constant(this),
                                    "Print",
                                    e.Left
                                )
                            },
                        },
                        this.Body
                    );
                    this.WriteOutput("Parsed.\nGenerated Expression:\n" + expr);
                    var func = Expression.Lambda(expr).Compile();
                    this.WriteOutput("Compiled.");
                    var ret = func.DynamicInvoke();
                    this.WriteOutput("Finished.\nReturned Type: " + (ret != null ? ret.GetType().Name : "null"));
                    if (ret != null)
                    {
                        this.Output += "Returned Value:\n" + (ret is IEnumerable && !(ret is String)
                            ? String.Join(", ", ((IEnumerable) ret).OfType<Object>().Select(e => e.ToString()))
                            : ret
                        );
                    }
                }
                catch (Exception ex)
                {
                    this.WriteOutput(ex.ToString());
                }
            }).Subscribe(_ =>
            {
            });
        }

        public void Reset()
        {
            this.Body = this.OriginalBody.Value;
            this.Output = "";
        }

        public void Print(Object obj)
        {
            
            this.Output += obj + "\n";
        }

        private void WriteOutput(String str)
        {
            this.Output += String.Format("[{0:HH:mm:ss.fff}] {1}\n", DateTime.Now, str);
        }

        private void OnPropertyChanged(String propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}