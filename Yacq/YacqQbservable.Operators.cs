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
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;

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
        /// Propagates the observable sequence that reacts first.
        /// </summary>
        /// <param name="second">Second observable sequence.</param>
        /// <returns>An observable sequence that surfaces either of the given sequences, whichever reacted first.</returns>
        public YacqQbservable<TSource> Amb(IObservable<TSource> second)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Amb(second));
        }

        /// <summary>
        /// Determines whether an observable sequence contains any elements.
        /// </summary>
        /// <returns>An observable sequence containing a single element determining whether the source sequence contains any elements.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> Any()
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.Any());
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
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based on element count information.
        /// </summary>
        /// <param name="count">Length of each buffer.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(Int32 count)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(count));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers which are produced based on element count information.
        /// </summary>
        /// <param name="count">Length of each buffer.</param>
        /// <param name="skip">Number of elements to skip between creation of consecutive buffers.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(Int32 count, Int32 skip)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(count, skip));
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each buffer.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(TimeSpan timeSpan)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(timeSpan));
        }

        /// <summary>
        /// Projects each element of an observable sequence into a buffer that's sent out when either it's full or a given amount of time has elapsed.
        /// </summary>
        /// <param name="timeSpan">Maximum time length of a window.</param>
        /// <param name="count">Maximum element count of a window.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(TimeSpan timeSpan, Int32 count)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(timeSpan, count));
        }

        /// <summary>
        /// Projects each element of an observable sequence into a buffer that's sent out when either it's full or a given amount of time has elapsed.
        /// </summary>
        /// <param name="timeSpan">Maximum time length of a buffer.</param>
        /// <param name="count">Maximum element count of a buffer.</param>
        /// <param name="scheduler">Scheduler to run buffering timers on.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(TimeSpan timeSpan, Int32 count, IScheduler scheduler)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(timeSpan, count, scheduler));
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping buffers which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each buffer.</param>
        /// <param name="scheduler">Scheduler to run buffering timers on.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(TimeSpan timeSpan, IScheduler scheduler)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(timeSpan, scheduler));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each buffer.</param>
        /// <param name="timeShift">Interval between creation of consecutive buffers.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(TimeSpan timeSpan, TimeSpan timeShift)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(timeSpan, timeShift));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more buffers which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each buffer.</param>
        /// <param name="timeShift">Interval between creation of consecutive buffers.</param>
        /// <param name="scheduler">Scheduler to run buffering timers on.</param>
        /// <returns>An observable sequence of buffers.</returns>
        public YacqQbservable<IList<TSource>> Buffer(TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.Buffer(timeSpan, timeShift, scheduler));
        }

        /// <summary>
        /// Converts the elements of an observable sequence to the specified type.
        /// </summary>
        /// <returns>An observable sequence that contains each element of the source sequence converted to the specified type.</returns>
        public YacqQbservable<TResult> Cast<TResult>()
        {
            return new YacqQbservable<TResult>(this.Symbols, ((IQbservable<Object>) this._source).Cast<TResult>());
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
        /// Continues an observable sequence that is terminated by an exception with the next observable sequence.
        /// </summary>
        /// <param name="second">Second observable sequence used to produce results when an error occurred in the first sequence.</param>
        /// <returns>An observable sequence containing the first sequence's elements, followed by the elements of the second sequence in case an exception occurred.</returns>
        public YacqQbservable<TSource> Catch(IObservable<TSource> second)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Catch(second));
        }

        /// <summary>
        /// Produces an enumerable sequence of consecutive (possibly empty) chunks of the source sequence.
        /// </summary>
        /// <returns>The enumerable sequence that returns consecutive (possibly empty) chunks upon each iteration.</returns>
        public YacqQueryable<IList<TSource>> Chunkify()
        {
            return new YacqQueryable<IList<TSource>>(this.Symbols, this._source.Chunkify());
        }

        /// <summary>
        /// Produces an enumerable sequence that returns elements collected/aggregated from the source sequence between consecutive iterations.
        /// </summary>
        /// <param name="getInitialCollector"><c>() =></c> Factory to create the initial collector object.</param>
        /// <param name="merge"><c>(it, s) =></c> Merges a sequence element with the current collector.</param>
        /// <param name="getNewCollector"><c>(it) =></c> Factory to replace the current collector by a new collector.</param>
        /// <returns>The enumerable sequence that returns collected/aggregated elements from the source sequence upon each iteration.</returns>
        public YacqQueryable<TResult> Collect<TResult>(String getInitialCollector, String merge, String getNewCollector)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Collect(
                YacqServices.ParseLambda<TResult>(this.Symbols, getInitialCollector),
                YacqServices.ParseLambda<Func<TResult, TSource, TResult>>(this.Symbols, merge, "it", "s"),
                YacqServices.ParseLambda<TResult, TResult>(this.Symbols, getNewCollector)
            ));
        }

        /// <summary>
        /// Produces an enumerable sequence that returns elements collected/aggregated from the source sequence between consecutive iterations.
        /// </summary>
        /// <param name="newCollector"><c>() =></c> Factory to create a new collector object.</param>
        /// <param name="merge"><c>(it, s) =></c> Merges a sequence element with the current collector.</param>
        /// <returns>The enumerable sequence that returns collected/aggregated elements from the source sequence upon each iteration.</returns>
        public YacqQueryable<TResult> Collect<TResult>(String newCollector, String merge)
        {
            return new YacqQueryable<TResult>(this.Symbols, this._source.Collect(
                YacqServices.ParseLambda<TResult>(this.Symbols, newCollector),
                YacqServices.ParseLambda<Func<TResult, TSource, TResult>>(this.Symbols, merge, "it", "s")
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
        /// Concatenates two observable sequences.
        /// </summary>
        /// <param name="second">Second observable sequence.</param>
        /// <returns>An observable sequence that contains the elements of the first sequence, followed by those of the second the sequence.</returns>
        public YacqQbservable<TSource> Concat(IObservable<TSource> second)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Concat(second));
        }

        /// <summary>
        /// Determines whether an observable sequence contains a specified element by using the default equality comparer.
        /// </summary>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <returns>An observable sequence containing a single element determining whether the source sequence contains an element that has the specified value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> Contains(TSource value)
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.Contains(value));
        }

        /// <summary>
        /// Determines whether an observable sequence contains a specified element by using a specified System.Collections.Generic.IEqualityComparer&lt;T&gt;.
        /// </summary>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>An observable sequence containing a single element determining whether the source sequence contains an element that has the specified value.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> Contains(TSource value, IEqualityComparer<TSource> comparer)
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.Contains(value, comparer));
        }

        /// <summary>
        /// Returns a <see cref="T:System.Int32"/> that represents the total number of elements in an observable sequence.
        /// </summary>
        /// <returns>An observable sequence containing a single element with the number of elements in the input sequence.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Int32> Count()
        {
            return new YacqQbservable<Int32>(this.Symbols, this._source.Count());
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the type parameter's default value in a singleton sequence if the sequence is empty.
        /// </summary>
        /// <returns>An observable sequence that contains the default value for the TSource type if the source is empty; otherwise, the elements of the source itself.</returns>
        public YacqQbservable<TSource> DefaultIfEmpty()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DefaultIfEmpty());
        }

        /// <summary>
        /// Returns the elements of the specified sequence or the specified value in a singleton sequence if the sequence is empty.
        /// </summary>
        /// <param name="defaultValue">The value to return if the sequence is empty.</param>
        /// <returns>An observable sequence that contains the specified default value if the source is empty; otherwise, the elements of the source itself.</returns>
        public YacqQbservable<TSource> DefaultIfEmpty(TSource defaultValue)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DefaultIfEmpty(defaultValue));
        }

        /// <summary>
        /// Time shifts the observable sequence by dueTime.
        /// The relative time intervals between the values are preserved.
        /// </summary>
        /// <param name="dueTime">Absolute time used to shift the observable sequence; the relative time shift gets computed upon subscription.</param>
        /// <returns>Time-shifted sequence.</returns>
        public YacqQbservable<TSource> Delay(DateTimeOffset dueTime)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Delay(dueTime));
        }

        /// <summary>
        /// Time shifts the observable sequence by dueTime.
        /// The relative time intervals between the values are preserved.
        /// </summary>
        /// <param name="dueTime">Absolute time used to shift the observable sequence; the relative time shift gets computed upon subscription.</param>
        /// <param name="scheduler">Scheduler to run the delay timers on.</param>
        /// <returns>Time-shifted sequence.</returns>
        public YacqQbservable<TSource> Delay(DateTimeOffset dueTime, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Delay(dueTime, scheduler));
        }

        /// <summary>
        /// Time shifts the observable sequence by dueTime.
        /// The relative time intervals between the values are preserved.
        /// </summary>
        /// <param name="dueTime">Relative time by which to shift the observable sequence.</param>
        /// <returns>Time-shifted sequence.</returns>
        public YacqQbservable<TSource> Delay(TimeSpan dueTime)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Delay(dueTime));
        }

        /// <summary>
        /// Time shifts the observable sequence by dueTime.
        /// The relative time intervals between the values are preserved.
        /// </summary>
        /// <param name="dueTime">Relative time by which to shift the observable sequence.</param>
        /// <param name="scheduler">Scheduler to run the delay timers on.</param>
        /// <returns>Time-shifted sequence.</returns>
        public YacqQbservable<TSource> Delay(TimeSpan dueTime, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Delay(dueTime, scheduler));
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
        /// Returns an observable sequence that contains only distinct elements.
        /// </summary>
        /// <returns>An observable sequence only containing the distinct elements from the source sequence.</returns>
        public YacqQbservable<TSource> Distinct()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Distinct());
        }

        /// <summary>
        /// Returns an observable sequence that contains only distinct elements according to the comparer.
        /// </summary>
        /// <param name="comparer">Equality comparer for source elements.</param>
        /// <returns>An observable sequence only containing the distinct elements from the source sequence.</returns>
        public YacqQbservable<TSource> Distinct(IEqualityComparer<TSource> comparer)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Distinct(comparer));
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
        /// Returns an observable sequence that contains only distinct contiguous elements.
        /// </summary>
        /// <returns>An observable sequence only containing the distinct contiguous elements from the source sequence.</returns>
        public YacqQbservable<TSource> DistinctUntilChanged()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DistinctUntilChanged());
        }

        /// <summary>
        /// Returns an observable sequence that contains only distinct contiguous elements according to the comparer.
        /// </summary>
        /// <param name="comparer">Equality comparer for source elements.</param>
        /// <returns>An observable sequence only containing the distinct contiguous elements from the source sequence.</returns>
        public YacqQbservable<TSource> DistinctUntilChanged(IEqualityComparer<TSource> comparer)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.DistinctUntilChanged(comparer));
        }

        /// <summary>
        /// Invokes the observer's methods for their side-effects.
        /// </summary>
        /// <param name="observer">Observer whose methods to invoke as part of the source sequence's observation.</param>
        /// <returns>The source sequence with the side-effecting behavior applied.</returns>
        public YacqQbservable<TSource> Do(IObserver<TSource> observer)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Do(observer));
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
        /// Invokes an action for each element in the observable sequence and invokes an action upon exceptional termination of the observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <param name="onError"><c>(ex) =></c> Action to invoke upon exceptional termination of the observable sequence.</param>
        /// <returns>The source sequence with the side-effecting behavior applied.</returns>
        public YacqQbservable<TSource> Do(String onNext, String onError)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Do(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it"),
                YacqServices.ParseLambda<Action<Exception>>(this.Symbols, onError, "ex")
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
        /// Returns the element at a specified index in a sequence.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>An observable sequence that produces the element at the specified position in the source sequence.</returns>
        public YacqQbservable<TSource> ElementAt(Int32 index)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.ElementAt(index));
        }

        /// <summary>
        /// Returns the element at a specified index in a sequence or a default value if the index is out of range.
        /// </summary>
        /// <param name="index">The zero-based index of the element to retrieve.</param>
        /// <returns>An observable sequence that produces the element at the specified position in the source sequence, or a default value if the index is outside the bounds of the source sequence.</returns>
        public YacqQbservable<TSource> ElementAtOrDefault(Int32 index)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.ElementAtOrDefault(index));
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
        /// Returns the first element of an observable sequence.
        /// </summary>
        /// <returns>Sequence containing the first element in the observable sequence.</returns>
        public YacqQbservable<TSource> FirstAsync()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.FirstAsync());
        }

        /// <summary>
        /// Returns the first element of an observable sequence that matches the predicate.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A predicate function to evaluate for elements in the sequence.</param>
        /// <returns>Sequence containing the first element in the observable sequence for which the predicate holds.</returns>
        public YacqQbservable<TSource> FirstAsync(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.FirstAsync(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Returns the first element of an observable sequence, or a default value if no value is found.
        /// </summary>
        /// <returns>Sequence containing the first element in the observable sequence, or a default value if no value is found.</returns>
        public YacqQbservable<TSource> FirstOrDefaultAsync()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.FirstOrDefaultAsync());
        }

        /// <summary>
        /// Returns the first element of an observable sequence that matches the predicate, or a default value if no value is found.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A predicate function to evaluate for elements in the sequence.</param>
        /// <returns>The first element in the observable sequence for which the predicate holds, or a default value if no value is found.</returns>
        public YacqQbservable<TSource> FirstOrDefaultAsync(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.FirstOrDefaultAsync(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
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
        /// Ignores all values in an observable sequence leaving only the termination messages.
        /// </summary>
        /// <returns>An empty observable sequence that signals termination, successful or exceptional, of the source sequence.</returns>
        public YacqQbservable<TSource> IgnoreElements()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.IgnoreElements());
        }

        /// <summary>
        /// Determines whether an observable sequence is empty.
        /// </summary>
        /// <returns>An observable sequence containing a single element determining whether the source sequence is empty.</returns>
        public YacqQbservable<Boolean> IsEmpty()
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.IsEmpty());
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
        /// Returns the last element of an observable sequence.
        /// </summary>
        /// <returns>Sequence containing the last element in the observable sequence.</returns>
        public YacqQbservable<TSource> LastAsync()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.LastAsync());
        }

        /// <summary>
        /// Returns the last element of an observable sequence that matches the predicate.
        /// </summary>
        /// <param name="predicate">A predicate function to evaluate for elements in the sequence.</param>
        /// <returns>Sequence containing the last element in the observable sequence for which the predicate holds.</returns>
        public YacqQbservable<TSource> LastAsync(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.LastAsync(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Returns the last element of an observable sequence, or a default value if no value is found.
        /// </summary>
        /// <returns>Sequence containing the last element in the observable sequence, or a default value if no value is found.</returns>
        public YacqQbservable<TSource> LastOrDefaultAsync()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.LastOrDefaultAsync());
        }

        /// <summary>
        /// Returns the last element of an observable sequence that matches the predicate, or a default value if no value is found.
        /// </summary>
        /// <param name="predicate">A predicate function to evaluate for elements in the sequence.</param>
        /// <returns>Sequence containing the last element in the observable sequence, or a default value if no value is found.</returns>
        public YacqQbservable<TSource> LastOrDefaultAsync(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.LastOrDefaultAsync(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Samples the most recent value in an observable sequence.
        /// </summary>
        /// <returns>The enumerable sequence that returns the last sampled element upon each iteration and subsequently blocks until the next element in the observable source sequence becomes available.</returns>
        public YacqQueryable<TSource> Latest()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Latest());
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
        /// Returns a <see cref="T:System.Int64"/> that represents the total number of elements in an observable sequence.
        /// </summary>
        /// <returns>An observable sequence containing a single element with the number of elements in the input sequence.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Int64> LongCount()
        {
            return new YacqQbservable<Int64>(this.Symbols, this._source.LongCount());
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
        /// Materializes the implicit notifications of an observable sequence as explicit notification values.
        /// </summary>
        /// <returns>An observable sequence containing the materialized notification values from the source sequence.</returns>
        public YacqQbservable<Notification<TSource>> Materialize()
        {
            return new YacqQbservable<Notification<TSource>>(this.Symbols, this._source.Materialize());
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
        /// Merges two observable sequences into a single observable sequence.
        /// </summary>
        /// <param name="second">Second observable sequence.</param>
        /// <returns>The observable sequence that merges the elements of the given sequences.</returns>
        public YacqQbservable<TSource> Merge(IObservable<TSource> second)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Merge(second));
        }

        /// <summary>
        /// Merges two observable sequences into a single observable sequence.
        /// </summary>
        /// <param name="second">Second observable sequence.</param>
        /// <param name="scheduler">Scheduler used to introduce concurrency for making subscriptions to the given sequences.</param>
        /// <returns>The observable sequence that merges the elements of the given sequences.</returns>
        public YacqQbservable<TSource> Merge(IObservable<TSource> second, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Merge(second, scheduler));
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
        /// Samples the most recent value in an observable sequence.
        /// </summary>
        /// <param name="initialValue">Initial value that will be yielded by the enumerable sequence if no element has been sampled yet.</param>
        /// <returns>The enumerable sequence that returns the last sampled element upon each iteration.</returns>
        public YacqQueryable<TSource> MostRecent(TSource initialValue)
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.MostRecent(initialValue));
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
        /// Samples the next value (blocking without buffering) from in an observable sequence.
        /// </summary>
        /// <returns>The enumerable sequence that blocks upon each iteration until the next element in the observable source sequence becomes available.</returns>
        public YacqQueryable<TSource> Next()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.Next());
        }

        /// <summary>
        /// Asynchronously notify observers on the specified scheduler.
        /// </summary>
        /// <param name="scheduler">Scheduler to notify observers on.</param>
        /// <returns>The source sequence whose observations happen on the specified scheduler.</returns>
        public YacqQbservable<TSource> ObserveOn(IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.ObserveOn(scheduler));
        }

        /// <summary>
        /// Asynchronously notify observers on the specified synchronization context.
        /// </summary>
        /// <param name="context">Synchronization context to notify observers on.</param>
        /// <returns>The source sequence whose observations happen on the specified synchronization context.</returns>
        public YacqQbservable<TSource> ObserveOn(SynchronizationContext context)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.ObserveOn(context));
        }

        /// <summary>
        /// Filters the elements of an observable sequence based on the specified type.
        /// </summary>
        /// <returns>An observable sequence that contains elements from the input sequence of type TResult.</returns>
        public YacqQbservable<TResult> OfType<TResult>()
        {
            return new YacqQbservable<TResult>(this.Symbols, ((IQbservable<Object>) this._source).OfType<TResult>());
        }

        /// <summary>
        /// Continues an observable sequence that is terminated normally or by an exception with the next observable sequence.
        /// </summary>
        /// <param name="second">Second observable sequence used to produce results after the first sequence terminates.</param>
        /// <returns>An observable sequence that concatenates the first and second sequence, even if the first sequence terminates exceptionally.</returns>
        public YacqQbservable<TSource> OnErrorResumeNext(IObservable<TSource> second)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.OnErrorResumeNext(second));
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
        /// <seealso cref="T:System.Reactive.Subjects.Subject"/>
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

