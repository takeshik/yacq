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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.Linq;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using XSpect.Yacq.Runner.Model;

namespace XSpect.Yacq.Runner.ViewModel
{
    public class CodeCollectionViewModel
        : ObservableCollection<CodeViewModel>
    {
        private readonly CodeCollection _model;

        public RelayCommand Fetch
        {
            get;
            private set;
        }

        public CodeCollectionViewModel(Boolean isInDesignMode)
        {
            this._model = new CodeCollection()
            {
                new Code()
                {
                    Id = "default",
                    Title = "(Default)",
                    Description = new Lazy<String>(() => "Default empty code."),
                    OriginalBody = new Lazy<String>(() => ""),
                    Output = "YACQ Silverlight Runner (version " + YacqServices.Version + ")",
                }
            };
            this._model.ForEach(c => this.Add(new CodeViewModel(c)));
            this._model.CollectionChanged += (s, e) =>
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        if (!isInDesignMode)
                        {
                            DispatcherHelper.UIDispatcher.BeginInvoke(() =>
                                this.Add(new CodeViewModel(e.NewItems.OfType<Code>().Single()))
                            );
                        }
                        break;
                }
            };
            this.Fetch = new RelayCommand(() => this._model.Fetch(), () => true);
        }
    }
}