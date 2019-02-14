using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace LanguageExt
{
    public static class ResultExtensions
    {
        [Pure]
        public static Task<Result<B>> MapResult<A, B>(this Task<Result<A>> resultTask, Func<A, B> f)
        {
            return resultTask.Map(result => result.Map(f));
        }

        [Pure]
        public static Task<Result<B>> BindResult<A, B>(this Task<Result<A>> resultTask, Func<A, Result<B>> f)
        {
            return resultTask.Map(result => result.Bind(f));
        }

        [Pure]
        public static Task<Result<B>> MapResultAsync<A, B>(this Task<Result<A>> resultTask, Func<A, Task<B>> f)
        {
            return resultTask.Bind(result => result.MapAsync(f));
        }

        [Pure]
        public static Task<Result<B>> BindResultAsync<A, B>(this Task<Result<A>> resultTask, Func<A, Task<Result<B>>> f)
        {
            return resultTask.Bind(result => result.BindAsync(f));
        }

        [Pure]
        public static Result<B> Bind<A, B>(this Result<A> result, Func<A, Result<B>> f)
        {
            return result.Match(
                f,
                ex => new Result<B>(ex));
        }

        [Pure]
        public static Task<Result<B>> BindAsync<A, B>(this Result<A> result, Func<A, Task<Result<B>>> f)
        {
            return result.Match(
                f,
                async ex => new Result<B>(ex));
        }

        [Pure]
        public static Option<A> TryGetValue<A>(this Result<A> result)
        {
            return result.Match(
                _ => _,
                ex => Option<A>.None);
        }
    }
}