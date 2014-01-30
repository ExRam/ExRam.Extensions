namespace System
{
    public static class MaybeExtensions
    {
        public static T? ToNullable<T>(this Maybe<T> maybe) where T : struct
        {
            return ((maybe.HasValue) ? (maybe.Value) : (new T?()));
        }
    }
}