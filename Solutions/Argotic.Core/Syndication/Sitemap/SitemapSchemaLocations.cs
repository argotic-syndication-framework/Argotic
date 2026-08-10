using System.Xml;

using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Writes the opt-in <c>xsi:schemaLocation</c> attribute for the two sitemap roots.
/// </summary>
/// <remarks>
///     <para>
///     The core pair is root-dependent, not namespace-dependent: <c>urlset</c> and
///     <c>sitemapindex</c> share one namespace but validate against different schemas. The caller
///     passes its root's schema, <see cref="SitemapXsd"/> or <see cref="SiteindexXsd"/>.
///     </para>
///     <para>
///     A pair is written only for a namespace with a known, published schema. The hreflang
///     extension's <c>xhtml</c> namespace has none, and a consumer-supplied extension's schema is
///     unknowable, so both are skipped. The attribute is a hint; a partial list is valid.
///     </para>
/// </remarks>
internal static class SitemapSchemaLocations
{
    /// <summary>The schema location for a <c>urlset</c> root.</summary>
    public const string SitemapXsd = "http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd";

    /// <summary>The schema location for a <c>sitemapindex</c> root.</summary>
    public const string SiteindexXsd = "http://www.sitemaps.org/schemas/sitemap/0.9/siteindex.xsd";

    private const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

    /// <summary>
    /// The extension namespaces with a published schema, keyed by namespace URI.
    /// </summary>
    /// <remarks>
    ///     The keys match the constructor literals in <c>SitemapNewsExtension</c>,
    ///     <c>SitemapImageExtension</c> and <c>SitemapVideoExtension</c>. Both columns keep the
    ///     <c>http</c> scheme: the namespaces are identifiers, and the schema locations are the
    ///     spellings the protocol's own examples use. The integration tier fetches the same three
    ///     files over <c>https</c>; see <c>LiveSchemaSource</c> in the test project.
    /// </remarks>
    private static readonly Dictionary<string, string> ExtensionSchemas = new(StringComparer.Ordinal)
    {
        ["http://www.google.com/schemas/sitemap-news/0.9"] = "http://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd",
        ["http://www.google.com/schemas/sitemap-image/1.1"] = "http://www.google.com/schemas/sitemap-image/1.1/sitemap-image.xsd",
        ["http://www.google.com/schemas/sitemap-video/1.1"] = "http://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd",
    };

    /// <summary>
    /// Writes the <c>xmlns:xsi</c> declaration and the <c>xsi:schemaLocation</c> attribute on the root element.
    /// </summary>
    /// <param name="writer">The writer whose root start tag is still open.</param>
    /// <param name="rootSchemaLocation">The root's own schema: <see cref="SitemapXsd"/> or <see cref="SiteindexXsd"/>.</param>
    /// <param name="supportedExtensions">The extension types to pair, after auto-detection has merged into them.</param>
    /// <remarks>
    ///     The pair order is deterministic: the core pair first, then extensions in list order,
    ///     de-duplicated by namespace URI. The single <c>WriteAttributeString</c> call makes the
    ///     writer emit the <c>xmlns:xsi</c> declaration itself.
    /// </remarks>
    public static void WriteXsiSchemaLocation(XmlWriter writer, string rootSchemaLocation, IList<Type> supportedExtensions)
    {
        List<string> tokens = [SitemapUtility.SitemapNamespace, rootSchemaLocation];
        HashSet<string> seen = new(StringComparer.Ordinal) { SitemapUtility.SitemapNamespace };

        foreach (ISyndicationExtension extension in SyndicationExtensionAdapter.GetExtensions(supportedExtensions))
        {
            if (ExtensionSchemas.TryGetValue(extension.XmlNamespace, out string? schemaLocation)
                && seen.Add(extension.XmlNamespace))
            {
                tokens.Add(extension.XmlNamespace);
                tokens.Add(schemaLocation);
            }
        }

        writer.WriteAttributeString("xsi", "schemaLocation", XsiNamespace, string.Join(' ', tokens));
    }
}