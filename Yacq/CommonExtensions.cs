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
 * 
 * This code is originally in: https://github.com/takeshik/common-exts.cs/
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace XSpect.Yacq
{
    [DebuggerStepThrough()]
    internal static class CommonExtensions
    {
        #region Let

        internal static TReturn Let<TReceiver, TReturn>(
            this TReceiver self,
            Func<TReceiver, TReturn> func
        )
        {
            return func(self);
        }

        internal static TReturn Let<T1, T2, TReturn>(
            this Tuple<T1, T2> tuple,
            Func<T1, T2, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2);
        }

        internal static TReturn Let<T1, T2, T3, TReturn>(
            this Tuple<T1, T2, T3> tuple,
            Func<T1, T2, T3, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2, tuple.Item3);
        }

        internal static TReturn Let<T1, T2, T3, T4, TReturn>(
            this Tuple<T1, T2, T3, T4> tuple,
            Func<T1, T2, T3, T4, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        }

        internal static TReturn Let<T1, T2, T3, T4, T5, TReturn>(
            this Tuple<T1, T2, T3, T4, T5> tuple,
            Func<T1, T2, T3, T4, T5, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5);
        }

        internal static TReturn Let<T1, T2, T3, T4, T5, T6, TReturn>(
            this Tuple<T1, T2, T3, T4, T5, T6> tuple,
            Func<T1, T2, T3, T4, T5, T6, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6);
        }

        internal static TReturn Let<T1, T2, T3, T4, T5, T6, T7, TReturn>(
            this Tuple<T1, T2, T3, T4, T5, T6, T7> tuple,
            Func<T1, T2, T3, T4, T5, T6, T7, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7);
        }

        internal static TReturn Let<T1, T2, T3, T4, T5, T6, T7, TRest, TReturn>(
            this Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple,
            Func<T1, T2, T3, T4, T5, T6, T7, TRest, TReturn> func
        )
        {
            return func(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7, tuple.Rest);
        }

        #endregion

        #region Apply

        internal static TReceiver Apply<TReceiver>(
            this TReceiver self,
            params Action<TReceiver>[] actions
        )
        {
            Array.ForEach(actions, a => a(self));
            return self;
        }

        internal static Tuple<T1, T2> Apply<T1, T2>(
            this Tuple<T1, T2> tuple,
            params Action<T1, T2>[] actions)
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2));
            return tuple;
        }

        internal static Tuple<T1, T2, T3> Apply<T1, T2, T3>(
            this Tuple<T1, T2, T3> tuple,
            params Action<T1, T2, T3>[] actions
        )
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2, tuple.Item3));
            return tuple;
        }

        internal static Tuple<T1, T2, T3, T4> Apply<T1, T2, T3, T4>(
            this Tuple<T1, T2, T3, T4> tuple,
            params Action<T1, T2, T3, T4>[] actions
        )
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4));
            return tuple;
        }

        internal static Tuple<T1, T2, T3, T4, T5> Apply<T1, T2, T3, T4, T5>(
            this Tuple<T1, T2, T3, T4, T5> tuple,
            params Action<T1, T2, T3, T4, T5>[] actions
        )
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5));
            return tuple;
        }

        internal static Tuple<T1, T2, T3, T4, T5, T6> Apply<T1, T2, T3, T4, T5, T6>(
            this Tuple<T1, T2, T3, T4, T5, T6> tuple,
            params Action<T1, T2, T3, T4, T5, T6>[] actions
        )
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6));
            return tuple;
        }

        internal static Tuple<T1, T2, T3, T4, T5, T6, T7> Apply<T1, T2, T3, T4, T5, T6, T7>(
            this Tuple<T1, T2, T3, T4, T5, T6, T7> tuple,
            params Action<T1, T2, T3, T4, T5, T6, T7>[] actions)
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7));
            return tuple;
        }

        internal static Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> Apply<T1, T2, T3, T4, T5, T6, T7, TRest>(
            this Tuple<T1, T2, T3, T4, T5, T6, T7, TRest> tuple,
            params Action<T1, T2, T3, T4, T5, T6, T7, TRest>[] actions
        )
        {
            Array.ForEach(actions, a => a(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4, tuple.Item5, tuple.Item6, tuple.Item7, tuple.Rest));
            return tuple;
        }

        #endregion

        #region Dispose

        internal static TReturn Dispose<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
            where TReceiver : IDisposable
        {
            using (self)
            {
                return func(self);
            }
        }

        internal static TReceiver Dispose<TReceiver>(this TReceiver self, Action<TReceiver> func)
            where TReceiver : IDisposable
        {
            using (self)
            {
                func(self);
            }
            return self;
        }

        #endregion

        #region Default

        internal static TReturn Default<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, Func<TReturn> funcIfDefault)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : funcIfDefault();
        }

        internal static TReturn Default<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfDefault)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : valueIfDefault;
        }

        internal static TReturn Default<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : default(TReturn);
        }

        internal static TReceiver Default<TReceiver>(this TReceiver self, Action<TReceiver> action, Func<TReceiver> funcIfDefault)
        {
            if (EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver)))
            {
                action(self);
                return self;
            }
            else
            {
                return funcIfDefault();
            }
        }

        internal static TReceiver Default<TReceiver>(this TReceiver self, Action<TReceiver> action, TReceiver valueIfDefault)
        {
            if (EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver)))
            {
                action(self);
                return self;
            }
            else
            {
                return valueIfDefault;
            }
        }

        internal static TReceiver Default<TReceiver>(this TReceiver self, Action<TReceiver> action)
        {
            if (!EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver)))
            {
                action(self);
                return self;
            }
            else
            {
                return default(TReceiver);
            }
        }

        #endregion

        #region Null

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, Func<TReturn> funcIfNull)
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : funcIfNull();
        }

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfNull)
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : valueIfNull;
        }

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : default(TReturn);
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action, Func<TReceiver> funcIfNull)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return funcIfNull();
            }
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action, TReceiver valueIfNull)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return valueIfNull;
            }
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return default(TReceiver);
            }
        }

        #endregion

        #region Nullable

        internal static Nullable<TReturn> Nullable<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, Func<TReturn> funcIfDefault)
            where TReturn : struct
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : funcIfDefault();
        }

        internal static Nullable<TReturn> Nullable<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfDefault)
            where TReturn : struct
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : valueIfDefault;
        }

        internal static Nullable<TReturn> Nullable<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func)
            where TReturn : struct
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? func(self)
                : default(Nullable<TReturn>);
        }

        #endregion

        #region If

        internal static TReturn If<TReceiver, TReturn>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            TReturn then,
            TReturn otherwise
        )
        {
            return predicate(self)
                ? then
                : otherwise;
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            TReceiver then
        )
        {
            return predicate(self)
                ? then
                : self;
        }

        internal static TReturn If<TReceiver, TReturn>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Func<TReceiver, TReturn> then,
            Func<TReceiver, TReturn> otherwise
        )
        {
            return predicate(self)
                ? then(self)
                : otherwise(self);
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Func<TReceiver, TReceiver> then
        )
        {
            return predicate(self)
                ? then(self)
                : self;
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Action<TReceiver> then,
            Action<TReceiver> otherwise
        )
        {
            return predicate(self)
                ? self.Apply(then)
                : self.Apply(otherwise);
        }

        internal static TReceiver If<TReceiver>(
            this TReceiver self,
            Func<TReceiver, Boolean> predicate,
            Action<TReceiver> then
        )
        {
            return predicate(self)
                ? self.Apply(then)
                : self;
        }

        #endregion

        internal static TResult[] SelectAll<TSource, TResult>(this TSource[] array, Func<TSource, TResult> selector)
        {
#if SILVERLIGHT
            return array.Select(selector).ToArray();
#else
            return Array.ConvertAll(array, e => selector(e));
#endif
        }

        internal static TSource[] WhereAll<TSource>(this TSource[] array, Func<TSource, Boolean> predicate)
        {
#if SILVERLIGHT
            return array.Where(predicate).ToArray();
#else
            return Array.FindAll(array, e => predicate(e));
#endif
        }

        internal static Boolean StartsWithInvariant(this String str, String value)
        {
            return str.StartsWith(value, StringComparison.InvariantCulture);
        }

        internal static Boolean EndsWithInvariant(this String str, String value)
        {
            return str.EndsWith(value, StringComparison.InvariantCulture);
        }

        #region Zip

        internal static IEnumerable<TResult> Zip<TSource1, TSource2, TSource3, TResult>(
            this IEnumerable<TSource1> source1,
            IEnumerable<TSource2> source2,
            IEnumerable<TSource3> source3,
            Func<TSource1, TSource2, TSource3, TResult> resultSelector
        )
        {
            using (var iter1 = source1.GetEnumerator())
            using (var iter2 = source2.GetEnumerator())
            using (var iter3 = source3.GetEnumerator())
            {
                while (iter1.MoveNext() && iter2.MoveNext() && iter3.MoveNext())
                {
                    yield return resultSelector(iter1.Current, iter2.Current, iter3.Current);
                }
            }
        }

        internal static IEnumerable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TResult>(
            this IEnumerable<TSource1> source1,
            IEnumerable<TSource2> source2,
            IEnumerable<TSource3> source3,
            IEnumerable<TSource4> source4,
            Func<TSource1, TSource2, TSource3, TSource4, TResult> resultSelector
        )
        {
            using (var iter1 = source1.GetEnumerator())
            using (var iter2 = source2.GetEnumerator())
            using (var iter3 = source3.GetEnumerator())
            using (var iter4 = source4.GetEnumerator())
            {
                while (iter1.MoveNext() && iter2.MoveNext() && iter3.MoveNext() && iter4.MoveNext())
                {
                    yield return resultSelector(iter1.Current, iter2.Current, iter3.Current, iter4.Current);
                }
            }
        }

        internal static IEnumerable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TResult>(
            this IEnumerable<TSource1> source1,
            IEnumerable<TSource2> source2,
            IEnumerable<TSource3> source3,
            IEnumerable<TSource4> source4,
            IEnumerable<TSource5> source5,
            Func<TSource1, TSource2, TSource3, TSource4, TSource5, TResult> resultSelector
        )
        {
            using (var iter1 = source1.GetEnumerator())
            using (var iter2 = source2.GetEnumerator())
            using (var iter3 = source3.GetEnumerator())
            using (var iter4 = source4.GetEnumerator())
            using (var iter5 = source5.GetEnumerator())
            {
                while (iter1.MoveNext() && iter2.MoveNext() && iter3.MoveNext() && iter4.MoveNext() && iter5.MoveNext())
                {
                    yield return resultSelector(iter1.Current, iter2.Current, iter3.Current, iter4.Current, iter5.Current);
                }
            }
        }

        internal static IEnumerable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult>(
            this IEnumerable<TSource1> source1,
            IEnumerable<TSource2> source2,
            IEnumerable<TSource3> source3,
            IEnumerable<TSource4> source4,
            IEnumerable<TSource5> source5,
            IEnumerable<TSource6> source6,
            Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TResult> resultSelector
        )
        {
            using (var iter1 = source1.GetEnumerator())
            using (var iter2 = source2.GetEnumerator())
            using (var iter3 = source3.GetEnumerator())
            using (var iter4 = source4.GetEnumerator())
            using (var iter5 = source5.GetEnumerator())
            using (var iter6 = source6.GetEnumerator())
            {
                while (iter1.MoveNext() && iter2.MoveNext() && iter3.MoveNext() && iter4.MoveNext() && iter5.MoveNext() && iter6.MoveNext())
                {
                    yield return resultSelector(iter1.Current, iter2.Current, iter3.Current, iter4.Current, iter5.Current, iter6.Current);
                }
            }
        }

        internal static IEnumerable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult>(
            this IEnumerable<TSource1> source1,
            IEnumerable<TSource2> source2,
            IEnumerable<TSource3> source3,
            IEnumerable<TSource4> source4,
            IEnumerable<TSource5> source5,
            IEnumerable<TSource6> source6,
            IEnumerable<TSource7> source7,
            Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TResult> resultSelector
        )
        {
            using (var iter1 = source1.GetEnumerator())
            using (var iter2 = source2.GetEnumerator())
            using (var iter3 = source3.GetEnumerator())
            using (var iter4 = source4.GetEnumerator())
            using (var iter5 = source5.GetEnumerator())
            using (var iter6 = source6.GetEnumerator())
            using (var iter7 = source7.GetEnumerator())
            {
                while (iter1.MoveNext() && iter2.MoveNext() && iter3.MoveNext() && iter4.MoveNext() && iter5.MoveNext() && iter6.MoveNext() && iter7.MoveNext())
                {
                    yield return resultSelector(iter1.Current, iter2.Current, iter3.Current, iter4.Current, iter5.Current, iter6.Current, iter7.Current);
                }
            }
        }

        internal static IEnumerable<TResult> Zip<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult>(
            this IEnumerable<TSource1> source1,
            IEnumerable<TSource2> source2,
            IEnumerable<TSource3> source3,
            IEnumerable<TSource4> source4,
            IEnumerable<TSource5> source5,
            IEnumerable<TSource6> source6,
            IEnumerable<TSource7> source7,
            IEnumerable<TSource8> source8,
            Func<TSource1, TSource2, TSource3, TSource4, TSource5, TSource6, TSource7, TSource8, TResult> resultSelector
        )
        {
            using (var iter1 = source1.GetEnumerator())
            using (var iter2 = source2.GetEnumerator())
            using (var iter3 = source3.GetEnumerator())
            using (var iter4 = source4.GetEnumerator())
            using (var iter5 = source5.GetEnumerator())
            using (var iter6 = source6.GetEnumerator())
            using (var iter7 = source7.GetEnumerator())
            using (var iter8 = source8.GetEnumerator())
            {
                while (iter1.MoveNext() && iter2.MoveNext() && iter3.MoveNext() && iter4.MoveNext() && iter5.MoveNext() && iter6.MoveNext() && iter7.MoveNext() && iter8.MoveNext())
                {
                    yield return resultSelector(iter1.Current, iter2.Current, iter3.Current, iter4.Current, iter5.Current, iter6.Current, iter7.Current, iter8.Current);
                }
            }
        }

        #endregion

        internal static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TSource : class
        {
            return source.Select(selector).Where(_ => _ != null);
        }

        internal static TSource FirstOrLast<TSource>(this IEnumerable<TSource> source, Func<TSource, Boolean> predicate)
        {
            var iter = source.GetEnumerator();
            TSource current = default(TSource);
            while (iter.MoveNext())
            {
                current = iter.Current;
                if (predicate(current))
                {
                    return current;
                }
            }
            return current;
        }

        internal static IEnumerable<IList<TSource>> PartitionBy<TSource>(this IEnumerable<TSource> source, Func<TSource, Boolean> predicate)
        {
            var list = new List<TSource>();
            var isInSplitter = false;
            foreach (var e in source)
            {
                if (predicate(e) != isInSplitter)
                {
                    isInSplitter = !isInSplitter;
                    if (list.Any())
                    {
                        yield return list;
                        list = new List<TSource>();
                    }
                }
                list.Add(e);
            }
            if (list.Any())
            {
                yield return list;
            }
        }

        internal static IEnumerable<TSource> EndWith<TSource>(this IEnumerable<TSource> source, params TSource[] values)
        {
            return source.Concat(values);
        }

        internal static TValue TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value)
                ? value
                : defaultValue;
        }

        internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToDictionary(p => p.Key, p => p.Value);
        }

        internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> source)
        {
            return source.ToDictionary(p => p.Item1, p => p.Item2);
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
