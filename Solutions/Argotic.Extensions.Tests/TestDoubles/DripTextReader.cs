namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// A <see cref="TextReader"/> that yields one character per read, and records whether it was disposed.
/// </summary>
/// <remarks>
///     <para>
///     The reader entry point currently drains its input with a single <c>ReadToEnd</c>, so chunk
///     boundaries do not exist for it. Once it filters as it reads, they do — and this is the double
///     that puts one wherever it can do damage: between the two halves of a surrogate pair, in the
///     middle of a run of dropped characters, and at end of input.
///     </para>
///     <para>
///     <see cref="DisposeCount"/> exists because the ownership rule is part of the contract, not a
///     detail. Nothing should dispose a reader it did not open.
///     </para>
/// </remarks>
/// <param name="content">The characters to serve.</param>
internal sealed class DripTextReader(string content) : TextReader
{
    private int position;

    /// <summary>
    /// Gets the number of times this reader has been disposed.
    /// </summary>
    public int DisposeCount { get; private set; }

    /// <inheritdoc/>
    public override int Peek() => this.position < content.Length ? content[this.position] : -1;

    /// <inheritdoc/>
    public override int Read() => this.position < content.Length ? content[this.position++] : -1;

    /// <inheritdoc/>
    public override int Read(char[] buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if (count == 0 || this.position >= content.Length)
        {
            return 0;
        }

        buffer[index] = content[this.position];
        this.position++;
        return 1;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.DisposeCount++;
        }

        base.Dispose(disposing);
    }
}