namespace Argotic.Common;

/// <summary>
/// Specifies the web content syndication format that the syndicated content conforms to.
/// </summary>
/// <seealso cref="EnumerationMetadataAttribute"/>
/// <seealso cref="MimeMediaTypeAttribute"/>
public enum SyndicationContentFormat
{
    /// <summary>
    /// No web content syndication format specified.
    /// </summary>
    [EnumerationMetadata(AlternateValue = "", DisplayName = "")]
    None = 0,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Attention Profiling Markup Language (APML) 1.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "APML 1.0", AlternateValue = "APML")]
    [MimeMediaType(Name = "text", SubName = "x-apml", Documentation = "https://github.com/apml/spec-0.6")]
    Apml = 1,

    /// <summary>
    ///  Indicates that the syndication resource conforms to the Atom 1.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Atom 1.0", AlternateValue = "feed")]
    [MimeMediaType(Name = "application", SubName = "atom+xml", Documentation = "https://www.rfc-editor.org/rfc/rfc4287.html")]
    Atom = 2,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Web Log Markup Language (BlogML) 2.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "BlogML 2.0", AlternateValue = "blog")]
    [MimeMediaType(Name = "application", SubName = "blog+xml", Documentation = "https://web.archive.org/web/20110725082012/http://blogml.org/")]
    BlogML = 3,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Microsummary Generator 0.1 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Microsummary Generator 0.1", AlternateValue = "generator")]
    [MimeMediaType(Name = "application", SubName = "x.microsummary+xml", Documentation = "https://web.archive.org/web/20121018220937/https://developer.mozilla.org/en-US/docs/Microsummary_XML_grammar_reference")]
    MicroSummaryGenerator = 4,

    /// <summary>
    /// Indicates that the syndication resource conforms to the News Markup Language (NewsML) G2 1.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "NewsML-G2 1.0", AlternateValue = "NewsML")]
    [MimeMediaType(Name = "text", SubName = "vnd.IPTC.NewsML", Documentation = "https://iptc.org/standards/newsml-g2/")]
    NewsML = 5,

    /// <summary>
    /// Indicates that the syndication resource conforms to the OpenSearch Description 1.1 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "OpenSearch Description 1.1", AlternateValue = "OpenSearchDescription")]
    [MimeMediaType(Name = "application", SubName = "opensearchdescription+xml", Documentation = "https://github.com/dewitt/opensearch/blob/master/opensearch-1-1-draft-6.md#opensearch-description-document")]
    OpenSearchDescription = 6,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Outline Processor Markup Language (OPML) 2.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "OPML 2.0", AlternateValue = "opml")]
    [MimeMediaType(Name = "text", SubName = "x-opml", Documentation = "https://opml.org/spec2.opml")]
    Opml = 7,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Really Simple Discovery (RSD) 1.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "RSD 1.0", AlternateValue = "rsd")]
    [MimeMediaType(Name = "application", SubName = "rsd+xml", Documentation = "https://cyber.harvard.edu/blogs/gems/tech/rsd.html")]
    Rsd = 8,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Really Simple Syndication (RSS) 2.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "RSS 2.0", AlternateValue = "rss")]
    [MimeMediaType(Name = "application", SubName = "rss+xml", Documentation = "https://www.rssboard.org/rss-specification")]
    Rss = 9,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Resource Description Framework (RDF) 1.0 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "RDF 1.0", AlternateValue = "RDF")]
    [MimeMediaType(Name = "application", SubName = "rdf+xml", Documentation = "https://www.w3.org/TR/2003/WD-rdf-concepts-20030123/#ref-rdf-mime-type")]
    Rdf = 10,

    /// <summary>
    ///  Indicates that the syndication resource conforms to the Atom Publishing Protocol 1.0 Category Document syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Atom Publishing Category 1.0", AlternateValue = "categories")]
    [MimeMediaType(Name = "application", SubName = "atomcat+xml", Documentation = "https://datatracker.ietf.org/doc/html/rfc5023#section-16.1")]
    AtomCategoryDocument = 11,

    /// <summary>
    ///  Indicates that the syndication resource conforms to the Atom Publishing Protocol 1.0 Service Document syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Atom Publishing Service 1.0", AlternateValue = "service")]
    [MimeMediaType(Name = "application", SubName = "atomsvc+xml", Documentation = "https://datatracker.ietf.org/doc/html/rfc5023#section-16.2")]
    AtomServiceDocument = 12,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Sitemap 0.9 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Sitemap 0.9", AlternateValue = "urlset")]
    [MimeMediaType(Name = "application", SubName = "xml", Documentation = "https://www.sitemaps.org/protocol.html")]
    Sitemap = 13,

    /// <summary>
    /// Indicates that the syndication resource conforms to the Sitemap Index 0.9 syndication format.
    /// </summary>
    [EnumerationMetadata(DisplayName = "Sitemap Index 0.9", AlternateValue = "sitemapindex")]
    [MimeMediaType(Name = "application", SubName = "xml", Documentation = "https://www.sitemaps.org/protocol.html")]
    SitemapIndex = 14,

    /// <summary>
    /// Indicates that the syndication resource is a stand-alone Atom 1.0 entry document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     RFC 4287 §2 defines two Atom document types: a feed document, whose document element is
    ///     <c>&lt;feed&gt;</c>, and a stand-alone entry document, whose document element is
    ///     <c>&lt;entry&gt;</c>. They are read by <c>AtomFeed</c> and <c>AtomEntry</c> respectively.
    ///     </para>
    ///     <para>
    ///     Both used to report <see cref="Atom"/>. The detector distinguished them — it tests the
    ///     two roots in separate arms — and then discarded the answer by assigning one value to both. So
    ///     the format check that rejects every other mismatched pairing compared <c>Atom</c> against
    ///     <c>Atom</c> and passed, and a feed handed to an <c>AtomEntry</c> produced a
    ///     default-constructed entry in silence. Callers of
    ///     <c>SyndicationDiscoveryUtility.SyndicationContentFormatGet</c> had the same problem from the
    ///     other side: the value they were given could not tell them which type to construct.
    ///     </para>
    ///     <para>
    ///     The Atom Publishing Protocol's two document types already had their own values —
    ///     <see cref="AtomCategoryDocument"/> and <see cref="AtomServiceDocument"/>. This is the third,
    ///     and the one that was missing.
    ///     </para>
    /// </remarks>
    [EnumerationMetadata(DisplayName = "Atom Entry Document 1.0", AlternateValue = "entry")]
    [MimeMediaType(Name = "application", SubName = "atom+xml", Documentation = "https://datatracker.ietf.org/doc/html/rfc4287#section-2")]
    AtomEntryDocument = 15
}