using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;
using Argotic.Syndication;

namespace Argotic.Publishing;

/// <summary>
/// Supports the discovery of the capabilities and locations of available <see cref="AtomWorkspace">collections</see> organized using one or more <see cref="AtomWorkspace">workspaces</see>.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="AtomServiceDocument"/> class implements the <i>app:service</i> element of the <a href="https://www.rfc-editor.org/rfc/rfc5023.html">Atom Publishing Protocol</a>.
///     </para>
///     <para>
///         Service documents represent server-defined groups of <see cref="AtomMemberResources">collections</see>, and are used to initialize the process of creating and editing resources.
///         These groups of <see cref="AtomMemberResources">collections</see> are called <see cref="AtomWorkspace">workspaces</see>. <see cref="AtomWorkspace">Workspaces</see> have names,
///         but no IRIs, and no specified processing model. The service document can indicate which <see cref="AtomAcceptedMediaRange">media types</see>, and which
///         <see cref="AtomCategoryDocument">categories</see>, a <see cref="AtomMemberResources">collection</see> will accept.
///     </para>
///     <para>
///         For authoring to commence, a client needs to discover the capabilities and locations of the available <see cref="AtomMemberResources">collections</see>.
///         Service documents are designed to support this discovery process. Service documents are identified with the <i>application/atomsvc+xml</i> media type.
///     </para>
/// </remarks>
/// <seealso cref="AtomCategoryDocument"/>
[MimeMediaType(Name = "application", SubName = "atomsvc+xml", Documentation = "http://bitworking.org/projects/atom/rfc5023.html#iana-atomsvc")]
public class AtomServiceDocument : ISyndicationResource, IExtensibleSyndicationObject, IAtomCommonObjectAttributes
{
    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat documentFormat = SyndicationContentFormat.AtomServiceDocument;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version documentVersion = new(1, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomServiceDocument"/> class.
    /// </summary>
    public AtomServiceDocument()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AtomServiceDocument"/> class using the supplied <see cref="IEnumerable{AtomWorkspace}"/> collection.
    /// </summary>
    /// <param name="workspaces">
    ///     A <see cref="IEnumerable{T}"/> collection of <see cref="AtomWorkspace"/> objects that represent server-defined groups of <see cref="AtomWorkspace"/> objects associate with the document.
    /// </param>
    /// <exception cref="ArgumentNullException">The <paramref name="workspaces"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="workspaces"/> collection does not contain at least one <see cref="AtomWorkspace"/>.</exception>
    public AtomServiceDocument(IEnumerable<AtomWorkspace> workspaces)
    {
        ArgumentNullException.ThrowIfNull(workspaces);

        foreach (AtomWorkspace workspace in workspaces)
        {
            this.Workspaces.Add(workspace);
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(this.Workspaces.Count, 1);
    }

    /// <summary>
    /// Gets or sets the <see cref="AtomWorkspace"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the workspace to get or set.</param>
    /// <returns>The <see cref="AtomWorkspace"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="AtomServiceDocument.Workspaces"/>.</exception>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public AtomWorkspace this[int index]
    {
        get => this.Workspaces[index];

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this.Workspaces[index] = value;
        }
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="AtomServiceDocument.Load(IXPathNavigable)"/>
    /// <seealso cref="AtomServiceDocument.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="AtomServiceDocument.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnDocumentLoaded(SyndicationResourceLoadedEventArgs e) => this.Loaded?.Invoke(this, e);

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
    /// Gets the IANA MIME media type identifier assigned to Atom service documents.
    /// </summary>
    /// <value><c>application/atomsvc+xml</c>.</value>
    /// <remarks>
    ///     See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">https://www.iana.org/assignments/media-types/media-types.xhtml</a> for a listing of the registered IANA MIME media types and subtypes.
    /// </remarks>
    public static string MediaType => "application/atomsvc+xml";

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    public SyndicationContentFormat Format => documentFormat;

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>Always <c>1.0</c>. The Atom Publishing Protocol has only ever had one version.</value>
    public Version Version => documentVersion;

    /// <summary>
    /// Gets the workspaces associated with this document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         RFC 5023 §8.3.1 requires at least one. Nothing enforces that on save — the collection-taking constructor checks it, but a document built
    ///         through this property alone can serialise with none.
    ///     </para>
    /// </remarks>
    public IList<AtomWorkspace> Workspaces { get; } = [];

    /// <summary>
    /// Asynchronously creates a new <see cref="AtomServiceDocument"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{AtomServiceDocument}"/> that represents the asynchronous operation. The task result contains the loaded <see cref="AtomServiceDocument"/>.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static async Task<AtomServiceDocument> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        AtomServiceDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Asynchronously creates a new <see cref="AtomServiceDocument"/> instance using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task{AtomServiceDocument}"/> that represents the asynchronous operation. The task result contains the loaded <see cref="AtomServiceDocument"/>.</returns>
    /// <remarks>
    ///     <para>
    ///         This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///         This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static async Task<AtomServiceDocument> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        AtomServiceDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="AtomServiceDocument"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="AtomServiceDocument"/>.
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="AtomServiceDocument"/>.
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
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(IXPathNavigable source) => this.Load(source, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event will be raised.
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
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event will be raised.
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
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(XmlReader reader) => this.Load(reader, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event will be raised.
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
    /// Loads this <see cref="AtomServiceDocument"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="AtomServiceDocument"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/> and the shared <see cref="HttpClient"/>.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Loads this <see cref="AtomServiceDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
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
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            source, httpClient, settings, SyndicationContentLengthLimits.Feed, requestOptions, cancellationToken).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.AtomServiceDocument);

        this.OnDocumentLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(Stream stream) => this.Save(stream, null);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomServiceDocument"/> instance. This value can be <see langword="null"/>.</param>
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
    public void Save(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        this.Save(writer, new SyndicationResourceSaveSettings());
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> and <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="AtomServiceDocument"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);

        writer.WriteStartElement("service", AtomUtility.AtomPublishingNamespace);
        // writer.WriteAttributeString("version", this.Version.ToString());

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

            foreach (AtomWorkspace workspace in this.Workspaces)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(workspace, settings.SupportedExtensions);
            }
        }
        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        AtomUtility.WriteCommonObjectAttributes(this, writer);

        foreach (AtomWorkspace workspace in this.Workspaces)
        {
            workspace.WriteTo(writer);
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="AtomServiceDocument"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="AtomServiceDocument.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="AtomServiceDocument.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
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
        adapter.Fill(this, SyndicationContentFormat.AtomServiceDocument);

        this.OnDocumentLoaded(eventData);
    }
}