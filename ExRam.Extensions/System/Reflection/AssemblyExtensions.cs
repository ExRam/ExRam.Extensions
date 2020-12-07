namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        public static DateTime GetCompilationDate(this Assembly assembly)
        {
            var maybeVersion = assembly.GetName().Version;
            if (maybeVersion is { } version)
                return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(version.Build).AddSeconds(version.Revision * 2);

            throw new InvalidOperationException($"{assembly} does not have a version.");
        }
    }
}