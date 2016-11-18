using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

// (c) Copyright 2014 ExRam GmbH & Co. KG http://www.exram.de
//
// Licensed using Microsoft Public License (Ms-PL)
// Full License description can be found in the LICENSE
// file.

namespace System.Linq
{
    public static partial class AsyncEnumerableExtensions
    {
        #region ByteInterfaceStream
        private sealed class JoinStream : Stream
        {
            private readonly IAsyncEnumerator<ArraySegment<byte>> _arraySegmentEnumerator;

            private ArraySegment<byte>? _currentInputSegment;

            public JoinStream(IAsyncEnumerator<ArraySegment<byte>> factory)
            {
                Contract.Requires(factory != null);

                this._arraySegmentEnumerator = factory;
            }

            #region Read
            public override int Read(byte[] buffer, int offset, int count)
            {
                return this.ReadAsync(buffer, offset, count, CancellationToken.None).Result;
            }
            #endregion

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                if (count == 0)
                    return 0;

                ArraySegment<byte> currentInputSegment;
                var currentNullableInputSegment = this._currentInputSegment;

                if (currentNullableInputSegment == null)
                {
                    try
                    {
                        if (await this._arraySegmentEnumerator.MoveNext(CancellationToken.None).ConfigureAwait(false))
                            currentInputSegment = this._arraySegmentEnumerator.Current;
                        else
                            return 0;
                    }
                    catch (AggregateException ex)
                    {
                        throw ex.GetBaseException();
                    }
                }
                else
                    currentInputSegment = currentNullableInputSegment.Value;

                var minToRead = Math.Min(currentInputSegment.Count, count);
                Buffer.BlockCopy(currentInputSegment.Array, currentInputSegment.Offset, buffer, offset, minToRead);

                currentInputSegment = new ArraySegment<byte>(currentInputSegment.Array, currentInputSegment.Offset + minToRead, currentInputSegment.Count - minToRead);
                this._currentInputSegment = ((currentInputSegment.Count > 0) ? ((ArraySegment<byte>?)currentInputSegment) : (null));

                return minToRead;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    this._arraySegmentEnumerator.Dispose();

                base.Dispose(disposing);
            }

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => false;

            public override long Length
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            public override long Position
            {
                get
                {
                    throw new NotSupportedException();
                }

                set
                {
                    throw new NotSupportedException();
                }
            }
        }
        #endregion

        public static Stream ToStream(this IAsyncEnumerable<ArraySegment<byte>> byteSegmentAsyncEnumerable)
        {
            Contract.Requires(byteSegmentAsyncEnumerable != null);

            return new JoinStream(byteSegmentAsyncEnumerable.GetEnumerator());
        }
    }
}
