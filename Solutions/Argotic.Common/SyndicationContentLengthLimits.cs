namespace Argotic.Common;

/// <summary>
/// The default limits on how much of an HTTP response each kind of resource will accept.
/// </summary>
/// <remarks>
///     <para>
///     Published as named constants rather than buried at the call sites so they are discoverable, and
///     so that a caller raising one knows what they are raising it from.
///     </para>
///     <para>
///     <b>These are keyed on the loading type, not on whether settings were supplied.</b> Keying the
///     sitemap allowance on <c>settings is null</c> would mean a caller who constructed a
///     <see cref="SyndicationResourceLoadSettings"/> for an unrelated reason — to set a retrieval limit,
///     say — silently lost 56 MiB of headroom. The type being loaded is what determines how large a
///     legitimate document can be, so the type is what supplies the default.
///     </para>
/// </remarks>
public static class SyndicationContentLengthLimits
{
    /// <summary>
    /// The default limit for a syndication feed: 8 MiB.
    /// </summary>
    /// <remarks>
    ///     Comfortably above any feed this library has been measured against. A thousand-item RSS
    ///     document with extension namespaces is under a megabyte.
    /// </remarks>
    public const long Feed = 8L * 1024 * 1024;

    /// <summary>
    /// The default limit for a sitemap or sitemap index: 64 MiB.
    /// </summary>
    /// <remarks>
    ///     The sitemap protocol permits 50,000 URLs in one document and caps it at 50 MB uncompressed.
    ///     A generated 50,000-URL document measures about 8.2 MB, so 64 MiB clears both the protocol
    ///     ceiling and any realistic document under it.
    /// </remarks>
    public const long Sitemap = 64L * 1024 * 1024;

    /// <summary>
    /// The default limit for a whole-site export such as BlogML: 64 MiB.
    /// </summary>
    /// <remarks>
    ///     A BlogML document is an export of an entire blog, not a feed of its recent items, so it is
    ///     sized by the history of the site rather than by a window over it.
    /// </remarks>
    public const long Archive = 64L * 1024 * 1024;

    /// <summary>
    /// The default limit for a discovery fetch: 2 MiB.
    /// </summary>
    /// <remarks>
    ///     Discovery reads an HTML page to find <c>link</c> elements or an <c>X-Pingback</c> header. It
    ///     is never the document the caller wanted, which is why it gets the smallest allowance.
    /// </remarks>
    public const long Discovery = 2L * 1024 * 1024;
}