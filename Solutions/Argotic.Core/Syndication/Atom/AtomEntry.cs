using System.Globalization;
using System.Text;
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
///         at <a href="http://www.atomenabled.org/developers/syndication/atom-format-spec.php">http://www.atomenabled.org/developers/syndication/atom-format-spec.php</a>.
///     </para>
///     <para>
///         Experience teaches that feeds that contain textual content are in general more useful than those that do not.
///         Some applications (one example is full-text indexers) require a minimum amount of text or (X)HTML to function reliably and predictably.
///         Feed producers should be aware of these issues. It is advisable that each <see cref="AtomEntry"/> object contain a non-empty <see cref="AtomEntry.Title"/>,
///         a non-empty <see cref="AtomEntry.Content"/> when content is defined, and a non-empty <see cref="AtomEntry.Summary"/> when the entry contains does not provide a <see cref="AtomEntry.Content"/>.
///         However, the absence of <see cref="AtomEntry.Summary"/> is not an error, and Atom Processors <b>must not</b> fail to function correctly as a consequence of such an absence.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the AtomEntry class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
///             region="AtomEntry"
///         />
///     </code>
/// </example>
[Serializable]
public class AtomEntry : ISyndicationResource, IAtomCommonObjectAttributes, IExtensibleSyndicationObject
{
    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat feedFormat = SyndicationContentFormat.Atom;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version feedVersion = new(1, 0);

    /// <summary>
    /// Private member to hold information that contains or links to the content of the entry.
    /// </summary>
    private AtomContent entryContent;

    /// <summary>
    /// Private member to hold a permanent, universally unique identifier for the entry.
    /// </summary>
    private AtomId entryId;

    /// <summary>
    /// Private member to hold a value indicating an instant in time associated with an event early in the life cycle of the entry.
    /// </summary>
    private DateTime entryPublishedOn = DateTime.MinValue;

    /// <summary>
    /// Private member to hold information about rights held in and over the entry.
    /// </summary>
    private AtomTextConstruct entryRights;

    /// <summary>
    /// Private member to hold the meta-data of the source feed that the entry was copied from.
    /// </summary>
    private AtomSource entrySource;

    /// <summary>
    /// Private member to hold information that conveys a short summary, abstract, or excerpt of the entry.
    /// </summary>
    private AtomTextConstruct entrySummary;

    /// <summary>
    /// Private member to hold information that conveys a human-readable title for the entry.
    /// </summary>
    private AtomTextConstruct entryTitle;

    /// <summary>
    /// Private member to hold a value indicating the most recent instant in time when the entry was modified in a way the publisher considers significant.
    /// </summary>
    private DateTime entryUpdatedOn = DateTime.MinValue;

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
    /// <exception cref="ArgumentNullException">The <paramref name="id"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
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
    public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

    /// <summary>
    /// Raises the <see cref="AtomEntry.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnEntryLoaded(SyndicationResourceLoadedEventArgs e)
    {
        this.Loaded?.Invoke(this, e);
    }

    /// <summary>
    /// Gets or sets the base URI other than the base URI of the document or external entity.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a base URI other than the base URI of the document or external entity. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is interpreted as a URI Reference as defined in <a href="http://www.ietf.org/rfc/rfc2396.txt">RFC 2396: Uniform Resource Identifiers</a>,
    ///         after processing according to <a href="http://www.w3.org/TR/xmlbase/#escaping">XML Base, Section 3.1 (URI Reference Encoding and Escaping)</a>.</para>
    /// </remarks>
    public Uri BaseUri { get; set; }

    /// <summary>
    /// Gets or sets the natural or formal language in which the content is written.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> that represents the natural or formal language in which the content is written. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     <para>
    ///         The value of this property is a language identifier as defined by <a href="http://www.ietf.org/rfc/rfc3066.txt">RFC 3066: Tags for the Identification of Languages</a>, or its successor.
    ///     </para>
    /// </remarks>
    public CultureInfo Language { get; set; }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the authors of this entry.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the authors of this entry.</value>
    /// <remarks>
    ///     <para>
    ///        An entry <b>must</b> contain one or more authors, unless the entry contains an <see cref="AtomEntry.Source"/> object that contains an author or,
    ///        in an Atom Feed Document, the <see cref="AtomFeed"/> contains an author itself.
    ///     </para>
    /// </remarks>
    public IList<AtomPersonConstruct> Authors { get; } = [];

