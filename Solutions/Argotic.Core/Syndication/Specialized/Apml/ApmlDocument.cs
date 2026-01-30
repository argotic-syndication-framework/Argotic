using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents an Attention Profiling Markup Language (APML) syndication resource.
/// </summary>
/// <remarks>
///     <para>
///         This implementation conforms to the Attention Profiling Markup Language (APML) 0.6 specification,
///         which can be found at <a href="http://apml.pbwiki.com/">http://apml.pbwiki.com</a>.
///     </para>
///     <para>
///         The purpose of this format is to provide an open standard that encapsulates a summary of your interests (across multiple profiles) in a simple, portable way.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlDocument class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Apml\ApmlDocumentExample.cs"
///             region="ApmlDocument"
///         />
///     </code>
/// </example>
[Serializable]
public class ApmlDocument : ISyndicationResource, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat documentFormat = SyndicationContentFormat.Apml;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version documentVersion = new(0, 6);

    /// <summary>
    /// Private member to hold header information for the document.
    /// </summary>
    private ApmlHead documentHead = new();

    /// <summary>
    /// Private member to hold the name of the default profile for the document.
    /// </summary>
    private string documentDefaultProfileName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlDocument"/> class.
    /// </summary>
    public ApmlDocument()
    {

    }

    /// <summary>
    /// Gets or sets the <see cref="ApmlProfile"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the profile to get or set.</param>
    /// <returns>The <see cref="ApmlProfile"/> at the specified index.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is less than zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="index"/> is equal to or greater than the count for <see cref="ApmlDocument.Profiles"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public ApmlProfile this[int index]
    {
        get => this.Profiles[index];
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this.Profiles[index] = value;
        }
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="ApmlDocument.Load(IXPathNavigable)"/>
    /// <seealso cref="ApmlDocument.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

    /// <summary>
    /// Raises the <see cref="ApmlDocument.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnDocumentLoaded(SyndicationResourceLoadedEventArgs e)
    {
        this.Loaded?.Invoke(this, e);
    }

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
    /// Gets the applications for this document.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlApplication"/> objects that represent the applications for this document.</value>
    public IList<ApmlApplication> Applications { get; } = [];

    /// <summary>
    /// Gets or sets the name of the default profile for this document.
    /// </summary>
    /// <value>The name of the default profile to use when either no user selection is made, or no user selection can be made.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string DefaultProfileName
    {
        get => documentDefaultProfileName;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            documentDefaultProfileName = value.Trim();
        }
    }

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication resource implements.</value>
    public SyndicationContentFormat Format => documentFormat;

    /// <summary>
    /// Gets or sets the basic administrative information for this document.
    /// </summary>
    /// <value>A <see cref="ApmlHead"/> object that describes the basic administrative information for this document.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public ApmlHead Head
    {
        get => documentHead;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            documentHead = value;
        }
    }

    /// <summary>
    /// Gets the attention profiles for this document.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlProfile"/> objects that represents the attention profiles for this document.</value>
    public IList<ApmlProfile> Profiles { get; } = [];

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>The <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to. The default value is <b>2.0</b>.</value>
    public Version Version => documentVersion;

    /// <summary>
    /// Creates a new <see cref="ApmlDocument"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ApmlDocument"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
    ///         var document = await ApmlDocument.CreateAsync(new Uri("https://example.com/apml.xml"));
    ///     </code>
    /// </example>
    public static async Task<ApmlDocument> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        ApmlDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Creates a new <see cref="ApmlDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="ApmlDocument"/> object loaded using the <paramref name="source"/> data.</returns>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<ApmlDocument> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ApmlDocument syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Loads this <see cref="ApmlDocument"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    ///     <para>After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default)
    {
        return LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);
    }

    /// <summary>
    /// Loads this <see cref="ApmlDocument"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
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
    ///     <para>After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(settings.Timeout);

        var encoding = settings.CharacterEncoding == System.Text.Encoding.UTF8 ? null : settings.CharacterEncoding;
        var navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(source, httpClient, encoding, requestOptions, timeoutCts.Token).ConfigureAwait(false);

        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Apml);

        this.OnDocumentLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="ApmlDocument"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="ApmlDocument"/>.
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="ApmlDocument"/>.
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
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Apml\ApmlDocumentExample.cs"
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(IXPathNavigable source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (settings == null)
        {
            settings = new SyndicationResourceLoadSettings();
        }
        XPathNavigator navigator = source.CreateNavigator();
        this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator));
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Apml\ApmlDocumentExample.cs"
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(Stream stream, SyndicationResourceLoadSettings settings)
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
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\Apml\ApmlDocumentExample.cs"
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the document remains empty.</exception>
    public void Load(XmlReader reader, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(reader);

        using XmlReader safeReader = XmlReader.Create(reader, SyndicationEncodingUtility.CreateSafeXmlReaderSettings());
        this.Load(new XPathDocument(safeReader), settings);
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
    ///             source="..\..\Argotic.Examples\Core\Apml\ApmlDocumentExample.cs"
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
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="ApmlDocument"/> instance. This value can be <b>null</b>.</param>
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
    ///             source="..\..\Argotic.Examples\Core\Apml\ApmlDocumentExample.cs"
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
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="ApmlDocument"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);
        writer.WriteStartElement("APML", ApmlUtility.ApmlNamespace);
        writer.WriteAttributeString("version", this.Version.ToString());

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);

            if (this.Head != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Head, settings.SupportedExtensions);
            }

            foreach (ApmlApplication application in this.Applications)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(application, settings.SupportedExtensions);
            }

            foreach (ApmlProfile profile in this.Profiles)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(profile, settings.SupportedExtensions);

                foreach (ApmlConcept explicitConcept in profile.ExplicitConcepts)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(explicitConcept, settings.SupportedExtensions);
                }

                foreach (ApmlSource explicitSource in profile.ExplicitSources)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(explicitSource, settings.SupportedExtensions);

                    foreach (ApmlAuthor explicitAuthor in explicitSource.Authors)
                    {
                        SyndicationExtensionAdapter.FillExtensionTypes(explicitAuthor, settings.SupportedExtensions);
                    }
                }

                foreach (ApmlConcept implicitConcept in profile.ImplicitConcepts)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(implicitConcept, settings.SupportedExtensions);
                }

                foreach (ApmlSource implicitSource in profile.ImplicitSources)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(implicitSource, settings.SupportedExtensions);

                    foreach (ApmlAuthor implicitAuthor in implicitSource.Authors)
                    {
                        SyndicationExtensionAdapter.FillExtensionTypes(implicitAuthor, settings.SupportedExtensions);
                    }
                }
            }
        }
        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        this.Head.WriteTo(writer);

        writer.WriteStartElement("Body", ApmlUtility.ApmlNamespace);
        writer.WriteAttributeString("defaultprofile", this.DefaultProfileName);

        foreach (ApmlProfile profile in this.Profiles)
        {
            profile.WriteTo(writer);
        }

        if (this.Applications.Count > 0)
        {
            writer.WriteStartElement("Applications", ApmlUtility.ApmlNamespace);
            foreach (ApmlApplication application in this.Applications)
            {
                application.WriteTo(writer);
            }
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="ApmlDocument"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="ApmlDocument.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="ApmlDocument.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the document remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);
        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Apml);
        this.OnDocumentLoaded(eventData);
    }
}