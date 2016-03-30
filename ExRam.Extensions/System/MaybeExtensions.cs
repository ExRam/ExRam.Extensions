using LanguageExt;

namespace System
{
    public static class MaybeExtensions
    {
        private static class FailFunctionHolder<T>
        {
            public static readonly Func<T> FailFunction = () =>
            {
                throw new ValueIsNoneException();
            };
        }

        public static T Value<T>(this Option<T> self) => self.IfNone(FailFunctionHolder<T>.FailFunction);
    }
}