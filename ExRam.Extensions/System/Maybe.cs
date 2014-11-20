namespace System
{
    public static class Maybe
    {
        public static Maybe<T> Create<T>(T value)
        {
            return new Maybe<T>(value);
        }

        public static T? ToNullable<T>(this Maybe<T> maybe) where T : struct
        {
            return ((maybe.HasValue) ? (maybe.Value) : (new T?()));
        }
    }
}