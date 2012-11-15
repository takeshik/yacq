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
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace XSpect.Yacq.Serialization
{
    [DataContract(Name = "Event")]
    internal class EventRef
        : MemberRef
    {
        private static readonly Dictionary<EventRef, EventInfo> _cache
            = new Dictionary<EventRef, EventInfo>();

        private static readonly Dictionary<EventInfo, EventRef> _reverseCache
            = new Dictionary<EventInfo, EventRef>();

        public static EventRef Serialize(EventInfo @event)
        {
            return _reverseCache.TryGetValue(@event)
                ?? new EventRef()
                   {
                       Type = TypeRef.Serialize(@event.ReflectedType),
                       Name = @event.Name,
                   }.Apply(e => _reverseCache.Add(@event, e));
        }

        public new EventInfo Deserialize()
        {
            return _cache.TryGetValue(this)
                ?? this.Type.Deserialize()
                       .GetEvent(this.Name, Binding)
                       .Apply(e => _cache.Add(this, e));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
