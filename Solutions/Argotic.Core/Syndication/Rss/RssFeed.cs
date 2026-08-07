using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a Really Simple Syndication (RSS) syndication feed.
/// </summary>
/// <remarks>
///     <para>
///         Implements RSS 2.0.10,
///         <a href="https://www.rssboard.org/rss-specification">https://www.rssboard.org/rss-specification</a>,
///         and follows the
///         <a href="https://www.rssboard.org/rss-profile">RSS Best Practices Profile</a> where the two go
///         beyond it.
///     </para>
///     <para>
///         The <c>&lt;rss&gt;</c> element carries a version attribute and exactly one
///         <see cref="Channel">channel</see>; everything a caller is interested in lives on the channel. RSS
///         is a loose format by design — most elements are optional, several are defined only by example, and
///         real feeds diverge further still — so this class parses permissively and writes conservatively.
///         Where the specification is ambiguous, the properties it fills record which reading was taken.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the RssFeed class." />
/// </example>
public class RssFeed : ISyndicationResource, IExtensibleSyndicationObject
{

    /// <summary>
    /// Private member to hold the syndication format for this syndication resource.
    /// </summary>
    private const SyndicationContentFormat feedFormat = SyndicationContentFormat.Rss;

    /// <summary>
    /// Private member to hold the version of the syndication format for this syndication resource conforms to.
    /// </summary>
    private static readonly Version feedVersion = new(2, 0);

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeed"/> class.
    /// </summary>
    public RssFeed()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeed"/> class using the supplied link and title.
    /// </summary>
    /// <param name="link">A <see cref="Uri"/> that represents the URL of the website associated with this feed.</param>
    /// <param name="title">Character data that provides the name of this feed.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="title"/> is an empty string.</exception>
    public RssFeed(Uri link, string title)
    {
        this.Channel.Link = link;
        this.Channel.Title = title;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssFeed"/> class using the supplied channel description.
    /// </summary>
    /// <param name="description">Character data that provides a human-readable characterization or summary of this feed.</param>
    /// <remarks>
    ///     The description is rendered as HTML, so any markup in it must be escaped. See
    ///     <see cref="RssChannel.Description"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="description"/> is an empty string.</exception>
    public RssFeed(string description)
    {
        this.Channel.Description = description;
    }

    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="RssFeed.Load(IXPathNavigable)"/>
    /// <seealso cref="RssFeed.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="RssFeed.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e) => this.Loaded?.Invoke(this, e);

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
    /// Gets or sets the single channel this feed contains.
    /// </summary>
    /// <value>Never <see langword="null"/> — a new <see cref="RssFeed"/> starts with an empty channel rather than none.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public RssChannel Channel
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    /// <value>Always <see cref="SyndicationContentFormat.Rss"/>.</value>
    public SyndicationContentFormat Format => feedFormat;

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>Always <c>2.0</c>. This class does not read or write RSS 0.9x or RSS 1.0.</value>
    public Version Version => feedVersion;

    /// <summary>
    /// Creates a new <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="RssFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <example>
    ///     <code language="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
    ///         var feed = await RssFeed.CreateAsync(new Uri("https://example.com/feed.xml"));
    ///     </code>
    /// </example>
    public static async Task<RssFeed> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        RssFeed syndicationResource = new();
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Creates a new <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="RssFeed"/> object loaded using the <paramref name="source"/> data.</returns>
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
    public static async Task<RssFeed> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        RssFeed syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Initializes a read-only <see cref="XPathNavigator"/> object for navigating through nodes in this <see cref="RssFeed"/>.
    /// </summary>
    /// <returns>A read-only <see cref="XPathNavigator"/> object.</returns>
    /// <remarks>
    ///     The <see cref="XPathNavigator"/> is positioned on the root element of the <see cref="RssFeed"/>. 
    ///     If there is no root element, the <see cref="XPathNavigator"/> is positioned on the first element in the XML representation of the <see cref="RssFeed"/>.
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
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(IXPathNavigable source) => this.Load(source, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="IXPathNavigable"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
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
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
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
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Load method." />
    /// </example>
    public void Load(XmlReader reader) => this.Load(reader, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
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
    /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="RssFeed"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/> and the shared <see cref="HttpClient"/>.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
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
        adapter.Fill(this, SyndicationContentFormat.Rss);

        this.OnFeedLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(Stream stream) => this.Save(stream, null);

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="RssFeed"/> instance. This value can be <see langword="null"/>.</param>
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
    ///     <code source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the Save method." />
    /// </example>
    public void Save(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        this.Save(writer, new SyndicationResourceSaveSettings());
    }

    /// <summary>
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> using the supplied <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="RssFeed"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);
        writer.WriteStartElement("rss");
        writer.WriteAttributeString("version", this.Version.ToString());

        if (this.Channel.SelfLink is not null)
        {
            writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");
        }

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);
            SyndicationExtensionAdapter.FillExtensionTypes(this.Channel, settings.SupportedExtensions);
            if (this.Channel.Cloud is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Channel.Cloud, settings.SupportedExtensions);
            }
            if (this.Channel.Image is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Channel.Image, settings.SupportedExtensions);
            }
            if (this.Channel.TextInput is not null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Channel.TextInput, settings.SupportedExtensions);
            }

            foreach (RssCategory category in this.Channel.Categories)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(category, settings.SupportedExtensions);
            }
            foreach (RssItem item in this.Channel.Items)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(item, settings.SupportedExtensions);

                if (item.Guid is not null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(item.Guid, settings.SupportedExtensions);
                }
                if (item.Source is not null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(item.Source, settings.SupportedExtensions);
                }

                foreach (RssCategory category in item.Categories)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(category, settings.SupportedExtensions);
                }
                foreach (RssEnclosure enclosure in item.Enclosures)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(enclosure, settings.SupportedExtensions);
                }
            }
        }

        SyndicationExtensionAdapter.WriteXmlNamespaceDeclarations(settings.SupportedExtensions, writer);

        this.Channel?.WriteTo(writer);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the syndication resource using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="RssFeed"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="RssFeed.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
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
        adapter.Fill(this, SyndicationContentFormat.Rss);
        this.OnFeedLoaded(eventData);
    }
}