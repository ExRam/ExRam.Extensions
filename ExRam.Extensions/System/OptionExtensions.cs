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

        public static OptionAsync<T> IfNoneAsync<T>(this OptionAsync<T> option, OptionAsync<T> fallback)
        {
            return option
                .ToOption()
                .IfNoneAsync(fallback.ToOption)
                .ToAsync();
        }

        public static OptionAsync<B> BindAsync<A, B>(this Option<A> self, Func<A, Task<Option<B>>> f)
        {
            return self
                .BindAsync(_ => f(_).ToAsync());
        }

        public static OptionAsync<B> BindAsync<A, B>(this OptionAsync<A> self, Func<A, Task<Option<B>>> f)
        {
            return self
                .BindAsync(async _ => f(_).ToAsync());
        }
    }
}