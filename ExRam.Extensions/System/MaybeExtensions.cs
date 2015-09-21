using System.Diagnostics.Contracts;
using Monad;

namespace System
{
    public static class MaybeExtensions
    {
        public static T? ToNullable<T>(this OptionStrict<T> maybe) where T : struct
        {
            Contract.Requires(maybe != null);

            return ((maybe.HasValue) ? (maybe.Value) : (new T?()));
        }
    }
}