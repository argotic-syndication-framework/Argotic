using System.Net;

namespace Argotic.Common;

/// <summary>
/// Represents the result of a conditional GET operation.
/// </summary>
/// <remarks>
/// This class replaces the use of <see cref="HttpWebResponse"/> for conditional GET operations,
/// providing a modern wrapper around <see cref="HttpResponseMessage"/>.
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

        if (response != null)
        {
            StatusCode = response.StatusCode;
            LastModified = response.Content.Headers.LastModified;
            ETag = response.Headers.ETag?.Tag;
            ContentLength = response.Content.Headers.ContentLength ?? -1;
            ContentType = response.Content.Headers.ContentType?.MediaType;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the resource was modified since the last request.
    /// </summary>
    /// <value><b>true</b> if the resource was modified; otherwise, <b>false</b>.</value>
    public bool WasModified { get; }

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    /// <value>The <see cref="HttpStatusCode"/> of the response, or the default value if no response was received.</value>
    public HttpStatusCode StatusCode { get; }

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