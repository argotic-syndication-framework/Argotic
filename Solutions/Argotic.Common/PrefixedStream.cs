namespace Argotic.Common;

/// <summary>
/// Presents an already-read prefix followed by the remainder of a stream, as one forward-only stream.
/// </summary>
/// <remarks>
/// <para>
/// Detecting a document's encoding means reading the beginning of it, and a network stream cannot be
/// rewound afterwards. Buffering the whole document to get around that is what this type exists to
/// stop: the head is read, sniffed, and then handed back to the decoder in front of the untouched
/// remainder.
/// </para>
/// <para>
/// <b>It does not own the inner stream</b> and never disposes it. Nothing should close what it did not
/// open, and the caller of <c>CreateSafeNavigator</c> still owns what they passed in.
/// </para>
/// <para>
/// <b>There is deliberately no seekable fast path.</b> A seekable stream could be rewound instead of
/// prefixed, which would avoid this type entirely for a <see cref="MemoryStream"/> or a buffered
/// response body — and that is most traffic today. It is still not worth having: two code paths means
/// two behaviours, and the one taken only by real sockets would be the one no test exercises. That is
/// the shape <c>docs/build-warnings.md</c> §4.4 records shipping three regressions.
/// </para>
/// </remarks>
/// <param name="prefix">The buffer holding the bytes already read from <paramref name="inner"/>.</param>
/// <param name="prefixLength">How many bytes of <paramref name="prefix"/> are real.</param>
/// <param name="inner">The stream those bytes came from, positioned after them.</param>
internal sealed class PrefixedStream(byte[] prefix, int prefixLength, Stream inner) : Stream
{
    private int prefixPosition;

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
    /// <remarks>
    ///     Never mixes prefix bytes and inner bytes in one call. A caller that asks for more than the
    ///     prefix has left gets the rest of the prefix and comes back for the remainder, which is a
    ///     short read — and a short read is something any correct stream consumer already handles,
    ///     because every network stream produces them.
    /// </remarks>
    public override int Read(Span<byte> buffer)
    {
        if (buffer.IsEmpty)
        {
            return 0;
        }

        int remainingInPrefix = prefixLength - this.prefixPosition;
        if (remainingInPrefix > 0)
        {
            int take = Math.Min(remainingInPrefix, buffer.Length);
            prefix.AsSpan(this.prefixPosition, take).CopyTo(buffer);
            this.prefixPosition += take;
            return take;
        }

        return inner.Read(buffer);
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}