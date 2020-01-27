using System.Threading.Tasks;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static async Task<byte[]> ReadAsync(this Stream stream, int count)
        {
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
        
        public static Task WriteAsync(this Stream stream, byte[] buffer)
        {
            return stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}