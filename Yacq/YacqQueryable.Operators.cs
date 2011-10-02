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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace XSpect.Yacq
{
    partial class YacqQueryable<TSource>
    {
        public TSource Aggregate(String func)
        {
            return this._source.Aggregate(
                YacqServices.ParseLambda<Func<TSource, TSource, TSource>>(this.Symbols, func, "a", "it")
            );
        }

        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, String func)
        {
            return this._source.Aggregate(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, func, "a", "it")
            );
        }

        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, String func, String selector)
        {
            return this._source.Aggregate(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, func, "a", "it"),
                YacqServices.ParseLambda<TAccumulate, TResult>(this.Symbols, selector)
            );
        }

        public TSource First(String predicate)
        {
            return this._source.First(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public TSource FirstOrDefault(String predicate)
        {
            return this._source.FirstOrDefault(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public TSource Last(String predicate)
        {
            return this._source.Last(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public TSource LastOrDefault(String predicate)
        {
            return this._source.LastOrDefault(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public TSource Single(String predicate)
        {
            return this._source.Single(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public TSource SingleOrDefault(String predicate)
        {
            return this._source.SingleOrDefault(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public Boolean Any(String predicate)
        {
            return this._source.Any(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public Boolean All(String predicate)
        {
            return this._source.All(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public Int32 Count(String predicate)
        {
            return this._source.Count(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public Int64 LongCount(String predicate)
        {
            return this._source.LongCount(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        public YacqQueryable<TSource> Where(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Where(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        public YacqQueryable<TResult> Select<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Select(
                YacqServices.ParseLambda<TSource, TResult>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TResult> SelectMany<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseLambda<TSource, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TResult> SelectMany<TCollection, TResult>(String collectionSelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseLambda<Func<TSource, Int32, IEnumerable<TCollection>>>(this.Symbols, collectionSelector, "it", "i"),
                YacqServices.ParseLambda<Func<TSource, TCollection, TResult>>(this.Symbols, resultSelector, "it", "c")
            ));
        }

        public YacqQueryable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Join(
                inner,
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, TInner, TResult>>(this.Symbols, resultSelector, "o", "i")
            ));
        }

        public YacqQueryable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Join(
                inner,
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, TInner, TResult>>(this.Symbols, resultSelector, "o", "i"),
                comparer
            ));
        }

        public YacqQueryable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupJoin(
                inner,
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, IEnumerable<TInner>, TResult>>(this.Symbols, resultSelector, "o", "i")
            ));
        }

        public YacqQueryable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupJoin(
                inner,
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, IEnumerable<TInner>, TResult>>(this.Symbols, resultSelector, "o", "i"),
                comparer
            ));
        }

        public YacqOrderedQueryable<TSource> OrderBy<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqOrderedQueryable<TSource> OrderBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        public YacqOrderedQueryable<TSource> OrderByDescending<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderByDescending(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqOrderedQueryable<TSource> OrderByDescending<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderByDescending(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        public YacqQueryable<TSource> TakeWhile(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.TakeWhile(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        public YacqQueryable<TSource> SkipWhile(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.SkipWhile(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        public YacqQueryable<IGrouping<TKey, TSource>> GroupBy<TKey>(String keySelector)
        {
            return new YacqQueryable<IGrouping<TKey, TSource>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqQueryable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(String keySelector, String elementSelector)
        {
            return new YacqQueryable<IGrouping<TKey, TElement>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector)
            ));
        }

        public YacqQueryable<IGrouping<TKey, TSource>> GroupBy<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<IGrouping<TKey, TSource>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        public YacqQueryable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(String keySelector, String elementSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<IGrouping<TKey, TElement>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                comparer
            ));
        }

        public YacqQueryable<TResult> GroupBy<TKey, TElement, TResult>(String keySelector, String elementSelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                YacqServices.ParseLambda<Func<TKey, IEnumerable<TElement>, TResult>>(this.Symbols, resultSelector, "it", "e")
            ));
        }

        public YacqQueryable<TResult> GroupBy<TKey, TElement, TResult>(String keySelector, String elementSelector, String resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                YacqServices.ParseLambda<Func<TKey, IEnumerable<TElement>, TResult>>(this.Symbols, resultSelector, "it", "e"),
                comparer
            ));
        }

        public YacqQueryable<TResult> Zip<TSecond, TResult>(IEnumerable<TSecond> source2, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Zip(
                source2,
                YacqServices.ParseLambda<Func<TSource, TSecond, TResult>>(this.Symbols, resultSelector, "it", "it2")
            ));
        }

        public YacqQueryable<TSource> DistinctUntilChanged<TKey>(String keySelector)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DistinctUntilChanged(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqQueryable<TSource> DistinctUntilChanged<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DistinctUntilChanged(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        public YacqQueryable<TSource> Expand(String selector)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Expand(
                YacqServices.ParseLambda<TSource, IEnumerable<TSource>>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TAccumulate> Scan<TAccumulate>(TAccumulate seed, String accumulator)
        {
            return new YacqQueryable<TAccumulate>(this.Symbols, this._source.Scan(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        public YacqQueryable<TSource> Scan(String accumulator)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Scan(
                YacqServices.ParseLambda<Func<TSource, TSource, TSource>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        public YacqQueryable DoWhile(String condition)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DoWhile(
                YacqServices.ParseLambda<Boolean>(this.Symbols, condition)
            ));
        }

        public YacqQueryable<TSource> Do(String onNext)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it")
            ));
        }

        public YacqQueryable<TSource> Do(String onNext, String onCompleted)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action>(this.Symbols, onCompleted, new String[0])
            ));
        }

        public YacqQueryable<TSource> Do(String onNext, String onError, String onCompleted)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action<Exception>>(this.Symbols, onError, "ex"),
                YacqServices.ParseLambda<Action>(this.Symbols, onCompleted, new String[0])
            ));
        }

        public YacqQueryable<TSource> Distinct<TKey>(String keySelector)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Distinct(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqQueryable<TSource> Distinct<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Distinct(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        public IList<TSource> MinBy<TKey>(String keySelector)
        {
            return this._source.MinBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            );
        }

        public IList<TSource> MinBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return this._source.MinBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            );
        }

        public IList<TSource> MaxBy<TKey>(String keySelector)
        {
            return this._source.MaxBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            );
        }

        public IList<TSource> MaxBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return this._source.MaxBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            );
        }

        public YacqQueryable<TResult> Share<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Share(
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TResult> Publish<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Publish(
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TResult> Memoize<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Memoize(
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TResult> Memoize<TResult>(Int32 readerCount, String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Memoize(
                readerCount,
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        public YacqQueryable<TSource> Catch<TException>(String handler)
            where TException : Exception
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Catch(
                YacqServices.ParseLambda<Func<TException, IEnumerable<TSource>>>(this.Symbols, handler, "ex")
            ));
        }

        public YacqQueryable<TSource> Finally(String finallyAction)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Finally(
                YacqServices.ParseLambda<Action>(this.Symbols, finallyAction, new String[0])
            ));
        }
    }

    partial class YacqOrderedQueryable<TSource>
    {
        public YacqOrderedQueryable<TSource> ThenBy<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqOrderedQueryable<TSource> ThenBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        public YacqOrderedQueryable<TSource> ThenByDescending<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenByDescending(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        public YacqOrderedQueryable<TSource> ThenByDescending<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenByDescending(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }
    }
}