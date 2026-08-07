using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents an Atom syndication feed, including metadata about the feed, and some or all the entries associated with it.
/// </summary>
/// <remarks>
///     <para>
///         Atom is an XML-based Web content and metadata syndication format that describes lists of related information known as <i>feeds</i>.
///         Feeds are composed of a number of items, known as <i>entries</i>, each with an extensible set of attached metadata.
///     </para>
///     <para>
///         This implementation conforms to the Atom 1.0 specification, which can be found
///         at <a href="https://www.rfc-editor.org/rfc/rfc4287.html">https://www.rfc-editor.org/rfc/rfc4287.html</a>.
///     </para>
///     <para>
///         <b>Three members are required and none of them is defaulted.</b> RFC 4287 §4.1.1 requires exactly one <see cref="Id"/>, one <see cref="Title"/>
///         and one <see cref="UpdatedOn"/>, plus at least one <see cref="Authors">author</see> unless every entry supplies its own. Save writes what is
///         set and omits what is not, so a feed built by leaving <see cref="UpdatedOn"/> alone serialises to well-formed XML that is not conformant Atom.
///     </para>
///     <para>
///         Entries sharing an <see cref="AtomEntry.Id"/> are the same entry, and their <see cref="AtomEntry.UpdatedOn"/> timestamps <i>should</i> differ.
///         A processor <i>may</i> display all of them or a subset; showing only the one with the latest <see cref="AtomEntry.UpdatedOn"/> is typical.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomFeed class." />
/// </example>
public class AtomFeed : ISyndicationResource, IAtomCommonObjectAttributes, IExtensibleSyndicationObject
{
    /// <summary>
    /// Private member to hold the collection of entries that comprise the distinct content published in the feed.
    /// </summary>
    private readonly List<AtomEntry> feedEntries = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomFeed"/> class.
    /// </summary>
    public AtomFeed()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomFeed"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
    /// </summary>
    /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this feed.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this feed.</param>
    /// <param name="utcUpdatedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this feed was modified in a way the publisher considers significant.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    public AtomFeed(AtomId id, AtomTextConstruct title, DateTime utcUpdatedOn)
    {
        this.Id = id;
        this.Title = title;
        this.UpdatedOn = utcUpdatedOn;
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="AtomFeed.Load(IXPathNavigable)"/>
    /// <seealso cref="AtomFeed.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="AtomFeed.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e) => this.Loaded?.Invoke(this, e);

    /// <summary>
    /// Gets or sets the base against which relative references inside this element are resolved.
    /// </summary>
    /// <value>The <c>xml:base</c> in effect for this element, or <see langword="null"/> when none is. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §2 gives <c>xml:base</c> the function described in section 5.1.1 of
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986: Uniform Resource Identifier (URI): Generic Syntax</a> — it establishes the base URI,
    ///         or IRI, for every relative reference in the attribute's effective scope. The value itself is a URI reference after processing according to
    ///         <a href="https://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.
    ///     </para>
    ///     <para>
    ///         Loading resolves inheritance: an element without an <c>xml:base</c> of its own reports the nearest ancestor's, so the value here is the
    ///         <i>effective</i> base a consumer can resolve an href against, not the literal attribute.
    ///     </para>
    /// </remarks>
    public Uri? BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>The language declared by <c>xml:lang</c>, or <see langword="null"/> when none is in scope. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 defines <c>atomLanguageTag</c> as a language identifier per
    ///         <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066 (BCP 47; now RFC 5646)</a>, or its successor. A tag this runtime cannot turn
    ///         into a <see cref="CultureInfo"/> is traced and dropped rather than failing the load.
    ///     </para>
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Extensions"/> holds at least one <see cref="ISyndicationExtension"/>; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the authors of this feed.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         RFC 4287 §4.1.1 requires at least one author here <i>unless</i> every child <see cref="AtomEntry"/> carries one of its own — the one
    ///         conformance rule in Atom that cannot be checked by looking at a single element.
    ///     </para>
    ///     <para>
    ///         These authors apply to an entry that supplies none itself and whose <see cref="AtomEntry.Source"/> supplies none either. An entry copied in
    ///         from elsewhere therefore keeps its original authorship through <c>atom:source</c> rather than silently acquiring this feed's.
    ///     </para>
    /// </remarks>
    public IList<AtomPersonConstruct> Authors { get; } = [];

    /// <summary>
    /// Gets the categories associated with this feed.
    /// </summary>
    public IList<AtomCategory> Categories { get; } = [];

    /// <summary>
    /// Gets the entities who contributed to this feed.
    /// </summary>
    public IList<AtomPersonConstruct> Contributors { get; } = [];

    /// <summary>
    /// Gets the distinct content published in this feed.
    /// </summary>
    public IList<AtomEntry> Entries => feedEntries;

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    public SyndicationContentFormat Format => SyndicationContentFormat.Atom;

    /// <summary>
    /// Gets or sets the agent used to generate this feed.
    /// </summary>
    /// <value>The <c>atom:generator</c>, or <see langword="null"/> when the feed names no agent. The default value is <see langword="null"/>.</value>
    public AtomGenerator? Generator { get; set; }

    /// <summary>
    /// Gets or sets an image that provides iconic visual identification for this feed.
    /// </summary>
    /// <value>The <c>atom:icon</c>, or <see langword="null"/> when the feed has none. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     The image <i>should</i> have an aspect ratio of one (horizontal) to one (vertical) and <i>should</i> be suitable for presentation at a small size.
    /// </remarks>
    public AtomIcon? Icon { get; set; }

    /// <summary>
    /// Gets or sets a permanent, universally unique identifier for this feed.
    /// </summary>
    /// <value>The <c>atom:id</c>. The default value is <see langword="null"/>; RFC 4287 §4.1.1 requires exactly one on a conformant document.</value>
    /// <remarks>
    ///     <para>
    ///         This identifier must never change — not when the feed is relocated, migrated, syndicated, republished, exported or imported, and not
    ///         across revisions. Store it alongside the feed. See <see cref="AtomId"/> for the comparison and normalisation rules that follow from that.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public AtomId? Id
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets references from this feed to one or more Web resources.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A feed <i>should</i> contain one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> property of <i>self</i>.
    ///         This is the preferred URI for retrieving Atom Feed Documents representing this Atom feed.
    ///     </para>
    ///     <para>
    ///         A feed must not contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> property of <i>alternate</i>
    ///         that has the same combination of <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> property values.
    ///     </para>
    /// </remarks>
    public IList<AtomLink> Links { get; } = [];

    /// <summary>
    /// Gets or sets an image that provides visual identification for this feed.
    /// </summary>
    /// <value>The <c>atom:logo</c>, or <see langword="null"/> when the feed has none. The default value is <see langword="null"/>.</value>
    /// <remarks>
    ///     The image <i>should</i> have an aspect ratio of 2 (horizontal) to 1 (vertical).
    /// </remarks>
    public AtomLogo? Logo { get; set; }

    /// <summary>
    /// Gets or sets information about rights held in and over this feed.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
    /// </remarks>
    public AtomTextConstruct? Rights { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable description or subtitle for this feed.
    /// </summary>
    public AtomTextConstruct? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this feed.
    /// </summary>
    /// <value>The <c>atom:title</c>. The default value is <see langword="null"/>; RFC 4287 §4.1.1 requires exactly one on a conformant feed.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public AtomTextConstruct? Title
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     The <c>atom:updated</c> timestamp. The default value is <see cref="DateTime.MinValue"/>, which means none was provided — and no
    ///     <c>atom:updated</c> is written when it is left there.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         <b>A conformant feed must carry this.</b> RFC 4287 §4.1.1 requires exactly one <c>atom:updated</c>, and the
    ///         sentinel default means a feed you build without setting it saves without the element — valid XML, invalid Atom, and aggregators that
    ///         order by update time will place it arbitrarily.
    ///     </para>
    ///     <para>
    ///         Supply it in UTC. Loading parses the RFC 3339 timestamp §3.3 requires; the value is significant modification as the <i>publisher</i> judges
    ///         it, so a typo fix need not move it and publishers <i>may</i> change it over time.
    ///     </para>
    /// </remarks>
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>Always <c>1.0</c>. Atom 1.0 is the only version this type reads or writes; Atom 0.3 documents are handled by a separate legacy adapter.</value>
    public Version Version { get; } = new(1, 0);

    /// <summary>
    /// Compares two specified <see cref="IList{AtomCategory}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<AtomCategory> source, IList<AtomCategory> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{AtomLink}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<AtomLink> source, IList<AtomLink> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{AtomPersonConstruct}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<AtomPersonConstruct> source, IList<AtomPersonConstruct> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Compares two specified <see cref="IList{AtomTextConstruct}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <c>1</c>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <c>-1</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is <see langword="null"/>.</exception>
    public static int CompareSequence(IList<AtomTextConstruct> source, IList<AtomTextConstruct> target)
        => ComparisonUtility.CompareSequence(source, target);

    /// <summary>
    /// Creates a new <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <example>
    ///     <code language="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
    ///         var feed = await AtomFeed.CreateAsync(new Uri("https://example.com/feed.xml"));
    ///     </code>
    /// </example>
    public static async Task<AtomFeed> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        AtomFeed syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Creates a new <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="AtomFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         Configure handler-level settings (credentials, proxy, cookies) on the <see cref="HttpClient"/> itself,
    ///         either when creating it manually or via <c>IHttpClientFactory.ConfigurePrimaryHttpMessageHandler</c>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<AtomFeed> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        AtomFeed syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="AtomFeed"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="AtomFeed"/>.
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="AtomFeed"/>.
    /// </remarks>
    public XPathNavigator CreateNavigator()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateDocumentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.Save(writer);
            writer.Flush();
        }

        stream.Seek(0, SeekOrigin.Begin);

        using XmlReader xmlReader = XmlReader.Create(stream, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        XPathDocument document = new(xmlReader);
        return document.CreateNavigator();
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(IXPathNavigable source) => this.Load(source, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(IXPathNavigable source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);

        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator));
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(Stream stream, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(stream);

        this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream, settings), settings);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(XmlReader reader) => this.Load(reader, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(XmlReader reader, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using XmlReader safeReader = XmlReader.Create(reader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        this.Load(new XPathDocument(safeReader), settings);
    }

    /// <summary>
    /// Loads this <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="AtomFeed"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/> and the shared <see cref="HttpClient"/>.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Loads this <see cref="AtomFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         Configure handler-level settings (credentials, proxy, cookies) on the <see cref="HttpClient"/> itself,
    ///         either when creating it manually or via <c>IHttpClientFactory.ConfigurePrimaryHttpMessageHandler</c>.
    ///     </para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            source, httpClient, settings, SyndicationContentLengthLimits.Feed, requestOptions, cancellationToken).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Atom);

        this.OnFeedLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(Stream stream) => this.Save(stream, null);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     <b>Save writes the object graph as it stands; it does not enforce RFC 4287's document-level
    ///     requirements.</b> A conformant feed document must carry exactly one <c>atom:id</c>,
    ///     <c>atom:title</c> and <c>atom:updated</c> (§4.1.1); members that are unset are simply
    ///     omitted, so a partially built feed produces well-formed XML that is not valid Atom. This is
    ///     deliberate — a tolerant reader must be able to round-trip what it read, including documents
    ///     that were invalid on arrival — but it means conformance of the output is the caller's to
    ///     ensure, not this method's to guarantee.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(Stream stream, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(stream);

        settings ??= new SyndicationResourceSaveSettings();

        XmlWriterSettings writerSettings = new()
        {
            OmitXmlDeclaration = false,
            Indent = !settings.MinimizeOutputSize,
            Encoding = settings.CharacterEncoding
        };

        using XmlWriter writer = XmlWriter.Create(stream, writerSettings);
        this.Save(writer, settings);
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        this.Save(writer, new SyndicationResourceSaveSettings());
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomFeed"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);

        writer.WriteStartElement("feed", AtomUtility.AtomNamespace);
        // writer.WriteAttributeString("version", this.Version.ToString());

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

            if (this.Generator is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Generator, settings.SupportedExtensions);
            }
            if (this.Icon is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Icon, settings.SupportedExtensions);
            }
            if (this.Id is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Id, settings.SupportedExtensions);
            }
            if (this.Logo is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Logo, settings.SupportedExtensions);
            }
            if (this.Rights is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Rights, settings.SupportedExtensions);
            }
            if (this.Subtitle is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Subtitle, settings.SupportedExtensions);
            }
            if (this.Title is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Title, settings.SupportedExtensions);
            }

            foreach (AtomPersonConstruct author in this.Authors)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(author, settings.SupportedExtensions);
            }
            foreach (AtomCategory category in this.Categories)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(category, settings.SupportedExtensions);
            }
            foreach (AtomPersonConstruct contributor in this.Contributors)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(contributor, settings.SupportedExtensions);
            }
            foreach (AtomEntry entry in this.Entries)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(entry, settings.SupportedExtensions);
            }
            foreach (AtomLink link in this.Links)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(link, settings.SupportedExtensions);
            }
        }
        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        AtomUtility.WriteCommonObjectAttributes(this, writer);

        this.Id?.WriteTo(writer);
        this.Title?.WriteTo(writer, "title");
        if (this.UpdatedOn != DateTime.MinValue)
        {
            writer.WriteElementString("updated", AtomUtility.AtomNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
        }

        this.WriteFeedOptionals(writer);
        this.WriteFeedCollections(writer);

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        foreach (AtomEntry entry in this.Entries)
        {
            entry.Save(writer, settings);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomFeed"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="AtomFeed.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomFeed.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings? settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Atom);

        this.OnFeedLoaded(eventData);
    }

    /// <summary>
    /// Saves the current <see cref="AtomFeed"/> collection entities to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    private void WriteFeedCollections(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        foreach (AtomLink link in this.Links)
        {
            link.WriteTo(writer);
        }

        foreach (AtomPersonConstruct author in this.Authors)
        {
            author.WriteTo(writer, "author");
        }

        foreach (AtomCategory category in this.Categories)
        {
            category.WriteTo(writer);
        }

        foreach (AtomPersonConstruct contributor in this.Contributors)
        {
            contributor.WriteTo(writer, "contributor");
        }
    }

    /// <summary>
    /// Saves the current <see cref="AtomFeed"/> optional entities to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    private void WriteFeedOptionals(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        this.Generator?.WriteTo(writer);

        this.Icon?.WriteTo(writer);

        this.Logo?.WriteTo(writer);

        this.Rights?.WriteTo(writer, "rights");

        this.Subtitle?.WriteTo(writer, "subtitle");
    }
}