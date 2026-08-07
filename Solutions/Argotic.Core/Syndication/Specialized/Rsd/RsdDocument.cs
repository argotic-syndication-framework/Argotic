using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a Really Simple Discovery (RSD) syndication resource.
/// </summary>
/// <remarks>
///     <para>
///         RSD was how a desktop blogging client found out where to post. A blog advertised one small XML
///         document — linked from its home page with <c>rel="EditURI"</c> — naming the editing endpoints it
///         offered, so that a user could type their blog's address and nothing more. It is of historical
///         interest: mainstream platforms no longer publish it, and the client applications that consumed it
///         are gone.
///     </para>
///     <para>
///         This implementation conforms to the Really Simple Discovery (RSD) 1.0 specification,
///         which can be found at <a href="https://cyber.harvard.edu/blogs/gems/tech/rsd.html">https://cyber.harvard.edu/blogs/gems/tech/rsd.html</a>.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rsd\RsdDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the RsdDocument class." />
/// </example>
public class RsdDocument : ISyndicationResource, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat documentFormat = SyndicationContentFormat.Opml;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version documentVersion = new(1, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="RsdDocument"/> class.
    /// </summary>
    public RsdDocument()
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="RsdApplicationInterface"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the application interface to get or set.</param>
    /// <returns>The <see cref="RsdApplicationInterface"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="RsdDocument.Interfaces"/>.</exception>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public RsdApplicationInterface this[int index]
    {
        get => this.Interfaces[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this.Interfaces[index] = value;
        }
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="RsdDocument.Load(IXPathNavigable)"/>
    /// <seealso cref="RsdDocument.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="RsdDocument.Loaded"/> event.
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
    /// Gets or sets the homepage of the engine that is providing these discovery services.
    /// </summary>
    /// <value>The <c>engineLink</c> element — the blogging software's own site, not the blog's — or <see langword="null"/> if none was specified.</value>
    public Uri? EngineLink { get; set; }

    /// <summary>
    /// Gets or sets the name of the engine that is providing these discovery services.
    /// </summary>
    /// <value>The <c>engineName</c> element, such as <c>WordPress</c>, or an <i>empty</i> string if none was specified.</value>
    public string EngineName
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    public SyndicationContentFormat Format => documentFormat;

    /// <summary>
    /// Gets or sets the homepage of the website that is hosting these discovery services.
    /// </summary>
    /// <value>The <c>homePageLink</c> element — the blog this document describes — or <see langword="null"/> if none was specified.</value>
    public Uri? Homepage { get; set; }

    /// <summary>
    /// Gets the application interfaces that comprise the discoverable services for this document.
    /// </summary>
    /// <remarks>
    ///     A blog usually advertises several, one per protocol it accepts, and marks one
    ///     <see cref="RsdApplicationInterface.IsPreferred">preferred</see>. Nothing enforces that exactly one
    ///     is preferred, or that any is.
    /// </remarks>
    public IList<RsdApplicationInterface> Interfaces { get; } = [];

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>Always <c>1.0</c>. This is what <see cref="Save(XmlWriter)"/> writes, not what a loaded document declared.</value>
    public Version Version => documentVersion;

    /// <summary>
    /// Creates a new <see cref="RsdDocument"/> instance asynchronously using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    ///     For scenarios requiring custom credentials, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static async Task<RsdDocument> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        RsdDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Creates a new <see cref="RsdDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="RsdDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     This overload accepts an <see cref="HttpClient"/> parameter, allowing the caller to manage the client's lifecycle.
    ///     This is the recommended pattern for use with <c>IHttpClientFactory</c> in ASP.NET Core applications.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public static async Task<RsdDocument> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        RsdDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Loads this <see cref="RsdDocument"/> instance asynchronously using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="RsdDocument"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
    ///     <para>
    ///         This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    ///         For scenarios requiring custom credentials, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.
    ///     </para>
    ///     <para>
    ///         After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Loads this <see cref="RsdDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
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
    ///         After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.
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
            source, httpClient, settings, SyndicationContentLengthLimits.Feed, requestOptions, cancellationToken).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Rsd);

        this.OnDocumentLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="RsdDocument"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="RsdDocument"/>. 
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="RsdDocument"/>.
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
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rsd\RsdDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(IXPathNavigable source) => this.Load(source, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event will be raised.
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
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rsd\RsdDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event will be raised.
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
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rsd\RsdDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(XmlReader reader) => this.Load(reader, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event will be raised.
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
    ///     <code source="..\..\Argotic.Examples\Core\Rsd\RsdDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(Stream stream) => this.Save(stream, null);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="RsdDocument"/> instance. This value can be <see langword="null"/>.</param>
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
    ///     <code source="..\..\Argotic.Examples\Core\Rsd\RsdDocumentExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
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
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="RsdDocument"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);
        writer.WriteStartElement("rsd", RsdUtility.RsdNamespace);
        writer.WriteAttributeString("version", this.Version.ToString());

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

            foreach (RsdApplicationInterface api in this.Interfaces)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(api, settings.SupportedExtensions);
            }
        }
        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        writer.WriteStartElement("service", RsdUtility.RsdNamespace);

        if (!string.IsNullOrEmpty(this.EngineName))
        {
            writer.WriteElementString("engineName", RsdUtility.RsdNamespace, this.EngineName);
        }

        if (this.EngineLink is not null)
        {
            writer.WriteElementString("engineLink", RsdUtility.RsdNamespace, this.EngineLink.ToString());
        }

        if (this.Homepage is not null)
        {
            writer.WriteElementString("homePageLink", RsdUtility.RsdNamespace, this.Homepage.ToString());
        }

        writer.WriteStartElement("apis", RsdUtility.RsdNamespace);
        foreach (RsdApplicationInterface api in this.Interfaces)
        {
            api.WriteTo(writer);
        }
        writer.WriteEndElement();

        writer.WriteEndElement();
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RsdDocument"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="RsdDocument.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RsdDocument.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
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
        adapter.Fill(this, SyndicationContentFormat.Rsd);
        this.OnDocumentLoaded(eventData);
    }
}