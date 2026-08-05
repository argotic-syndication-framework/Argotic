using System.Buffers;

namespace Argotic.Common;

/// <summary>
/// Holds a drained HTTP response body in a rented buffer, and hands it out as a stream.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why not a <c>byte[]</c>.</b> The point of draining a response before parsing it is to keep a
/// synchronous reader off the socket, not to put the document back in memory — and Phase 3 had just
/// finished removing exactly that copy from the load path. Returning a fresh array would have handed it
/// straight back, and the polling benchmark would have measured no improvement at all while the commit
/// message claimed one.
/// </para>
/// <para>
/// The buffer is rented from <see cref="ArrayPool{T}"/>, so a steady-state poller allocates nothing per
/// response once the pool has warmed. The lifetime is the <c>using</c> in whichever load method drained
/// the response: the navigator is fully built before that scope ends, so nothing outlives the return.
/// </para>
/// </remarks>
internal sealed class PooledContentBuffer : IDisposable
{
    private byte[] buffer;
    private bool returned;

    private PooledContentBuffer(int capacity)
    {
        this.buffer = ArrayPool<byte>.Shared.Rent(Math.Max(capacity, 1));
    }

    /// <summary>
    /// Gets the number of bytes held.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Creates a buffer sized from a declared content length, or a small one when none was declared.
    /// </summary>
    /// <param name="declaredLength">The length the response declared, if any.</param>
    /// <returns>A new buffer. The caller owns it.</returns>
    /// <remarks>
    ///     Sized from the declaration rather than from the limit. A 64 MiB sitemap allowance must not
    ///     cause a 3 KB sitemap to rent 64 MiB.
    /// </remarks>
    public static PooledContentBuffer ForDeclaredLength(long? declaredLength)
        => new(declaredLength is { } declared and > 0 and <= int.MaxValue ? (int)declared : 8 * 1024);

    /// <summary>
    /// Appends bytes to the buffer, growing it if necessary.
    /// </summary>
    /// <param name="source">The bytes to append.</param>
    public void Write(ReadOnlySpan<byte> source)
    {
        ObjectDisposedException.ThrowIf(this.returned, this);

        if (this.Length + source.Length > this.buffer.Length)
        {
            int wanted = Math.Max(this.buffer.Length * 2, this.Length + source.Length);
            byte[] grown = ArrayPool<byte>.Shared.Rent(wanted);
            this.buffer.AsSpan(0, this.Length).CopyTo(grown);
            ArrayPool<byte>.Shared.Return(this.buffer);
            this.buffer = grown;
        }

        source.CopyTo(this.buffer.AsSpan(this.Length));
        this.Length += source.Length;
    }

    /// <summary>
    /// Presents the bytes held as a readable, seekable stream.
    /// </summary>
    /// <returns>A stream over the buffer. It does not own the buffer and need not be disposed.</returns>
    public Stream AsStream() => new MemoryStream(this.buffer, 0, this.Length, writable: false);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (this.returned)
        {
            return;
        }

        // Guarded by a flag rather than trusting the caller: returning the same array twice corrupts
        // the pool for the whole process, and would do so silently and far from here.
        this.returned = true;
        ArrayPool<byte>.Shared.Return(this.buffer);
    }
}