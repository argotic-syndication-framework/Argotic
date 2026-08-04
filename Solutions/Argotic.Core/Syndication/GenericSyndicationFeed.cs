using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;

namespace Argotic.Syndication;

/// <summary>
/// Represents a format agnostic view of a syndication feed.
/// </summary>
/// <seealso cref="AtomFeed"/>
/// <seealso cref="RssFeed"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the GenericSyndicationFeed class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\GenericSyndicationFeedExample.cs" 
///             region="GenericSyndicationFeed" 
///         />
///     </code>
/// </example>
[Serializable]
public class GenericSyndicationFeed
{
    /// <summary>
    /// Private member to hold the underlying syndication resource that is being abstracted by this generic feed.
    /// </summary>
    private ISyndicationResource feedResource;

    /// <summary>
    /// Private member to hold the type of syndication format that the syndication feed implements.
    /// </summary>
    private SyndicationContentFormat feedFormat = SyndicationContentFormat.None;

    /// <summary>
    /// Private member to hold the title of the syndication feed.
    /// </summary>
    private string feedTitle = string.Empty;

    /// <summary>
    /// Private member to hold the description of the syndication feed.
    /// </summary>
    private string feedDescription = string.Empty;

    /// <summary>
    /// Private member to hold a date-time indicating the most recent instant in time when the feed was modified in a way the publisher considers significant.
    /// </summary>
    private DateTime feedLastUpdatedOn = DateTime.MinValue;

    /// <summary>
    /// Private member to hold the natural or formal language in which the feed content is written.
    /// </summary>
    private CultureInfo feedLanguage;

    /// <summary>
    /// Initializes a new instance of the <see cref="GenericSyndicationFeed"/> class.
    /// </summary>
    public GenericSyndicationFeed()
    {
    }

    /// <summary>
    /// Occurs when the generic syndication feed state has been changed by a load operation.
    /// </summary>
    /// <seealso cref="GenericSyndicationFeed.LoadAsync(Uri, CancellationToken)"/>
    public event EventHandler<SyndicationResourceLoadedEventArgs> Loaded;

    /// <summary>
    /// Raises the <see cref="GenericSyndicationFeed.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e)
    {
        this.Loaded?.Invoke(this, e);
    }

    /// <summary>
    /// Gets the categories associated with this feed.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="GenericSyndicationCategory"/> objects that represent the categories associated with this feed.
    /// </value>
    public IList<GenericSyndicationCategory> Categories { get; } = [];

    /// <summary>
    /// Gets character data that provides a human-readable characterization or summary of this feed.
    /// </summary>
    /// <value>
    ///     Character data that provides a human-readable characterization or summary of this feed. 
    ///     The default value is an <b>empty</b> string, which indicates that no description was specified.
    /// </value>
    public string Description => feedDescription;

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication feed implements.
    /// </summary>
    /// <value>The <see cref="SyndicationContentFormat"/> enumeration value that indicates the type of syndication format that this syndication feed implements.</value>
    public SyndicationContentFormat Format => feedFormat;

    /// <summary>
    /// Gets the distinct content published in this feed.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="GenericSyndicationItem"/> objects that represent distinct content published in this feed.
    ///     The default value is an <b>empty</b> collection, which indicates that no discrete content was published in this feed.
    /// </value>
    public IList<GenericSyndicationItem> Items { get; } = [];

    /// <summary>
    /// Gets the natural or formal language in which the feed content is written.
    /// </summary>
    /// <value>
    ///     A <see cref="CultureInfo"/> that represents the natural or formal language in which this feed's content is written. 
    ///     The default value is a <b>null</b> reference, which indicates that no natural or formal language was specified.
    /// </value>
    public CultureInfo Language => feedLanguage;

    /// <summary>
    /// Gets a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> object that represents a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant. 
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that update date was specified.
    /// </value>
    public DateTime LastUpdatedOn => feedLastUpdatedOn;

    /// <summary>
    /// Gets the syndication resource that is being abstracted by this generic feed.
    /// </summary>
    /// <value>
    ///     An object that implements the <see cref="ISyndicationResource"/> interface that represents the actual syndication feed that is being abstracted by this generic feed. 
    ///     The default value is a <b>null</b> reference, which indicates that this generic feed has not been initialized using a syndication resource.
    /// </value>
    public ISyndicationResource Resource => feedResource;

