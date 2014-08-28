﻿// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        public static IObservable<TResult> SelectAsync<TSource, TResult>(this IObservable<TSource> source, Func<TSource, Task<TResult>> selector)
        {
            Contract.Requires(source != null);
            Contract.Requires(selector != null);

            return source
                .SelectMany(x => selector(x).ToObservable());
        }
    }
}