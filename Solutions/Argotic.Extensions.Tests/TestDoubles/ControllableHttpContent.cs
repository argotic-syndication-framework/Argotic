using System.Net;
using System.Net.Http.Headers;

namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// An <see cref="HttpContent"/> that records whether and how much of it was read.
/// </summary>
/// <remarks>
///     <para>
///     <b>A handler cannot see <see cref="HttpCompletionOption"/>, but the content can.</b>
///     <see cref="HttpClient"/> calls <c>LoadIntoBufferAsync</c> after the handler returns under
///     <c>ResponseContentRead</c> and does not under <c>ResponseHeadersRead</c>, so
///     <see cref="WasRead"/> is set exactly once in the first case and never in the second. That makes
///     it a deterministic observable, which matters: the alternative is asserting on elapsed time, and
///     a timing-based test for this would be flaky in exactly the direction that makes people delete it.
///     </para>
///     <para>
///     <see cref="BytesRead"/> is the second half. "Did it read the body" and "how much of the body did
///     it read before giving up" are different questions, and a size cap that drains the whole response
///     and then checks the total has already spent the memory it exists to save.
///     </para>
/// </remarks>
/// <param name="body">The bytes to serve.</param>
/// <param name="declareLength">
///     <see langword="true"/> to report <c>Content-Length</c>. <see langword="false"/> models a chunked
///     response — and, more importantly, a decompressed one: <c>AutomaticDecompression</c> strips
///     <c>Content-Length</c> from every response it decompresses, so an absent length is the normal
///     case on a compressing origin rather than an exotic one.
/// </param>
internal sealed class ControllableHttpContent(byte[] body, bool declareLength = true) : HttpContent
{
    /// <summary>
    /// Gets a value indicating whether anything has read this content.
    /// </summary>
    public bool WasRead { get; private set; }

    /// <summary>
    /// Gets the number of bytes handed over so far.
    /// </summary>
    public long BytesRead { get; private set; }

    /// <summary>
    /// Gets the number of bytes this content will serve in total.
    /// </summary>
    public int Length => body.Length;

    /// <summary>
    /// Builds a response carrying this content.
    /// </summary>
    /// <param name="contentType">The media type to declare.</param>
    /// <returns>A 200 response. The caller owns it.</returns>
    public HttpResponseMessage InAResponse(string contentType = "application/xml")
    {
        HttpResponseMessage response = new(HttpStatusCode.OK) { Content = this };
        response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        return response;
    }

    /// <inheritdoc/>
    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => this.SerializeToStreamAsync(stream, context, CancellationToken.None);

    /// <inheritdoc/>
    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(stream);

        this.WasRead = true;

        // Written in chunks rather than one WriteAsync so that BytesRead is meaningful when a reader
        // abandons the body part-way through, which is the whole point of a streaming size cap.
        const int ChunkSize = 4_096;
        for (int offset = 0; offset < body.Length; offset += ChunkSize)
        {
            int count = Math.Min(ChunkSize, body.Length - offset);
            await stream.WriteAsync(body.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);
            this.BytesRead += count;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    ///     Overridden so that reading can be <i>abandoned</i>. The base implementation serializes the
    ///     whole body into a buffer and hands back a stream over it, so <see cref="BytesRead"/> would
    ///     reach the full length no matter what the reader did — and a test for "stopped part-way
    ///     through" would pass or fail for reasons having nothing to do with the code under test.
    ///     This yields bytes on demand instead, and counts what is actually taken.
    /// </remarks>
    protected override Task<Stream> CreateContentReadStreamAsync()
        => Task.FromResult<Stream>(new CountingStream(this, body));

    /// <inheritdoc/>
    protected override bool TryComputeLength(out long length)
    {
        length = declareLength ? body.Length : 0;
        return declareLength;
    }

    /// <summary>
    /// Serves the body a chunk at a time, recording how much was taken before the reader stopped.
    /// </summary>
    private sealed class CountingStream(ControllableHttpContent owner, byte[] content) : Stream
    {
        private int position;

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            return this.Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            int available = content.Length - this.position;
            int take = Math.Min(buffer.Length, available);
            if (take <= 0)
            {
                return 0;
            }

            content.AsSpan(this.position, take).CopyTo(buffer);
            this.position += take;
            owner.WasRead = true;
            owner.BytesRead += take;
            return take;
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}