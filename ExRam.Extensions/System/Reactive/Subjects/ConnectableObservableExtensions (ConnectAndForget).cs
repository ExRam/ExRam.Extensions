// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Diagnostics.Contracts;

namespace System.Reactive.Subjects
{
    public static partial class ConnectableObservableExtensions
    {
        public static IObservable<T> ConnectAndForget<T>(this IConnectableObservable<T> source)
        {
            Contract.Requires(source != null);

            source.Connect();
            return source;
        }
    }
}
