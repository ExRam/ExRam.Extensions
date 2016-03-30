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
    }
}