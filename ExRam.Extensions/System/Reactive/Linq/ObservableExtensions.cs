// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using LanguageExt.SomeHelp;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        private sealed class ResourceEachUsingObserver<T> : IObserver<T>, IDisposable
        {
            private readonly IObserver<T> _baseObserver;
            private readonly object _syncRoot = new object();
            private readonly Func<T, IDisposable> _resourceFactory;

            private IDisposable _resource;
           
            public ResourceEachUsingObserver(IObserver<T> baseObserver, Func<T, IDisposable> resourceFactory)
            {
                _baseObserver = baseObserver;
                _resourceFactory = resourceFactory;
            }

            public void OnCompleted()
            {
                Dispose();
                _baseObserver.OnCompleted();
            }

            public void OnError(Exception error)
            {
                Dispose();
                _baseObserver.OnError(error);
            }

            public void OnNext(T value)
            {
                lock (_syncRoot)
                {
                    _resource?.Dispose();
                    _resource = _resourceFactory(value);
                }

                _baseObserver.OnNext(value);
            }

            public void Dispose()
            {
                lock (_syncRoot)
                {
                    _resource?.Dispose();
                    _resource = null;
                }
            }
        }

        private sealed class ObservableEachUsingObserver<TSource, TOther> : IObserver<TSource>, IDisposable
        {
            private sealed class InnerObserver : IObserver<TOther>
            {
                private readonly IDisposable _subscription;
                private readonly ObservableEachUsingObserver<TSource, TOther> _parentObserver;

                public InnerObserver(ObservableEachUsingObserver<TSource, TOther> parentObserver, IDisposable subscription)
                {
                    _parentObserver = parentObserver;
                    _subscription = subscription;
                }

                public void OnCompleted()
                {
                    _subscription.Dispose();
                }

                public void OnError(Exception error)
                {
                    _parentObserver.OnError(error);
                }

                public void OnNext(TOther value)
                {
                }
            }

            private readonly object _syncRoot = new object();
            private readonly IObserver<TSource> _baseObserver;
            private readonly Func<TSource, IObservable<TOther>> _observableFactory;

            private IDisposable _resource;

            public ObservableEachUsingObserver(IObserver<TSource> baseObserver, Func<TSource, IObservable<TOther>> observableFactory)
            {
                _baseObserver = baseObserver;
                _observableFactory = observableFactory;
            }

            public void OnCompleted()
            {
                Dispose();
                _baseObserver.OnCompleted();
            }

            public void OnError(Exception error)
            {
                Dispose();
                _baseObserver.OnError(error);
            }

            public void OnNext(TSource value)
            {
                lock (_syncRoot)
                {
                    var subscription = new SingleAssignmentDisposable();

                    _resource?.Dispose();
                    _resource = subscription;

                    subscription.Disposable = _observableFactory(value)
                        .Subscribe(new InnerObserver(this, subscription));
                }

                _baseObserver.OnNext(value);
            }
            
            public void Dispose()
            {
                lock (_syncRoot)
                {
                    _resource?.Dispose();
                    _resource = null;
                }
            }
        }

        private sealed class GroupedObservableImpl<TKey, TSource> : IGroupedObservable<TKey, TSource>
        {
            private readonly IObservable<TSource> _baseObservable;

            public GroupedObservableImpl(IObservable<TSource> baseObservable, TKey key)
            {
                Key = key;
                _baseObservable = baseObservable;
            }

            public IDisposable Subscribe(IObserver<TSource> observer)
            {
                return _baseObservable.Subscribe(observer);
            }

            public TKey Key { get; }
        }

        private sealed class NotifyCollectionChangedEventPatternSource : EventPatternSourceBase<object, NotifyCollectionChangedEventArgs>, INotifyCollectionChanged
        {
            public NotifyCollectionChangedEventPatternSource(IObservable<EventPattern<object, NotifyCollectionChangedEventArgs>> source) : base(source, (invokeAction, eventPattern) => invokeAction(eventPattern.Sender, eventPattern.EventArgs))
            {
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add
                {
                    Add(value, (o, e) => value(o, e));
                }

                remove
                {
                    Remove(value);
                }
            }
        }

        private sealed class NotifyPropertyChangedEventPatternSource : EventPatternSourceBase<object, PropertyChangedEventArgs>, INotifyPropertyChanged
        {
            public NotifyPropertyChangedEventPatternSource(IObservable<EventPattern<object, PropertyChangedEventArgs>> source) : base(source, (invokeAction, eventPattern) => invokeAction(eventPattern.Sender, eventPattern.EventArgs))
            {
            }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add
                {
                    Add(value, (o, e) => value(o, e));
                }

                remove
                {
                    Remove(value);
                }
            }
        }

        public static IObservable<(TSource1, TSource2)> CombineLatest<TSource1, TSource2>(this IObservable<TSource1> first, IObservable<TSource2> second)
        {
            return first.CombineLatest(second, ValueTuple.Create);
        }

        public static IObservable<(TSource1, TSource2, TSource3)> CombineLatest<TSource1, TSource2, TSource3>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3)
        {
            return source1.CombineLatest(source2, source3, ValueTuple.Create);
        }

        public static IObservable<(TSource1, TSource2, TSource3, TSource4)> CombineLatest<TSource1, TSource2, TSource3, TSource4>(this IObservable<TSource1> source1, IObservable<TSource2> source2, IObservable<TSource3> source3, IObservable<TSource4> source4)
        {
            return source1.CombineLatest(source2, source3, source4, ValueTuple.Create);
        }

        public static IObservable<T> Concat<T>(this IObservable<T> source, Func<Option<T>, IObservable<T>> continuationSelector)
        {
            return Observable.Create<T>(obs =>
            {
                var lastValue = Option<T>.None;
                var subscription = new SerialDisposable();
                var firstSubscription = new SingleAssignmentDisposable();

                subscription.Disposable = firstSubscription;

                firstSubscription.Disposable = source.Subscribe(
                    value =>
                    {
                        lastValue = value;
                        obs.OnNext(value);
                    },
                    ex =>
                    {
                        subscription.Dispose();
                        obs.OnError(ex);
                    },
                    () =>
                    {
                        subscription.Disposable.Dispose();
                        subscription.Disposable = continuationSelector(lastValue).Subscribe(obs);
                    });

                return subscription;
            });
        }

        public static IAsyncEnumerable<T> Current<T>(this IObservable<T> source)
        {
            return AsyncEnumerable.Create(Core);

            async IAsyncEnumerator<T> Core(CancellationToken ct)
            {
                var replaySubject = new ReplaySubject<Notification<T>>(1);

                using (source.Materialize().Multicast(replaySubject).Connect())
                {
                    while (true)
                    {
                        var notification = await replaySubject
                            .FirstAsync()
                            .ToTask(ct);

                        if (notification.HasValue)
                            yield return notification.Value;
                        else if (notification.Exception != null)
                            throw notification.Exception;
                        else
                            yield break;
                    }
                }
            }
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval) where T : struct
        {
            return source.Debounce(debounceInterval, false, Scheduler.Default);
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval, IScheduler scheduler) where T : struct
        {
            return source.Debounce(debounceInterval, false, scheduler);
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval, bool emitLatestValue) where T : struct
        {
            return source.Debounce(debounceInterval, emitLatestValue, Scheduler.Default);
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval, bool emitLatestValue, IScheduler scheduler) where T : struct
        {
            return Observable
                .Using(
                    () => new SerialDisposable(),
                    serial => Observable
                        .Create<T>(obs =>
                        {
                            var isCompleted = false;
                            var isDebouncing = false;
                            var syncRoot = new object();
                            Option<T> maybeLatestValue;

                            return source
                                .Subscribe(
                                    value =>
                                    {
                                        lock (syncRoot)
                                        {
                                            if (!isDebouncing)
                                            {
                                                isDebouncing = true;

                                                maybeLatestValue = Option<T>.None;
                                                obs.OnNext(value);

                                                serial.Disposable = scheduler.Schedule(value, debounceInterval, (self, state) =>
                                                {
                                                    lock (syncRoot)
                                                    {
                                                        isDebouncing = false;

                                                        if (!isCompleted && emitLatestValue)
                                                            maybeLatestValue.IfSome(latestValue => obs.OnNext(latestValue));
                                                    }
                                                });
                                            }
                                            else
                                            {
                                                maybeLatestValue = value.ToSome();
                                            }
                                        }
                                    },
                                    ex =>
                                    {
                                        lock (syncRoot)
                                        {
                                            isCompleted = true;
                                            obs.OnError(ex);
                                        }
                                    },
                                    () =>
                                    {
                                        lock (syncRoot)
                                        {
                                            isCompleted = true;
                                            obs.OnCompleted();
                                        }
                                    });
                        }));
        }

        public static IObservable<T> DefaultIfEmpty<T>(this IObservable<T> source, IObservable<T> defaultObservable)
        {
            return source.Concat(maybe => !maybe.IsSome ? defaultObservable : Observable.Empty<T>());
        }

        public static IObservable<T> EachUsing<T>(this IObservable<T> source, Func<T, IDisposable> resourceFactory)
        {
            return Observable.Create<T>(obs =>
            {
                var eachUsingObserver = new ResourceEachUsingObserver<T>(obs, resourceFactory);

                return StableCompositeDisposable.Create(source.Subscribe(eachUsingObserver), eachUsingObserver);
            });
        }

        public static IObservable<TSource> EachUsing<TSource, TOther>(this IObservable<TSource> source, Func<TSource, IObservable<TOther>> observableFactory)
        {
            return Observable.Create<TSource>(obs =>
            {
                var eachUsingObserver = new ObservableEachUsingObserver<TSource, TOther>(obs, observableFactory);

                return StableCompositeDisposable.Create(source.Subscribe(eachUsingObserver), eachUsingObserver);
            });
        }

        public static IObservable<T> Forever<T>(T value)
        {
            return Observable.Create<T>(observer =>
            {
                observer.OnNext(value);
                return Disposable.Empty;
            });
        }

        public static IObservable<T> KeepOpen<T>(this IObservable<T> source)
        {
            return source.Concat(Observable.Never<T>());
        }

        public static IObservable<Unit> OnCompletion<T>(this IObservable<T> source)
        {
            return Observable.Create<Unit>(observer => source.Subscribe(
                x =>
                {
                },
                observer.OnError,
                () =>
                {
                    observer.OnNext(Unit.Default);
                    observer.OnCompleted();
                }));
        }

        public static IObservable<Exception?> OnCompletionOrError<T>(this IObservable<T> source)
        {
            return Observable.Create<Exception?>(observer => source.Subscribe(
                x =>
                {
                },
                ex =>
                {
                    observer.OnNext(ex);
                    observer.OnCompleted();
                },
                () =>
                {
                    observer.OnNext(default);
                    observer.OnCompleted();
                }));
        }

        public static IObservable<T> RepeatWhileEmpty<T>(this IObservable<T> source)
        {
            return source.RepeatWhileEmpty(null);
        }

        public static IObservable<T> RepeatWhileEmpty<T>(this IObservable<T> source, int repeatCount)
        {
            return source.RepeatWhileEmpty((int?)repeatCount);
        }

        private static IObservable<T> RepeatWhileEmpty<T>(this IObservable<T> source, int? repeatCount)
        {
            if (repeatCount.HasValue && repeatCount.Value == 0)
                return Observable.Empty<T>();

            return source
                .Concat(maybe => !maybe.IsSome ? source.RepeatWhileEmpty(repeatCount - 1) : Observable.Empty<T>());
        }

        public static IObservable<TAccumulate> ScanAsync<TSource, TAccumulate>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, Task<TAccumulate>> accumulator)
        {
            return source
                .Scan(Task.FromResult(seed), async (currentTask, value) => await accumulator(await currentTask.ConfigureAwait(false), value).ConfigureAwait(false))
                .SelectMany(x => x);
        }

        public static IObservable<Unit> SelectMany<TSource>(this IObservable<TSource> source, Func<TSource, CancellationToken, Task> taskSelector)
        {
            return source
                .SelectMany((x, ct) => taskSelector(x, ct).AsUnitTask());
        }

        public static IObservable<TSource> StateScan<TSource, TState>(this IObservable<TSource> source, TState seed, Func<TState, TSource, TState> stateFunction)
        {
            return source
                .Scan(
                    (seed, default(TSource)),
                    (stateTuple, value) => (stateFunction(stateTuple.Item1, value), value))
                .Select(stateTuple => stateTuple.Item2);
        }

        public static IObservable<T> TakeUntil<T>(this IObservable<T> source, CancellationToken ct)
        {
            return source.TakeUntil(ct.ToObservable());
        }

        public static IObservable<TSource> TakeWhileInclusive<TSource>(this IObservable<TSource> source, Func<TSource, bool> predicate)
        {
            return source
                .TakeWhileInclusive((x, i) => predicate(x));
        }

        public static IObservable<TSource> TakeWhileInclusive<TSource>(this IObservable<TSource> source, Func<TSource, int, bool> predicate)
        {
            return Observable.Using(
                () => new SingleAssignmentDisposable(),
                disposable => Observable.Create<TSource>(
                    o =>
                    {
                        var index = 0;

                        return disposable.Disposable = source
                            .Subscribe(
                                x =>
                                {
                                    o.OnNext(x);
                                    if (!predicate(x, checked(index++)))
                                        o.OnCompleted();
                                },
                                o.OnError,
                                o.OnCompleted);
                    }));
        }

        public static IObservable<T> ThrowOnCancellation<T>(this IObservable<T> source, CancellationToken ct)
        {
            return source.TakeUntil(ct)
                .Concat(Observable.If(
                    () => ct.IsCancellationRequested,
                    Observable.Throw<T>(new OperationCanceledException())));
        }

        public static IObservable<Counting<T>> ToCounting<T>(this IObservable<T> source)
        {
            return source.Select((x, i) => new Counting<T>((ulong)i, x));
        }

        public static IGroupedObservable<TKey, TSource> ToGroup<TKey, TSource>(this IObservable<TSource> source, TKey key)
        {
            return new GroupedObservableImpl<TKey, TSource>(source, key);
        }

        public static INotifyCollectionChanged ToNotifyCollectionChangedEventPattern(this IObservable<NotifyCollectionChangedEventArgs> source, object sender)
        {
            return new NotifyCollectionChangedEventPatternSource(source.Select(x => new EventPattern<NotifyCollectionChangedEventArgs>(sender, x)));
        }

        public static INotifyPropertyChanged ToNotifyPropertyChangedEventPattern(this IObservable<PropertyChangedEventArgs> source, object sender)
        {
            return new NotifyPropertyChangedEventPatternSource(source.Select(x => new EventPattern<PropertyChangedEventArgs>(sender, x)));
        }

        public static IObservable<Option<T>> TryFirstAsync<T>(this IObservable<T> source)
        {
            return source
                .Select(x => (Option<T>)x)
                .FirstOrDefaultAsync();
        }

        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, CancellationToken, Task<bool>> predicate)
        {
            return source
                .SelectMany(x => WithCancellation(
                    ct => predicate(x, ct)
                        .ToObservable()
                        .Where(b => b)
                        .Select(b => x)));
        }

        public static IObservable<T> WhereNotNull<T>(this IObservable<T> source) where T : class
        {
            return source
                .Where(x => x != null)
                .Select(x => x!);
        }

        public static IObservable<T> WithCancellation<T>(Func<CancellationToken, IObservable<T>> observableFactory)
        {
            return Observable
                .Using(
                    () => new CancellationDisposable(),
                    cts => observableFactory(cts.Token));
        }
    }
}