    /// <summary>
    /// Gets character data that provides the name of this feed.
    /// </summary>
    /// <value>
    ///     Character data that provides the name of this feed. 
    ///     The default value is an <b>empty</b> string, which indicates that no title was specified.
    /// </value>
    public string Title => feedTitle;

    /// <summary>
    /// Asynchronously creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code lang="cs" title="The following code example demonstrates the usage of the CreateAsync method.">
    ///         <code
    ///             source="..\..\Argotic.Examples\Core\GenericSyndicationFeedExample.cs"
    ///             region="CreateAsync(Uri source)"
    ///         />
    ///     </code>
    /// </example>
    public static async Task<GenericSyndicationFeed> CreateAsync(Uri source, SyndicationResourceLoadSettings? settings = null, CancellationToken cancellationToken = default)
    {
        GenericSyndicationFeed syndicationResource = new();
        ArgumentNullException.ThrowIfNull(source);
        await syndicationResource.LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, settings, null, cancellationToken).ConfigureAwait(false);

        return syndicationResource;
    }

    /// <summary>
    /// Asynchronously creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    public static async Task<GenericSyndicationFeed> CreateAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        GenericSyndicationFeed syndicationResource = new();
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        await syndicationResource.LoadAsync(source, httpClient, settings, requestOptions, cancellationToken).ConfigureAwait(false);

        return syndicationResource;
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="string"/>.
    /// </summary>
    /// <param name="str">The <b>String</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="str"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="str"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(string str)
    {
        ArgumentNullException.ThrowIfNull(str);
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(str);
        this.Load(navigator, new SyndicationResourceLoadSettings(), new SyndicationResourceLoadedEventArgs(navigator));
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(Stream stream)
    {
        this.Load(stream, null);
    }

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <b>Stream</b> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> data does not conform to a supported syndication content format. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(Stream stream, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(stream);
        XPathNavigator navigator;
        if (settings != null)
        {
            navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream, settings.CharacterEncoding);
        }
        else
        {
            navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream);
        }
        this.Load(navigator, settings ?? new SyndicationResourceLoadSettings(), new SyndicationResourceLoadedEventArgs(navigator));
    }

    /// <summary>
    /// Initializes the generic syndication feed using the supplied <see cref="AtomFeed"/>.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference.</exception>
    public void Parse(AtomFeed feed)
    {
        ArgumentNullException.ThrowIfNull(feed);
        feedResource = feed;
        feedFormat = SyndicationContentFormat.Atom;

        if (feed.Title != null && !string.IsNullOrEmpty(feed.Title.Content))
        {
            feedTitle = feed.Title.Content;
        }

        if (feed.Subtitle != null && !string.IsNullOrEmpty(feed.Title.Content))
        {
            feedDescription = feed.Subtitle.Content;
        }

        if (feed.UpdatedOn != DateTime.MinValue)
        {
            feedLastUpdatedOn = feed.UpdatedOn;
        }

        if (feed.Language != null)
        {
            feedLanguage = feed.Language;
        }

        foreach (AtomCategory category in feed.Categories)
        {
            GenericSyndicationCategory genericCategory = new(category);
            this.Categories.Add(genericCategory);
        }

        foreach (AtomEntry entry in feed.Entries)
        {
            GenericSyndicationItem genericItem = new(entry);
            this.Items.Add(genericItem);
        }
    }

    /// <summary>
    /// Initializes the generic syndication feed using the supplied <see cref="RssFeed"/>.
    /// </summary>
    /// <param name="feed">The <see cref="RssFeed"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is a null reference.</exception>
    public void Parse(RssFeed feed)
    {
        ArgumentNullException.ThrowIfNull(feed);
        feedResource = feed;
        feedFormat = SyndicationContentFormat.Rss;

        if (!string.IsNullOrEmpty(feed.Channel.Title))
        {
            feedTitle = feed.Channel.Title;
        }

        if (!string.IsNullOrEmpty(feed.Channel.Description))
        {
            feedDescription = feed.Channel.Description;
        }

        if (feed.Channel.LastBuildDate != DateTime.MinValue)
        {
            feedLastUpdatedOn = feed.Channel.LastBuildDate;
        }

        if (feed.Channel.Language != null)
        {
            feedLanguage = feed.Channel.Language;
        }

        foreach (RssCategory category in feed.Channel.Categories)
        {
            GenericSyndicationCategory genericCategory = new(category);
            this.Categories.Add(genericCategory);
        }

        foreach (RssItem item in feed.Channel.Items)
        {
            GenericSyndicationItem genericItem = new(item);
            this.Items.Add(genericItem);
        }
    }

    /// <summary>
    /// Initializes the generic syndication feed using the supplied <see cref="OpmlDocument"/>.
    /// Since OpmlDocument hasn't direct mappings to feeds in this method we simply initialize 
    /// feedFormat to Opml and feedResource to opmlDocument
    /// </summary>
    /// <param name="opmlDocument">The <see cref="OpmlDocument"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="opmlDocument"/> is a null reference.</exception>
    public void Parse(OpmlDocument opmlDocument)
    {
        ArgumentNullException.ThrowIfNull(opmlDocument);
        feedResource = opmlDocument;
        feedFormat = SyndicationContentFormat.Opml;
    }

    /// <summary>
    /// Asynchronously loads this <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>The <see cref="GenericSyndicationFeed"/> is loaded using the default <see cref="SyndicationResourceLoadSettings"/>.</para>
    ///     <para>This method uses the shared <see cref="HttpClient"/> from <see cref="SyndicationEncodingUtility.SharedHttpClient"/>.</para>
    ///     <para>After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default)
    {
        return LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);
    }

    /// <summary>
    /// Asynchronously loads this <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <b>null</b>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <b>null</b>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     If <paramref name="settings"/> has a <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> of <see cref="System.Text.Encoding.UTF8"/>
    ///                     the character encoding of the <paramref name="source"/> will be attempted to be determined automatically, Otherwise, the specified character encoding will be used.
    ///                     If automatic detection fails, a character encoding of <see cref="System.Text.Encoding.UTF8"/> is used by default.
    ///                 </description>
    ///             </item>
    ///             <item>
    ///                 <description>
    ///                     After the load operation has successfully completed, the <see cref="Loaded"/> event will be raised.
    ///                 </description>
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is a null reference.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> data does not conform to the expected syndication content format. In this case, the feed remains empty.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(settings.Timeout);

        System.Text.Encoding? encoding = settings.CharacterEncoding == System.Text.Encoding.UTF8 ? null : settings.CharacterEncoding;
        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(source, httpClient, encoding, requestOptions, timeoutCts.Token).ConfigureAwait(false);

        this.Load(navigator, settings, new SyndicationResourceLoadedEventArgs(navigator, source));
    }

    /// <summary>
    /// Loads the generic syndication feed using the specified <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="navigator">A read-only <see cref="XPathNavigator"/> object for navigating through the syndication resource information.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the load operation of the <see cref="GenericSyndicationFeed"/>.</param>
    /// <param name="eventData">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data used when raising the <see cref="GenericSyndicationFeed.Loaded"/> event.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event is raised using the specified <paramref name="eventData"/>.
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
        SyndicationResourceMetadata metadata = new(navigator);

        if (metadata.Format == SyndicationContentFormat.Atom)
        {
            AtomFeed feed = new();
            SyndicationResourceAdapter adapter = new(navigator, settings);
            adapter.Fill(feed, SyndicationContentFormat.Atom);

            this.Parse(feed);
        }
        else if (metadata.Format == SyndicationContentFormat.Rss)
        {
            RssFeed feed = new();
            SyndicationResourceAdapter adapter = new(navigator, settings);
            adapter.Fill(feed, SyndicationContentFormat.Rss);

            this.Parse(feed);
        }
        else if (metadata.Format == SyndicationContentFormat.Opml)
        {
            OpmlDocument opmlDoc = new();
            SyndicationResourceAdapter adapter = new(navigator, settings);
            adapter.Fill(opmlDoc, SyndicationContentFormat.Opml);

            this.Parse(opmlDoc);
        }
        this.OnFeedLoaded(eventData);
    }
}