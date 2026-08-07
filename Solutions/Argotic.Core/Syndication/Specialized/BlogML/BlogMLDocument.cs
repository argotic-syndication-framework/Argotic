using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a Web Log Markup Language (BlogML) syndication resource.
/// </summary>
/// <remarks>
///     <para>
///         BlogML 2.0 is a blog-migration format: one document holds an entire blog — every post with its
///         comments, trackbacks and attachments, plus the author and category tables they refer to — so that a
///         site can be lifted from one engine and dropped into another. It is not a syndication format, and
///         nothing subscribes to it. It is also effectively dead: the format has not changed since 2006, and
///         current platforms export their own shapes instead.
///     </para>
///     <para>
///         This implementation conforms to the BlogML 2.0 specification,
///         which can be found at <a href="https://web.archive.org/web/20210506123858/http://blogml.org/">https://web.archive.org/web/20210506123858/http://blogml.org/</a>.
///     </para>
///     <para>
///         Because a document is a whole blog rather than a window onto one, these are the largest resources
///         the library handles, and the asynchronous loads read them under the archive size limit rather than
///         the feed one.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the BlogMLDocument class." />
/// </example>
public class BlogMLDocument : ISyndicationResource, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat documentFormat = SyndicationContentFormat.BlogML;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version documentVersion = new(2, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="BlogMLDocument"/> class.
    /// </summary>
    public BlogMLDocument()
    {
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="BlogMLDocument.Load(IXPathNavigable)"/>
    /// <seealso cref="BlogMLDocument.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="BlogMLDocument.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnDocumentLoaded(SyndicationResourceLoadedEventArgs e) => this.Loaded?.Invoke(this, e);

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets the authors of this web log.
    /// </summary>
    /// <remarks>
    ///     The document's author table. A post does not embed its authors; <see cref="BlogMLPost.Authors"/>
    ///     holds <see cref="BlogMLAuthor.Id"/> strings that are resolved against this collection. Nothing here
    ///     enforces that a referenced identifier exists, so a document can be structurally valid and still have
    ///     posts pointing at authors it does not define.
    /// </remarks>
    public IList<BlogMLAuthor> Authors { get; } = [];

    /// <summary>
    /// Gets the categories for this web log.
    /// </summary>
    /// <remarks>
    ///     The document's category table, referenced by identifier from <see cref="BlogMLPost.Categories"/>
    ///     under the same rules — and by <see cref="BlogMLCategory.ParentId"/>, which is how the category tree
    ///     is expressed, since the categories themselves are stored flat.
    /// </remarks>
    public IList<BlogMLCategory> Categories { get; } = [];

    /// <summary>
    /// Gets the extended properties of this web log.
    /// </summary>
    /// <remarks>
    ///     Blog-wide settings the format does not model, written as <c>property</c> elements with <c>name</c>
    ///     and <c>value</c> attributes. The vocabulary is whatever the exporting engine chose.
    /// </remarks>
    public Dictionary<string, string> ExtendedProperties { get; } = [];

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    public SyndicationContentFormat Format => documentFormat;

    /// <summary>
    /// Gets or sets a date-time indicating when this BlogML document was created.
    /// </summary>
    /// <value>
    ///     The <c>date-created</c> attribute of the document — when this export was taken, not when the blog began.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no creation date-time was provided, and suppresses the attribute on save.
    /// </value>
    /// <remarks>
    ///     Supply this in Coordinated Universal Time. BlogML dates are written as RFC 3339.
    /// </remarks>
    public DateTime GeneratedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the posts for this web log.
    /// </summary>
    /// <remarks>
    ///     Every post in the blog, each carrying its own comments, trackbacks and attachments. This is where
    ///     the bulk of a document lives.
    /// </remarks>
    public IList<BlogMLPost> Posts { get; } = [];

    /// <summary>
    /// Gets or sets the root URL of this web log.
    /// </summary>
    /// <value>The <c>root-url</c> attribute — the blog's base address, against which relative post URLs are resolved — or <see langword="null"/> if none was specified.</value>
    public Uri? RootUrl { get; set; }

    /// <summary>
    /// Gets or sets the sub-title of this web log.
    /// </summary>
    /// <value>The <c>sub-title</c> element, or <see langword="null"/> if the blog has none. Unlike <see cref="Title"/>, this is optional and is omitted from the output when null.</value>
    public BlogMLTextConstruct? Subtitle { get; set; }

    /// <summary>
    /// Gets or sets the title of this web log.
    /// </summary>
    /// <value>The <c>title</c> element. Never <see langword="null"/> — a new document starts with an empty text construct, and the setter rejects null.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public BlogMLTextConstruct Title
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>Always <c>2.0</c>. It is reported only: <see cref="Save(XmlWriter)"/> writes no <c>version</c> attribute, so a saved document does not declare which version it is.</value>
    public Version Version => documentVersion;

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code language="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
    ///         var document = await BlogMLDocument.CreateAsync(new Uri("https://example.com/blog.xml"));
    ///     </code>
    /// </example>
    public static async Task<BlogMLDocument> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        BlogMLDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Creates a new <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="BlogMLDocument"/> object loaded using the <paramref name="source"/> data.</returns>
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
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static async Task<BlogMLDocument> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        BlogMLDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Loads this <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="BlogMLDocument"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>
    ///         After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Loads this <see cref="BlogMLDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    ///     <para>
    ///         Configure handler-level settings (credentials, proxy, cookies) on the <see cref="HttpClient"/> itself,
    ///         either when creating it manually or via <c>IHttpClientFactory.ConfigurePrimaryHttpMessageHandler</c>.
    ///     </para>
    ///     <para>
    ///         After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            source, httpClient, settings, SyndicationContentLengthLimits.Archive, requestOptions, cancellationToken).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.BlogML);

        this.OnDocumentLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="BlogMLDocument"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="BlogMLDocument"/>. 
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="BlogMLDocument"/>.
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
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(IXPathNavigable source) => this.Load(source, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
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
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
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
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(XmlReader reader) => this.Load(reader, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(XmlReader reader, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using XmlReader safeReader = XmlReader.Create(reader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        this.Load(new XPathDocument(safeReader), settings);
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(Stream stream) => this.Save(stream, null);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="BlogMLDocument"/> instance. This value can be <see langword="null"/>.</param>
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
    ///     <code source="..\..\Argotic.Examples\Core\BlogML\BlogMLDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
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
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="BlogMLDocument"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);
        // No version attribute: the BlogML 2.0 schema declares blogType with date-created and root-url and
        // nothing else, and admits no attribute wildcard, so writing one would make the output invalid
        // against the very schema it claims to conform to. Version is a property of the object model only.
        writer.WriteStartElement("blog", BlogMLUtility.BlogMLNamespace);

        if (settings.AutoDetectExtensions)
        {
            this.FillExtensionTypes(settings);
        }
        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        if (this.GeneratedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("date-created", SyndicationDateTimeUtility.ToRfc3339DateTime(this.GeneratedOn));
        }

        if (this.RootUrl is not null)
        {
            writer.WriteAttributeString("root-url", this.RootUrl.ToString());
        }

        this.Title?.WriteTo(writer, "title");

        this.Subtitle?.WriteTo(writer, "sub-title");

        if (this.Authors.Count > 0)
        {
            writer.WriteStartElement("authors", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLAuthor author in this.Authors)
            {
                author.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (this.ExtendedProperties.Count > 0)
        {
            writer.WriteStartElement("extended-properties", BlogMLUtility.BlogMLNamespace);
            foreach (string property in this.ExtendedProperties.Keys)
            {
                writer.WriteStartElement("property", BlogMLUtility.BlogMLNamespace);
                writer.WriteAttributeString("name", property);
                writer.WriteAttributeString("value", this.ExtendedProperties[property]);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        if (this.Categories.Count > 0)
        {
            writer.WriteStartElement("categories", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        if (this.Posts.Count > 0)
        {
            writer.WriteStartElement("posts", BlogMLUtility.BlogMLNamespace);
            foreach (BlogMLPost post in this.Posts)
            {
                post.WriteTo(writer);
            }
            writer.WriteEndElement();
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Fills the supported extensions collection of the supplied <see cref="SyndicationResourceSaveSettings"/> object based on syndication extensions present in the current instance hierarchy.
    /// </summary>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object whose <see cref="SyndicationResourceSaveSettings.SupportedExtensions"/> collection is to be filled.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    private void FillExtensionTypes(SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

        if (this.Subtitle is not null)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this.Subtitle, settings.SupportedExtensions);
        }
        if (this.Title is not null)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this.Title, settings.SupportedExtensions);
        }

        foreach (BlogMLAuthor author in this.Authors)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(author, settings.SupportedExtensions);

            if (author.Title is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(author.Title, settings.SupportedExtensions);
            }
        }

        foreach (BlogMLCategory category in this.Categories)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(category, settings.SupportedExtensions);

            if (category.Title is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(category.Title, settings.SupportedExtensions);
            }
        }

        foreach (BlogMLPost post in this.Posts)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(post, settings.SupportedExtensions);

            if (post.Content is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Content, settings.SupportedExtensions);
            }
            if (post.Excerpt is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Excerpt, settings.SupportedExtensions);
            }
            if (post.Name is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Name, settings.SupportedExtensions);
            }
            if (post.Title is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(post.Title, settings.SupportedExtensions);
            }

            foreach (BlogMLAttachment attachment in post.Attachments)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(attachment, settings.SupportedExtensions);
            }

            foreach (BlogMLComment comment in post.Comments)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(comment, settings.SupportedExtensions);

                if (comment.Content is not null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(comment.Content, settings.SupportedExtensions);
                }
                if (comment.Title is not null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(comment.Title, settings.SupportedExtensions);
                }
            }

            foreach (BlogMLTrackback trackback in post.Trackbacks)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(trackback, settings.SupportedExtensions);
            }
        }
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="BlogMLDocument"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="BlogMLDocument.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="BlogMLDocument.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings? settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);
        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.BlogML);
        this.OnDocumentLoaded(eventData);
    }
}