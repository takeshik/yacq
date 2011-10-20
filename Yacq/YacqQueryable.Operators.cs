﻿// -*- mode: csharp; encoding: utf-8; tab-width: 4; c-basic-offset: 4; indent-tabs-mode: nil; -*-
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
using System.Collections.Generic;
using System.Linq;

namespace XSpect.Yacq
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
                YacqServices.ParseLambda<TAccumulate, TResult>(this.Symbols, selector)
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
        public TSource Aggregate(String func)
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
        public Boolean All(String predicate)
        {
            return this._source.All(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Determines whether any element of a sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns><c>true</c> if any elements in the source sequence pass the test in the specified predicate; otherwise, <c>false</c>.</returns>
        public Boolean Any(String predicate)
        {
            return this._source.Any(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Converts a generic <see cref="IEnumerable{TSource}"/> to a generic <see cref="YacqQueryable{TSource}"/>.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that represents the input sequence.</returns>
        public YacqQueryable<TSource> AsQueryable()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.AsQueryable());
        }

        /// <summary>
        /// Generates a sequence of non-overlapping adjacent buffers over the source sequence.
        /// </summary>
        /// <param name="count">Number of elements for allocated buffers.</param>
        /// <returns>Sequence of buffers containing source sequence elements.</returns>
        public YacqQueryable<IList<TSource>> Buffer(Int32 count)
        {
            return new YacqQueryable<IList<TSource>>(this.Symbols, this._source.Buffer(count));
        }

        /// <summary>
        /// Generates a sequence of buffers over the source sequence, with specified length and possible overlap.
        /// </summary>
        /// <param name="count">Number of elements for allocated buffers.</param>
        /// <param name="skip">Number of elements to skip between the start of consecutive buffers.</param>
        /// <returns>Sequence of buffers containing source sequence elements.</returns>
        public YacqQueryable<IList<TSource>> Buffer(Int32 count, Int32 skip)
        {
            return new YacqQueryable<IList<TSource>>(this.Symbols, this._source.Buffer(count, skip));
        }

        /// <summary>
        /// Converts the elements of an <see cref="IQueryable"/> to the specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to convert the elements of the source to.</typeparam>
        /// <returns>An <see cref="YacqQueryable{TResult}"/> that contains each element of the source sequence converted to the specified type.</returns>
        public YacqQueryable<TResult> Cast<TResult>()
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Cast<TResult>());
        }

        /// <summary>
        /// Creates a sequence that corresponds to the source sequence, concatenating it with the sequence resulting from calling an exception handler function in case of an error.
        /// </summary>
        /// <typeparam name="TException">Exception type to catch.</typeparam>
        /// <param name="handler"><c>(ex) =></c> Handler to invoke when an exception of the specified type occurs.</param>
        /// <returns>Source sequence, concatenated with an exception handler result sequence in case of an error.</returns>
        public YacqQueryable<TSource> Catch<TException>(String handler)
            where TException : Exception
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Catch(
                YacqServices.ParseLambda<Func<TException, IEnumerable<TSource>>>(this.Symbols, handler, "ex")
            ));
        }

        /// <summary>
        /// Creates a sequence that returns the elements of the first sequence, switching to the second in case of an error.
        /// </summary>
        /// <param name="second">Second sequence, concatenated to the result in case the first sequence completes exceptionally.</param>
        /// <returns>The first sequence, followed by the second sequence in case an error is produced.</returns>
        public YacqQueryable<TSource> Catch(IEnumerable<TSource> second)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Catch(second));
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
        public Int32 Count(String predicate)
        {
            return this._source.Count(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the type parameter's default value in a singleton collection if the sequence is empty.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains default value if the source is empty; otherwise, the source.</returns>
        public YacqQueryable<TSource> DefaultIfEmpty()
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
        /// Returns elements with a distinct key value by using the default equality comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector.</param>
        /// <returns>Sequence that contains the elements from the source sequence with distinct key values.</returns>
        public YacqQueryable<TSource> Distinct<TKey>(String keySelector)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Distinct(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Returns elements with a distinct key value by using the specified equality comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector.</param>
        /// <param name="comparer">Comparer used to compare key values.</param>
        /// <returns>Sequence that contains the elements from the source sequence with distinct key values.</returns>
        public YacqQueryable<TSource> Distinct<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Distinct(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Returns distinct elements from a sequence by using the default equality comparer to compare values.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains distinct elements from the source.</returns>
        public YacqQueryable<TSource> Distinct()
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
        /// Returns consecutive distinct elements based on a key value by using the specified equality comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector.</param>
        /// <returns>Sequence without adjacent non-distinct elements.</returns>
        public YacqQueryable<TSource> DistinctUntilChanged<TKey>(String keySelector)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DistinctUntilChanged(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Returns consecutive distinct elements based on a key value by using the specified equality comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector.</param>
        /// <param name="comparer">Comparer used to compare key values.</param>
        /// <returns>Sequence without adjacent non-distinct elements.</returns>
        public YacqQueryable<TSource> DistinctUntilChanged<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DistinctUntilChanged(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Returns consecutive distinct elements by using the default equality comparer to compare values.
        /// </summary>
        /// <returns>Sequence without adjacent non-distinct elements.</returns>
        public YacqQueryable<TSource> DistinctUntilChanged()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DistinctUntilChanged());
        }

        /// <summary>
        /// Returns consecutive distinct elements by using the specified equality comparer to compare values.
        /// </summary>
        /// <param name="comparer">Comparer used to compare values.</param>
        /// <returns>Sequence without adjacent non-distinct elements.</returns>
        public YacqQueryable<TSource> DistinctUntilChanged(IEqualityComparer<TSource> comparer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DistinctUntilChanged(comparer));
        }

#if !SILVERLIGHT
        /// <summary>
        /// Lazily invokes observer methods for each value in the sequence, and upon successful or exceptional termination.
        /// </summary>
        /// <param name="observer">Observer to invoke notification calls on.</param>
        /// <returns>Sequence exhibiting the side-effects of observer method invocation upon enumeration.</returns>
        public YacqQueryable<TSource> Do(IObserver<TSource> observer)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(observer));
        }
#endif

        /// <summary>
        /// Lazily invokes an action for each value in the sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element.</param>
        /// <returns>Sequence exhibiting the specified side-effects upon enumeration.</returns>
        public YacqQueryable<TSource> Do(String onNext)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it")
            ));
        }

        /// <summary>
        /// Lazily invokes an action for each value in the sequence, and executes an action upon exceptional termination.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element.</param>
        /// <param name="onError"><c>(ex) =></c> Action to invoke on exceptional termination of the sequence.</param>
        /// <returns>Sequence exhibiting the specified side-effects upon enumeration.</returns>
        public YacqQueryable<TSource> Do(String onNext, String onError)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action<Exception>>(this.Symbols, onError, "ex")
            ));
        }

        /// <summary>
        /// Lazily invokes an action for each value in the sequence, and executes an action upon successful or exceptional termination.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element.</param>
        /// <param name="onError"><c>(ex) =></c> Action to invoke on exceptional termination of the sequence.</param>
        /// <param name="onCompleted"><c>() =></c> Action to invoke on successful termination of the sequence.</param>
        /// <returns>Sequence exhibiting the specified side-effects upon enumeration.</returns>
        public YacqQueryable<TSource> Do(String onNext, String onError, String onCompleted)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action<Exception>>(this.Symbols, onError, "ex"),
                YacqServices.ParseLambda<Action>(this.Symbols, onCompleted, new String[0])
            ));
        }

        /// <summary>
        /// Generates an enumerable sequence by repeating a source sequence as long as the given loop postcondition holds.
        /// </summary>
        /// <param name="condition"><c>(it) =></c> Loop condition.</param>
        /// <returns>Sequence generated by repeating the given sequence until the condition evaluates to <c>false</c>.</returns>
        public YacqQueryable DoWhile(String condition)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.DoWhile(
                YacqServices.ParseLambda<Boolean>(this.Symbols, condition)
            ));
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
        /// Expands the sequence by recursively applying a selector function.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function to retrieve the next sequence to expand.</param>
        /// <returns>Sequence with results from the recursive expansion of the source sequence.</returns>
        public YacqQueryable<TSource> Expand(String selector)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Expand(
                YacqServices.ParseLambda<TSource, IEnumerable<TSource>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Creates a sequence whose termination or disposal of an enumerator causes a finally action to be executed.
        /// </summary>
        /// <param name="finallyAction"><c>() =></c> Action to run upon termination of the sequence, or when an enumerator is disposed.</param>
        /// <returns>Source sequence with guarantees on the invocation of the finally action.</returns>
        public YacqQueryable<TSource> Finally(String finallyAction)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Finally(
                YacqServices.ParseLambda<Action>(this.Symbols, finallyAction, new String[0])
            ));
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The first element in source that passes the test in <paramref name="predicate"/>.</returns>
        public TSource First(String predicate)
        {
            return this._source.First(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>default(<typeparamref name="TSource"/>) if source is empty or if no element passes the test specified by <paramref name="predicate"/>; otherwise, the first element in source that passes the test specified by <paramref name="predicate"/>.</returns>
        public TSource FirstOrDefault(String predicate)
        {
            return this._source.FirstOrDefault(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, IEnumerable<TInner>, TResult>>(this.Symbols, resultSelector, "o", "i"),
                comparer
            ));
        }

        /// <summary>
        /// Hides the enumerable sequence object identity.
        /// </summary>
        /// <returns>Enumerable sequence with the same behavior as the original, but hiding the source object identity.</returns>
        /// <remarks>AsQueryable doesn't hide the object identity, and simply acts as a cast to the IQueryable&lt;TSource&gt; interface.</remarks>
        public YacqQueryable<TSource> Hide()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Hide());
        }

        /// <summary>
        /// Ignores all elements in the source sequence.
        /// </summary>
        /// <returns>Source sequence without its elements.</returns>
        public YacqQueryable<TSource> IgnoreElements()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.IgnoreElements());
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, outerKeySelector),
                YacqServices.ParseLambda<TInner, TKey>(this.Symbols, innerKeySelector),
                YacqServices.ParseLambda<Func<TSource, TInner, TResult>>(this.Symbols, resultSelector, "o", "i"),
                comparer
            ));
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a specified condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The last element in source that passes the test specified by <paramref name="predicate"/>.</returns>
        public TSource Last(String predicate)
        {
            return this._source.Last(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the last element of a sequence that satisfies a condition or a default value if no such element is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>default(<typeparamref name="TSource"/>) if source is empty or if no elements pass the test in the predicate function; otherwise, the last element of source that passes the test in the predicate function.</returns>
        public TSource LastOrDefault(String predicate)
        {
            return this._source.LastOrDefault(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns an <see cref="Int64"/> that represents the number of elements in a sequence that satisfy a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>The number of elements in source that satisfy the condition in the predicate function.</returns>
        public Int64 LongCount(String predicate)
        {
            return this._source.LongCount(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the elements with the maximum key value by using the default comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector used to extract the key for each element in the sequence.</param>
        /// <returns>List with the elements that share the same maximum key value.</returns>
        public IList<TSource> MaxBy<TKey>(String keySelector)
        {
            return this._source.MaxBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            );
        }

        /// <summary>
        /// Returns the elements with the minimum key value by using the specified comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector used to extract the key for each element in the sequence.</param>
        /// <param name="comparer">Comparer used to determine the maximum key value.</param>
        /// <returns>List with the elements that share the same maximum key value.</returns>
        public IList<TSource> MaxBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return this._source.MaxBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            );
        }

        /// <summary>
        /// Memoizes the source sequence within a selector function where each enumerator can get access to all of the sequence's elements without causing multiple enumerations over the source.
        /// </summary>
        /// <typeparam name="TResult">Result sequence element type.</typeparam>
        /// <param name="selector"><c>(it) =></c> Selector function with memoized access to the source sequence for each enumerator.</param>
        /// <returns>Sequence resulting from applying the selector function to the memoized view over the source sequence.</returns>
        public YacqQueryable<TResult> Memoize<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Memoize(
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Memoizes the source sequence within a selector function where a specified number of enumerators can get access to all of the sequence's elements without causing multiple enumerations over the source.
        /// </summary>
        /// <typeparam name="TResult">Result sequence element type.</typeparam>
        /// <param name="readerCount">Number of enumerators that can access the underlying buffer. Once every enumerator has obtained an element from the buffer, the element is removed from the buffer.</param>
        /// <param name="selector"><c>(it) =></c> Selector function with memoized access to the source sequence for a specified number of enumerators.</param>
        /// <returns>Sequence resulting from applying the selector function to the memoized view over the source sequence.</returns>
        public YacqQueryable<TResult> Memoize<TResult>(Int32 readerCount, String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Memoize(
                readerCount,
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns the elements with the minimum key value by using the default comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector used to extract the key for each element in the sequence.</param>
        /// <returns>List with the elements that share the same minimum key value.</returns>
        public IList<TSource> MinBy<TKey>(String keySelector)
        {
            return this._source.MinBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            );
        }

        /// <summary>
        /// Returns the elements with the minimum key value by using the specified comparer to compare key values.
        /// </summary>
        /// <typeparam name="TKey">Key type.</typeparam>
        /// <param name="keySelector"><c>(it) =></c> Key selector used to extract the key for each element in the sequence.</param>
        /// <param name="comparer">Comparer used to determine the minimum key value.</param>
        /// <returns>List with the elements that share the same minimum key value.</returns>
        public IList<TSource> MinBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return this._source.MinBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            );
        }

        /// <summary>
        /// Filters the elements of an <see cref="IQueryable"/> based on a specified type.
        /// </summary>
        /// <typeparam name="TResult">The type to filter the elements of the sequence on.</typeparam>
        /// <returns>A collection that contains the elements from the source that have type <typeparamref name="TResult"/>.</returns>
        public YacqQueryable<TResult> OfType<TResult>()
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.OfType<TResult>());
        }

        /// <summary>
        /// Creates a sequence that concatenates both given sequences, regardless of whether an error occurs.
        /// </summary>
        /// <param name="second">Second sequence.</param>
        /// <returns>Sequence concatenating the elements of both sequences, ignoring errors.</returns>
        public YacqQueryable<TSource> OnErrorResumeNext(IEnumerable<TSource> second)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.OnErrorResumeNext(second));
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Publishes the source sequence within a selector function where each enumerator can obtain a view over a tail of the source sequence.
        /// </summary>
        /// <typeparam name="TResult">Result sequence element type.</typeparam>
        /// <param name="selector"><c>(it) =></c> Selector function with published access to the source sequence for each enumerator.</param>
        /// <returns>Sequence resulting from applying the selector function to the published view over the source sequence.</returns>
        public YacqQueryable<TResult> Publish<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Publish(
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Repeats and concatenates the source sequence infinitely.
        /// </summary>
        /// <returns>Sequence obtained by concatenating the source sequence to itself infinitely.</returns>
        public YacqQueryable<TSource> Repeat()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Repeat());
        }

        /// <summary>
        /// Repeats and concatenates the source sequence the given number of times.
        /// </summary>
        /// <param name="count">Number of times to repeat the source sequence.</param>
        /// <returns>Sequence obtained by concatenating the source sequence to itself the specified number of times.</returns>
        public YacqQueryable<TSource> Repeat(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Repeat(count));
        }

        /// <summary>
        /// Creates a sequence that retries enumerating the source sequence as long as an error occurs.
        /// </summary>
        /// <returns>Sequence concatenating the results of the source sequence as long as an error occurs.</returns>
        public YacqQueryable<TSource> Retry()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Retry());
        }

        /// <summary>
        /// Creates a sequence that retries enumerating the source sequence as long as an error occurs, with the specified maximum number of retries.
        /// </summary>
        /// <param name="retryCount">Maximum number of retries.</param>
        /// <returns>Sequence concatenating the results of the source sequence as long as an error occurs.</returns>
        public YacqQueryable<TSource> Retry(Int32 retryCount)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Retry(retryCount));
        }

        /// <summary>
        /// Inverts the order of the elements in a sequence.
        /// </summary>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> whose elements correspond to those of the input sequence in reverse order.</returns>
        public YacqQueryable<TSource> Reverse()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Reverse());
        }

        /// <summary>
        /// Generates a sequence of accumulated values by scanning the source sequence and applying an accumulator function.
        /// </summary>
        /// <typeparam name="TAccumulate">Accumulation type.</typeparam>
        /// <param name="seed">Accumulator seed value.</param>
        /// <param name="accumulator"><c>(a, it) =></c> Accumulation function to apply to the current accumulation value and each element of the sequence.</param>
        /// <returns>Sequence with all intermediate accumulation values resulting from scanning the sequence.</returns>
        public YacqQueryable<TAccumulate> Scan<TAccumulate>(TAccumulate seed, String accumulator)
        {
            return new YacqQueryable<TAccumulate>(this.Symbols, this._source.Scan(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        /// <summary>
        /// Generates a sequence of accumulated values by scanning the source sequence and applying an accumulator function.
        /// </summary>
        /// <param name="accumulator"><c>(a, it) =></c> Accumulation function to apply to the current accumulation value and each element of the sequence.</param>
        /// <returns>Sequence with all intermediate accumulation values resulting from scanning the sequence.</returns>
        public YacqQueryable<TSource> Scan(String accumulator)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Scan(
                YacqServices.ParseLambda<Func<TSource, TSource, TSource>>(this.Symbols, accumulator, "a", "it")
            ));
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
                YacqServices.ParseLambda<TSource, TResult>(this.Symbols, selector)
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
                YacqServices.ParseLambda<Func<TSource, IEnumerable<TCollection>>>(this.Symbols, collectionSelector, "it"),
                YacqServices.ParseLambda<Func<TSource, TCollection, TResult>>(this.Symbols, resultSelector, "it", "c")
            ));
        }

        /// <summary>
        /// Projects each element of a sequence to an given sequence and flattens the resulting sequences into one sequence.
        /// </summary>
        /// <typeparam name="TOther">Second source sequence element type.</typeparam>
        /// <param name="other">Inner sequence each source sequenec element is projected onto.</param>
        /// <returns>Sequence flattening the sequences that result from projecting elements in the source sequence.</returns>
        public YacqQueryable<TOther> SelectMany<TOther>(IEnumerable<TOther> other)
        {
            return new YacqQueryable<TOther>(this.Symbols, this._source.SelectMany(other));
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
                YacqServices.ParseLambda<TSource, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Shares the source sequence within a selector function where each enumerator can fetch the next element from the source sequence.
        /// </summary>
        /// <typeparam name="TResult">Result sequence element type.</typeparam>
        /// <param name="selector"><c>(it) =></c> Selector function with shared access to the source sequence for each enumerator.</param>
        /// <returns>Sequence resulting from applying the selector function to the shared view over the source sequence.</returns>
        public YacqQueryable<TResult> Share<TResult>(String selector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Share(
                YacqServices.ParseLambda<IEnumerable<TSource>, IEnumerable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition, and throws an exception if more than one such element exists.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate"/>.</returns>
        public TSource Single(String predicate)
        {
            return this._source.Single(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Returns the only element of a sequence that satisfies a specified condition or a default value if no such element exists; this method throws an exception if more than one element satisfies the condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test an element for a condition.</param>
        /// <returns>The single element of the input sequence that satisfies the condition in <paramref name="predicate"/>, or default(<typeparamref name="TSource"/>) if no such element is found.</returns>
        public TSource SingleOrDefault(String predicate)
        {
            return this._source.SingleOrDefault(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            );
        }

        /// <summary>
        /// Bypasses a specified number of elements in a sequence and then returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains elements that occur after the specified index in the input sequence.</returns>
        public YacqQueryable<TSource> Skip(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Skip(count));
        }

        /// <summary>
        /// Bypasses a specified number of contiguous elements from the end of the sequence and returns the remaining elements.
        /// </summary>
        /// <param name="count">The number of elements to skip from the end of the sequence before returning the remaining elements.</param>
        /// <returns>Sequence bypassing the specified number of elements counting from the end of the source sequence.</returns>
        public YacqQueryable<TSource> SkipLast(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.SkipLast(count));
        }

        /// <summary>
        /// Bypasses elements in a sequence as long as a specified condition is <c>true</c> and then returns the remaining elements.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from source starting at the first element in the linear series that does not pass the test specified by <paramref name="predicate"/>.</returns>
        public YacqQueryable<TSource> SkipWhile(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.SkipWhile(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Returns the source sequence prefixed with the specified value.
        /// </summary>
        /// <param name="values">Values to prefix the sequence with.</param>
        /// <returns>Sequence starting with the specified prefix value, followed by the source sequence.</returns>
        public YacqQueryable<TSource> StartWith(TSource[] values)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.StartWith(values));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the start of a sequence.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>An <see cref="YacqQueryable{TSource}"/> that contains the specified number of elements from the start of the source.</returns>
        public YacqQueryable<TSource> Take(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Take(count));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the end of the sequence.
        /// </summary>
        /// <param name="count">The number of elements to take from the end of the sequence.</param>
        /// <returns>Sequence with the specified number of elements counting from the end of the source sequence.</returns>
        public YacqQueryable<TSource> TakeLast(Int32 count)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.TakeLast(count));
        }

        /// <summary>
        /// Returns elements from a sequence as long as a specified condition is <c>true</c>.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQueryable{TSource}"/> that contains elements from the input sequence occurring before the element at which the test specified by <paramref name="predicate"/> no longer passes.</returns>
        public YacqQueryable<TSource> TakeWhile(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.TakeWhile(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
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
        public YacqQueryable<TSource> Where(String predicate)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Where(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
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
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        #region Non-Expression-based methods

        /// <summary>
        /// Enumerates the sequence and invokes the given action for each value in the sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element.</param>
        public void ForEach(String onNext)
        {
            this._source.ForEach(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it").Compile()
            );
        }

        #endregion
    }
}