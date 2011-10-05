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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace XSpect.Yacq
{
    partial class YacqQbservable<TSource>
    {
        /// <summary>
        /// Applies an accumulator function over an observable sequence. The specified seed value is used as the initial accumulator value.
        /// </summary>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator"><c>(a, it) =></c> An accumulator function to be invoked on each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with the final accumulator value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<TAccumulate> Aggregate<TAccumulate>(TAccumulate seed, String accumulator)
        {
            return new YacqQbservable<TAccumulate>(this.Symbols, this._source.Aggregate(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        /// <summary>
        /// Applies an accumulator function over an observable sequence.
        /// </summary>
        /// <param name="accumulator"><c>(a, it) =></c> An accumulator function to be invoked on each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with the final accumulator value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<TSource> Aggregate(String accumulator)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Aggregate(
                YacqServices.ParseLambda<Func<TSource, TSource, TSource>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        /// <summary>
        /// Determines whether all elements of an observable sequence satisfy a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element determining whether all elements in the source sequence pass the test in the specified predicate.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> All(String predicate)
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.All(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Determines whether any element of an observable sequence satisfies a condition.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element determining whether any elements in the source sequence pass the test in the specified predicate.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> Any(String predicate)
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.Any(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers.
        /// </summary>
        /// <param name="bufferClosingSelector"><c>(it) =></c> A function invoked to define the boundaries of the produced buffers. A new buffer is started when the previous one is closed.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer<TBufferClosing>(String bufferClosingSelector)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(
                YacqServices.ParseLambda<IObservable<TBufferClosing>>(this.Symbols, bufferClosingSelector)
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers.
        /// </summary>
        /// <param name="bufferOpenings">Observable sequence whose elements denote the creation of new buffers.</param>
        /// <param name="bufferClosingSelector"><c>(it) =></c> A function invoked to define the closing of each produced buffer.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer<TBufferOpening, TBufferClosing>(IObservable<TBufferOpening> bufferOpenings, String bufferClosingSelector)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(
                bufferOpenings,
                YacqServices.ParseLambda<TBufferOpening, IObservable<TBufferClosing>>(this.Symbols, bufferClosingSelector)
            ));
        }

        /// <summary>
        /// Continues an observable sequence that is terminated by an exception of the specified type with the observable sequence produced by the handler.
        /// </summary>
        /// <param name="handler"><c>(ex) =></c> Exception handler function, producing another observable sequence.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing the source sequence's elements, followed by the elements produced by the handler's resulting observable sequence in case an exception occurred.</returns>
        public YacqQbservable<TSource> Catch<TException>(String handler)
            where TException : Exception
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Catch(
                YacqServices.ParseLambda<Func<TException, IObservable<TSource>>>(this.Symbols, handler, "ex")
            ));
        }

        /// <summary>
        /// Merges two observable sequences into one observable sequence by using the selector function whenever one of the observable sequences produces an element.
        /// </summary>
        /// <param name="second">Second observable source.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> Function to invoke whenever either of the sources produces an element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing the result of combining elements of both sources using the specified result selector function.</returns>
        public YacqQbservable<TResult> CombineLatest<TSecond, TResult>(IObservable<TSecond> second, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.CombineLatest(
                second,
                YacqServices.ParseLambda<Func<TSource, TSecond, TResult>>(this.Symbols, resultSelector, "it", "it2")
            ));
        }

        /// <summary>
        /// Returns an observable sequence that contains only distinct elements according to the keySelector.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to compute the comparison key for each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> only containing the distinct elements, based on a computed key value, from the source sequence.</returns>
        public YacqQbservable<TSource> Distinct<TKey>(String keySelector)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Distinct(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Returns an observable sequence that contains only distinct elements according to the keySelector and the comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to compute the comparison key for each element.</param>
        /// <param name="comparer">Equality comparer for source elements.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> only containing the distinct elements, based on a computed key value, from the source sequence.</returns>
        public YacqQbservable<TSource> Distinct<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Distinct(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Returns an observable sequence that contains only distinct contiguous elements according to the keySelector.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to compute the comparison key for each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> only containing the distinct contiguous elements, based on a computed key value, from the source sequence.</returns>
        public YacqQbservable<TSource> DistinctUntilChanged<TKey>(String keySelector)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DistinctUntilChanged(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Returns an observable sequence that contains only distinct contiguous elements according to the keySelector and the comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to compute the comparison key for each element.</param>
        /// <param name="comparer">Equality comparer for computed key values.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> only containing the distinct contiguous elements, based on a computed key value, from the source sequence.</returns>
        public YacqQbservable<TSource> DistinctUntilChanged<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DistinctUntilChanged(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Invokes an action for each element in the observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <returns>The source sequence with the side-effecting behavior applied.</returns>
        public YacqQbservable<TSource> Do(String onNext)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it")
            ));
        }

        /// <summary>
        /// Invokes an action for each element in the observable sequence and invokes an action upon graceful termination of the observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <param name="onCompleted"><c>() =></c> Action to invoke upon graceful termination of the observable sequence.</param>
        /// <returns>The source sequence with the side-effecting behavior applied.</returns>
        public YacqQbservable<TSource> Do(String onNext, String onCompleted)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action>(this.Symbols, onNext, new String[0])
            ));
        }

        /// <summary>
        /// Invokes an action for each element in the observable sequence and invokes an action upon graceful or exceptional termination of the observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <param name="onError"><c>(ex) =></c> Action to invoke upon exceptional termination of the observable sequence.</param>
        /// <param name="onCompleted"><c>() =></c> Action to invoke upon graceful termination of the observable sequence.</param>
        /// <returns>The source sequence with the side-effecting behavior applied.</returns>
        public YacqQbservable<TSource> Do(String onNext, String onError, String onCompleted)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action<Exception>>(this.Symbols, onNext, "ex"),
                YacqServices.ParseLambda<Action>(this.Symbols, onNext, new String[0])
            ));
        }

        /// <summary>
        /// Repeats source as long as condition holds.
        /// </summary>
        /// <param name="condition"><c>(it) =></c> Action to determine the condition.</param>
        public YacqQbservable<TSource> DoWhile(String condition)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DoWhile(
                YacqServices.ParseLambda<Boolean>(this.Symbols, condition)
            ));
        }

        /// <summary>
        /// Expands an observable sequence by recursively invoking selector.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function to invoke for each produced element, resulting in another sequence to which the selector will be invoked recursively again.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing all the elements produced by the recursive expansion.</returns>
        public YacqQbservable<TSource> Expand(String selector)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Expand(
                YacqServices.ParseLambda<TSource, IObservable<TSource>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Expands an observable sequence by recursively invoking selector.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function to invoke for each produced element, resulting in another sequence to which the selector will be invoked recursively again.</param>
        /// <param name="scheduler">Scheduler on which to perform the expansion.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing all the elements produced by the recursive expansion.</returns>
        public YacqQbservable<TSource> Expand(String selector, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Expand(
                YacqServices.ParseLambda<TSource, IObservable<TSource>>(this.Symbols, selector),
                scheduler
            ));
        }

        /// <summary>
        /// Invokes a specified action after source observable sequence terminates normally or by an exception.
        /// </summary>
        /// <param name="finallyAction"><c>() =></c> Action to invoke after the source observable sequence terminates.</param>
        /// <returns>Source sequence with the action-invoking termination behavior applied.</returns>
        public YacqQbservable<TSource> Finally(String finallyAction)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Finally(
                YacqServices.ParseLambda<Action>(this.Symbols, finallyAction, new String[0])
            ));
        }

        /// <summary>
        /// Runs two observable sequences in parallel and combines their last elemenets.
        /// </summary>
        /// <param name="second">Second observable sequence.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> Result selector function to invoke with the last elements of both sequences.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> with the result of calling the selector function with the last elements of both input sequences.</returns>
        public YacqQbservable<TResult> ForkJoin<TSecond, TResult>(IObservable<TSecond> second, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.ForkJoin(
                second,
                YacqServices.ParseLambda<Func<TSource, TSecond, TResult>>(this.Symbols, resultSelector, "it", "it2")
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence and selects the resulting elements by using a specified function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an observable group.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.</returns>
        public YacqQbservable<IGroupedObservable<TKey, TElement>> GroupBy<TKey, TElement>(String keySelector, String elementSelector)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TElement>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector)
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function and comparer and selects the resulting elements by using a specified function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an observable group.</param>
        /// <param name="comparer">An equality comparer to compare keys with.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.</returns>
        public YacqQbservable<IGroupedObservable<TKey, TElement>> GroupBy<TKey, TElement>(String keySelector, String elementSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TElement>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                comparer
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.</returns>
        public YacqQbservable<IGroupedObservable<TKey, TSource>> GroupBy<TKey>(String keySelector)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TSource>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function and comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="comparer">An equality comparer to compare keys with.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.</returns>
        public YacqQbservable<IGroupedObservable<TKey, TSource>> GroupBy<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TSource>>(this.Symbols, this._source.GroupBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function.
        /// A duration selector function is used to control the lifetime of groups.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="durationSelector"><c>(it) =></c> A function to signal the expiration of a group.</param>
        /// <returns>
        /// A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.
        /// If a group's lifetime expires, a new group with the same key value can be created once an element with such a key value is encoutered.
        /// </returns>
        public YacqQbservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TKey, TDuration>(String keySelector, String durationSelector)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TSource>>(this.Symbols, this._source.GroupByUntil(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<IGroupedObservable<TKey, TSource>, IObservable<TDuration>>(this.Symbols, durationSelector)
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function and comparer.
        /// A duration selector function is used to control the lifetime of groups.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="durationSelector"><c>(it) =></c> A function to signal the expiration of a group.</param>
        /// <param name="comparer">An equality comparer to compare keys with.</param>
        /// <returns>
        /// A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.
        /// If a group's lifetime expires, a new group with the same key value can be created once an element with such a key value is encoutered.
        /// </returns>
        public YacqQbservable<IGroupedObservable<TKey, TSource>> GroupByUntil<TKey, TDuration>(String keySelector, String durationSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TSource>>(this.Symbols, this._source.GroupByUntil(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<IGroupedObservable<TKey, TSource>, IObservable<TDuration>>(this.Symbols, durationSelector),
                comparer
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function and selects the resulting elements by using a specified function.
        /// A duration selector function is used to control the lifetime of groups.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an observable group.</param>
        /// <param name="durationSelector"><c>(it) =></c> A function to signal the expiration of a group.</param>
        /// <returns>
        /// A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.
        /// If a group's lifetime expires, a new group with the same key value can be created once an element with such a key value is encoutered.
        /// </returns>
        public YacqQbservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TKey, TElement, TDuration>(String keySelector, String elementSelector, String durationSelector)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TElement>>(this.Symbols, this._source.GroupByUntil(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                YacqServices.ParseLambda<IGroupedObservable<TKey, TElement>, IObservable<TDuration>>(this.Symbols, durationSelector)
            ));
        }

        /// <summary>
        /// Groups the elements of an observable sequence according to a specified key selector function and comparer and selects the resulting elements by using a specified function.
        /// A duration selector function is used to control the lifetime of groups.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract the key for each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A function to map each source element to an element in an observable group.</param>
        /// <param name="durationSelector"><c>(it) =></c> A function to signal the expiration of a group.</param>
        /// <param name="comparer">An equality comparer to compare keys with.</param>
        /// <returns>
        /// A <see cref="YacqQbservable{TSource}"/> of observable groups, each of which corresponds to a unique key value, containing all elements that share that same key value.
        /// If a group's lifetime expires, a new group with the same key value can be created once an element with such a key value is encoutered.
        /// </returns>
        public YacqQbservable<IGroupedObservable<TKey, TElement>> GroupByUntil<TKey, TElement, TDuration>(String keySelector, String elementSelector, String durationSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<IGroupedObservable<TKey, TElement>>(this.Symbols, this._source.GroupByUntil(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                YacqServices.ParseLambda<IGroupedObservable<TKey, TElement>, IObservable<TDuration>>(this.Symbols, durationSelector),
                comparer
            ));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on overlapping durations, and groups the results.
        /// </summary>
        /// <param name="right">The right observable sequence to join elements for.</param>
        /// <param name="leftDurationSelector"><c>(it) =></c> A function to select the duration of each element of the left observable sequence, used to determine overlap.</param>
        /// <param name="rightDurationSelector"><c>(it) =></c> A function to select the duration of each element of the right observable sequence, used to determine overlap.</param>
        /// <param name="resultSelector"><c>(l, r) =></c> A function invoked to compute a result element for any element of the left sequence with overlapping elements from the right observable sequence.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains result elements computed from source elements that have an overlapping duration.</returns>
        public YacqQbservable<TResult> GroupJoin<TRight, TLeftDuration, TRightDuration, TResult>(IObservable<TRight> right, String leftDurationSelector, String rightDurationSelector, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.GroupJoin(
                right,
                YacqServices.ParseLambda<TSource, IObservable<TLeftDuration>>(this.Symbols, leftDurationSelector),
                YacqServices.ParseLambda<TRight, IObservable<TRightDuration>>(this.Symbols, rightDurationSelector),
                YacqServices.ParseLambda<Func<TSource, IObservable<TRight>, TResult>>(this.Symbols, resultSelector, "l", "r")
            ));
        }

        /// <summary>
        /// Correlates the elements of two sequences based on overlapping durations.
        /// </summary>
        /// <param name="right">The right observable sequence to join elements for.</param>
        /// <param name="leftDurationSelector"><c>(it) =></c> A function to select the duration of each element of the left observable sequence, used to determine overlap.</param>
        /// <param name="rightDurationSelector"><c>(it) =></c> A function to select the duration of each element of the right observable sequence, used to determine overlap.</param>
        /// <param name="resultSelector"><c>(l, r) =></c> A function invoked to compute a result element for any two overlapping elements of the left and right observable sequences.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains result elements computed from source elements that have an overlapping duration.</returns>
        public YacqQbservable<TResult> Join<TRight, TLeftDuration, TRightDuration, TResult>(IObservable<TRight> right, String leftDurationSelector, String rightDurationSelector, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Join(
                right,
                YacqServices.ParseLambda<TSource, IObservable<TLeftDuration>>(this.Symbols, leftDurationSelector),
                YacqServices.ParseLambda<TRight, IObservable<TRightDuration>>(this.Symbols, rightDurationSelector),
                YacqServices.ParseLambda<Func<TSource, TRight, TResult>>(this.Symbols, rightDurationSelector, "l", "r")
            ));
        }

        /// <summary>
        /// Bind the source to the parameter without sharing subscription side-effects.
        /// </summary>
        /// <param name="function"><c>(it) =></c> A function which specifies the side effects.</param>
        public YacqQbservable<TResult> Let<TResult>(String function)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Let(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, function)
            ));
        }

        /// <summary>
        /// Comonadic bind operator.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A selector function.</param>
        public YacqQbservable<TResult> ManySelect<TResult>(String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.ManySelect(
                YacqServices.ParseLambda<IObservable<TSource>, TResult>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Comonadic bind operator.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A selector function.</param>
        /// <param name="scheduler">Scheduler on which to perform the expansion.</param>
        public YacqQbservable<TResult> ManySelect<TResult>(String selector, IScheduler scheduler)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.ManySelect(
                YacqServices.ParseLambda<IObservable<TSource>, TResult>(this.Symbols, selector),
                scheduler
            ));
        }

        /// <summary>
        /// Returns the elements in an observable sequence with the maximum key value.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> Key selector function.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a list of zero or more elements that have a maximum key value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IList<TSource>> MaxBy<TKey>(String keySelector)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.MaxBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Returns the elements in an observable sequence with the maximum key value according to the specified comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> Key selector function.</param>
        /// <param name="comparer">Comparer used to compare key values.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a list of zero or more elements that have a maximum key value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IList<TSource>> MaxBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.MaxBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Returns the elements in an observable sequence with the minimum key value.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> Key selector function.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a list of zero or more elements that have a minimum key value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IList<TSource>> MinBy<TKey>(String keySelector)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.MinBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Returns the elements in an observable sequence with the minimum key value according to the specified comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> Key selector function.</param>
        /// <param name="comparer">Comparer used to compare key values.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a list of zero or more elements that have a minimum key value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IList<TSource>> MinBy<TKey>(String keySelector, IComparer<TKey> comparer)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.MinBy(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Returns an observable sequence that contains the elements of a sequence produced by multicasting the source sequence within a selector function.
        /// </summary>
        /// <param name="subjectSelector"><c>(it) =></c> Factory function to create an intermediate subject through which the source sequence's elements will be multicast to the selector function.</param>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence subject to the policies enforced by the created subject.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Multicast<TIntermediate, TResult>(String subjectSelector, String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Multicast(
                YacqServices.ParseLambda<ISubject<TSource, TIntermediate>>(this.Symbols, subjectSelector),
                YacqServices.ParseLambda<IObservable<TIntermediate>, IObservable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Publish<TResult>(String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Publish(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence and starts with initialValue.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive immediately receive the initial value, followed by all notifications of the source from the time of the subscription on.</param>
        /// <param name="initialValue">Initial value received by observers upon subscription.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        /// <seealso cref="T:System.Reactive.Subjects.Subject" />
        public YacqQbservable<TResult> Publish<TResult>(String selector, TSource initialValue)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Publish(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                initialValue
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence containing only the last notification.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will only receive the last notification of the source.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> PublishLast<TResult>(String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.PublishLast(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying all notifications.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying bufferSize notifications.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="bufferSize">Maximum element count of the replay buffer.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, Int32 bufferSize)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                bufferSize
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying bufferSize notifications.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="bufferSize">Maximum element count of the replay buffer.</param>
        /// <param name="scheduler">Scheduler where connected observers within the selector function will be invoked on.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, Int32 bufferSize, IScheduler scheduler)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                bufferSize,
                scheduler
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying bufferSize notifications within window.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="bufferSize">Maximum element count of the replay buffer.</param>
        /// <param name="window">Maximum time length of the replay buffer.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, Int32 bufferSize, TimeSpan window)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                bufferSize,
                window
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying bufferSize notifications within window.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="bufferSize">Maximum element count of the replay buffer.</param>
        /// <param name="window">Maximum time length of the replay buffer.</param>
        /// <param name="scheduler">Scheduler where connected observers within the selector function will be invoked on.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, Int32 bufferSize, TimeSpan window, IScheduler scheduler)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                bufferSize,
                window,
                scheduler
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying all notifications.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="scheduler">Scheduler where connected observers within the selector function will be invoked on.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, IScheduler scheduler)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                scheduler
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying all notifications within window.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="window">Maximum time length of the replay buffer.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, TimeSpan window)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                window
            ));
        }

        /// <summary>
        /// Returns an observable sequence that is the result of invoking the selector on a connectable observable sequence that shares a single subscription to the underlying sequence replaying all notifications within window.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> Selector function which can use the multicasted source sequence as many times as needed, without causing multiple subscriptions to the source sequence. Subscribers will receive all the notifications of the source.</param>
        /// <param name="window">Maximum time length of the replay buffer.</param>
        /// <param name="scheduler">Scheduler where connected observers within the selector function will be invoked on.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements of a sequence produced by multicasting the source sequence within a selector function.</returns>
        public YacqQbservable<TResult> Replay<TResult>(String selector, TimeSpan window, IScheduler scheduler)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Replay(
                YacqServices.ParseLambda<IObservable<TSource>, IObservable<TResult>>(this.Symbols, selector),
                window,
                scheduler
            ));
        }

        /// <summary>
        /// Applies an accumulator function over an observable sequence and returns each intermediate result. The specified seed value is used as the initial accumulator value.
        /// </summary>
        /// <param name="seed">The initial accumulator value.</param>
        /// <param name="accumulator"><c>(a, it) =></c> An accumulator function to be invoked on each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing the accumulated values.</returns>
        public YacqQbservable<TAccumulate> Scan<TAccumulate>(TAccumulate seed, String accumulator)
        {
            return new YacqQbservable<TAccumulate>(this.Symbols, this._source.Scan(
                seed,
                YacqServices.ParseLambda<Func<TAccumulate, TSource, TAccumulate>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        /// <summary>
        /// Applies an accumulator function over an observable sequence and returns each intermediate result.  
        /// </summary>
        /// <param name="accumulator"><c>(a, it) =></c> An accumulator function to be invoked on each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing the accumulated values.</returns>
        public YacqQbservable<TSource> Scan(String accumulator)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Scan(
                YacqServices.ParseLambda<Func<TSource, TSource, TSource>>(this.Symbols, accumulator, "a", "it")
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence into a new form.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A transform function to apply to each source element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> whose elements are the result of invoking the transform function on each element of source.</returns>
        public YacqQbservable<TResult> Select<TResult>(String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Select(
                YacqServices.ParseLambda<TSource, TResult>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and flattens the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <param name="collectionSelector"><c>(it) =></c> A transform function to apply to each element.</param>
        /// <param name="resultSelector"><c>(it, c) =></c> A transform function to apply to each element of the intermediate sequence.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> whose elements are the result of invoking the one-to-many transform function collectionSelector on each element of the input sequence and then mapping each of those sequence elements and their corresponding source element to a result element.</returns>
        public YacqQbservable<TResult> SelectMany<TCollection, TResult>(String collectionSelector, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseLambda<TSource, IEnumerable<TCollection>>(this.Symbols, collectionSelector),
                YacqServices.ParseLambda<Func<TSource, TCollection, TResult>>(this.Symbols, resultSelector, "it", "c")
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and flattens the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> A transform function to apply to each element.</param>
        /// <param name="onError"><c>(ex) =></c> A transform function to apply when an error occurs in the source sequence.</param>
        /// <param name="onCompleted"><c>() =></c> A transform function to apply when the end of the source sequence is reached.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> whose elements are the result of invoking the one-to-many transform function corresponding to each notification in the input sequence.</returns>
        public YacqQbservable<TResult> SelectMany<TResult>(String onNext, String onError, String onCompleted)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseLambda<TSource, IObservable<TResult>>(this.Symbols, onNext),
                YacqServices.ParseLambda<Func<Exception, IObservable<TResult>>>(this.Symbols, onError, "ex"),
                YacqServices.ParseLambda<IObservable<TResult>>(onCompleted)
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and flattens the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A transform function to apply to each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> whose elements are the result of invoking the one-to-many transform function on each element of the input sequence.</returns>
        public YacqQbservable<TResult> SelectMany<TResult>(String selector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.SelectMany(
                YacqServices.ParseLambda<TSource, IObservable<TResult>>(this.Symbols, selector)
            ));
        }

        /// <summary>
        /// Bypasses values in an observable sequence as long as a specified condition is <c>true</c> and then returns the remaining values.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements from the input sequence starting at the first element in the linear series that does not pass the test specified by predicate.</returns>
        public YacqQbservable<TSource> SkipWhile(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SkipWhile(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Returns values from an observable sequence as long as a specified condition is <c>true</c>, and then skips the remaining values.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each element for a condition.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains the elements from the input sequence that occur before the element at which the test no longer passes.</returns>
        public YacqQbservable<TSource> TakeWhile(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.TakeWhile(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Matches when the observable sequence has an available value and projects the value.
        /// </summary>
        /// <param name="selector"><c>(it) =></c> A selector function.</param>
        public QueryablePlan<TResult> Then<TResult>(String selector)
        {
            return this._source.Then(
                YacqServices.ParseLambda<TSource, TResult>(this.Symbols, selector)
            );
        }

        /// <summary>
        /// Creates a dictionary from an observable sequence according to a specified key selector function, and an element selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A transform function to produce a result element value from each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IDictionary<TKey, TElement>> ToDictionary<TKey, TElement>(String keySelector, String elementSelector)
        {
            return new YacqQbservable<IDictionary<TKey, TElement>>(this.Symbols, this._source.ToDictionary(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector)
            ));
        }

        /// <summary>
        /// Creates a dictionary from an observable sequence according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> >A function to extract a key from each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IDictionary<TKey, TElement>> ToDictionary<TKey, TElement>(String keySelector, String elementSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<IDictionary<TKey, TElement>>(this.Symbols, this._source.ToDictionary(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                comparer
            ));
        }

        /// <summary>
        /// Creates a dictionary from an observable sequence according to a specified key selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IDictionary<TKey, TSource>> ToDictionary<TKey>(String keySelector)
        {
            return new YacqQbservable<IDictionary<TKey, TSource>>(this.Symbols, this._source.ToDictionary(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Creates a dictionary from an observable sequence according to a specified key selector function, and a comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a dictionary mapping unique key values onto the corresponding source sequence's element.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IDictionary<TKey, TSource>> ToDictionary<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<IDictionary<TKey, TSource>>(this.Symbols, this._source.ToDictionary(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Creates a lookup from an observable sequence according to a specified key selector function, and an element selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A transform function to produce a result element value from each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a lookup mapping unique key values onto the corresponding source sequence's elements.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<ILookup<TKey, TElement>> ToLookup<TKey, TElement>(String keySelector, String elementSelector)
        {
            return new YacqQbservable<ILookup<TKey, TElement>>(this.Symbols, this._source.ToLookup(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector)
            ));
        }

        /// <summary>
        /// Creates a lookup from an observable sequence according to a specified key selector function, a comparer, and an element selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="elementSelector"><c>(it) =></c> A transform function to produce a result element value from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a lookup mapping unique key values onto the corresponding source sequence's elements.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<ILookup<TKey, TElement>> ToLookup<TKey, TElement>(String keySelector, String elementSelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<ILookup<TKey, TElement>>(this.Symbols, this._source.ToLookup(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                YacqServices.ParseLambda<TSource, TElement>(this.Symbols, elementSelector),
                comparer
            ));
        }

        /// <summary>
        /// Creates a lookup from an observable sequence according to a specified key selector function.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a lookup mapping unique key values onto the corresponding source sequence's elements.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<ILookup<TKey, TSource>> ToLookup<TKey>(String keySelector)
        {
            return new YacqQbservable<ILookup<TKey, TSource>>(this.Symbols, this._source.ToLookup(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector)
            ));
        }

        /// <summary>
        /// Creates a lookup from an observable sequence according to a specified key selector function, and a comparer.
        /// </summary>
        /// <param name="keySelector"><c>(it) =></c> A function to extract a key from each element.</param>
        /// <param name="comparer">An equality comparer to compare keys.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing a single element with a lookup mapping unique key values onto the corresponding source sequence's elements.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on <see cref="IEnumerable{T}"/> in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<ILookup<TKey, TSource>> ToLookup<TKey>(String keySelector, IEqualityComparer<TKey> comparer)
        {
            return new YacqQbservable<ILookup<TKey, TSource>>(this.Symbols, this._source.ToLookup(
                YacqServices.ParseLambda<TSource, TKey>(this.Symbols, keySelector),
                comparer
            ));
        }

        /// <summary>
        /// Filters the elements of an observable sequence based on a predicate.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A function to test each source element for a condition.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> that contains elements from the input sequence that satisfy the condition.</returns>
        public YacqQbservable<TSource> Where(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Where(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping windows.
        /// </summary>
        /// <param name="windowClosingSelector"><c>(it) =></c> A function invoked to define the boundaries of the produced windows. A new window is started when the previous one is closed.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window<TWindowClosing>(String windowClosingSelector)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(
                YacqServices.ParseLambda<IObservable<TWindowClosing>>(this.Symbols, windowClosingSelector)
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows.
        /// </summary>
        /// <param name="windowOpenings">Observable sequence whose elements denote the creation of new windows.</param>
        /// <param name="windowClosingSelector"><c>(it) =></c> A function invoked to define the closing of each produced window.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window<TWindowOpening, TWindowClosing>(IObservable<TWindowOpening> windowOpenings, String windowClosingSelector)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(
                windowOpenings,
                YacqServices.ParseLambda<TWindowOpening, IObservable<TWindowClosing>>(this.Symbols, windowClosingSelector)
            ));
        }

        /// <summary>
        /// Merges an observable sequence and an enumerable sequence into one observable sequence by using the selector function.
        /// </summary>
        /// <param name="second">Second enumerable source.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> Function to invoke for each consecutive pair of elements from the first and second source.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing the result of pairwise combining the elements of the first and second source using the specified result selector function.</returns>
        public YacqQbservable<TResult> Zip<TSecond, TResult>(IEnumerable<TSecond> second, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Zip(
                second,
                YacqServices.ParseLambda<Func<TSource, TSecond, TResult>>(this.Symbols, resultSelector, "it", "it2")
            ));
        }

        /// <summary>
        /// Merges two observable sequences into one observable sequence by combining their elements in a pairwise fashion.
        /// </summary>
        /// <param name="second">Second observable source.</param>
        /// <param name="resultSelector"><c>(it, it2) =></c> Function to invoke for each consecutive pair of elements from the first and second source.</param>
        /// <returns>A <see cref="YacqQbservable{TSource}"/> containing the result of pairwise combining the elements of the first and second source using the specified result selector function.</returns>
        public YacqQbservable<TResult> Zip<TSecond, TResult>(IObservable<TSecond> second, String resultSelector)
        {
            return new YacqQbservable<TResult>(this.Symbols, this._source.Zip(
                second,
                YacqServices.ParseLambda<Func<TSource, TSecond, TResult>>(this.Symbols, resultSelector, "it", "it2")
            ));
        }

        #region Non-Expression-based methods

        /// <summary>
        /// Subscribes an element handler to an observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public IDisposable Subscribe(String onNext)
        {
            return this._source.Subscribe(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it").Compile()
            );
        }

        /// <summary>
        /// Subscribes an element handler and a completion handler to an observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <param name="onCompleted"><c>() =></c> Action to invoke upon graceful termination of the observable sequence.</param>
        /// <returns><see cref="IDisposable"/> object used to unsubscribe from the observable sequence.</returns>
        public IDisposable Subscribe(String onNext, String onCompleted)
        {
            return this._source.Subscribe(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it").Compile(),
                YacqServices.ParseLambda<Action>(this.Symbols, onCompleted, new String[0]).Compile()
            );
        }

        /// <summary>
        /// Subscribes an element handler, an exception handler, and a completion handler to an observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <param name="onError"><c>(ex) =></c> Action to invoke upon exceptional termination of the observable sequence.</param>
        /// <param name="onCompleted"><c>() =></c> Action to invoke upon graceful termination of the observable sequence.</param>
        /// <returns>IDisposable object used to unsubscribe from the observable sequence.</returns>
        public IDisposable Subscribe(String onNext, String onError, String onCompleted)
        {
            return this._source.Subscribe(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it").Compile(),
                YacqServices.ParseLambda<Action<Exception>>(this.Symbols, onNext, "ex").Compile(),
                YacqServices.ParseLambda<Action>(this.Symbols, onCompleted, new String[0]).Compile()
            );
        }

        #endregion
    }
}