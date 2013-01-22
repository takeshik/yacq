// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
// $Id $
/* YACQ Runner
 *   Runner and Compiler frontend of YACQ
 * Copyright © 2011-2013 Takeshi KIRIYA (aka takeshik) <takeshik@yacq.net>
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
 * 
 * This code is originally in: https://github.com/takeshik/cs-util-codes/
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace XSpect
{
    public static class CommandlineParser
    {
        public class CommandlineOption
        {
            public String Id
            {
                get;
                private set;
            }

            public Boolean AllowValue
            {
                get;
                private set;
            }

            public String Description
            {
                get;
                private set;
            }

            public ICollection<Char> ShortNames
            {
                get;
                private set;
            }

            public ICollection<String> LongNames
            {
                get;
                private set;
            }

            internal CommandlineOption(
                String id,
                Boolean allowValue,
                String description,
                IEnumerable<Char> shortNames,
                IEnumerable<String> longNames
            )
            {
                this.Id = id;
                this.AllowValue = allowValue;
                this.Description = description;
                this.ShortNames = shortNames != null
                    ? shortNames.ToArray()
                    : Arrays.Empty<Char>();
                this.LongNames = longNames != null
                    ? longNames.ToArray()
                    : Arrays.Empty<String>();
            }

            public override String ToString()
            {
                return this.Id;
            }
        }

        private static readonly CommandlineOption _invalid = Option("_invalid");

        private static readonly CommandlineOption _parameter = Option("_param");

        public static CommandlineOption Option(
            String id,
            Boolean allowValue = false,
            String description = "",
            IEnumerable<Char> shortNames = null,
            params String[] longNames
        )
        {
            return new CommandlineOption(id, allowValue, description, shortNames, longNames);
        }

        public static ILookup<String, String> Parse(String[] args, params CommandlineOption[] options)
        {
            return options
                .SelectMany(o => o.ShortNames
                    .Select(c => Tuple.Create("-" + c, o))
                    .Concat(o.LongNames.Select(s => Tuple.Create("--" + s, o)))
                )
                .ToDictionary()
                .Let(map => args
                    .TakeWhile(s => s != "--")
                    .SelectMany((a, i) => a.Length > 1 && a[0] == '-' && a[1] != '-'
                        ? a.Substring(1, a.Length - 2)
                              .Select(c => MakeTuple(map, "-" + c, o => null))
                              .EndWith(MakeTuple(map, "-" + a.Last(), o => o.AllowValue ? args[i + 1] : null))
                        : new[] { a.StartsWith("--")
                              ? a.Split(new [] { '=', }, 2).Let(_ =>
                                    MakeTuple(map, _[0], o => _.Length == 2 ? _[1] : null)
                                )
                              : MakeTuple(_parameter, a),
                        }
                    )
                )
                .ToArray()
                .Let(_ => _.Where((t, i) => t.Item1 != _parameter || i == 0 || !_[i - 1].Item1.AllowValue))
                .Concat(args
                    .SkipWhile(s => s != "--")
                    .Skip(1)
                    .Select(a => MakeTuple(_parameter, a))
                )
                .ToLookup(t => t.Item1.Null(o => o.Id), t => t.Item2);
        }

        public static ILookup<String, String> Parse(IEnumerable<String> args, IEnumerable<CommandlineOption> options)
        {
            return Parse(
                args as String[] ?? args.ToArray(),
                options as CommandlineOption[] ?? options.ToArray()
            );
        }

        private static Tuple<CommandlineOption, String> MakeTuple(CommandlineOption option, String value)
        {
            return Tuple.Create(option, value);
        }

        private static Tuple<CommandlineOption, String> MakeTuple(
            IDictionary<String, CommandlineOption> map,
            String key,
            Func<CommandlineOption, String> valueGenerator
        )
        {
            return map.GetValue(key, _invalid).Let(o => MakeTuple(o, o != _invalid ? valueGenerator(o) : key));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
