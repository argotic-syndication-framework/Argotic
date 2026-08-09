namespace Argotic.Common;

/// <summary>
/// Options for individual HTTP requests (headers only).
/// </summary>
/// <remarks>
/// <para>
/// This record provides a simple way to customize request-level settings like HTTP headers.
/// For handler-level settings (credentials, proxy, cookies), configure the <see cref="HttpClient"/>
/// directly or use <c>IHttpClientFactory</c> in dependency injection scenarios.
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
    /// <value>An <c>Accept</c> header value, such as <c>application/rss+xml, application/xml;q=0.9</c>, or <see langword="null"/> to send none. The default value is <see langword="null"/>.</value>
    public string? Accept { get; init; }

    /// <summary>
    /// Gets the value of the User-Agent HTTP header.
    /// </summary>
    /// <value>The user agent to send, or <see langword="null"/> to keep the framework's own, <see cref="SyndicationDiscoveryUtility.FrameworkUserAgent"/>. Setting this replaces it rather than appending to it. The default value is <see langword="null"/>.</value>
    public string? UserAgent { get; init; }

    /// <summary>
    /// Gets the value of the Referer HTTP header.
    /// </summary>
    /// <value>An absolute <c>http</c> or <c>https</c> URI, or <see langword="null"/> or an <i>empty</i> string to send no <c>Referer</c>. Anything else throws. The default value is <see langword="null"/>.</value>
    public string? Referer { get; init; }

    /// <summary>
    /// Gets custom headers to include in the request.
    /// </summary>
    /// <value>Request header name/value pairs, or <see langword="null"/> for none. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     An entry naming a header the request already carries is skipped rather than overwriting it,
    ///     so this cannot displace the <c>User-Agent</c> or the conditional-GET validators. An entry
    ///     naming a <i>content</i> header — <c>Content-Type</c> and its kin — throws: those describe a
    ///     request body, and a syndication fetch has none.
    /// </remarks>
    public IReadOnlyDictionary<string, string>? CustomHeaders { get; init; }

    /// <summary>
    /// Applies the options to an <see cref="HttpRequestMessage"/>.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequestMessage"/> to configure.</param>
    /// <remarks>
    ///     <para>
    ///     A value this method cannot send is an error, not a no-op. Every setter here used to
    ///     be a <c>Try</c> that discarded its result, so an <see cref="Accept"/> of <c>"@@@"</c> or a
    ///     <see cref="Referer"/> of <c>"example.com/x"</c> produced a request with the header simply
    ///     absent — and the caller learned about it, if at all, as a <c>406</c> from a server they had
    ///     no reason to suspect.
    ///     </para>
    ///     <para>
    ///     A relative <see cref="Referer"/> was worse than dropped. <c>Uri.TryCreate(…, Absolute, …)</c>
    ///     accepts <c>"/relative/path"</c> on this platform and yields <c>file:///relative/path</c>, so
    ///     the header was sent — disclosing a local-looking path to a remote origin. Only <c>http</c>
    ///     and <c>https</c> are accepted now.
    ///     </para>
    ///     <para>
    ///     An empty <see cref="Referer"/> still means "do not send one", which is long-standing and
    ///     pinned.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">
    ///     A header value cannot be sent: <see cref="Accept"/> or <see cref="UserAgent"/> is not a
    ///     valid header value, <see cref="Referer"/> is not an absolute <c>http</c> or <c>https</c>
    ///     URI, or a <see cref="CustomHeaders"/> entry names a content header.
    /// </exception>
    public void ApplyTo(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (Accept is not null)
        {
            request.Headers.Accept.ParseAdd(Accept);
        }

        if (UserAgent is not null)
        {
            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(UserAgent);
        }

        if (!string.IsNullOrEmpty(Referer))
        {
            if (!Uri.TryCreate(Referer, UriKind.Absolute, out Uri? referrerUri)
                || (referrerUri.Scheme != Uri.UriSchemeHttp && referrerUri.Scheme != Uri.UriSchemeHttps))
            {
                throw new FormatException(
                    $"The referer '{Referer}' is not an absolute http or https URI, so it cannot be sent as a Referer header.");
            }

            request.Headers.Referrer = referrerUri;
        }

        if (CustomHeaders is not null)
        {
            foreach (var (key, value) in CustomHeaders)
            {
                // NonValidated rather than Contains: the latter throws InvalidOperationException on a
                // content header name, so the guard written to avoid clobbering a header was itself
                // the thing that failed - with a BCL message about HttpContent that named nothing the
                // caller had written.
                if (request.Headers.NonValidated.Contains(key))
                {
                    continue;
                }

                if (!request.Headers.TryAddWithoutValidation(key, value))
                {
                    throw new FormatException(
                        $"The custom header '{key}' cannot be set on a request message. Content headers such as "
                        + "Content-Type describe a request body, which a syndication fetch does not have.");
                }
            }
        }
    }
}