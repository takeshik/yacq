using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
#if WINDOWS_PHONE
using Microsoft.Phone.Reactive;
#else
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
#endif

// AsynchronousExtensions, written by neuecc.
// See: http://neue.cc/2010/11/26_286.html
namespace AsynchronousExtensions
{
    internal static class Progress
    {
        public static Progress<T> Create<T>(T value, double currentLength, double totalLength)
        {
            return new Progress<T>(value, currentLength, totalLength);
        }
    }

    internal class Progress<T>
    {
        public T Value
        {
            get;
            private set;
        }
        public double TotalLength
        {
            get;
            private set;
        }
        public double CurrentLength
        {
            get;
            private set;
        }
        public int Percentage
        {
            get
            {
                return (TotalLength <= 0 || CurrentLength <= 0)
                    ? 0
                    : (int) ((CurrentLength / TotalLength) * 100);
            }
        }

        public Progress(T value, double currentLength, double totalLength)
        {
            Value = value;
            TotalLength = totalLength;
            CurrentLength = currentLength;
        }
    }

    internal static class WebRequestExtensions
    {
        public static IObservable<WebResponse> GetResponseAsObservable(this WebRequest request)
        {
            return ObservableForCompatible.Create<WebResponse>(observer =>
            {
                var disposable = new BooleanDisposable();

                Observable.FromAsyncPattern<WebResponse>(request.BeginGetResponse, ar =>
                {
                    var res = request.EndGetResponse(ar);
                    if (disposable.IsDisposed)
                        res.Close();
                    return res;
                })().Subscribe(observer);

                return disposable;
            });
        }

        public static IObservable<Stream> GetRequestStreamAsObservable(this WebRequest request)
        {
            return ObservableForCompatible.Create<Stream>(observer =>
            {
                var disposable = new BooleanDisposable();

                Observable.FromAsyncPattern<Stream>(request.BeginGetRequestStream, ar =>
                {
                    var res = request.EndGetRequestStream(ar);
                    if (disposable.IsDisposed)
                        res.Close();
                    return res;
                })().Subscribe(observer);

                return disposable;
            });
        }

        public static IObservable<byte[]> DownloadDataAsync(this WebRequest request)
        {
            return Observable.Defer(() => request.GetResponseAsObservable()).SelectMany(r => r.DownloadDataAsync());
        }

        public static IObservable<Progress<byte[]>> DownloadDataAsyncWithProgress(this WebRequest request, int chunkSize = 65536)
        {
            return Observable.Defer(() => request.GetResponseAsObservable()).SelectMany(r => r.DownloadDataAsyncWithProgress(chunkSize));
        }

        public static IObservable<string> DownloadStringAsync(this WebRequest request)
        {
            return DownloadStringAsync(request, Encoding.UTF8);
        }

        public static IObservable<string> DownloadStringAsync(this WebRequest request, Encoding encoding)
        {
            return Observable.Defer(() => request.GetResponseAsObservable()).SelectMany(r => r.DownloadStringAsync(encoding));
        }

        public static IObservable<string> DownloadStringLineAsync(this WebRequest request)
        {
            return DownloadStringLineAsync(request, Encoding.UTF8);
        }

        public static IObservable<string> DownloadStringLineAsync(this WebRequest request, Encoding encoding)
        {
            return Observable.Defer(() => request.GetResponseAsObservable()).SelectMany(r => r.DownloadStringLineAsync(encoding));
        }

        public static IObservable<WebResponse> UploadStringAsync(this WebRequest request, string data)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return request.UploadDataAsync(bytes);
        }

