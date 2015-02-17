using System.Diagnostics.Contracts;

namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        public static DateTime GetCompilationDate(this Assembly assembly)
        {
            Contract.Requires(assembly != null);
            Contract.Requires(assembly.GetName().Version != null);

            var version = assembly.GetName().Version;
            return new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(version.Build).AddSeconds(version.Revision * 2);
        }
    }
}