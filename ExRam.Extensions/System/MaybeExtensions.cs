using System.Collections.Generic;
using System.Linq;
using LanguageExt;

namespace System
{
    public static class MaybeExtensions
    {
        public static T GetValue<T>(this Option<T> self)
        {
            if (self.IsNone)
                throw new InvalidOperationException();

            return self.IfNoneUnsafe(default(Func<T>));
        }

        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this Option<T> self)
        {
            return self
                .Map(AsyncEnumerable.Return)
                .IfNone(AsyncEnumerable.Empty<T>());
        }
    }
}