using System.Xml;

namespace Argotic.Common;

/// <summary>
/// A <see cref="TextReader"/> that drops characters XML cannot represent, as they are read.
/// </summary>
/// <remarks>
/// <para>
/// The pipeline this replaces materialised the whole document as a string, scanned it, and rebuilt it
/// into a second string before the parser saw a character. This filters in place instead, so a document
/// is never held twice.
/// </para>
/// <para>
/// <b>The load-bearing invariant is that a read never reports zero characters while input remains.</b>
/// <see cref="TextReader.Read(char[], int, int)"/> documents zero as meaning no characters are left, and
/// <c>XmlTextReaderImpl</c> treats it that way. A chunk consisting entirely of dropped characters — a
/// corrupt feed carrying a long run of NULs, which is not hypothetical — must therefore make this reader
/// loop and read again rather than return what it happens to have. Returning zero there would truncate
/// the document silently, with no error anywhere.
/// </para>
/// <para>
/// <b>Two pending slots, not one.</b> There are two different reasons to hold a character back and
/// conflating them corrupts a surrogate pair at a chunk boundary. One is a character that has been
/// <i>classified and accepted</i> and is waiting for room in the caller's buffer — the low half of a
/// valid astral character, whose validity was decided when its high half was examined. The other is a
/// character that has been <i>read but not yet classified</i>, because it was consumed as lookahead
/// while deciding about the character before it. Feeding the first kind back through classification
/// would see a low surrogate with no preceding high, drop it as unpaired, and emit a lone high surrogate
/// to the parser — turning a valid emoji into a parse error. So accepted output is emitted verbatim and
/// never reclassified.
/// </para>
/// <para>
/// The output queue also makes the caller's buffer size irrelevant. A one-character read is not a
/// special case: it emits the head of the queue and leaves the rest for the next call, so there is no
/// arithmetic that only works for large buffers.
/// </para>
/// </remarks>
/// <param name="inner">The reader to filter. This reader does not own it unless told otherwise.</param>
/// <param name="leaveOpen">
///     <see langword="true"/> — the default — to leave <paramref name="inner"/> open when this reader is
///     disposed. Nothing should close what it did not open.
/// </param>
internal sealed class XmlSanitizingTextReader(TextReader inner, bool leaveOpen = true) : TextReader
{
    /// <summary>The next character to emit, already classified and accepted, or -1.</summary>
    private int accepted = -1;

    /// <summary>The character after that — only ever the low half of a pair — or -1.</summary>
    private int acceptedNext = -1;

    /// <summary>A character read as lookahead and not yet classified, or -1.</summary>
    private int lookahead = -1;

    /// <summary>Set once the inner reader reports end of input, so it is never asked again.</summary>
    private bool exhausted;

    /// <inheritdoc/>
    /// <remarks>
    ///     <para>
    ///     <b>Cold, and not therefore removable.</b> The only consumer of this reader is
    ///     <see cref="System.Xml.XmlReader"/>, which reads in blocks, so neither this nor
    ///     <see cref="Read()"/> has ever executed — and a coverage report says so plainly enough to
    ///     invite deleting both.
    ///     </para>
    ///     <para>
    ///     They cannot go. <see cref="TextReader"/> does not implement its scalar members in terms of
    ///     the block overload; its documented default "returns -1". So removing these would not fall
    ///     back to the sanitising path — it would report end of input to any caller reading a
    ///     character at a time, silently, on the first call.
    ///     </para>
    ///     <para>
    ///     Both share <c>Fill</c> and <c>Dequeue</c> with the block overload rather than duplicating
    ///     the classification, which is what keeps a path nothing exercises from drifting away from
    ///     one that everything does.
    ///     </para>
    /// </remarks>
    public override int Peek()
    {
        this.Fill();
        return this.accepted;
    }

    /// <inheritdoc/>
    /// <remarks>See the remarks on <see cref="Peek"/> for why this exists despite never running.</remarks>
    public override int Read()
    {
        this.Fill();
        return this.Dequeue();
    }

    /// <inheritdoc/>
    public override int Read(char[] buffer, int index, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        return this.Read(buffer.AsSpan(index, count));
    }

    /// <inheritdoc/>
    public override int Read(Span<char> buffer)
    {
        int written = 0;

        while (written < buffer.Length)
        {
            this.Fill();

            int next = this.Dequeue();
            if (next < 0)
            {
                break;
            }

            buffer[written++] = (char)next;
        }

        return written;
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !leaveOpen)
        {
            inner.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Ensures the output queue holds at least one character, unless the input is exhausted.
    /// </summary>
    /// <remarks>
    ///     This is where the never-return-zero invariant lives: the loop only ends when a character has
    ///     been accepted or the inner reader is genuinely finished. A run of dropped characters, however
    ///     long, is consumed here rather than reported as end of input.
    /// </remarks>
    private void Fill()
    {
        while (this.accepted < 0 && !this.exhausted)
        {
            int current = this.Next();
            if (current < 0)
            {
                this.exhausted = true;
                return;
            }

            char character = (char)current;

            if (char.IsHighSurrogate(character))
            {
                int partner = this.Next();
                if (partner >= 0 && char.IsLowSurrogate((char)partner))
                {
                    // A valid astral character. Both halves are accepted here, and the low half is
                    // emitted verbatim when its turn comes rather than being classified again.
                    this.accepted = current;
                    this.acceptedNext = partner;
                    return;
                }

                // An unpaired high surrogate: drop it. Its partner has been consumed but not judged, so
                // it goes back to be considered on its own merits - it may itself open a valid pair.
                if (partner >= 0)
                {
                    this.lookahead = partner;
                }
                else
                {
                    this.exhausted = true;
                }

                continue;
            }

            if (XmlConvert.IsXmlChar(character))
            {
                this.accepted = current;
                return;
            }

            // Invalid, and not the start of a pair. Drop it and keep going - this is the loop that
            // stops a chunk of dropped characters from looking like end of input.
        }
    }

    private int Next()
    {
        if (this.lookahead >= 0)
        {
            int held = this.lookahead;
            this.lookahead = -1;
            return held;
        }

        return inner.Read();
    }

    private int Dequeue()
    {
        int head = this.accepted;
        this.accepted = this.acceptedNext;
        this.acceptedNext = -1;
        return head;
    }
}