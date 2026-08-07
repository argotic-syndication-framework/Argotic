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
///     The response size limits in <see cref="SyndicationContentLengthLimits"/> do not apply to this
///     path, and that is deliberate. Every other way this library fetches a body reads it
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
    /// <param name="response">The HTTP response message, or <see langword="null"/> if the resource was not modified.</param>
    /// <param name="wasModified">Indicates whether the resource was modified since the last request.</param>
    internal ConditionalGetResult(HttpResponseMessage? response, bool wasModified)
    {
        this.response = response;
        WasModified = wasModified;

        if (response is not null)
        {
            StatusCode = response.StatusCode;
            LastModified = response.Content.Headers.LastModified;
            ETag = response.Headers.ETag?.ToString();
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
    /// <exception cref="ArgumentNullException">The <paramref name="notModified"/> is <see langword="null"/>.</exception>
    internal ConditionalGetResult(HttpResponseMessage notModified)
    {
        ArgumentNullException.ThrowIfNull(notModified);

        WasModified = false;
        StatusCode = notModified.StatusCode;
        LastModified = notModified.Content.Headers.LastModified;
        ETag = notModified.Headers.ETag?.ToString();
        ContentLength = -1;
    }

    /// <summary>
    /// Gets a value indicating whether the resource was modified since the last request.
    /// </summary>
    /// <value><see langword="true"/> when the origin returned a body; otherwise, <see langword="false"/>, which is what a <c>304</c> produces.</value>
    public bool WasModified { get; }

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    /// <value>The <see cref="HttpStatusCode"/> of the response, or <see langword="null"/> if no response was received.</value>
    public HttpStatusCode? StatusCode { get; }

    /// <summary>
    /// Gets the date and time the resource was last modified.
    /// </summary>
    /// <value>The origin's <c>Last-Modified</c>, or <see langword="null"/> if it sent none. Present on a <c>304</c> as well as on a <c>200</c>.</value>
    public DateTimeOffset? LastModified { get; }

    /// <summary>
    /// Gets the entity tag of the resource.
    /// </summary>
    /// <value>
    ///     The entity tag exactly as the origin sent it — quotes included, and prefixed <c>W/</c> when
    ///     the origin marked it weak — or <see langword="null"/> if it sent none. Store it whole: a tag
    ///     stripped of its quotes is not a well-formed entity tag, and
    ///     <see cref="SyndicationValidators.ApplyTo(HttpRequestMessage)"/> refuses it.
    /// </value>
    /// <remarks>
    ///     This is <see cref="System.Net.Http.Headers.EntityTagHeaderValue.ToString"/> rather than its
    ///     <see cref="System.Net.Http.Headers.EntityTagHeaderValue.Tag"/>, which is only the opaque
    ///     quoted string: weakness lives on the separate
    ///     <see cref="System.Net.Http.Headers.EntityTagHeaderValue.IsWeak"/>, so reading <c>Tag</c>
    ///     round-tripped <c>W/"v1"</c> as a strong <c>"v1"</c>. <c>If-None-Match</c> uses the weak
    ///     comparison function, under which those two match, so revalidation still succeeded; what was
    ///     lost was the honesty of this property and the correctness of an <c>If-Match</c> or
    ///     <c>If-Range</c> built from it, both of which compare strictly.
    /// </remarks>
    public string? ETag { get; }

    /// <summary>
    /// Gets the content length of the response.
    /// </summary>
    /// <value>
    ///     The length the origin declared, in bytes, or <c>-1</c> when it declared none — which is the
    ///     case for every decompressed and every chunked response. It is not the number of bytes read:
    ///     nothing has been read when this object is handed back.
    /// </value>
    public long ContentLength { get; }

    /// <summary>
    /// Gets the content type of the response.
    /// </summary>
    /// <value>The media type alone, without any <c>charset</c> parameter, or <see langword="null"/> if the origin sent no <c>Content-Type</c>.</value>
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