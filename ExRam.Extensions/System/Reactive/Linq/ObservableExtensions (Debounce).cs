﻿// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Monad;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval) where T : struct
        {
            Contract.Requires(source != null);

            return source.Debounce(debounceInterval, false, Scheduler.Default);
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval, IScheduler scheduler) where T : struct
        {
            Contract.Requires(source != null);

            return source.Debounce(debounceInterval, false, scheduler);
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval, bool emitLatestValue) where T : struct
        {
            Contract.Requires(source != null);

            return source.Debounce(debounceInterval, emitLatestValue, Scheduler.Default);
        }

        public static IObservable<T> Debounce<T>(this IObservable<T> source, TimeSpan debounceInterval, bool emitLatestValue, IScheduler scheduler) where T : struct
        {
            Contract.Requires(source != null);

            return Observable
                .Using(
                    () => new SerialDisposable(),
                    serial => Observable
                        .Create<T>(obs =>
                        {
                            var isCompleted = false;
                            var isDebouncing = false;
                            var syncRoot = new object();
                            OptionStrict<T> latestValue;

                            return source
                                .Subscribe(
                                    value =>
                                    {
                                        lock(syncRoot)
                                        {
                                            if (!isDebouncing)
                                            {
                                                isDebouncing = true;

                                                latestValue = OptionStrict<T>.Nothing;
                                                obs.OnNext(value);

                                                serial.Disposable = scheduler.Schedule(value, debounceInterval, (self, state) =>
                                                {
                                                    lock(syncRoot)
                                                    {
                                                        isDebouncing = false;

                                                        if ((!isCompleted) && (emitLatestValue) && (latestValue.HasValue))
                                                            obs.OnNext(latestValue.Value);
                                                    }
                                                });
                                            }
                                            else
                                            {
                                                latestValue = value.ToOptionStrict();
                                            }
                                        }
                                    },
                                    ex =>
                                    {
                                        lock(syncRoot)
                                        {
                                            isCompleted = true;
                                            obs.OnError(ex);
                                        }
                                    },
                                    () =>
                                    {
                                        lock(syncRoot)
                                        {
                                            isCompleted = true;
                                            obs.OnCompleted();
                                        }
                                    });
                        }));
        }
    }
}