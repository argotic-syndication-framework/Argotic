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
///         This implementation conforms to the RSS 2.0.10 specification, 
///         which can be found at <a href="http://www.rssboard.org/rss-specification">http://www.rssboard.org/rss-specification</a>.
///     </para>
///     <para>
///         This implementation also conforms to the <i>RSS Best Practices Profile</i> guidelines as close as possible, 
///         which can be found at <a href="http://www.rssboard.org/rss-profile">http://www.rssboard.org/rss-profile</a>.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssFeed class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" 
///             region="RssFeed" 
///         />
///     </code>
/// </example>
[Serializable]
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
    /// Private member to hold information about the meta-data and contents of the feed.
    /// </summary>
    private RssChannel feedChannel = new();
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
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is an empty string.</exception>
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
    ///     The description character data <b>must</b> be suitable for presentation as HTML.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is an empty string.</exception>
    public RssFeed(string description)
    {
        this.Channel.Description = description;
    }
    /// <summary>
    /// Occurs when the syndication resource state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="RssFeed.Load(IXPathNavigable)"/>
    /// <seealso cref="RssFeed.Load(XmlReader)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;
    /// <summary>
    /// Raises the <see cref="RssFeed.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e)
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
    /// Gets or sets information about the meta-data and contents of the feed.
    /// </summary>
    /// <value>A <see cref="RssChannel"/> object that represents information about the meta-data and contents of the feed.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public RssChannel Channel
    {
        get
        {
            return feedChannel;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            feedChannel = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication resource implements.
    /// </summary>
    /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication resource implements.</value>
    public SyndicationContentFormat Format
    {
        get
        {
            return feedFormat;
        }
    }

    /// <summary>
    /// Gets the <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to.
    /// </summary>
    /// <value>The <see cref="Version"/> of the <see cref="SyndicationContentFormat"/> that this syndication resource conforms to. The default value is <b>2.0</b>.</value>
    public Version Version
    {
        get
        {
            return feedVersion;
        }
    }
    /// <summary>
    /// Compares two specified <see cref="IList{RssCategory}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<RssCategory> source, IList<RssCategory> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and the shared <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="RssFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     <para>This method uses the shared <see cref="HttpClient"/> for simple scenarios without custom credentials or proxy.</para>
    ///     <para>For scenarios requiring authentication, proxy, or other handler-level configuration, use the overload that accepts an <see cref="HttpClient"/>.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public static async Task<RssFeed> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        RssFeed syndicationResource = new();
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);
        return syndicationResource;
    }

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
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
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" 
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
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
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" 
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
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
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the Load method.">
    ///         <code 
    ///             source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" 
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="RssFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="reader"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(XmlReader reader, SyndicationResourceLoadSettings settings)
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="OperationCanceledException">The operation was canceled via the <paramref name="cancellationToken"/>.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default)
    {
        return LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);
    }

    /// <summary>
    /// Loads this <see cref="RssFeed"/> instance asynchronously using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
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
        adapter.Fill(this, SyndicationContentFormat.Rss);

        this.OnFeedLoaded(new SyndicationResourceLoadedEventArgs(navigator, source));
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
    ///             source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" 
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
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="RssFeed"/> instance. This value can be <b>null</b>.</param>
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
    ///             source="..\..\Argotic.Examples\Core\Rss\RssFeedExample.cs" 
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
    /// Saves the syndication resource to the specified <see cref="XmlWriter"/> using the supplied <see cref="SyndicationResourceSaveSettings"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to save the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceSaveSettings"/> object used to configure the persistence of the <see cref="RssFeed"/> instance.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="XmlException">The operation would not result in well-formed XML for the syndication resource.</exception>
    public void Save(XmlWriter writer, SyndicationResourceSaveSettings settings)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(settings);
        writer.WriteStartElement("rss");
        writer.WriteAttributeString("version", this.Version.ToString());

        if (this.Channel != null && this.Channel.SelfLink != null)
        {
            writer.WriteAttributeString("xmlns", "atom", null, "http://www.w3.org/2005/Atom");
        }

        if (settings.AutoDetectExtensions)
        {
            SyndicationExtensionAdapter.FillExtensionTypes(this, settings.SupportedExtensions);
            SyndicationExtensionAdapter.FillExtensionTypes(this.Channel, settings.SupportedExtensions);
            if (this.Channel.Cloud != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Channel.Cloud, settings.SupportedExtensions);
            }
            if (this.Channel.Image != null)
            {
                SyndicationExtensionAdapter.FillExtensionTypes(this.Channel.Image, settings.SupportedExtensions);
            }
            if (this.Channel.TextInput != null)
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

                if (item.Guid != null)
                {
                    SyndicationExtensionAdapter.FillExtensionTypes(item.Guid, settings.SupportedExtensions);
                }
                if (item.Source != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings settings, SyndicationResourceLoadedEventArgs eventData)
    {
        ArgumentNullException.ThrowIfNull(navigator);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(eventData);
        SyndicationResourceAdapter adapter = new(navigator, settings);
        adapter.Fill(this, SyndicationContentFormat.Rss);
        this.OnFeedLoaded(eventData);
    }
}