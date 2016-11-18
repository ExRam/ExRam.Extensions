using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        #region ReadAsync
        public static async Task<byte[]> ReadAsync(this Stream stream, int count)
        {
            Contract.Requires(stream != null);

            var buffer = new byte[count];

            while (count > 0)
            {
                var read = await stream
                    .ReadAsync(buffer, (buffer.Length - count), count)
                    .ConfigureAwait(false);

                if (read == 0)
                    throw new EndOfStreamException();

                count -= read;
            }

            return buffer;
        }
        #endregion

        #region ReadByteAsync
        public static async Task<int> ReadByteAsync(this Stream stream)
        {
            try
            {
                return (await stream.ReadAsync(1).ConfigureAwait(false))[0];
            }
            catch (EndOfStreamException)
            {
                return -1;
            }
        }
        #endregion

        #region WriteAsync
        public static Task WriteAsync(this Stream stream, byte[] buffer)
        {
            Contract.Requires(stream != null);
            Contract.Requires(buffer != null);

            return stream.WriteAsync(buffer, 0, buffer.Length);
        }
        #endregion
    }
}