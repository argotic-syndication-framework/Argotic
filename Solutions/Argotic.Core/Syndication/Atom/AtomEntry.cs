using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents exactly one Atom entry, outside the context of an <see cref="AtomFeed">Atom feed</see>.
/// </summary>
/// <seealso cref="AtomFeed"/>
/// <seealso cref="AtomFeed.Entries"/>
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
///         Experience teaches that feeds that contain textual content are in general more useful than those that do not.
///         Some applications (one example is full-text indexers) require a minimum amount of text or (X)HTML to function reliably and predictably.
///         Feed producers should be aware of these issues. It is advisable that each <see cref="AtomEntry"/> object contain a non-empty <see cref="AtomEntry.Title"/>,
///         a non-empty <see cref="AtomEntry.Content"/> when content is defined, and a non-empty <see cref="AtomEntry.Summary"/> when the entry contains does not provide a <see cref="AtomEntry.Content"/>.
///         However, the absence of <see cref="AtomEntry.Summary"/> is not an error, and Atom Processors must not fail to function correctly as a consequence of such an absence.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the AtomEntry class." />
/// </example>
public class AtomEntry : ISyndicationResource, IAtomCommonObjectAttributes, IExtensibleSyndicationObject
{
    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat feedFormat = SyndicationContentFormat.AtomEntryDocument;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version feedVersion = new(1, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntry"/> class.
    /// </summary>
    public AtomEntry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomEntry"/> class using the supplied <see cref="AtomId"/>, <see cref="AtomTextConstruct"/>, and <see cref="DateTime"/>.
    /// </summary>
    /// <param name="id">A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</param>
    /// <param name="title">A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</param>
    /// <param name="utcUpdatedOn">
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant.
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    public AtomEntry(AtomId id, AtomTextConstruct title, DateTime utcUpdatedOn)
    {
        this.Id = id;
        this.Title = title;
        this.UpdatedOn = utcUpdatedOn;
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="AtomEntry.Load(IXPathNavigable)"/>
    /// <seealso cref="AtomEntry.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="AtomEntry.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnEntryLoaded(SyndicationResourceLoadedEventArgs e) => this.Loaded?.Invoke(this, e);

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
    /// Gets the authors of this entry.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///        An entry must contain one or more authors, unless the entry contains an <see cref="AtomEntry.Source"/> object that contains an author or,
    ///        in an Atom Feed Document, the <see cref="AtomFeed"/> contains an author itself.
    ///     </para>
    /// </remarks>
    public IList<AtomPersonConstruct> Authors { get; } = [];

    /// <summary>
    /// Gets the categories associated with this entry.
    /// </summary>
    public IList<AtomCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets information that contains or links to the content of this entry.
    /// </summary>
    public AtomContent? Content { get; set; }

    /// <summary>
    /// Gets the entities who contributed to this entry.
    /// </summary>
    public IList<AtomPersonConstruct> Contributors { get; } = [];

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    public SyndicationContentFormat Format => feedFormat;

    /// <summary>
    /// Gets or sets a permanent, universally unique identifier for this entry.
    /// </summary>
    /// <value>The <c>atom:id</c>. The default value is <see langword="null"/>; RFC 4287 §4.1.2 requires exactly one on a conformant document.</value>
    /// <remarks>
    ///     <para>
    ///         This identifier must never change — not when the entry is relocated, migrated, syndicated, republished, exported or imported, and not
    ///         across revisions. Store it alongside the entry. See <see cref="AtomId"/> for the comparison and normalisation rules that follow from that.
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
    /// Gets references from this entry to one or more Web resources.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An entry must not contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> property of <i>alternate</i>
    ///         that has the same combination of <see cref="AtomLink.ContentType"/> and <see cref="AtomLink.ContentLanguage"/> property values.
    ///     </para>
    /// </remarks>
    public IList<AtomLink> Links { get; } = [];

    /// <summary>
    /// Gets or sets a date-time indicating an instant in time associated with an event early in the life cycle of this entry.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates an instant in time associated with an event early in the life cycle of this entry.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no publication time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime PublishedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets information about rights held in and over this entry.
    /// </summary>
    /// <remarks>
    ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
    ///     If an <see cref="AtomEntry"/> does not provide any rights information, then the <see cref="AtomFeed.Rights"/> of the containing feed, if present, is considered to apply to the entry.
    /// </remarks>
    public AtomTextConstruct? Rights { get; set; }

    /// <summary>
    /// Gets or sets the meta-data of the source feed that this entry was copied from.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomSource"/> is designed to allow the aggregation of entries from different feeds while retaining information about an entry's source feed.
    ///         For this reason, Atom Processors that are performing such aggregation <i>should</i> include at least the required feed-level meta-data elements
    ///         (<see cref="AtomFeed.Id">id</see>, <see cref="AtomFeed.Title">title</see>, and <see cref="AtomFeed.UpdatedOn">updated</see>) in the <see cref="AtomSource"/>.
    ///     </para>
    /// </remarks>
    public AtomSource? Source { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a short summary, abstract, or excerpt for this entry.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         It is not advisable for the<see cref="Summary"/> property to duplicate <see cref="Title"/> or <see cref="Content"/> because Atom Processors might assume there is a useful summary when there is none.
    ///     </para>
    ///     <para>
    ///         Entries must contain a <see cref="Summary"/> in either of the following cases:
    ///         <list type="number">
    ///             <item>
    ///                 <description>
    ///                      The <see cref="AtomEntry"/> contains an <see cref="Content"/> property that has a <see cref="AtomContent.Source"/> property (and is thus empty).
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      The <see cref="AtomEntry"/> contains content that is encoded in Base64; i.e., the <see cref="AtomContent.ContentType"/> property of <see cref="Content"/> property
    ///                      is a <a href="https://www.rfc-editor.org/rfc/rfc4288.html">MIME media type</a> in the sense of BCP 13 (RFC 4288, now RFC 6838), but is not an <a href="https://www.rfc-editor.org/rfc/rfc3023.html">XML media type</a> (RFC 3023, now RFC 7303),
    ///                      does not begin with <c>text/</c>, and does not end with <c>/xml</c> or <c>+xml</c>.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public AtomTextConstruct? Summary { get; set; }

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this entry.
    /// </summary>
    /// <value>The <c>atom:title</c>. The default value is <see langword="null"/>; RFC 4287 §4.1.2 requires exactly one on a conformant entry.</value>
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
    /// Gets or sets a date-time indicating the most recent instant in time when this entry was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     The <c>atom:updated</c> timestamp. The default value is <see cref="DateTime.MinValue"/>, which means none was provided — and no
    ///     <c>atom:updated</c> is written when it is left there.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         <b>A conformant entry must carry this.</b> RFC 4287 §4.1.2 requires exactly one <c>atom:updated</c>, and the
    ///         sentinel default means a entry you build without setting it saves without the element — valid XML, invalid Atom, and aggregators that
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
    public Version Version => feedVersion;

    /// <summary>
    /// Asynchronously creates a new <see cref="AtomEntry"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{AtomEntry}"/> that represents the asynchronous operation. The task result contains the loaded <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the CreateAsync method." />
    /// </example>
    public static async Task<AtomEntry> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        AtomEntry syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Asynchronously creates a new <see cref="AtomEntry"/> instance using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{AtomEntry}"/> that represents the asynchronous operation. The task result contains the loaded <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    public static async Task<AtomEntry> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        AtomEntry syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="AtomEntry"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="AtomEntry"/>.
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="AtomEntry"/>.
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
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(IXPathNavigable source) => this.Load(source, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    public virtual void Load(IXPathNavigable source, SyndicationResourceLoadSettings? settings)
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
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
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
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(XmlReader reader) => this.Load(reader, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    public void Load(XmlReader reader, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using XmlReader safeReader = XmlReader.Create(reader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        this.Load(new XPathDocument(safeReader), settings);
    }

    /// <summary>
    /// Asynchronously loads the <see cref="AtomEntry"/> from the specified <see cref="Uri"/> using the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Asynchronously loads the <see cref="AtomEntry"/> from the specified <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         If <paramref name="settings"/> names no <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> — which is
    ///         the default — the encoding of the <paramref name="source"/> is determined from its byte-order mark or XML declaration, falling
    ///         back to <see cref="System.Text.Encoding.UTF8"/> if it declares neither. Naming one overrides what the document declares.
    ///     </para>
    ///     <para>
    ///         After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    public virtual async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            source, httpClient, settings, SyndicationContentLengthLimits.Feed, requestOptions, cancellationToken).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.AtomEntryDocument);

        this.OnEntryLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(Stream stream) => this.Save(stream, null);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomEntry"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     <b>Save writes the object graph as it stands; it does not enforce RFC 4287's document-level
    ///     requirements.</b> A conformant entry document must carry exactly one <c>atom:id</c>,
    ///     <c>atom:title</c> and <c>atom:updated</c> (§4.1.2); members that are unset are simply
    ///     omitted, so output conformance is the caller's to ensure, not this method's to guarantee.
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
    ///     <code source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
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
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomEntry"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public virtual void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);

        writer.WriteStartElement("entry", AtomUtility.AtomNamespace);

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

            if (this.Content is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Content, settings.SupportedExtensions);
            }
            if (this.Id is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Id, settings.SupportedExtensions);
            }
            if (this.Rights is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Rights, settings.SupportedExtensions);
            }
            if (this.Source is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Source, settings.SupportedExtensions);
            }
            if (this.Summary is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Summary, settings.SupportedExtensions);
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

        this.WriteEntryOptionals(writer);
        this.WriteEntryCollections(writer);

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomEntry"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="AtomEntry.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings? settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.AtomEntryDocument);

        this.OnEntryLoaded(eventData);
    }

    /// <summary>
    /// Saves the current <see cref="AtomEntry"/> collection entities to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    private void WriteEntryCollections(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

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

        foreach (AtomLink link in this.Links)
        {
            link.WriteTo(writer);
        }
    }

    /// <summary>
    /// Saves the current <see cref="AtomEntry"/> optional entities to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    private void WriteEntryOptionals(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        this.Content?.WriteTo(writer);

        if (this.PublishedOn != DateTime.MinValue)
        {
            writer.WriteElementString("published", AtomUtility.AtomNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.PublishedOn));
        }

        this.Rights?.WriteTo(writer, "rights");

        this.Source?.WriteTo(writer);

        this.Summary?.WriteTo(writer, "summary");
    }
}