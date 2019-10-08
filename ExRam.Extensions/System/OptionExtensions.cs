using System;
using System.Threading.Tasks;

namespace LanguageExt
{
    public static class OptionExtensions
    {
        public static Option<T> IfNone<T>(this Option<T> option, Func<Option<T>> none)
        {
            return option
                .Match(x => x, none);
        }

        public static Task<Option<T>> IfNoneAsync<T>(this Option<T> option, Func<Task<Option<T>>> none)
        {
            return option
                .MatchAsync(x => Task.FromResult((Option<T>)x), none);
        }

        public static async Task<Option<T>> IfNoneAsync<T>(this Task<Option<T>> option, Func<Task<Option<T>>> none)
        {
            return await (await option.ConfigureAwait(false))
                .MatchAsync(x => Task.FromResult((Option<T>)x), none)
                .ConfigureAwait(false);
        }

        public static TValue? ToNullable<TValue>(this Option<TValue> value) where TValue : class
        {
            return value.IfNoneUnsafe(default(TValue)!);
        }
    }
}