using System.Diagnostics.Contracts;

namespace System.Text
{
    public static class EncodingExtensions
    {
        public static string GetString(this Encoding encoding, byte[] bytes)
        {
            Contract.Requires(encoding != null);
            Contract.Requires(bytes != null);

            return encoding.GetString(bytes, 0, bytes.Length);
        }
    }
}