        public static IObservable<Progress<Unit>> UploadStringAsyncWithProgress(this WebRequest request, string data, int chunkSize = 65536)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            return request.UploadDataAsyncWithProgress(bytes, chunkSize);
        }

        public static IObservable<WebResponse> UploadValuesAsync(this WebRequest request, IDictionary<string, string> parameters)
        {
            var parameter = parameters.Select(kvp => Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value))
                .Aggregate(new StringBuilder(), (sb, x) => sb.Append(x)).ToString();
            var bytes = Encoding.UTF8.GetBytes(parameter);

            return request.UploadDataAsync(bytes);
        }

        public static IObservable<Progress<Unit>> UploadValuesAsyncWithProgress(this WebRequest request, IDictionary<string, string> parameters, int chunkSize = 65536)
        {
            var parameter = parameters.Select(kvp => Uri.EscapeDataString(kvp.Key) + "=" + Uri.EscapeDataString(kvp.Value))
               .Aggregate(new StringBuilder(), (sb, x) => sb.Append(x)).ToString();
            var bytes = Encoding.UTF8.GetBytes(parameter);

            return request.UploadDataAsyncWithProgress(bytes, chunkSize);
        }

        public static IObservable<WebResponse> UploadDataAsync(this WebRequest request, byte[] data)
        {
            return Observable.Defer(() => request.GetRequestStreamAsObservable())
                .SelectMany(stream => stream.WriteAsObservable(data, 0, data.Length).Finally(() => stream.Close()))
                .TakeLast(1)
                .SelectMany(_ => request.GetResponseAsObservable());
        }

        public static IObservable<Progress<Unit>> UploadDataAsyncWithProgress(this WebRequest request, byte[] data, int chunkSize = 65536)
        {
            return Observable.Defer(() => request.GetRequestStreamAsObservable())
                .SelectMany(stream => stream.WriteAsync(data, chunkSize))
                .Scan(0, (i, _) => i + 1)
                .Select(i =>
                {
                    var currentLength = i * chunkSize;
                    if (currentLength > data.Length)
                        currentLength = data.Length;
                    return Progress.Create(new Unit(), currentLength, data.Length);
                });
        }
    }

    internal static class WebResponseExtensions
    {
        public static IObservable<byte[]> DownloadDataAsync(this WebResponse response)
        {
            return Observable.Defer(() => response.GetResponseStream().ReadAsync())
                .Finally(() => response.Close())
                .Aggregate(new List<byte>(), (list, bytes) =>
                {
                    list.AddRange(bytes);
                    return list;
                })
                .Select(x => x.ToArray());
        }

        public static IObservable<Progress<byte[]>> DownloadDataAsyncWithProgress(this WebResponse response, int chunkSize = 65536)
        {
            return Observable.Defer(() => response.GetResponseStream().ReadAsync(chunkSize))
                .Finally(() => response.Close())
                .Scan(Progress.Create(new byte[0], 0, 0),
                    (p, bytes) => Progress.Create(bytes, p.CurrentLength + bytes.Length, response.ContentLength));
        }

        public static IObservable<string> DownloadStringAsync(this WebResponse response)
        {
            return DownloadStringAsync(response, Encoding.UTF8);
        }

        public static IObservable<string> DownloadStringAsync(this WebResponse response, Encoding encoding)
        {
            return response.DownloadDataAsync().Select(x => encoding.GetString(x, 0, x.Length));
        }

        public static IObservable<string> DownloadStringLineAsync(this WebResponse response)
        {
            return DownloadStringLineAsync(response, Encoding.UTF8);
        }

        public static IObservable<string> DownloadStringLineAsync(this WebResponse response, Encoding encoding)
        {
            return Observable.Defer(() => response.GetResponseStream().ReadLineAsync(encoding))
                .Finally(() => response.Close());
        }
    }

    internal static class StreamExtensions
    {
        public static IObservable<Unit> WriteAsObservable(this Stream stream, byte[] buffer, int offset, int count)
        {
            return Observable.FromAsyncPattern((ac, o) => stream.BeginWrite(buffer, offset, count, ac, o), stream.EndWrite)();
        }

        public static IObservable<int> ReadAsObservable(this Stream stream, byte[] buffer, int offset, int count)
        {
            return Observable.FromAsyncPattern<int>((ac, o) => stream.BeginRead(buffer, offset, count, ac, o), stream.EndRead)();
        }

        public static IObservable<Unit> WriteAsync(this Stream stream, string data)
        {
            return WriteAsync(stream, data, Encoding.UTF8);
        }

        public static IObservable<Unit> WriteAsync(this Stream stream, string data, Encoding encoding)
        {
            return WriteAsync(stream, encoding.GetBytes(data));
        }

        public static IObservable<Unit> WriteAsync(this Stream stream, IEnumerable<byte> data, int chunkSize = 65536)
        {
            return WriteAsync(stream, data.ToObservable(), chunkSize);
        }

        public static IObservable<Unit> WriteAsync(this Stream stream, IObservable<byte> data, int chunkSize = 65536)
        {
            return Observable.Defer(() => data)
                .Buffer(chunkSize)
                .SelectMany(l => stream.WriteAsObservable(l.ToArray(), 0, l.Count))
                .Finally(() => stream.Close());
        }

        public static IObservable<Unit> WriteLineAsync(this Stream stream, string data)
        {
            return WriteLineAsync(stream, data, Encoding.UTF8);
        }

        public static IObservable<Unit> WriteLineAsync(this Stream stream, string data, Encoding encoding)
        {
            return WriteAsync(stream, data + Environment.NewLine, encoding);
        }

        public static IObservable<Unit> WriteLineAsync(this Stream stream, IEnumerable<string> data)
        {
            return WriteLineAsync(stream, data, Encoding.UTF8);
        }

        public static IObservable<Unit> WriteLineAsync(this Stream stream, IObservable<string> data)
        {
            return WriteLineAsync(stream, data, Encoding.UTF8);
        }

        public static IObservable<Unit> WriteLineAsync(this Stream stream, IEnumerable<string> data, Encoding encoding)
        {
            return WriteLineAsync(stream, data.ToObservable(), encoding);
        }

        public static IObservable<Unit> WriteLineAsync(this Stream stream, IObservable<string> data, Encoding encoding)
        {
            return WriteAsync(stream, data.SelectMany(s => encoding.GetBytes(s + Environment.NewLine)));
        }

        public static IObservable<byte[]> ReadAsync(this Stream stream, int chunkSize = 65536)
        {
            return Observable.Defer(() => Observable.Return(new byte[chunkSize], Scheduler.CurrentThread))
                .SelectMany(buffer => stream.ReadAsObservable(buffer, 0, chunkSize),
                    (buffer, readCount) => new
                    {
                        buffer,
                        readCount
                    })
                .Repeat()
                .TakeWhile(a => a.readCount != 0)
                .Select(a =>
                {
                    if (a.readCount == chunkSize)
                        return a.buffer;

                    var newBuffer = new byte[a.readCount];
                    Array.Copy(a.buffer, newBuffer, a.readCount);
                    return newBuffer;
                })
                .Finally(() => stream.Close());
        }

        public static IObservable<string> ReadLineAsync(this Stream stream, int chunkSize = 65536)
        {
            return ReadLineAsync(stream, Encoding.UTF8, chunkSize);
        }

        public static IObservable<string> ReadLineAsync(this Stream stream, Encoding encoding, int chunkSize = 65536)
        {
            return ObservableForCompatible.Create<string>(observer =>
            {
                var decoder = encoding.GetDecoder();
                var bom = encoding.GetChars(encoding.GetPreamble()).FirstOrDefault();
                var sb = new StringBuilder();
                var prev = default(char);

                return stream.ReadAsync(chunkSize)
                    .SelectMany(bytes =>
                    {
                        var charBuffer = new char[encoding.GetMaxCharCount(bytes.Length)];
                        var count = decoder.GetChars(bytes, 0, bytes.Length, charBuffer, 0);
                        return charBuffer.Take(count);
                    })
                    .Subscribe(
                        c =>
                        {
                            if (c == bom)
                            {
                            } // skip bom
                            else if (prev == '\r' && c == '\n')
                            {
                            } // when \r\n do nothing
                            else if (c == '\r' || c == '\n')   // reach at EndOfLine
                            {
                                var str = sb.ToString();
                                sb.Length = 0;
                                observer.OnNext(str);
                            }
                            else
                                sb.Append(c); // normally char

                            prev = c;
                        },
                        observer.OnError,
                        () =>
                        {
                            var str = sb.ToString();
                            if (str != "")
                                observer.OnNext(str);
                            observer.OnCompleted();
                        });
            });
        }
    }

    internal static class ObservableForCompatible
    {
#if WINDOWS_PHONE
        public static IObservable<IList<T>> Buffer<T>(this IObservable<T> source, int count)
        {
            return source.BufferWithCount(count);
        }
#endif

        public static IObservable<TSource> Create<TSource>(Func<IObserver<TSource>, IDisposable> subscribe)
        {
#if WINDOWS_PHONE
            return Observable.CreateWithDisposable(subscribe);
#else
            return Observable.Create(subscribe);
#endif
        }
    }
}

