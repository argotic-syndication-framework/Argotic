namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// A non-seekable stream that returns at most one byte per read.
/// </summary>
/// <remarks>
///     <para>
///     Two properties, both of which the suite could not previously express. It is <b>not seekable</b>,
///     so <c>GetStreamBytes</c> takes its <c>CopyTo</c> arm — the one <c>.endjin/build-warnings.md</c>
///     §2.19 records at 1 of 2 branches, never executed by any test or benchmark in this repository,
///     and the one a live network stream takes. And it <b>satisfies no read in full</b>, which is how a
///     socket actually behaves and why a bounded head read has to use <c>ReadAtLeast</c> rather than a
///     single <c>Read</c>: a naive read would sniff whatever landed in the first TCP segment.
///     </para>
///     <para>
///     One byte at a time is not realism for its own sake — it forces every possible chunk boundary,
///     which is where a streaming decode gets its arithmetic wrong.
///     </para>
/// </remarks>
/// <param name="content">The bytes to serve.</param>
internal sealed class DripStream(byte[] content) : Stream
{
    private int position;

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
        if (buffer.Length == 0 || this.position >= content.Length)
        {
            return 0;
        }

        buffer[0] = content[this.position];
        this.position++;
        return 1;
    }

    /// <inheritdoc/>
    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void SetLength(long value) => throw new NotSupportedException();

    /// <inheritdoc/>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}