#if !SILVERLIGHT
        /// <summary>
        /// Makes an observable sequence remotable.
        /// </summary>
        /// <returns>The observable sequence that supports remote subscriptions.</returns>
        public YacqQbservable<TSource> Remotable()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Remotable());
        }
#endif

        /// <summary>
        /// Repeats the observable sequence indefinitely.
        /// </summary>
        /// <returns>The observable sequence producing the elements of the given sequence repeatedly and sequentially.</returns>
        public YacqQbservable<TSource> Repeat()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Repeat());
        }

        /// <summary>
        /// Repeats the observable sequence a specified number of times.
        /// </summary>
        /// <param name="repeatCount">Number of times to repeat the sequence.</param>
        /// <returns>The observable sequence producing the elements of the given sequence repeatedly.</returns>
        public YacqQbservable<TSource> Repeat(Int32 repeatCount)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Repeat(repeatCount));
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
        /// Repeats the source observable sequence until it successfully terminates.
        /// </summary>
        /// <returns>Observable sequence producing the elements of the given sequence repeatedly until it terminates successfully.</returns>
        public YacqQbservable<TSource> Retry()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Retry());
        }

        /// <summary>
        /// Repeats the source observable sequence the specified number of times or until it successfully terminates.
        /// </summary>
        /// <param name="retryCount">Number of times to repeat the sequence.</param>
        /// <returns>Observable sequence producing the elements of the given sequence repeatedly until it terminates successfully.</returns>
        public YacqQbservable<TSource> Retry(Int32 retryCount)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Retry(retryCount));
        }

        /// <summary>
        /// Samples the observable sequence at sampling ticks.
        /// </summary>
        /// <param name="sampler">Sampling tick sequence.</param>
        /// <returns>Sampled observable sequence.</returns>
        public YacqQbservable<TSource> Sample<TSample>(IObservable<TSample> sampler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Sample(sampler));
        }

        /// <summary>
        /// Samples the observable sequence at each interval.
        /// </summary>
        /// <param name="interval">Interval at which to sample.</param>
        /// <returns>Sampled observable sequence.</returns>
        public YacqQbservable<TSource> Sample(TimeSpan interval)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Sample(interval));
        }

        /// <summary>
        /// Samples the observable sequence at each interval.
        /// </summary>
        /// <param name="interval">Interval at which to sample.</param>
        /// <param name="scheduler">Scheduler to run the sampling timer on.</param>
        /// <returns>Sampled observable sequence.</returns>
        public YacqQbservable<TSource> Sample(TimeSpan interval, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Sample(interval, scheduler));
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
                YacqServices.ParseLambda<TSource, IObservable<TCollection>>(this.Symbols, collectionSelector),
                YacqServices.ParseLambda<Func<TSource, TCollection, TResult>>(this.Symbols, resultSelector, "it", "c")
            ));
        }

        /// <summary>
        /// Projects each element of an observable sequence to an observable sequence and flattens the resulting observable sequences into one observable sequence.
        /// </summary>
        /// <param name="other">An observable sequence to project each element from the source sequence onto.</param>
        /// <returns>An observable sequence whose elements are the result of projecting each source element onto the other sequence and merging all the resulting sequences together.</returns>
        public YacqQbservable<TOther> SelectMany<TOther>(IObservable<TOther> other)
        {
            return new YacqQbservable<TOther>(this.Symbols, this._source.SelectMany(other));
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
        /// Determines whether two sequences are equal by comparing the elements pairwise.
        /// </summary>
        /// <param name="second">Second observable sequence to compare.</param>
        /// <returns>An observable sequence that contains a single element which indicates whether both sequences are equal.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> SequenceEqual(IObservable<TSource> second)
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.SequenceEqual(second));
        }

        /// <summary>
        /// Determines whether two sequences are equal by comparing the elements pairwise using a specified equality comparer.
        /// </summary>
        /// <param name="second">Second observable sequence to compare.</param>
        /// <param name="comparer">Comparer used to compare elements of both sequences.</param>
        /// <returns>An observable sequence that contains a single element which indicates whether both sequences are equal.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<Boolean> SequenceEqual(IObservable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            return new YacqQbservable<Boolean>(this.Symbols, this._source.SequenceEqual(second, comparer));
        }

        /// <summary>
        /// Returns the only element of an observable sequence and throws an exception if there is not exactly one element in the observable sequence.
        /// </summary>
        /// <returns>Sequence containing the single element in the observable sequence.</returns>
        public YacqQbservable<TSource> SingleAsync()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SingleAsync());

        }

        /// <summary>
        /// Returns the only element of an observable sequence that matches the predicate and throws an exception if there is not exactly one element in the observable sequence.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A predicate function to evaluate for elements in the sequence.</param>
        /// <returns>Sequence containing the single element in the observable sequence.</returns>
        public YacqQbservable<TSource> SingleAsync(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SingleAsync(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Returns the only element of an observable sequence, or a default value if the observable sequence is empty; this method throws an exception if there is more than one element in the observable sequence.
        /// </summary>
        /// <returns>Sequence containing the single element in the observable sequence, or a default value if no value is found.</returns>
        public YacqQbservable<TSource> SingleOrDefaultAsync()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SingleOrDefaultAsync());
        }

        /// <summary>
        /// Returns the only element of an observable sequence that matches the predicate, or a default value if no value is found; this method throws an exception if there is more than one element in the observable sequence.
        /// </summary>
        /// <param name="predicate"><c>(it) =></c> A predicate function to evaluate for elements in the sequence.</param>
        /// <returns>Sequence containing the single element in the observable sequence, or a default value if no value is found.</returns>
        public YacqQbservable<TSource> SingleOrDefaultAsync(String predicate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SingleOrDefaultAsync(
                YacqServices.ParseLambda<TSource, Boolean>(this.Symbols, predicate)
            ));
        }

        /// <summary>
        /// Bypasses a specified number of values in an observable sequence and then returns the remaining values.
        /// </summary>
        /// <param name="count">The number of elements to skip before returning the remaining elements.</param>
        /// <returns>An observable sequence that contains the elements that occur after the specified index in the input sequence.</returns>
        public YacqQbservable<TSource> Skip(Int32 count)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Skip(count));
        }

        /// <summary>
        /// Bypasses a specified number of elements at the end of an observable sequence.
        /// </summary>
        /// <param name="count">Number of elements to bypass at the end of the source sequence.</param>
        /// <returns>An observable sequence containing the source sequence elements except for the bypassed ones at the end.</returns>
        public YacqQbservable<TSource> SkipLast(Int32 count)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SkipLast(count));
        }

        /// <summary>
        /// Returns the values from the source observable sequence only after the other observable sequence produces a value.
        /// </summary>
        /// <param name="other">Observable sequence that triggers propagation of elements of the source sequence.</param>
        /// <returns>An observable sequence containing the elements of the source sequence starting from the point the other sequence triggered propagation.</returns>
        public YacqQbservable<TSource> SkipUntil<TOther>(IObservable<TOther> other)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SkipUntil(other));
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
        /// Prepends a sequence of values to an observable sequence.
        /// </summary>
        /// <param name="scheduler">Scheduler to emit the prepended values on.</param>
        /// <param name="values">Values to prepend to the specified sequence.</param>
        /// <returns>The source sequence prepended with the specified values.</returns>
        public YacqQbservable<TSource> StartWith(IScheduler scheduler, TSource[] values)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.StartWith(scheduler, values));
        }

        /// <summary>
        /// Prepends a sequence of values to an observable sequence.
        /// </summary>
        /// <param name="values">Values to prepend to the specified sequence.</param>
        /// <returns>The source sequence prepended with the specified values.</returns>
        public YacqQbservable<TSource> StartWith(TSource[] values)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.StartWith(values));
        }

        /// <summary>
        /// Asynchronously subscribes and unsubscribes observers on the specified scheduler.
        /// </summary>
        /// <param name="scheduler">Scheduler to perform subscription and unsubscription actions on.</param>
        /// <returns>The source sequence whose subscriptions and unsubscriptions happen on the specified scheduler.</returns>
        public YacqQbservable<TSource> SubscribeOn(IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SubscribeOn(scheduler));
        }

        /// <summary>
        /// Asynchronously subscribes and unsubscribes observers on the specified synchronization context.
        /// </summary>
        /// <param name="context">Synchronization context to perform subscription and unsubscription actions on.</param>
        /// <returns>The source sequence whose subscriptions and unsubscriptions happen on the specified synchronization context.</returns>
        public YacqQbservable<TSource> SubscribeOn(SynchronizationContext context)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.SubscribeOn(context));
        }

        /// <summary>
        /// Synchronizes the observable sequence.
        /// </summary>
        /// <returns>The source sequence whose outgoing calls to observers are synchronized.</returns>
        public YacqQbservable<TSource> Synchronize()
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Synchronize());
        }

        /// <summary>
        /// Synchronizes the observable sequence.
        /// </summary>
        /// <param name="gate">Gate object to synchronize each observer call on.</param>
        /// <returns>The source sequence whose outgoing calls to observers are synchronized on the given gate object.</returns>
        public YacqQbservable<TSource> Synchronize(Object gate)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Synchronize(gate));
        }

        /// <summary>
        /// Returns a specified number of contiguous values from the start of an observable sequence.
        /// </summary>
        /// <param name="count">The number of elements to return.</param>
        /// <returns>An observable sequence that contains the specified number of elements from the start of the input sequence.</returns>
        public YacqQbservable<TSource> Take(Int32 count)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Take(count));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the end of an observable sequence.
        /// </summary>
        /// <param name="count">Number of elements to take from the end of the source sequence.</param>
        /// <returns>An observable sequence containing the specified number of elements from the of the source sequence.</returns>
        public YacqQbservable<TSource> TakeLast(Int32 count)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.TakeLast(count));
        }

        /// <summary>
        /// Returns the values from the source observable sequence until the other observable sequence produces a value.
        /// </summary>
        /// <param name="other">Observable sequence that terminates propagation of elements of the source sequence.</param>
        /// <returns>An observable sequence containing the elements of the source sequence up to the point the other sequence interrupted further propagation.</returns>
        public YacqQbservable<TSource> TakeUntil<TOther>(IObservable<TOther> other)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.TakeUntil(other));
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
        /// Ignores values from an observable sequence which are followed by another value before dueTime.
        /// </summary>
        /// <param name="dueTime">Duration of the throttle period for each value.</param>
        /// <returns>The throttled sequence.</returns>
        public YacqQbservable<TSource> Throttle(TimeSpan dueTime)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Throttle(dueTime));
        }

        /// <summary>
        /// Ignores values from an observable sequence which are followed by another value before dueTime.
        /// </summary>
        /// <param name="dueTime">Duration of the throttle period for each value.</param>
        /// <param name="scheduler">Scheduler to run the throttle timers on.</param>
        /// <returns>The throttled sequence.</returns>
        public YacqQbservable<TSource> Throttle(TimeSpan dueTime, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Throttle(dueTime, scheduler));
        }

        /// <summary>
        /// Records the time interval between consecutive values in an observable sequence.
        /// </summary>
        /// <returns>An observable sequence with time interval information on values.</returns>
        public YacqQbservable<TimeInterval<TSource>> TimeInterval()
        {
            return new YacqQbservable<TimeInterval<TSource>>(this.Symbols, this._source.TimeInterval());
        }

        /// <summary>
        /// Records the time interval between consecutive values in an observable sequence.
        /// </summary>
        /// <param name="scheduler">Scheduler used to compute time intervals.</param>
        /// <returns>An observable sequence with time interval information on values.</returns>
        public YacqQbservable<TimeInterval<TSource>> TimeInterval(IScheduler scheduler)
        {
            return new YacqQbservable<TimeInterval<TSource>>(this.Symbols, this._source.TimeInterval(scheduler));
        }

        /// <summary>
        /// Returns either the observable sequence or an TimeoutException if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Time when a timeout occurs.</param>
        /// <returns>The source sequence with a TimeoutException in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(DateTimeOffset dueTime)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime));
        }

        /// <summary>
        /// Returns the source observable sequence or the other observable sequence if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Time when a timeout occurs.</param>
        /// <param name="other">Sequence to return in case of a timeout.</param>
        /// <returns>The source sequence switching to the other sequence in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(DateTimeOffset dueTime, IObservable<TSource> other)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime, other));
        }

        /// <summary>
        /// Returns the source observable sequence or the other observable sequence if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Time when a timeout occurs.</param>
        /// <param name="other">Sequence to return in case of a timeout.</param>
        /// <param name="scheduler">Scheduler to run the timeout timers on.</param>
        /// <returns>The source sequence switching to the other sequence in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(DateTimeOffset dueTime, IObservable<TSource> other, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime, other, scheduler));
        }

        /// <summary>
        /// Returns either the observable sequence or an TimeoutException if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Time when a timeout occurs.</param>
        /// <param name="scheduler">Scheduler to run the timeout timers on.</param>
        /// <returns>The source sequence with a TimeoutException in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(DateTimeOffset dueTime, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime, scheduler));
        }

        /// <summary>
        /// Returns either the observable sequence or an TimeoutException if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Maxmimum duration between values before a timeout occurs.</param>
        /// <returns>The source sequence with a TimeoutException in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(TimeSpan dueTime)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime));
        }

        /// <summary>
        /// Returns the source observable sequence or the other observable sequence if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Maxmimum duration between values before a timeout occurs.</param>
        /// <param name="other">Sequence to return in case of a timeout.</param>
        /// <returns>The source sequence switching to the other sequence in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(TimeSpan dueTime, IObservable<TSource> other)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime, other));
        }

        /// <summary>
        /// Returns the source observable sequence or the other observable sequence if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Maxmimum duration between values before a timeout occurs.</param>
        /// <param name="other">Sequence to return in case of a timeout.</param>
        /// <param name="scheduler">Scheduler to run the timeout timers on.</param>
        /// <returns>The source sequence switching to the other sequence in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(TimeSpan dueTime, IObservable<TSource> other, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime, other, scheduler));
        }

        /// <summary>
        /// Returns either the observable sequence or an TimeoutException if dueTime elapses.
        /// </summary>
        /// <param name="dueTime">Maxmimum duration between values before a timeout occurs.</param>
        /// <param name="scheduler">Scheduler to run the timeout timers on.</param>
        /// <returns>The source sequence with a TimeoutException in case of a timeout.</returns>
        public YacqQbservable<TSource> Timeout(TimeSpan dueTime, IScheduler scheduler)
        {
            return new YacqQbservable<TSource>(this.Symbols, this._source.Timeout(dueTime, scheduler));
        }

        /// <summary>
        /// Records the timestamp for each value in an observable sequence.
        /// </summary>
        /// <returns>An observable sequence with timestamp information on values.</returns>
        public YacqQbservable<Timestamped<TSource>> Timestamp()
        {
            return new YacqQbservable<Timestamped<TSource>>(this.Symbols, this._source.Timestamp());
        }

        /// <summary>
        /// Records the timestamp for each value in an observable sequence.
        /// </summary>
        /// <param name="scheduler">Scheduler used to compute timestamps.</param>
        /// <returns>An observable sequence with timestamp information on values.</returns>
        public YacqQbservable<Timestamped<TSource>> Timestamp(IScheduler scheduler)
        {
            return new YacqQbservable<Timestamped<TSource>>(this.Symbols, this._source.Timestamp(scheduler));
        }

        /// <summary>
        /// Creates an array from an observable sequence.
        /// </summary>
        /// <returns>An observable sequence containing a single element with an array containing all the elements of the source sequence.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<TSource[]> ToArray()
        {
            return new YacqQbservable<TSource[]>(this.Symbols, this._source.ToArray());
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
        /// Creates a list from an observable sequence.
        /// </summary>
        /// <returns>An observable sequence containing a single element with a list containing all the elements of the source sequence.</returns>
        /// <remarks>The return value of this operator differs from the corresponding operator on IEnumerable in order to retain asynchronous behavior.</remarks>
        public YacqQbservable<IList<TSource>> ToList()
        {
            return new YacqQbservable<IList<TSource>>(this.Symbols, this._source.ToList());
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
        /// Converts an <see cref="IQbservable{TSource}"/> sequence to an <see cref="IQueryable{TSource}"/> sequence.
        /// </summary>
        /// <returns>The <see cref="IQueryable{TSource}"/> sequence containing the elements in the <see cref="IQbservable{TSource}"/> sequence.</returns>
        public YacqQueryable<TSource> ToQueryable()
        {
            return new YacqQueryable<TSource>(this.Symbols, this._source.ToQueryable());
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
        /// Projects each element of an observable sequence into consecutive non-overlapping windows which are produced based on element count information.
        /// </summary>
        /// <param name="count">Length of each window.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(Int32 count)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(count));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows which are produced based on element count information.
        /// </summary>
        /// <param name="count">Length of each window.</param>
        /// <param name="skip">Number of elements to skip between creation of consecutive windows.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(Int32 count, Int32 skip)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(count, skip));
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping windows which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each window.</param>
        /// <returns>The sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(TimeSpan timeSpan)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(timeSpan));
        }

        /// <summary>
        /// Projects each element of an observable sequence into a window that is completed when either it's full or a given amount of time has elapsed.
        /// </summary>
        /// <param name="timeSpan">Maximum time length of a window.</param>
        /// <param name="count">Maximum element count of a window.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(TimeSpan timeSpan, Int32 count)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(timeSpan, count));
        }

        /// <summary>
        /// Projects each element of an observable sequence into a window that is completed when either it's full or a given amount of time has elapsed.
        /// </summary>
        /// <param name="timeSpan">Maximum time length of a window.</param>
        /// <param name="count">Maximum element count of a window.</param>
        /// <param name="scheduler">Scheduler to run windowing timers on.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(TimeSpan timeSpan, Int32 count, IScheduler scheduler)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(timeSpan, count, scheduler));
        }

        /// <summary>
        /// Projects each element of an observable sequence into consecutive non-overlapping windows which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each window.</param>
        /// <param name="scheduler">Scheduler to run windowing timers on.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(TimeSpan timeSpan, IScheduler scheduler)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(timeSpan, scheduler));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each window.</param>
        /// <param name="timeShift">Interval between creation of consecutive windows.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(TimeSpan timeSpan, TimeSpan timeShift)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(timeSpan, timeShift));
        }

        /// <summary>
        /// Projects each element of an observable sequence into zero or more windows which are produced based on timing information.
        /// </summary>
        /// <param name="timeSpan">Length of each window.</param>
        /// <param name="timeShift">Interval between creation of consecutive windows.</param>
        /// <param name="scheduler">Scheduler to run windowing timers on.</param>
        /// <returns>An observable sequence of windows.</returns>
        public YacqQbservable<IObservable<TSource>> Window(TimeSpan timeSpan, TimeSpan timeShift, IScheduler scheduler)
        {
            return new YacqQbservable<IObservable<TSource>>(this.Symbols, this._source.Window(timeSpan, timeShift, scheduler));
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
        /// Subscribes an element handler and an exception handler to an observable sequence.
        /// </summary>
        /// <param name="onNext"><c>(it) =></c> Action to invoke for each element in the observable sequence.</param>
        /// <param name="onError"><c>(ex) =></c> Action to invoke upon exceptional termination of the observable sequence.</param>
        /// <returns>IDisposable object used to unsubscribe from the observable sequence.</returns>
        public IDisposable Subscribe(String onNext, String onError)
        {
            return this._source.Subscribe(
                YacqServices.ParseLambda<Action<TSource>>(this.Symbols, onNext, "it").Compile(),
                YacqServices.ParseLambda<Action>(this.Symbols, onError, "ex").Compile()
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