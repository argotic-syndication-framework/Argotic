using System.Net;

namespace Argotic.Common;

/// <summary>
/// Represents the result of a conditional GET operation.
/// </summary>
/// <remarks>
///     <para>
///     This class replaces the use of <see cref="HttpWebResponse"/> for conditional GET operations,
///     providing a modern wrapper around <see cref="HttpResponseMessage"/>.
///     </para>
///     <para>
///     <b>The response size limits in <see cref="SyndicationContentLengthLimits"/> do not apply to
///     this path, and that is deliberate.</b> Every other way this library fetches a body reads it
///     into a buffer it owns and refuses one larger than the limit for the resource being loaded.
///     This type does not read the body at all — <c>ConditionalGetAsync</c> completes on headers, so
///     <see cref="GetResponseStream"/> hands out the live response stream and nothing has been
///     downloaded until the caller reads it.
///     </para>
///     <para>
///     It was not always so. Until the modification heuristic was deleted this method completed on
///     content, and the whole body was buffered before this object existed — unbounded and eager
///     both. <see cref="ContentLength"/> reported the <i>buffered</i> length, which looked helpful and
///     was worthless: by the time you could read it you had already paid. It now reports what the
///     origin declared, or <c>-1</c> when it declared nothing.
///     </para>
///     <para>
///     Callers who want a bound apply one as they read, or use
///     <c>SyndicationResourceReader.LoadIfModifiedAsync</c>, which does.
///     </para>
/// </remarks>
public sealed class ConditionalGetResult : IDisposable, IAsyncDisposable
{
    private readonly HttpResponseMessage? response;
    private bool disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalGetResult"/> class.
    /// </summary>
    /// <param name="response">The HTTP response message, or null if the resource was not modified.</param>
    /// <param name="wasModified">Indicates whether the resource was modified since the last request.</param>
    internal ConditionalGetResult(HttpResponseMessage? response, bool wasModified)
    {
        this.response = response;
        WasModified = wasModified;

        if (response is not null)
        {
            StatusCode = response.StatusCode;
            LastModified = response.Content.Headers.LastModified;
            ETag = response.Headers.ETag?.Tag;
            ContentLength = response.Content.Headers.ContentLength ?? -1;
            ContentType = response.Content.Headers.ContentType?.MediaType;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalGetResult"/> class from a 304 response.
    /// </summary>
    /// <param name="notModified">The <c>304 Not Modified</c> response whose validators to keep.</param>
    /// <remarks>
    ///     <para>
    ///     Separate from the two-parameter constructor because a 304 is not "no response" — it is a
    ///     response with no <i>body</i>. It still carries an <c>ETag</c> and a <c>Last-Modified</c>,
    ///     and RFC 9110 requires it to carry the same <c>ETag</c> a 200 would have. Servers rotate one
    ///     on a 304 legitimately, so discarding it meant a polling caller re-sent a stale validator for
    ///     as long as it kept polling.
    ///     </para>
    ///     <para>
    ///     The response itself is not retained. The caller disposes it immediately after; there is no
    ///     body to hand out and <see cref="GetResponseStream"/> must keep returning
    ///     <see cref="Stream.Null"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="notModified"/> is a null reference.</exception>
    internal ConditionalGetResult(HttpResponseMessage notModified)
    {
        ArgumentNullException.ThrowIfNull(notModified);

        WasModified = false;
        StatusCode = notModified.StatusCode;
        LastModified = notModified.Content.Headers.LastModified;
        ETag = notModified.Headers.ETag?.Tag;
        ContentLength = -1;
    }

    /// <summary>
    /// Gets a value indicating whether the resource was modified since the last request.
    /// </summary>
    /// <value><b>true</b> if the resource was modified; otherwise, <b>false</b>.</value>
    public bool WasModified { get; }

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    /// <value>The <see cref="HttpStatusCode"/> of the response, or null if no response was received.</value>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the date and time the resource was last modified.
    /// </summary>
    /// <value>A <see cref="DateTimeOffset"/> representing when the resource was last modified, or null if not available.</value>
    public DateTimeOffset? LastModified { get; }

    /// <summary>
    /// Gets the entity tag of the resource.
    /// </summary>
    /// <value>A string representing the ETag of the resource, or null if not available.</value>
    public string? ETag { get; }

    /// <summary>
    /// Gets the content length of the response.
    /// </summary>
    /// <value>The content length in bytes, or -1 if not available.</value>
    public long ContentLength { get; }

    /// <summary>
    /// Gets the content type of the response.
    /// </summary>
    /// <value>A string representing the media type, or null if not available.</value>
    public string? ContentType { get; }

    /// <summary>
    /// Gets the response stream for reading the content.
    /// </summary>
    /// <returns>A <see cref="Stream"/> for reading the response content, or <see cref="Stream.Null"/> if no response is available.</returns>
    /// <remarks>
    ///     The live response stream — see the remarks on <see cref="ConditionalGetResult"/>. Reading it
    ///     is what downloads the body, and nothing bounds how much of it there is.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
    public Stream GetResponseStream()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return response?.Content.ReadAsStream() ?? Stream.Null;
    }

    /// <summary>
    /// Asynchronously gets the response stream for reading the content.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Stream"/> for reading the response content.</returns>
    /// <remarks>
    ///     The live response stream — see the remarks on <see cref="ConditionalGetResult"/>. Reading it
    ///     is what downloads the body, and nothing bounds how much of it there is.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The object has been disposed.</exception>
    public Task<Stream> GetResponseStreamAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return response?.Content.ReadAsStreamAsync(cancellationToken) ?? Task.FromResult(Stream.Null);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ConditionalGetResult"/>.
    /// </summary>
    public void Dispose()
    {
        if (!disposed)
        {
            response?.Dispose();
            disposed = true;
        }
    }

    /// <summary>
    /// Asynchronously releases all resources used by the <see cref="ConditionalGetResult"/>.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public ValueTask DisposeAsync()
    {
        if (!disposed)
        {
            response?.Dispose();
            disposed = true;
        }

        return ValueTask.CompletedTask;
    }
}