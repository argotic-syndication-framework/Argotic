namespace Argotic.Extensions.Tests.TestDoubles;

/// <summary>
/// Provides access to the real sample documents linked in from the Examples project.
/// </summary>
/// <remarks>
///     <para>
///     These are the counterpart to <see cref="FeedTestData"/>, and the difference matters. The inline
///     literals in that class average ten XML elements per document and half of them carry five or
///     fewer; these files average fifty-six. More to the point, the inline corpus declares no
///     <c>itunes:</c>, <c>media:</c>, <c>sx:</c>, <c>content:</c>, <c>dcterms:</c>, <c>slash:</c>,
///     <c>sy:</c>, <c>wfw:</c>, <c>geo:</c> or <c>lj:</c> namespace at all, while
///     <see cref="RssFeedWithExtensions"/> alone declares twenty and carries seventy-three prefixed
///     elements across five nested extension structures.
///     </para>
///     <para>
///     Until these were linked in, the test project had no <c>Content</c> or <c>None</c> items
///     whatsoever, so no test could read a file fixture even if it wanted to. Every workflow that reads
///     extension data off a realistic feed was exercised only by the Examples project - and
///     <c>run-all --skip-network</c> filters those out, so a green example run said nothing about them.
///     </para>
/// </remarks>
internal static class SampleFeeds
{
    /// <summary>An RSS 2.0 feed with no extension namespaces.</summary>
    public const string RssFeed = "RssFeed.xml";

    /// <summary>An RSS 2.0 feed declaring twenty extension namespaces.</summary>
    public const string RssFeedWithExtensions = "RssFeedWithExtensions.xml";

    /// <summary>An Atom 1.0 feed with no extension namespaces.</summary>
    public const string AtomFeed = "AtomFeed.xml";

    /// <summary>An Atom 1.0 feed declaring nine extension namespaces.</summary>
    public const string AtomFeedWithExtensions = "AtomFeedWithExtensions.xml";

    /// <summary>A standalone Atom entry document.</summary>
    public const string AtomEntryDocument = "AtomEntryDocument.xml";

    /// <summary>An OPML 2.0 outline document.</summary>
    public const string OpmlDocument = "OpmlDocument.xml";

    /// <summary>An APML 0.6 attention profile.</summary>
    public const string ApmlDocument = "ApmlDocument.xml";

    /// <summary>A BlogML 2.0 blog export.</summary>
    public const string BlogMLDocument = "BlogMLDocument.xml";

    /// <summary>An RSD 1.0 service discovery document.</summary>
    public const string RsdDocument = "RsdDocument.xml";

    /// <summary>A small feed used for format-agnostic loading.</summary>
    public const string GenericFeed = "GenericFeed.xml";

    /// <summary>A sitemap with no extensions.</summary>
    public const string Sitemap = "sitemap.xml";

    /// <summary>A sitemap index.</summary>
    public const string SitemapIndex = "sitemap_index.xml";

    /// <summary>A sitemap carrying the Google image extension.</summary>
    public const string SitemapImage = "sitemap_image.xml";

    /// <summary>A sitemap carrying the Google news extension.</summary>
    public const string SitemapNews = "sitemap_news.xml";

    /// <summary>A sitemap carrying the Google video extension.</summary>
    public const string SitemapVideo = "sitemap_video.xml";

    /// <summary>
    /// Gets the names of every linked sample document.
    /// </summary>
    public static IReadOnlyList<string> All { get; } =
    [
        ApmlDocument,
        AtomEntryDocument,
        AtomFeed,
        AtomFeedWithExtensions,
        BlogMLDocument,
        GenericFeed,
        OpmlDocument,
        RsdDocument,
        RssFeed,
        RssFeedWithExtensions,
        Sitemap,
        SitemapImage,
        SitemapIndex,
        SitemapNews,
        SitemapVideo,
    ];

    /// <summary>
    /// Resolves the path of a linked sample document.
    /// </summary>
    /// <param name="fileName">The document's file name.</param>
    /// <returns>The absolute path of the document in the test output directory.</returns>
    public static string PathTo(string fileName) => Path.Combine(AppContext.BaseDirectory, "SampleData", fileName);

    /// <summary>
    /// Opens a linked sample document for reading.
    /// </summary>
    /// <param name="fileName">The document's file name.</param>
    /// <returns>A readable stream over the document. The caller owns it.</returns>
    public static FileStream Open(string fileName) => File.OpenRead(PathTo(fileName));

    /// <summary>
    /// Reads a linked sample document as text.
    /// </summary>
    /// <param name="fileName">The document's file name.</param>
    /// <returns>The document's content.</returns>
    public static string ReadAllText(string fileName) => File.ReadAllText(PathTo(fileName));
}