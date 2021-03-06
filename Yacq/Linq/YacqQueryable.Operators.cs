﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using XSpect.Yacq.Expressions;
using Expression = System.Linq.Expressions.Expression;

namespace XSpect.Yacq.Linq
{
    partial class YacqQueryable<TSource>
    {
        /// <summary>
        /// Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <typeparam name="TResult">The type of the resulting value.</typeparam>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func"><c>(a, it) =></c> An accumulator function to invoke on each element.</param>
        /// <param name="selector"><c>(it) =></c> A function to transform the final accumulator value into the result value.</param>
        /// <returns>The transformed final accumulator value.</returns>
        public TResult Aggregate<TAccumulate, TResult>(TAccumulate seed, String func, String selector)
        {
            return this._source.Aggregate(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, func, "a", "it"),
                YacqServices.ParseFunc<TAccumulate, TResult>(this.Symbols, selector)
            );
        }

        /// <summary>
        /// Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value.
        /// </summary>
        /// <typeparam name="TAccumulate">The type of the accumulator value.</typeparam>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func"><c>(a, it) =></c> An accumulator function to invoke on each element.</param>
        /// <returns>The final accumulator value.</returns>
        public TAccumulate Aggregate<TAccumulate>(TAccumulate seed, String func)
        {
            return this._source.Aggregate(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, func, "a", "it")
            );
        }

        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>
        /// <param name="func"><c>(a, it) =></c> An accumulator function to apply to each element.</param>
        /// <returns>The final accumulator value.</returns>
        public new TSource Aggregate(String func)
        {
            return this._source.Aggregate(
                YacqServices.ParseLambda<Func<TSource, TSource, TSource>>(this.Symbols, func, "a", "it")
            );
        }

        /// <summary>
        /// Determines whether all the elements of a sequence satisfy a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns><c>true</c> if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, <c>false</c>.</returns>
        public new Boolean All(String predicate)
        {
            return this._source.All(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns><c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.</returns>
        public new Boolean Any(String predicate)
        {
            return this._source.Any(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Converts a generic <see cref="IEnumerable{TSource}"/> to a generic <see cref="YacqQueryable{TSource}"/>.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that represents the input sequence.</returns>
        public new YacqQueryable<TSource> AsQueryable()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.AsQueryable());
        }

        /// <summary>
        /// Converts the elements of an <see cref="IQueryable"/> to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the elements of the source to.</typeparam>
        /// <returns>An <see cref="YacqQueryable{TResult}"/> that contains each element of the source sequence converted to the specified type.</returns>
        public new YacqQueryable<TResult> Cast<TResult>()
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Cast<TResult>());
        }

        /// <summary>
        /// Concatenates two sequences.
        /// </summary>
        /// <param name="source2">The sequence to concatenate to the first sequence.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the concatenated elements of the two input sequences.</returns>
        public YacqQueryable<TSource> Concat(IEnumerable<TSource> source2)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Concat(source2));
        }

        /// <summary>
        /// Returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The number of elements in the sequence that satisfies the condition in the predicate function.</returns>
        public new Int32 Count(String predicate)
        {
            return this._source.Count(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains default value if the source is empty; otherwise, the source.</returns>
        public new YacqQueryable<TSource> DefaultIfEmpty()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DefaultIfEmpty());
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <param name="defaultValue">The value to return if the sequence is empty.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains <paramref name="defaultValue"/> if the source is empty; otherwise, the source.</returns>
        public YacqQueryable<TSource> DefaultIfEmpty(TSource defaultValue)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DefaultIfEmpty(defaultValue));
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains distinct elements from the source.</returns>
        public new YacqQueryable<TSource> Distinct()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Distinct());
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using a specified <see cref="IEqualityComparer{TSource}"/> to compare values.
        /// </summary>
        /// <param name="comparer">An <see cref="T:System.Collections.Generic.IEqualityComparer`1"/> to compare values.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains distinct elements from the source.</returns>
        public YacqQueryable<TSource> Distinct(IEqualityComparer<TSource> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Distinct(comparer));
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the default equality comparer to compare values.
        /// </summary>
        /// <param name="source2">An <see cref="IEnumerable{TSource}"/> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the set difference of the two sequences.</returns>
        public YacqQueryable<TSource> Except(IEnumerable<TSource> source2)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Except(source2));
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the specified <see cref="IEqualityComparer{TSource}"/> to compare values.
        /// </summary>
        /// <param name="source2">An <see cref="IEnumerable{TSource}"/> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TSource}"/> to compare values.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the set difference of the two sequences.</returns>
        public YacqQueryable<TSource> Except(IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Except(source2, comparer));
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The first element in source that passes the test in <paramref name="predicate"/>.</returns>
        public new TSource First(String predicate)
        {
            return this._source.First(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>default(<typeparamref name="TSource"/>) if source is empty or if no element passes the test specified by <paramref name="predicate"/>; otherwise, the first element in source that passes the test specified by <paramref name="predicate"/>.</returns>
        public new TSource FirstOrDefault(String predicate)
        {
            return this._source.FirstOrDefault(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="IGrouping{TKey,TElement}"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <param name="resultSelector"><c>(it, e) =></c> A function to create a result value from each group.</param>
        /// <returns>An <see cref="YacqQueryable{TResult}"/> that has a type argument of <typeparamref name="TResult"/> and where each element represents a projection over a group and its key.</returns>
        public YacqQueryable<TResult> GroupBy<TKey, TElement, TResult>(String keySelector, String elementSelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseFunc<TSource, TElement>(this.Symbols, elementSelector),
                YacqServices.ParseLambda<Func<TKey, IEnumerable<TElement>, TResult>>(this.Symbols, resultSelector, "it", "e")
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. Keys are compared by using a specified comparer and the elements of each group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="IGrouping{TKey,TElement}"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <param name="resultSelector"><c>(it, e) =></c> A function to create a result value from each group.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that has a type argument of <typeparamref name="TResult"/> and where each element represents a projection over a group and its key.</returns>
        public YacqQueryable<TResult> GroupBy<TKey, TElement, TResult>(String keySelector, String elementSelector, String resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseFunc<TSource, TElement>(this.Symbols, elementSelector),
                YacqServices.ParseLambda<Func<TKey, IEnumerable<TElement>, TResult>>(this.Symbols, resultSelector, "it", "e"),
                comparer
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and projects the elements for each group by using a specified function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="IGrouping{TKey,TElement}"/>.</typeparam>
        /// <returns>An YacqQueryable&lt;IGrouping&lt;TKey, TElement&gt;&gt; where each <see cref="IGrouping{TKey,TElement}"/> contains a sequence of objects of type <typeparamref name="TElement"/> and a key.</returns>
        public YacqQueryable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(String keySelector, String elementSelector)
        {
            return new YacqQueryable<IGrouping<TKey, TElement>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseFunc<TSource, TElement>(this.Symbols, elementSelector)
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence and projects the elements for each group by using a specified function. Key values are compared by using a specified comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector"/>.</typeparam>
        /// <typeparam name="TElement">The type of the elements in each <see cref="IGrouping{TKey,TElement}"/>.</typeparam>
        /// <returns>An YacqQueryable&lt;IGrouping&lt;TKey, TElement&gt;&gt; where each <see cref="IGrouping{TKey,TElement}"/> contains a sequence of objects of type <typeparamref name="TElement"/> and a key.</returns>
        public YacqQueryable<IGrouping<TKey, TElement>> GroupBy<TKey, TElement>(String keySelector, String elementSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<IGrouping<TKey, TElement>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseFunc<TSource, TElement>(this.Symbols, elementSelector),
                comparer
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <returns>An YacqQueryable&lt;IGrouping&lt;TKey, TSource&gt;&gt; where each <see cref="IGrouping{TKey,TElement}"/> object contains a sequence of objects and a key.</returns>
        public YacqQueryable<IGrouping<TKey, TSource>> GroupBy<TKey>(String keySelector)
        {
            return new YacqQueryable<IGrouping<TKey, TSource>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and compares the keys by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented in <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>An IQueryable&lt;IGrouping&lt;TKey, TSource&gt;&gt; where each <see cref="IGrouping{TKey,TElement}"/> contains a sequence of objects and a key.</returns>
        public YacqQueryable<IGrouping<TKey, TSource>> GroupBy<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<IGrouping<TKey, TSource>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value returned by <paramref name="resultSelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <param name="resultSelector"><c>(it, e) =></c> A function to create a result value from each group.</param>
        /// <returns>An <see cref="YacqQueryable{TResult}"/> that has a type argument of <typeparamref name="TResult"/> and where each element represents a projection over a group and its key.</returns>
        public YacqQueryable<TResult> GroupBy<TResult>(String keySelector, String elementSelector, String resultSelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, typeof(TSource), keySelector);
            var elementLambda = YacqServices.ParseLambda(this.Symbols, typeof(TSource), elementSelector);
            var resultLambda = YacqServices.ParseLambda(this.Symbols, resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, keyLambda.ReturnType, "it"),
                YacqExpression.AmbiguousParameter(this.Symbols, typeof(IEnumerable<>).MakeGenericType(elementLambda.ReturnType), "e")
            );
            return new YacqQueryable<TResult>(this.Symbols, this.Provider.CreateQuery<TResult>(Expression.Call(
                typeof(Queryable),
                "GroupBy",
                new [] { typeof(TSource), keyLambda.ReturnType, elementLambda.ReturnType, resultLambda.ReturnType, },
                this.Expression,
                keyLambda,
                elementLambda,
                resultLambda
            )));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results. The default equality comparer is used to compare keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector"><c>(o, i) =></c>A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements of type <typeparamref name="TResult"/> obtained by performing a grouped join on two sequences.</returns>
        public YacqQueryable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupJoin(
                inner,
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseFunc<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, IEnumerable<TInner>, TResult>>(this.Symbols, resultSelector, "o", "i")
            ));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results. A specified <see cref="IEqualityComparer{TKey}"/> is used to compare keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector"><c>(o, i) =></c> A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <param name="comparer">A comparer to hash and compare keys.</param>
        /// <returns>An <see cref="IEqualityComparer{TKey}"/> that contains elements of type <typeparamref name="TResult"/> obtained by performing a grouped join on two sequences.</returns>
        public YacqQueryable<TResult> GroupJoin<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.GroupJoin(
                inner,
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseFunc<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, IEnumerable<TInner>, TResult>>(this.Symbols, resultSelector, "o", "i"),
                comparer
            ));
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the default equality comparer to compare values.
        /// </summary>
        /// <param name="source2">A sequence whose distinct elements that also appear in the first sequence are returned.</param>
        /// <returns>A sequence that contains the set intersection of the two sequences.</returns>
        public YacqQueryable<TSource> Intersect(IEnumerable<TSource> source2)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Intersect(source2));
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the specified <see cref="IEqualityComparer{TSource}"/> to compare values.
        /// </summary>
        /// <param name="source2">An <see cref="IEnumerable{TSource}"/> whose distinct elements that also appear in the first sequence are returned.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TSource}"/> to compare values.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the set intersection of the two sequences.</returns>
        public YacqQueryable<TSource> Intersect(IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Intersect(source2, comparer));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector"><c>(o, i) =></c> A function to create a result element from two matching elements.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that has elements of type <typeparamref name="TResult"/> obtained by performing an inner join on two sequences.</returns>
        public YacqQueryable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Join(
                inner,
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseFunc<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, TInner, TResult>>(this.Symbols, resultSelector, "o", "i")
            ));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys. A specified <see cref="IEqualityComparer{TKey}"/> is used to compare keys.
        /// </summary>
        /// <typeparam name="TInner">The type of the elements of the second sequence.</typeparam>
        /// <typeparam name="TKey">The type of the keys returned by the key selector functions.</typeparam>
        /// <typeparam name="TResult">The type of the result elements.</typeparam>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector"><c>(o, i) =></c> A function to create a result element from two matching elements.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to hash and compare keys.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that has elements of type <typeparamref name="TResult"/> obtained by performing an inner join on two sequences.</returns>
        public YacqQueryable<TResult> Join<TInner, TKey, TResult>(IEnumerable<TInner> inner, String outerKeySelector, String innerKeySelector, String resultSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Join(
                inner,
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseFunc<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, TInner, TResult>>(this.Symbols, resultSelector, "o", "i"),
                comparer
            ));
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The last element in source that passes the test specified by <paramref name="predicate"/>.</returns>
        public new TSource Last(String predicate)
        {
            return this._source.Last(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>default(<typeparamref name="TSource"/>) if source is empty or if no elements pass the test in the predicate function; otherwise, the last element of source that passes the test in the predicate function.</returns>
        public new TSource LastOrDefault(String predicate)
        {
            return this._source.LastOrDefault(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns an <see cref="Int64"/> that represents the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The number of elements in source that satisfy the condition in the predicate function.</returns>
        public new Int64 LongCount(String predicate)
        {
            return this._source.LongCount(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Filters the elements of an <see cref="IQueryable"/> based on a specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <returns>A collection that contains the elements from the source that have type <typeparamref name="TResult"/>.</returns>
        public new YacqQueryable<TResult> OfType<TResult>()
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.OfType<TResult>());
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable<TSource> OrderBy<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable<TSource> OrderBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public new YacqOrderedQueryable<TSource> OrderBy(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, typeof(TSource), keySelector);
            return new YacqOrderedQueryable<TSource>(this.Symbols, (IOrderedQueryable<TSource>) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new [] { typeof(TSource), keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public YacqOrderedQueryable<TSource> OrderByDescending<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderByDescending(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function that is represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TKey}"/> to compare keys.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public YacqOrderedQueryable<TSource> OrderByDescending<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.OrderByDescending(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public new YacqOrderedQueryable<TSource> OrderByDescending(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, typeof(TSource), keySelector);
            return new YacqOrderedQueryable<TSource>(this.Symbols, (IOrderedQueryable<TSource>) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "OrderByDescending",
                new [] { typeof(TSource), keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Inverts the order of the elements in a sequence.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> whose elements correspond to those of the input sequence in reverse order.</returns>
        public new YacqQueryable<TSource> Reverse()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Reverse());
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector"/>.</typeparam>
        /// <param name="selector"><c>(it) =></c> A projection function to apply to each element.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking a projection function on each element of source.</returns>
        public YacqQueryable<TResult> Select<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Select(
                YacqServices.ParseFunc<TSource, TResult>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TSource}"/> and invokes a result selector function on each element therein. The resulting values from each intermediate sequence are combined into a single, one-dimensional sequence and returned.
        /// </summary>
        /// <typeparam name="TCollection">The type of the intermediate elements collected by the function represented by <paramref name="collectionSelector"/>.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="collectionSelector"><c>(it) =></c> A projection function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector"><c>(it, c) =></c> A projection function to apply to each element of each intermediate sequence.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector"/> on each element of source and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        public YacqQueryable<TResult> SelectMany<TCollection, TResult>(String collectionSelector, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseFunc<TSource, IEnumerable<TCollection>>(this.Symbols, collectionSelector),
                YacqServices.ParseLambda<Func<TSource, TCollection, TResult>>(this.Symbols, resultSelector, "it", "c")
            ));
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TSource}"/> and invokes a result selector function on each element therein. The resulting values from each intermediate sequence are combined into a single, one-dimensional sequence and returned.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements of the resulting sequence.</typeparam>
        /// <param name="collectionSelector"><c>(it) =></c> A projection function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector"><c>(it, c) =></c> A projection function to apply to each element of each intermediate sequence.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector"/> on each element of source and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        public YacqQueryable<TResult> SelectMany<TResult>(String collectionSelector, String resultSelector)
        {
            var collectionLambda = YacqServices.ParseLambda(this.Symbols, typeof(TSource), collectionSelector);
            var collectionType = collectionLambda.ReturnType.GetEnumerableElementType();
            var resultLambda = YacqServices.ParseLambda(this.Symbols, typeof(TResult), resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, typeof(TSource), "it"),
                YacqExpression.AmbiguousParameter(this.Symbols, collectionLambda.ReturnType, "c")
            );
            return new YacqQueryable<TResult>(this.Symbols, this.Provider.CreateQuery<TResult>(Expression.Call(
                typeof(Queryable),
                "Select",
                new [] { typeof(TSource), collectionType, typeof(TResult), },
                this.Expression,
                collectionLambda,
                resultLambda
            )));
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TSource}"/> and combines the resulting sequences into one sequence.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements of the sequence returned by the function represented by <paramref name="selector"/>.</typeparam>
        /// <param name="selector"><c>(it) =></c> A projection function to apply to each element.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.</returns>
        public YacqQueryable<TResult> SelectMany<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseFunc<TSource, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate"/>.</returns>
        public new TSource Single(String predicate)
        {
            return this._source.Single(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate"/>, or default(<typeparamref name="TSource"/>) if no such element is found.</returns>
        public new TSource SingleOrDefault(String predicate)
        {
            return this._source.SingleOrDefault(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains elements that occur after the specified index in the input sequence.</returns>
        public new YacqQueryable<TSource> Skip(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Skip(count));
        }

        /// <summary>
        /// Bypasses elements in a sequence as long as a specified condition is <c>true</c> and then returns the remaining elements.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from source starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate"/>.</returns>
        public new YacqQueryable<TSource> SkipWhile(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.SkipWhile(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the specified number of elements from the start of the source.</returns>
        public new YacqQueryable<TSource> Take(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Take(count));
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is <c>true</c>.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate"/> no longer passes.</returns>
        public new YacqQueryable<TSource> TakeWhile(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.TakeWhile(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Produces the set union of two sequences by using the default equality comparer.
        /// </summary>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
        public YacqQueryable<TSource> Union(IEnumerable<TSource> source2)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Union(source2));
        }

        /// <summary>
        /// Produces the set union of two sequences by using a specified <see cref="IEqualityComparer{TSource}"/>.
        /// </summary>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <param name="comparer">An <see cref="IEqualityComparer{TSource}"/> to compare values.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
        public YacqQueryable<TSource> Union(IEnumerable<TSource> source2, IEqualityComparer<TSource> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Union(source2, comparer));
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        public new YacqQueryable<TSource> Where(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Where(
                YacqServices.ParseFunc<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Merges two sequences by using the specified predicate function.
        /// </summary>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="source2">The second sequence to merge.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> A function that specifies how to merge the elements from the two sequences.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains merged elements of two input sequences.</returns>
        public YacqQueryable<TResult> Zip<TResult>(IEnumerable source2, String resultSelector)
        {
            var source2Type = source2.GetType().GetEnumerableElementType();
            var resultLambda = YacqServices.ParseLambda(this.Symbols, typeof(TResult), resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "it"),
                YacqExpression.AmbiguousParameter(this.Symbols, source2Type, "it2")
            );
            return new YacqQueryable<TResult>(this.Symbols, this.Provider.CreateQuery<TResult>(Expression.Call(
                typeof(Queryable),
                "Zip",
                new [] { typeof(TResult), source2Type, typeof(TResult), },
                this.Expression,
                Expression.Constant(source2),
                resultLambda
            )));
        }

        /// <summary>
        /// Merges two sequences by using the specified predicate function.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="source2">The second sequence to merge.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> A function that specifies how to merge the elements from the two sequences.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains merged elements of two input sequences.</returns>
        public YacqQueryable<TResult> Zip<TSecond, TResult>(IEnumerable<TSecond> source2, String resultSelector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Zip(
                source2,
                YacqServices.ParseLambda<Func<TSource, TSecond, TResult>>(this.Symbols, resultSelector, "it", "it2")
            ));
        }
    }

    partial class YacqOrderedQueryable<TSource>
    {
        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable<TSource> ThenBy<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="IComparer{TKey}"/> to compare keys.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable<TSource> ThenBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenBy(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable<TSource> ThenBy(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqOrderedQueryable<TSource>(this.Symbols, (IOrderedQueryable<TSource>) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "ThenBy",
                new [] { typeof(TSource), keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.
        /// </summary>
        /// <typeparam name="TKey">The type of the key returned by the function represented by <paramref name="keySelector"/>.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public YacqOrderedQueryable<TSource> ThenByDescending<TKey>(String keySelector)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenByDescending(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in descending order by using a specified comparer.
        /// </summary>
        /// <typeparam name="TKey">The type of the key that is returned by the <paramref name="keySelector"/> function.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="comparer">An <see cref="IComparer{TKey}"/> to compare keys.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending according to a key.</returns>
        public YacqOrderedQueryable<TSource> ThenByDescending<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqOrderedQueryable<TSource>(this.Symbols, this._source.ThenByDescending(
                YacqServices.ParseFunc<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public YacqOrderedQueryable<TSource> ThenByDescending(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqOrderedQueryable<TSource>(this.Symbols, (IOrderedQueryable<TSource>) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "ThenByDescending",
                new [] { typeof(TSource), keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }
    }

    partial class YacqQueryable
    {
        /// <summary>
        /// Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value, and the specified function is used to select the result value.
        /// </summary>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func"><c>(a, it) =></c> An accumulator function to invoke on each element.</param>
        /// <param name="selector"><c>(it) =></c> A function to transform the final accumulator value into the result value.</param>
        /// <returns>The transformed final accumulator value.</returns>
        public dynamic Aggregate(Object seed, String func, String selector)
        {
            var seedType = seed.GetType();
            var funcLambda = YacqServices.ParseLambda(this.Symbols, func,
                YacqExpression.AmbiguousParameter(this.Symbols, seedType, "a"),
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "it")
            );
            var selectorLambda = YacqServices.ParseLambda(this.Symbols, seedType, selector);
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Aggregate",
                new [] { this.ElementType, seedType, selectorLambda.ReturnType, },
                this.Expression,
                Expression.Constant(seed),
                funcLambda,
                selectorLambda
            ));
        }

        /// <summary>
        /// Applies an accumulator function over a sequence. The specified seed value is used as the initial accumulator value.
        /// </summary>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="func"><c>(a, it) =></c> An accumulator function to invoke on each element.</param>
        /// <returns>The final accumulator value.</returns>
        public dynamic Aggregate(Object seed, String func)
        {
            var seedType = seed.GetType();
            var funcLambda = YacqServices.ParseLambda(this.Symbols, func,
                YacqExpression.AmbiguousParameter(this.Symbols, seedType, "a"),
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "it")
            );
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Aggregate",
                new [] { this.ElementType, seedType, },
                this.Expression,
                Expression.Constant(seed),
                funcLambda
            ));
        }

        /// <summary>
        /// Applies an accumulator function over a sequence.
        /// </summary>
        /// <param name="func"><c>(a, it) =></c> An accumulator function to apply to each element.</param>
        /// <returns>The final accumulator value.</returns>
        public dynamic Aggregate(String func)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Aggregate",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, func,
                    YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "a"),
                    YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "it")
                )
            ));
        }

        /// <summary>
        /// Determines whether all the elements of a sequence satisfy a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns><c>true</c> if every element of the source sequence passes the test in the specified predicate, or if the sequence is empty; otherwise, <c>false</c>.</returns>
        public Boolean All(String predicate)
        {
            return this.Provider.Execute<Boolean>(Expression.Call(
                typeof(Queryable),
                "All",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns><c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.</returns>
        public Boolean Any(String predicate)
        {
            return this.Provider.Execute<Boolean>(Expression.Call(
                typeof(Queryable),
                "Any",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Determines whether a sequence contains any elements.
        /// </summary>
        /// <returns><c>true</c> if the source sequence contains any elements; otherwise, <c>false</c>.</returns>
        public Boolean Any()
        {
            return this.Provider.Execute<Boolean>(Expression.Call(
                typeof(Queryable),
                "Any",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Converts a generic <see cref="IEnumerable{TSource}"/> to a generic <see cref="YacqQueryable{TSource}"/>.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that represents the input sequence.</returns>
        public YacqQueryable AsQueryable()
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "AsQueryable",
                new [] { this.ElementType, },
                this.Expression
            )));
        }

        /// <summary>
        /// Converts the elements of an <see cref="IQueryable"/> to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the elements of the source to.</typeparam>
        /// <returns>An <see cref="YacqQueryable{TResult}"/> that contains each element of the source sequence converted to the specified type.</returns>
        public YacqQueryable<TResult> Cast<TResult>()
        {
            return new YacqQueryable<TResult>(this.Symbols, this.Provider.CreateQuery<TResult>(Expression.Call(
                typeof(Queryable),
                "Cast",
                new [] { typeof(TResult), },
                this.Expression
            )));
        }

        /// <summary>
        /// Concatenates two sequences.
        /// </summary>
        /// <param name="source2">The sequence to concatenate to the first sequence.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the concatenated elements of the two input sequences.</returns>
        public YacqQueryable Concat(IEnumerable source2)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Concat",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(source2)
            )));
        }

        /// <summary>
        /// Determines whether a sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="item">The object to locate in the sequence.</param>
        /// <returns><c>true</c> if the input sequence contains an element that has the specified value; otherwise, <c>false</c>.</returns>
        public Boolean Contains(Object item)
        {
            return this.Provider.Execute<Boolean>(Expression.Call(
                typeof(Queryable),
                "Contains",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(item)
            ));
        }

        /// <summary>
        /// Returns the number of elements in a sequence.
        /// </summary>
        /// <returns>The number of elements in the input sequence.</returns>
        public Int32 Count()
        {
            return this.Provider.Execute<Int32>(Expression.Call(
                typeof(Queryable),
                "Count",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the number of elements in the specified sequence that satisfies a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The number of elements in the sequence that satisfies the condition in the predicate function.</returns>
        public Int32 Count(String predicate)
        {
            return this.Provider.Execute<Int32>(Expression.Call(
                typeof(Queryable),
                "Count",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains default value if the source is empty; otherwise, the source.</returns>
        public YacqQueryable DefaultIfEmpty()
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "DefaultIfEmpty",
                new [] { this.ElementType, },
                this.Expression
            )));
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the specified value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <param name="defaultValue">The value to return if the sequence is empty.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains <paramref name="defaultValue"/> if the source is empty; otherwise, the source.</returns>
        public YacqQueryable DefaultIfEmpty(Object defaultValue)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "DefaultIfEmpty",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(defaultValue)
            )));
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains distinct elements from the source.</returns>
        public YacqQueryable Distinct()
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Distinct",
                new [] { this.ElementType, },
                this.Expression
            )));
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>The element at the specified position in the source.</returns>
        public dynamic ElementAt(Int32 index)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "ElementAt",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(index)
            ));
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence or a default value if the index is out of range.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>Default value if <paramref name="index" /> is outside the bounds of the source; otherwise, the element at the specified position in the source.</returns>
        public dynamic ElementAtOrDefault(Int32 index)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "ElementAtOrDefault",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(index)
            ));
        }

        /// <summary>
        /// Produces the set difference of two sequences by using the default equality comparer to compare values.
        /// </summary>
        /// <param name="source2">An <see cref="IEnumerable{TSource}"/> whose elements that also occur in the first sequence will not appear in the returned sequence.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the set difference of the two sequences.</returns>
        public YacqQueryable Except(IEnumerable source2)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Except",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(source2)
            )));
        }

        /// <summary>
        /// Returns the first element of a sequence.
        /// </summary>
        /// <returns>The first element in the source.</returns>
        public dynamic First()
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "First",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The first element in source that passes the test in <paramref name="predicate"/>.</returns>
        public dynamic First(String predicate)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "First",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Returns the first element of a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <returns>Default value if the source is empty; otherwise, the first element in the source.</returns>
        public dynamic FirstOrDefault()
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "FirstOrDefault",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>Default value if source is empty or if no element passes the test specified by <paramref name="predicate"/>; otherwise, the first element in source that passes the test specified by <paramref name="predicate"/>.</returns>
        public dynamic FirstOrDefault(String predicate)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "FirstOrDefault",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and creates a result value from each group and its key. The elements of each group are projected by using a specified function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <param name="resultSelector"><c>(it, e) =></c> A function to create a result value from each group.</param>
        /// <returns>An <see cref="YacqQueryable{TResult}"/> that has the result type and where each element represents a projection over a group and its key.</returns>
        public YacqQueryable GroupBy(String keySelector, String elementSelector, String resultSelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            var elementLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, elementSelector);
            var resultLambda = YacqServices.ParseLambda(this.Symbols, resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, keyLambda.ReturnType, "it"),
                YacqExpression.AmbiguousParameter(this.Symbols, typeof(IEnumerable<>).MakeGenericType(elementLambda.ReturnType), "e")
            );
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "GroupBy",
                new [] { this.ElementType, keyLambda.ReturnType, elementLambda.ReturnType, resultLambda.ReturnType, },
                this.Expression,
                keyLambda,
                elementLambda,
                resultLambda
            )));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function and projects the elements for each group by using a specified function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an <see cref="IGrouping{TKey,TElement}"/>.</param>
        /// <returns>An YacqQueryable&lt;IGrouping&lt;TKey, TElement&gt;&gt; where each <see cref="IGrouping{TKey,TElement}"/> contains a sequence of objects of the element type and a key.</returns>
        public YacqQueryable GroupBy(String keySelector, String elementSelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            var elementLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, elementSelector);
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "GroupBy",
                new [] { this.ElementType, keyLambda.ReturnType, elementLambda.ReturnType, },
                this.Expression,
                keyLambda,
                elementLambda
            )));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <returns>An YacqQueryable&lt;IGrouping&lt;TKey, TSource&gt;&gt; where each <see cref="IGrouping{TKey,TElement}"/> object contains a sequence of objects and a key.</returns>
        public YacqQueryable GroupBy(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "GroupBy",
                new [] { this.ElementType, keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on key equality and groups the results. The default equality comparer is used to compare keys.
        /// </summary>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector"><c>(o, i) =></c>A function to create a result element from an element from the first sequence and a collection of matching elements from the second sequence.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements of the result type obtained by performing a grouped join on two sequences.</returns>
        public YacqQueryable GroupJoin(IEnumerable inner, String outerKeySelector, String innerKeySelector, String resultSelector)
        {
            var innerType = inner.GetType().GetEnumerableElementType();
            var outerKeyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, outerKeySelector);
            var innerKeyLambda = YacqServices.ParseLambda(this.Symbols, innerType, innerKeySelector);
            var resultLambda = YacqServices.ParseLambda(this.Symbols, resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "o"),
                YacqExpression.AmbiguousParameter(this.Symbols, typeof(IEnumerable<>).MakeGenericType(innerType), "i")
            );
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "GroupJoin",
                new [] { this.ElementType, innerType, outerKeyLambda.ReturnType, resultLambda.ReturnType, },
                this.Expression,
                Expression.Constant(inner),
                outerKeyLambda,
                innerKeyLambda,
                resultLambda
            )));
        }

        /// <summary>
        /// Produces the set intersection of two sequences by using the default equality comparer to compare values.
        /// </summary>
        /// <param name="source2">A sequence whose distinct elements that also appear in the first sequence are returned.</param>
        /// <returns>A sequence that contains the set intersection of the two sequences.</returns>
        public YacqQueryable Intersect(IEnumerable source2)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Intersect",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(source2)
            )));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.
        /// </summary>
        /// <param name="inner">The sequence to join to the first sequence.</param>
        /// <param name="outerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the first sequence.</param>
        /// <param name="innerKeySelector"><c>(it) =></c> A function to extract the join key from each element of the second sequence.</param>
        /// <param name="resultSelector"><c>(o, i) =></c> A function to create a result element from two matching elements.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that has elements of the result type obtained by performing an inner join on two sequences.</returns>
        public YacqQueryable Join(IEnumerable inner, String outerKeySelector, String innerKeySelector, String resultSelector)
        {
            var innerType = inner.GetType().GetEnumerableElementType();
            var outerKeyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, outerKeySelector);
            var innerKeyLambda = YacqServices.ParseLambda(this.Symbols, innerType, innerKeySelector);
            var resultLambda = YacqServices.ParseLambda(this.Symbols, resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "o"),
                YacqExpression.AmbiguousParameter(this.Symbols, innerType, "i")
            );
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Join",
                new [] { this.ElementType, innerType, outerKeyLambda.ReturnType, resultLambda.ReturnType, },
                this.Expression,
                Expression.Constant(inner),
                outerKeyLambda,
                innerKeyLambda,
                resultLambda
            )));
        }

        /// <summary>
        /// Returns the last element in a sequence.
        /// </summary>
        /// <returns>The value at the last position in the source.</returns>
        public dynamic Last()
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Last",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The last element in source that passes the test specified by <paramref name="predicate"/>.</returns>
        public dynamic Last(String predicate)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Last",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Returns the last element in a sequence, or a default value if the sequence contains no elements.
        /// </summary>
        /// <returns>Default value if the source is empty; otherwise, the last element in the source.</returns>
        public dynamic LastOrDefault()
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "LastOrDefault",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>Default value if source is empty or if no elements pass the test in the predicate function; otherwise, the last element of source that passes the test in the predicate function.</returns>
        public dynamic LastOrDefault(String predicate)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "LastOrDefault",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Returns an <see cref="Int64" /> that represents the total number of elements in a sequence.
        /// </summary>
        /// <returns>The number of elements in the source.</returns>
        public Int64 LongCount()
        {
            return this.Provider.Execute<Int64>(Expression.Call(
                typeof(Queryable),
                "LongCount",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns an <see cref="Int64"/> that represents the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The number of elements in source that satisfy the condition in the predicate function.</returns>
        public Int64 LongCount(String predicate)
        {
            return this.Provider.Execute<Int64>(Expression.Call(
                typeof(Queryable),
                "LongCount",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Filters the elements of an <see cref="IQueryable"/> based on a specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <returns>A collection that contains the elements from the source that have type <typeparamref name="TResult"/>.</returns>
        public YacqQueryable<TResult> OfType<TResult>()
        {
            return new YacqQueryable<TResult>(this.Symbols, this.Provider.CreateQuery<TResult>(Expression.Call(
                typeof(Queryable),
                "OfType",
                new [] { typeof(TResult), },
                this.Expression
            )));
        }

        /// <summary>
        /// Sorts the elements of a sequence in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable OrderBy(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqOrderedQueryable(this.Symbols, (IOrderedQueryable) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "OrderBy",
                new [] { this.ElementType, keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Sorts the elements of a sequence in descending order according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from an element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public YacqOrderedQueryable OrderByDescending(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqOrderedQueryable(this.Symbols, (IOrderedQueryable) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "OrderByDescending",
                new [] { this.ElementType, keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Inverts the order of the elements in a sequence.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> whose elements correspond to those of the input sequence in reverse order.</returns>
        public YacqQueryable Reverse()
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Join",
                new [] { this.ElementType, },
                this.Expression
            )));
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A projection function to apply to each element.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking a projection function on each element of source.</returns>
        public YacqQueryable Select(String selector)
        {
            var selectorLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, selector);
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Select",
                new [] { this.ElementType, selectorLambda.ReturnType, },
                this.Expression,
                selectorLambda
            )));
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TSource}"/> and invokes a result selector function on each element therein. The resulting values from each intermediate sequence are combined into a single, one-dimensional sequence and returned.
        /// </summary>
        /// <param name="collectionSelector"><c>(it) =></c> A projection function to apply to each element of the input sequence.</param>
        /// <param name="resultSelector"><c>(it, c) =></c> A projection function to apply to each element of each intermediate sequence.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking the one-to-many projection function <paramref name="collectionSelector"/> on each element of source and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        public YacqQueryable SelectMany(String collectionSelector, String resultSelector)
        {
            var collectionLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, collectionSelector);
            var collectionType = collectionLambda.ReturnType.GetEnumerableElementType();
            var resultLambda = YacqServices.ParseLambda(this.Symbols, resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "it"),
                YacqExpression.AmbiguousParameter(this.Symbols, collectionLambda.ReturnType, "c")
            );
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Select",
                new [] { this.ElementType, collectionType, resultLambda.ReturnType, },
                this.Expression,
                collectionLambda,
                resultLambda
            )));
        }

        /// <summary>
        /// Projects each element of a sequence to an <see cref="IEnumerable{TSource}"/> and combines the resulting sequences into one sequence.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A projection function to apply to each element.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> whose elements are the result of invoking a one-to-many projection function on each element of the input sequence.</returns>
        public YacqQueryable SelectMany(String selector)
        {
            var selectorLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, selector);
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "SelectMany",
                new [] { this.ElementType, selectorLambda.ReturnType, },
                this.Expression,
                selectorLambda
            )));
        }

        /// <summary>
        /// Determines whether two sequences are equal by using the default equality comparer to compare elements.
        /// </summary>
        /// <param name="source2">An <see cref="IEnumerable" /> whose elements to compare to those of the first sequence.</param>
        /// <returns><c>true</c> if the two source sequences are of equal length and their corresponding elements compare equal; otherwise, <c>>false</c>.</returns>
        public Boolean SequenceEqual(IEnumerable source2)
        {
            return this.Provider.Execute<Boolean>(Expression.Call(
                typeof(Queryable),
                "SequenceEqual",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(source2)
            ));
        }

        /// <summary>
        /// Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.
        /// </summary>
        /// <returns>The single element of the input sequence.</returns>
        public dynamic Single()
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Single",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate"/>.</returns>
        public dynamic Single(String predicate)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "Single",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.
        /// </summary>
        /// <returns>The single element of the input sequence, or default value if the sequence contains no elements.</returns>
        public dynamic SingleOrDefault()
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "SingleOrDefault",
                new [] { this.ElementType, },
                this.Expression
            ));
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate"/>, or default value if no such element is found.</returns>
        public dynamic SingleOrDefault(String predicate)
        {
            return this.Provider.Execute(Expression.Call(
                typeof(Queryable),
                "SingleOrDefault",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            ));
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains elements that occur after the specified index in the input sequence.</returns>
        public YacqQueryable Skip(Int32 count)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Skip",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(count)
            )));
        }

        /// <summary>
        /// Bypasses elements in a sequence as long as a specified condition is <c>true</c> and then returns the remaining elements.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from source starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate"/>.</returns>
        public YacqQueryable SkipWhile(String predicate)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "SkipWhile",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            )));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the specified number of elements from the start of the source.</returns>
        public YacqQueryable Take(Int32 count)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Take",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(count)
            )));
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is <c>true</c>.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate"/> no longer passes.</returns>
        public YacqQueryable TakeWhile(String predicate)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "TakeWhile",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            )));
        }

        /// <summary>
        /// Produces the set union of two sequences by using the default equality comparer.
        /// </summary>
        /// <param name="source2">A sequence whose distinct elements form the second set for the union operation.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the elements from both input sequences, excluding duplicates.</returns>
        public YacqQueryable Union(IEnumerable source2)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Union",
                new [] { this.ElementType, },
                this.Expression,
                Expression.Constant(source2)
            )));
        }

        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from the input sequence that satisfy the condition specified by <paramref name="predicate"/>.</returns>
        public YacqQueryable Where(String predicate)
        {
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Where",
                new [] { this.ElementType, },
                this.Expression,
                YacqServices.ParseLambda(this.Symbols, this.ElementType, predicate)
            )));
        }

        /// <summary>
        /// Merges two sequences by using the specified predicate function.
        /// </summary>
        /// <param name="source2">The second sequence to merge.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> A function that specifies how to merge the elements from the two sequences.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains merged elements of two input sequences.</returns>
        public YacqQueryable Zip(IEnumerable source2, String resultSelector)
        {
            var source2Type = source2.GetType().GetEnumerableElementType();
            var resultLambda = YacqServices.ParseLambda(this.Symbols, resultSelector,
                YacqExpression.AmbiguousParameter(this.Symbols, this.ElementType, "it"),
                YacqExpression.AmbiguousParameter(this.Symbols, source2Type, "it2")
            );
            return new YacqQueryable(this.Symbols, this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "Zip",
                new [] { this.ElementType, source2Type, resultLambda.ReturnType, },
                this.Expression,
                Expression.Constant(source2),
                resultLambda
            )));
        }
    }

    partial class YacqOrderedQueryable
    {
        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in ascending order according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted according to a key.</returns>
        public YacqOrderedQueryable ThenBy(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqOrderedQueryable(this.Symbols, (IOrderedQueryable) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "ThenBy",
                new [] { this.ElementType, keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }

        /// <summary>
        /// Performs a subsequent ordering of the elements in a sequence in descending order, according to a key.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>An <see cref="YacqOrderedQueryable{TSource}"/> whose elements are sorted in descending order according to a key.</returns>
        public YacqOrderedQueryable ThenByDescending(String keySelector)
        {
            var keyLambda = YacqServices.ParseLambda(this.Symbols, this.ElementType, keySelector);
            return new YacqOrderedQueryable(this.Symbols, (IOrderedQueryable) this.Provider.CreateQuery(Expression.Call(
                typeof(Queryable),
                "ThenByDescending",
                new [] { this.ElementType, keyLambda.ReturnType, },
                this.Expression,
                keyLambda
            )));
        }
    }
}
// vim:set ft=cs fenc=utf-8 ts=4 sw=4 sts=4 et:
