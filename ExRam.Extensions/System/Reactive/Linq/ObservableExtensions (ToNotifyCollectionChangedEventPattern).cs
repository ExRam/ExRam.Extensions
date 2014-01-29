// (c) Copyright 2013 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace System.Reactive.Linq
{
    public static partial class ObservableExtensions
    {
        #region EventPatternSource
        private sealed class EventPatternSource : EventPatternSourceBase<object, NotifyCollectionChangedEventArgs>, INotifyCollectionChanged
        {
            public EventPatternSource(IObservable<EventPattern<object, NotifyCollectionChangedEventArgs>> source) : base(source, (invokeAction, eventPattern) => invokeAction(eventPattern.Sender, eventPattern.EventArgs))
            {
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged
            {
                add
                {
                    base.Add(value, (o, e) => value(o, e));
                }

                remove
                {
                    base.Remove(value);
                }
            }
        }
        #endregion

        public static INotifyCollectionChanged ToNotifyCollectionChangedEventPattern(this IObservable<NotifyCollectionChangedEventArgs> source, object sender)
        {
            Contract.Requires(source != null);

            return new EventPatternSource(source.Select(x => new EventPattern<NotifyCollectionChangedEventArgs>(sender, x)));
        }
    }
}
