namespace Argotic.Benchmarks;

/// <summary>
/// A non-seekable, forward-only stream that returns at most a fixed number of bytes per read.
/// </summary>
/// <remarks>
/// <para>
/// Every stream benchmark in this harness wraps its bytes in a <see cref="MemoryStream"/>, which is
/// seekable and satisfies any read in full. That is not the shape a feed arrives in.
/// <c>SyndicationEncodingUtility.GetStreamBytes</c> branches on <see cref="Stream.CanSeek"/>, and its
/// non-seekable arm — a synchronous <c>CopyTo</c> — has never executed in this repository's test suite
/// either. So the branch that runs against a real socket is measured by nothing and covered by nothing.
/// </para>
/// <para>
/// A network stream also routinely returns far less than asked for, which is why a bounded head read
/// must use <c>ReadAtLeast</c> rather than a single <c>Read</c>. Setting
/// <see cref="MaxBytesPerRead"/> to 1 forces every possible chunk boundary; a few thousand is the
/// realistic shape.
/// </para>
/// </remarks>
/// <param name="content">The bytes to serve.</param>
/// <param name="maxBytesPerRead">The most this stream will return from a single read.</param>
internal sealed class ShortReadStream(byte[] content, int maxBytesPerRead) : Stream
{
    private int position;

    /// <summary>
    /// Gets the most this stream will return from a single read.
    /// </summary>
    public int MaxBytesPerRead { get; } = maxBytesPerRead;

    /// <inheritdoc/>
    public override bool CanRead => true;

    /// <inheritdoc/>
    public override bool CanSeek => false;

    /// <inheritdoc/>
    public override bool CanWrite => false;

    /// <inheritdoc/>
    public override long Length => throw new NotSupportedException();

    /// <inheritdoc/>
    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override void Flush()
    {
    }

    /// <inheritdoc/>
    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return this.Read(buffer.AsSpan(offset, count));
    }

    /// <inheritdoc/>
    public override int Read(Span<byte> buffer)
    {
        int available = content.Length - this.position;
        int take = Math.Min(Math.Min(buffer.Length, this.MaxBytesPerRead), available);
        if (take <= 0)
        {
            return 0;
        }

        content.AsSpan(this.position, take).CopyTo(buffer);
        this.position += take;
        return take;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}