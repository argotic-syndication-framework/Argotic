namespace Argotic.Common;

/// <summary>
/// Options for individual HTTP requests (headers only).
/// </summary>
/// <remarks>
/// <para>
/// This record provides a simple way to customize request-level settings like HTTP headers.
/// For handler-level settings (credentials, proxy, cookies), configure the <see cref="HttpClient"/>
/// directly or use <see cref="IHttpClientFactory"/> in dependency injection scenarios.
/// </para>
/// <para>
/// Example usage:
/// <code>
/// var options = new SyndicationRequestOptions
/// {
///     Accept = "application/xml",
///     UserAgent = "MyApp/1.0"
/// };
/// await feed.LoadAsync(uri, httpClient, requestOptions: options);
/// </code>
/// </para>
/// </remarks>
public sealed record SyndicationRequestOptions
{
    /// <summary>
    /// Gets the value of the Accept HTTP header.
    /// </summary>
    /// <value>The MIME type(s) to accept, or <c>null</c> to use the default.</value>
    public string? Accept { get; init; }

    /// <summary>
    /// Gets the value of the User-Agent HTTP header.
    /// </summary>
    /// <value>The user agent string, or <c>null</c> to use the default.</value>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Gets the value of the Referer HTTP header.
    /// </summary>
    /// <value>The referer URL, or <c>null</c> to not include a referer.</value>
    public string? Referer { get; init; }

    /// <summary>
    /// Gets custom headers to include in the request.
    /// </summary>
    /// <value>A read-only dictionary of header name/value pairs, or <c>null</c> for no custom headers.</value>
    public IReadOnlyDictionary<string, string>? CustomHeaders { get; init; }

    /// <summary>
    /// Applies the options to an <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to configure.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> is a null reference.</exception>
    public void ApplyTo(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (Accept != null)
        {
            request.Headers.Accept.TryParseAdd(Accept);
        }

        if (UserAgent != null)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.TryParseAdd(UserAgent);
        }

        if (Referer != null && Uri.TryCreate(Referer, UriKind.Absolute, out var referrerUri))
        {
            request.Headers.Referrer = referrerUri;
        }

        if (CustomHeaders != null)
        {
            foreach (var (key, value) in CustomHeaders)
            {
                if (!request.Headers.Contains(key))
                {
                    request.Headers.TryAddWithoutValidation(key, value);
                }
            }
        }
    }
}