// Common extension methods by takeshik.
internal static class Extension
{
    internal static Boolean If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate)
    {
        return predicate(self);
    }

    internal static TResult If<TReceiver, TResult>(this TReceiver self, Func<TReceiver, Boolean> predicate, TResult valueIfTrue, TResult valueIfFalse)
    {
        if (self == null)
        {
            return default(TResult);
        }
        else if (self == null || predicate(self))
        {
            return valueIfTrue;
        }
        else
        {
            return valueIfFalse;
        }
    }

    internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, TReceiver valueIfTrue)
    {
        return self.If(predicate, valueIfTrue, self);
    }

    internal static TResult If<TReceiver, TResult>(this TReceiver self, Func<TReceiver, Boolean> predicate, Func<TReceiver, TResult> funcIfTrue, Func<TReceiver, TResult> funcIfFalse)
    {
        if (predicate(self))
        {
            return funcIfTrue(self);
        }
        else
        {
            return funcIfFalse(self);
        }
    }

    internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Func<TReceiver, TReceiver> funcIfTrue)
    {
        return self.If(predicate, funcIfTrue, _ => _);
    }

    internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Action<TReceiver> actionIfTrue, Action<TReceiver> actionIfFalse)
    {
        if (predicate(self))
        {
            actionIfTrue(self);
        }
        else
        {
            actionIfFalse(self);
        }
        return self;
    }

    internal static TReceiver If<TReceiver>(this TReceiver self, Func<TReceiver, Boolean> predicate, Action<TReceiver> actionIfTrue)
    {
        return self.If(predicate, actionIfTrue, _ =>
        {
        });
    }

    internal static TResult Null<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func, TResult valueIfNull)
        where TReceiver : class
    {
        if (self == null)
        {
            return valueIfNull;
        }
        else
        {
            return func(self);
        }
    }

    internal static TResult Null<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func)
        where TReceiver : class
    {
        return Null(self, func, default(TResult));
    }

    internal static void Null<TReceiver>(this TReceiver self, Action<TReceiver> action)
    {
        if (self != null)
        {
            action(self);
        }
    }

    internal static TResult Let<TReceiver, TResult>(this TReceiver self, Func<TReceiver, TResult> func)
    {
        return func(self);
    }

    internal static TReceiver Apply<TReceiver>(this TReceiver self, params Action<TReceiver>[] actions)
    {
        return Apply(self, (IEnumerable<Action<TReceiver>>) actions);
    }

    internal static TReceiver Apply<TReceiver>(this TReceiver self, IEnumerable<Action<TReceiver>> actions)
    {
        foreach (var a in actions)
        {
            a(self);
        }
        return self;
    }

    internal static IEnumerable<TResult> Choose<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        where TSource : class
    {
        return source.Select(selector).Where(_ => _ != null);
    }
}
