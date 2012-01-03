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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using XSpect.Yacq.Runner.Model;

namespace XSpect.Yacq.Runner.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm/getstarted
    /// </para>
    /// </summary>
    public class CodeViewModel : ViewModelBase
    {
        private readonly Code _model;

        public String Id
        {
            get
            {
                return this._model.Id;
            }
            set
            {
                this._model.Id = value;
                this.RaisePropertyChanged("Id");
            }
        }

        public String Title
        {
            get
            {
                return this._model.Title;
            }
            set
            {
                this._model.Title = value;
                this.RaisePropertyChanged("Title");
            }
        }

        public String Description
        {
            get
            {
                return this._model.Description.Value;
            }
            set
            {
                this._model.Description = new Lazy<String>(() => value);
                this.RaisePropertyChanged("Description");
            }
        }

        public String Body
        {
            get
            {
                return this._model.Body;
            }
            set
            {
                this._model.Body = value;
                this.RaisePropertyChanged("Body");
            }
        }

        public String Output
        {
            get
            {
                return this._model.Output;
            }
            set
            {
                this._model.Output = value;
                this.RaisePropertyChanged("Output");
            }
        }

        public RelayCommand Run
        {
            get;
            private set;
        }

        public RelayCommand Reset
        {
            get;
            private set;
        }

        public CodeViewModel(Code code)
        {
            this._model = code;
            this._model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Output" || e.PropertyName == "Body")
                {
                    DispatcherHelper.UIDispatcher.BeginInvoke(() =>
                        this.RaisePropertyChanged(e.PropertyName)
                    );
                }
            };
            this.Run = new RelayCommand(() => this._model.Run(), () => !String.IsNullOrWhiteSpace(this.Body));
            this.Reset = new RelayCommand(() => this._model.Reset());
        }
    }
}