    /// <summary>
    /// Gets the categories associated with this entry.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomCategory"/> objects that represent the categories associated with this entry.</value>
    public IList<AtomCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets information that contains or links to the content of this entry.
    /// </summary>
    /// <value>A <see cref="AtomContent"/> object that represents information that contains or links to the content of this entry.</value>
    public AtomContent Content
    {
        get => entryContent;
        set => entryContent = value;
    }

    /// <summary>
    /// Gets the entities who contributed to this entry.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomPersonConstruct"/> objects that represent the entities who contributed to this entry.</value>
    public IList<AtomPersonConstruct> Contributors { get; } = [];

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication resource implements.</value>
    public SyndicationContentFormat Format => feedFormat;

    /// <summary>
    /// Gets or sets a permanent, universally unique identifier for this entry.
    /// </summary>
    /// <value>A <see cref="AtomId"/> object that represents a permanent, universally unique identifier for this entry.</value>
    /// <remarks>
    ///     <para>
    ///         When an <i>Atom Document</i> is relocated, migrated, syndicated, republished, exported, or imported, the content of its universally unique identifier <b>must not</b> change.
    ///         Put another way, an <see cref="AtomId"/> pertains to all instantiations of a particular <see cref="AtomEntry"/>; revisions retain the same
    ///         content in their <see cref="AtomId"/> properties. It is suggested that the<see cref="AtomId"/> be stored along with the associated resource.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public AtomId Id
    {
        get => entryId;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            entryId = value;
        }
    }

    /// <summary>
    /// Gets references from this entry to one or more Web resources.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="AtomLink"/> objects that represent references from this entry to one or more Web resources.</value>
    /// <remarks>
    ///     <para>
    ///         An entry <b>must not</b> contain more than one <see cref="AtomLink"/> with a <see cref="AtomLink.Relation"/> property of <i>alternate</i>
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
    public DateTime PublishedOn
    {
        get => entryPublishedOn;
        set => entryPublishedOn = value;
    }

    /// <summary>
    /// Gets or sets information about rights held in and over this entry.
    /// </summary>
    /// <value>A <see cref="AtomTextConstruct"/> object that represents information about rights held in and over this entry.</value>
    /// <remarks>
    ///     The <see cref="Rights"/> property <i>should not</i> be used to convey machine-readable licensing information.
    ///     If an <see cref="AtomEntry"/> does not provide any rights information, then the <see cref="AtomFeed.Rights"/> of the containing feed, if present, is considered to apply to the entry.
    /// </remarks>
    public AtomTextConstruct Rights
    {
        get => entryRights;
        set => entryRights = value;
    }

    /// <summary>
    /// Gets or sets the meta-data of the source feed that this entry was copied from.
    /// </summary>
    /// <value>A <see cref="AtomSource"/> object that represents the meta-data of the source feed that this entry was copied from.</value>
    /// <remarks>
    ///     <para>
    ///         The <see cref="AtomSource"/> is designed to allow the aggregation of entries from different feeds while retaining information about an entry's source feed.
    ///         For this reason, Atom Processors that are performing such aggregation <i>should</i> include at least the required feed-level meta-data elements
    ///         (<see cref="AtomFeed.Id">id</see>, <see cref="AtomFeed.Title">title</see>, and <see cref="AtomFeed.UpdatedOn">updated</see>) in the <see cref="AtomSource"/>.
    ///     </para>
    /// </remarks>
    public AtomSource Source
    {
        get => entrySource;
        set => entrySource = value;
    }

    /// <summary>
    /// Gets or sets information that conveys a short summary, abstract, or excerpt for this entry.
    /// </summary>
    /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a short summary, abstract, or excerpt for this entry.</value>
    /// <remarks>
    ///     <para>
    ///         It is not advisable for the<see cref="Summary"/> property to duplicate <see cref="Title"/> or <see cref="Content"/> because Atom Processors might assume there is a useful summary when there is none.
    ///     </para>
    ///     <para>
    ///         Entries <b>must</b> contain a <see cref="Summary"/> in either of the following cases:
    ///         <list type="number">
    ///             <item>
    ///                 <description>
    ///                      The <see cref="AtomEntry"/> contains an <see cref="Content"/> property that has a <see cref="AtomContent.Source"/> property (and is thus empty).
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                      The <see cref="AtomEntry"/> contains content that is encoded in Base64; i.e., the <see cref="AtomContent.ContentType"/> property of <see cref="Content"/> property
    ///                      is a <a href="http://www.ietf.org/rfc/rfc4288.txt">MIME media type</a>, but is not an <a href="http://www.ietf.org/rfc/rfc3023.txt">XML media type</a>,
    ///                      does not begin with <b>text/</b>, and does not end with <b>/xml</b> or <b>+xml</b>.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    public AtomTextConstruct Summary
    {
        get => entrySummary;
        set => entrySummary = value;
    }

    /// <summary>
    /// Gets or sets information that conveys a human-readable title for this entry.
    /// </summary>
    /// <value>A <see cref="AtomTextConstruct"/> object that represents information that conveys a human-readable title for this entry.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public AtomTextConstruct Title
    {
        get => entryTitle;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            entryTitle = value;
        }
    }

    /// <summary>
    /// Gets or sets a date-time indicating the most recent instant in time when this entry was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates the most recent instant in time when this entry was modified in a way the publisher considers significant.
    ///     Publishers <i>may</i> change the value of this element over time. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update time was provided.
    /// </value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime UpdatedOn
    {
        get => entryUpdatedOn;
        set => entryUpdatedOn = value;
    }

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>The <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to. The default value is <b>2.0</b>.</value>
    public Version Version => feedVersion;

    /// <summary>
    /// Asynchronously creates a new <see cref="AtomEntry"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{AtomEntry}"/> that represents the asynchronous operation. The task result contains the loaded <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
    ///         <code
    ///             source=".\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
    ///             region="CreateAsync(Uri source)"
    ///         />
    ///     </code>
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{AtomEntry}"/> that represents the asynchronous operation. The task result contains the loaded <see cref="AtomEntry"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
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
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Document,
            Indent = true,
            OmitXmlDeclaration = false
        };

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
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
    ///             region="Load(IXPathNavigable source)"
    ///         />
    ///     </code>
    /// </example>
    public void Load(IXPathNavigable source)
    {
        this.Load(source, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    public void Load(IXPathNavigable source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (settings == null)
        {
            settings = new SyndicationResourceLoadSettings();
        }

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator));
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
    ///             region="Load(Stream stream)"
    ///         />
    ///     </code>
    /// </example>
    public void Load(Stream stream)
    {
        this.Load(stream, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    public void Load(Stream stream, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (settings != null)
        {
            this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding), settings);
        }
        else
        {
            this.Load(SyndicationEncodingUtility.CreateSafeNavigator(stream), settings);
        }
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the entry remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
    ///             region="Load(XmlReader reader)"
    ///         />
    ///     </code>
    /// </example>
    public void Load(XmlReader reader)
    {
        this.Load(reader, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default)
    {
        return LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads the <see cref="AtomEntry"/> from the specified <see cref="Uri"/> using the specified <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/>
    ///         the character encoding of the <paramref name="source"/> will be attempted to be determined automatically. Otherwise, the specified character encoding will be used.
    ///         If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
    ///     </para>
    ///     <para>
    ///         After the load operation has successfully completed, the <see cref="AtomEntry.Loaded"/> event will be raised.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(settings.Timeout);

        Encoding? encoding = settings.CharacterEncoding == System.Text.Encoding.UTF8 ? null : settings.CharacterEncoding;
        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(source, httpClient, encoding, requestOptions, timeoutCts.Token).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Atom);

        this.OnEntryLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Save method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
    ///             region="Save(Stream stream)"
    ///         />
    ///     </code>
    /// </example>
    public void Save(Stream stream)
    {
        this.Save(stream, null);
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomEntry"/> instance. This value can be <b>null</b>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(Stream stream, SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (settings == null)
        {
            settings = new SyndicationResourceSaveSettings();
        }

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
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Save method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Atom\AtomEntryExample.cs"
    ///             region="Save(XmlWriter writer)"
    ///         />
    ///     </code>
    /// </example>
    public void Save(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        this.Save(writer, new SyndicationResourceSaveSettings());
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomEntry"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);

        writer.WriteStartElement("entry", AtomUtility.AtomNamespace);

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

            if (this.Content != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Content, settings.SupportedExtensions);
            }
            if (this.Id != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Id, settings.SupportedExtensions);
            }
            if (this.Rights != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Rights, settings.SupportedExtensions);
            }
            if (this.Source != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Source, settings.SupportedExtensions);
            }
            if (this.Summary != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Summary, settings.SupportedExtensions);
            }
            if (this.Title != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the entry remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings? settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Atom);

        this.OnEntryLoaded(eventData);
    }

    /// <summary>
    /// Saves the current <see cref="AtomEntry"/> collection entities to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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