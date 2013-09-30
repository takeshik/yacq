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
 * 
 * This code is originally in: https://github.com/takeshik/cs-util-codes/
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace XSpect
{
    [DebuggerStepThrough()]
    internal static class Flows
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

        #region Null

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, params Func<TReturn>[] funcsIfNull)
        {
            return EqualityComparer<TReceiver>.Default.Equals(self, default(TReceiver))
                ? funcsIfNull
                      .Select(f => f())
                      .FirstOrDefault(_ => !EqualityComparer<TReturn>.Default.Equals(_, default(TReturn)))
                : func(self);
        }

        internal static TReturn Null<TReceiver, TReturn>(this TReceiver self, Func<TReceiver, TReturn> func, TReturn valueIfNull = default(TReturn))
            where TReceiver : class
        {
            return self != null
                ? func(self)
                : valueIfNull;
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action, params Func<TReceiver>[] funcsIfNull)
            where TReceiver : class
        {
            if (self != null)
            {
                action(self);
                return self;
            }
            else
            {
                return funcsIfNull.Select(f => f()).FirstOrDefault(_ => _ != null);
            }
        }

        internal static TReceiver Null<TReceiver>(this TReceiver self, Action<TReceiver> action, TReceiver valueIfNull = default(TReceiver))
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

        #endregion

        #region Nullable

        internal static TReturn Nullable<TReceiver, TReturn>(this Nullable<TReceiver> self, Func<TReceiver, TReturn> func, TReturn valueIfNull = default(TReturn))
            where TReceiver : struct
        {
            return self != null
                ? func(self.Value)
                : valueIfNull;
        }

        internal static TReceiver Nullable<TReceiver>(this Nullable<TReceiver> self, Action<TReceiver> action, params Func<Nullable<TReceiver>>[] funcsIfNull)
            where TReceiver : struct
        {
            if (self != null)
            {
                action(self.Value);
                return self.Value;
            }
            else
            {
                return funcsIfNull.Select(f => f()).FirstOrDefault(_ => _ != null) ?? default(TReceiver);
            }
        }

        internal static TReceiver Nullable<TReceiver>(this Nullable<TReceiver> self, Action<TReceiver> action, TReceiver valueIfNull = default(TReceiver))
            where TReceiver : struct
        {
            if (self != null)
            {
                action(self.Value);
                return self.Value;
            }
            else
            {
                return valueIfNull;
            }
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
    }

    [DebuggerStepThrough()]
    internal static class Disposables
    {
        private sealed class AnonymousDisposable
            : IDisposable
        {
            private readonly Action _body;

            public AnonymousDisposable(Action body)
            {
                this._body = body;
            }

            public void Dispose()
            {
                this._body();
            }
        }

        public static IDisposable From(Action body)
        {
            return new AnonymousDisposable(body);
        }

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
    }

    [DebuggerStepThrough()]
    internal static class Enumerables
    {
        internal static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
            where TSource : class
        {
            return source.Select(selector).Where(_ => _ != null);
        }

        internal static IEnumerable<IList<TSource>> Buffer<TSource>(this IEnumerable<TSource> source, Int32 count)
        {
            List<TSource> buffer = null;
            var i = 0;
            foreach (var e in source)
            {
                if (i % count == 0)
                {
                    if (i > 0)
                    {
                        yield return buffer;
                    }
                    buffer = new List<TSource>(count);
                }
                buffer.Add(e);
                ++i;
            }
            if (buffer.Any())
            {
                yield return buffer;
            }
        }

        internal static IEnumerable<TSource> Concat<TSource>(this IEnumerable<IEnumerable<TSource>> sources)
        {
            return sources
                .Where(source => source != null)
                .SelectMany(source => source);
        }

        internal static IEnumerable<TSource> Repeat<TSource>(this IEnumerable<TSource> source)
        {
            while (true)
            {
                foreach (var e in source)
                {
                    yield return e;
                }
            }
// ReSharper disable FunctionNeverReturns
        }
// ReSharper restore FunctionNeverReturns

        internal static IEnumerable<TSource> In<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, params TKey[] keys)
        {
            return source.Where(e => keys.Contains(keySelector(e)));
        }

        internal static IEnumerable<TSource> In<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEnumerable<TKey> keys)
        {
            return In(source, keySelector, keys.ToArray());
        }

        internal static IEnumerable<TSource> Between<TSource>(this IEnumerable<TSource> source, TSource from, TSource to)
            where  TSource : IComparable<TSource>
        {
            return source.Where(e => from.CompareTo(e) <= 0 && e.CompareTo(to) <= 0);
        }

        internal static IEnumerable<TSource> Between<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TKey from, TKey to)
            where TKey : IComparable<TKey>
        {
            return source.Where(e => keySelector(e).Let(k => from.CompareTo(k) <= 0 && k.CompareTo(to) <= 0));
        }

        #region IndexOf

        internal static Int32 IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, Boolean> predicate)
        {
            return source
                .Select((e, i) => Tuple.Create<TSource, int>(e, i))
                .First(_ => predicate(_.Item1))
                .Item2;
        }

        internal static Int32 IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, Int32, Boolean> predicate)
        {
            return source
                .Select((e, i) => Tuple.Create<TSource, int>(e, i))
                .First(_ => predicate(_.Item1, _.Item2))
                .Item2;
        }

        internal static Int32 IndexOf<TSource>(this IEnumerable<TSource> source, TSource element, IEqualityComparer<TSource> comparer)
        {
            return IndexOf(source, e => comparer.Equals(e, element));
        }

        internal static Int32 IndexOf<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            return IndexOf(source, element, EqualityComparer<TSource>.Default);
        }

        #endregion

        #region LastIndexOf

        internal static Int32 LastIndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, Boolean> predicate)
        {
            return source
                .Select((e, i) => Tuple.Create(e, i))
                .Last(_ => predicate(_.Item1))
                .Item2;
        }

        internal static Int32 LastIndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, Int32, Boolean> predicate)
        {
            return source
                .Select((e, i) => Tuple.Create(e, i))
                .Last(_ => predicate(_.Item1, _.Item2))
                .Item2;
        }

        internal static Int32 LastIndexOf<TSource>(this IEnumerable<TSource> source, TSource element, IEqualityComparer<TSource> comparer)
        {
            return LastIndexOf(source, e => comparer.Equals(e, element));
        }

        internal static Int32 LastIndexOf<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            return LastIndexOf(source, element, EqualityComparer<TSource>.Default);
        }

        #endregion

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

        internal static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, Int32 count)
        {
            var queue = new Queue<TSource>();
            foreach (var e in source)
            {
                queue.Enqueue(e);
                if (queue.Count > count)
                {
                    queue.Dequeue();
                }
            }
            return queue;
        }

        internal static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, Int32 count)
        {
            var queue = new Queue<TSource>();
            foreach (var e in source)
            {
                queue.Enqueue(e);
                if (queue.Count > count)
                {
                    yield return queue.Dequeue();
                }
            }
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

        internal static IEnumerable<TSource> StartWith<TSource>(this IEnumerable<TSource> source, params TSource[] values)
        {
            return values.Concat(source);
        }

        internal static IEnumerable<TSource> EndWith<TSource>(this IEnumerable<TSource> source, params TSource[] values)
        {
            return source.Concat(values);
        }

        #region Generate

        internal static IEnumerable<TResult> Generate<T, TResult>(
            this T initialValue,
            Func<T, T> iterator,
            Func<T, Boolean> predicate,
            Func<T, TResult> selector
        )
        {
            var arg = initialValue;
            while (predicate(arg))
            {
                yield return selector(arg);
                arg = iterator(arg);
            }
        }

        internal static IEnumerable<TResult> Generate<T, TResult>(
            this T initialValue,
            Func<T, Int32, T> iterator,
            Func<T, Int32, Boolean> predicate,
            Func<T, Int32, TResult> selector
        )
        {
            var index = 0;
            var arg = initialValue;
            while (predicate(arg, index))
            {
                yield return selector(arg, index);
                arg = iterator(arg, index++);
            }
        }

        internal static IEnumerable<T> Generate<T>(
            this T initialValue,
            Func<T, T> iterator,
            Func<T, Boolean> predicate
        )
        {
            return initialValue.Generate(iterator, predicate, _ => _);
        }

        internal static IEnumerable<T> Generate<T>(
            this T initialValue,
            Func<T, Int32, T> iterator,
            Func<T, Int32, Boolean> predicate
        )
        {
            return initialValue.Generate(iterator, predicate, (_, i) => _);
        }

        internal static IEnumerable<T> Generate<T>(this T initialValue, Func<T, T> iterator)
        {
            return initialValue.Generate(iterator, _ => true, _ => _);
        }

        internal static IEnumerable<T> Generate<T>(this T initialValue, Func<T, Int32, T> iterator)
        {
            return initialValue.Generate(iterator, (_, i) => true, (_, i) => _);
        }

        #endregion

        internal static IEnumerable<TSource> Expand<TSource>(this IEnumerable<TSource> source, Func<TSource, IEnumerable<TSource>> selector)
        {
            return source
                .Generate(xs => xs.SelectMany(selector), xs => xs.Any())
                .SelectMany(xs => xs);
        }

        internal static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext)
        {
            foreach (var e in source)
            {
                onNext(e);
            }
        }

        internal static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource, Int32> onNext)
        {
            var i = 0;
            foreach (var e in source)
            {
                onNext(e, i++);
            }
        }

        internal static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource> onNext)
        {
            foreach (var e in source)
            {
                onNext(e);
                yield return e;
            }
        }

        internal static IEnumerable<TSource> Do<TSource>(this IEnumerable<TSource> source, Action<TSource, Int32> onNext)
        {
            var i = 0;
            foreach (var e in source)
            {
                onNext(e, i++);
            }
            return source;
        }
    }

    [DebuggerStepThrough()]
    internal static class Arrays
    {
        internal static T[] From<T>(params T[] array)
        {
            return array;
        }

        internal static T[] Empty<T>()
        {
            // HACK: depends on internal implementation.
            return (T[]) Enumerable.Empty<T>();
        }

        internal static Int32 BinarySearch<T>(this T[] array, T value)
        {
            return Array.BinarySearch(array, value);
        }

        internal static Int32 BinarySearch<T>(this T[] array, T value, IComparer<T> comparer)
        {
            return Array.BinarySearch(array, value, comparer);
        }

        internal static Int32 BinarySearch<T>(this T[] array, Int32 index, Int32 length, T value, IComparer<T> comparer)
        {
            return Array.BinarySearch(array, index, length, value, comparer);
        }

        internal static TResult[] SelectAll<TElement, TResult>(this TElement[] array, Func<TElement, TResult> selector)
        {
#if SILVERLIGHT
            return array.Select(selector).ToArray();
#else
            return Array.ConvertAll(array, new Converter<TElement, TResult>(selector));
#endif
        }

        internal static TElement[] WhereAll<TElement>(this TElement[] array, Func<TElement, Boolean> predicate)
        {
#if SILVERLIGHT
            return array.Where(predicate).ToArray();
#else
            return Array.FindAll(array, (new Predicate<TElement>(predicate)));
#endif
        }

        internal static TElement Last<TElement>(this TElement[] array, Func<TElement, Boolean> predicate)
        {
#if SILVERLIGHT
            return array.Last(predicate);
#else
            return Array.FindLast(array, new Predicate<TElement>(predicate));
#endif
        }

        internal static ArraySegment<T> Range<T>(this T[] array, Int32 offset, Int32 count)
        {
            return new ArraySegment<T>(array, offset, count);
        }
    }

    [DebuggerStepThrough()]
    internal static class Lists
    {
        internal static void AddRange<TElement>(this IList<TElement> list, IEnumerable<TElement> elements)
        {
            foreach (var e in elements)
            {
                list.Add(e);
            }
        }

        internal static void AddRange<TElement>(this IList<TElement> list, params TElement[] elements)
        {
            AddRange(list, (IEnumerable<TElement>) elements);
        }

        internal static ReadOnlyCollection<TElement> AsReadOnly<TElement>(this IList<TElement> list)
        {
            return new ReadOnlyCollection<TElement>(list);
        }

        internal static void InsertRange<TElement>(this IList<TElement> list, Int32 index, IEnumerable<TElement> elements)
        {
            foreach (var e in elements)
            {
                list.Insert(index++, e);
            }
        }

        internal static void InsertRange<TElement>(this IList<TElement> list, Int32 index, params TElement[] elements)
        {
            InsertRange(list, index, (IEnumerable<TElement>) elements);
        }

        internal static Int32 RemoveAll<TElement>(this IList<TElement> list, Func<TElement, Boolean> predicate)
        {
            var targets = list.Select((e, i) => Tuple.Create(e, i)).Where(_ => predicate(_.Item1))
                .Select(_ => _.Item2)
                .OrderByDescending(i => i)
                .ToArray();
            foreach (var i in targets)
            {
                list.RemoveAt(i);
            }
            return targets.Length;
        }

        internal static Int32 RemoveAll<TElement>(this IList<TElement> list, Func<TElement, Int32, Boolean> predicate)
        {
            var targets = list.Select((e, i) => Tuple.Create(e, i)).Where(_ => predicate(_.Item1, _.Item2))
                .Select(_ => _.Item2)
                .OrderByDescending(i => i)
                .ToArray();
            foreach (var i in targets)
            {
                list.RemoveAt(i);
            }
            return targets.Length;
        }

        internal static void RemoveRange<TElement>(this IList<TElement> list, Int32 index, Int32 count)
        {
            Enumerable.Range(index, count)
                .Reverse()
                .ForEach(list.RemoveAt);
        }

        internal static TElement At<TElement>(this IList<TElement> source, Int32 index)
        {
            return index < 0
                ? source[source.Count + index]
                : source[index];
        }

        internal static IEnumerable<TElement> Slice<TElement>(IList<TElement> list, Int32 from, Int32 to)
        {
            switch (Math.Sign(from))
            {
                case -1:
                    switch (Math.Sign(to))
                    {
                        case -1:
                            return from >= to
                                ? list.Skip(list.Count + to).Take(from - to + 1).Reverse()
                                : list.Skip(list.Count + from).Take(to - from + 1);
                        case 0:
                            return list.Skip(list.Count + from).EndWith(list[0]);
                        case 1:
                            return list.Skip(list.Count + from).Concat(list.Take(to + 1));
                    }
                    break;
                case 0:
                    switch (Math.Sign(to))
                    {
                        case -1:
                            return list.Skip(list.Count + to).Reverse().StartWith(list[0]);
                        case 0:
                            return list.Take(1);
                        case 1:
                            return list.Take(to + 1);
                    }
                    break;
                case 1:
                    switch (Math.Sign(to))
                    {
                        case -1:
                            return list.Skip(list.Count + to).Concat(list.Take(from + 1)).Reverse();
                        case 0:
                            return list.Take(from + 1).Reverse();
                        case 1:
                            return from <= to
                                ? list.Skip(from).Take(to - from + 1)
                                : list.Skip(to).Take(from - to + 1).Reverse();
                    }
                    break;
            }
            throw new ArgumentException();
        }
    }

    [DebuggerStepThrough()]
    internal static class Dictionaries
    {
        internal static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            return dictionary.TryGetValue(key, out value)
                ? value
                : defaultValue;
        }

        internal static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defaultValueFactory)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value)
                ? value
                : defaultValueFactory();
        }

        internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        {
            return source.ToDictionary(p => p.Key, p => p.Value);
        }

        internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> source)
        {
            return source.ToDictionary(p => p.Item1, p => p.Item2);
        }

        internal static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<TKey> keys, IEnumerable<TValue> values)
        {
            return keys.Zip(values, Tuple.Create).ToDictionary();
        }
    }

    [DebuggerStepThrough()]
    internal static class Strings
    {
        internal static Boolean StartsWithInvariant(this String str, String value)
        {
            return str.StartsWith(value, StringComparison.InvariantCulture);
        }

        internal static Boolean EndsWithInvariant(this String str, String value)
        {
            return str.EndsWith(value, StringComparison.InvariantCulture);
        }

        #region Stringify

        internal static String Stringify(this IEnumerable<Char> chars)
        {
            return new String(chars.ToArray());
        }

        internal static String Stringify(this IEnumerable<Char> chars, String joinner)
        {
            return String.Join(joinner, chars);
        }

        internal static String Stringify(this IEnumerable<String> strings)
        {
            return String.Concat(strings);
        }

        internal static String Stringify(this IEnumerable<String> strings, String joinner)
        {
            return String.Join(joinner, strings);
        }

        internal static String Stringify<TSource>(this IEnumerable<TSource> source, String joinner)
        {
            return String.Join(joinner, source);
        }

        internal static String Stringify<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            return String.Concat(source.Select(selector));
        }

        internal static String Stringify<TSource>(this IEnumerable<TSource> source, Func<TSource, String> selector)
        {
            return String.Concat(source.Select(selector));
        }

        internal static String Stringify<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, String joinner)
        {
            return String.Join(joinner, source.Select(selector));
        }

        internal static String Stringify<TSource>(this IEnumerable<TSource> source, Func<TSource, String> selector, String joinner)
        {
            return String.Join(joinner, source.Select(selector));
        }

        #endregion
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
