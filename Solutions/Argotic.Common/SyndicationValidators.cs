namespace Argotic.Common;

/// <summary>
/// The cache validators a caller holds for a resource, and sends back to ask whether it has changed.
/// </summary>
/// <param name="LastModified">
///     The <c>Last-Modified</c> the origin last reported, sent as <c>If-Modified-Since</c>. This value
///     can be <see langword="null"/>.
/// </param>
/// <param name="ETag">
///     The <c>ETag</c> the origin last reported, sent as <c>If-None-Match</c>. This value can be
///     <see langword="null"/>. It must be sent exactly as received, quotes and any <c>W/</c> prefix included.
/// </param>
/// <remarks>
///     <para>
///     One type for a pair that always travels together and is meaningless apart: a validator is only
///     useful when you send back precisely what the origin gave you. Passing them as a
///     <see cref="DateTime"/> and a <see cref="string"/> made two mistakes easy — reordering them at a
///     call site, and inventing a <see cref="DateTime"/> for a resource that never sent one.
///     </para>
///     <para>
///     Deliberately not on <see cref="SyndicationRequestOptions"/>. Every path that consumes
///     request options funnels through a fetch that calls
///     <see cref="System.Net.Http.HttpResponseMessage.EnsureSuccessStatusCode"/>, and <c>304</c> is not
///     a success code. Folding validators into the general options would therefore give every
///     <c>LoadAsync</c> the ability to ask a question whose answer it can only receive as an exception.
///     A 304 is a modelled outcome on the conditional APIs and nowhere else.
///     </para>
/// </remarks>
public sealed record SyndicationValidators(DateTimeOffset? LastModified, string? ETag)
{
    /// <summary>
    /// Gets validators for a resource never yet fetched, which make the request unconditional.
    /// </summary>
    /// <value>A <see cref="SyndicationValidators"/> holding neither value.</value>
    /// <remarks>
    ///     Named rather than left to <c>new(null, null)</c> so that "I have nothing to revalidate
    ///     against" reads as a decision at the call site instead of as two omitted arguments.
    /// </remarks>
    public static SyndicationValidators None { get; } = new(null, null);

    /// <summary>
    /// Applies the validators to a request as <c>If-Modified-Since</c> and <c>If-None-Match</c>.
    /// </summary>
    /// <param name="request">The request to make conditional.</param>
    /// <remarks>
    ///     An <see cref="ETag"/> that is not a well-formed entity tag — most commonly one whose quotes
    ///     were stripped in storage — throws rather than being dropped. Dropping it degraded the
    ///     request to an unconditional GET, which succeeds, returns a body, and looks exactly like a
    ///     resource that changed. The caller would re-download on every poll and never learn why.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="request"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <see cref="ETag"/> is not a valid entity tag.</exception>
    public void ApplyTo(HttpRequestMessage request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (this.LastModified is { } lastModified)
        {
            request.Headers.IfModifiedSince = lastModified;
        }

        if (!string.IsNullOrEmpty(this.ETag))
        {
            request.Headers.IfNoneMatch.ParseAdd(this.ETag);
        }
    }
}