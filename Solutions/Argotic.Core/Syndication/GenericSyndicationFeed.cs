using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Data.Adapters;

namespace Argotic.Syndication;

/// <summary>
/// Represents a format-agnostic view of a syndication feed.
/// </summary>
/// <remarks>
///     <para>
///         Reach for this when you have a URL and do not know, or do not care, whether it points at RSS
///         or Atom, and all you want is a title, a description, a date, a language and a list of items.
///         It sniffs the format, loads it into the real type, and projects the handful of fields the two
///         have in common. Reach for <see cref="RssFeed"/> or <see cref="AtomFeed"/> instead the moment
///         you need anything else — including whenever you intend to <i>write</i> a feed, which this type
///         cannot do at all.
///     </para>
///     <para>
///         What it flattens away is most of the feed. There is no author, no link, no identifier, no
///         enclosure, no content distinct from the summary, and no syndication extensions — a
///         <c>podcast:transcript</c> or a <c>georss:point</c> is parsed and then simply not surfaced
///         here. It is also lossy where it does project: an Atom entry with no
///         <see cref="AtomEntry.PublishedOn"/> reports its <see cref="AtomEntry.UpdatedOn"/> as
///         <see cref="GenericSyndicationItem.PublishedOn"/>, and an entry with no summary reports its
///         content — so two items that differ on the wire can arrive identical.
///     </para>
///     <para>
///         Nothing is lost permanently. <see cref="Resource"/> holds the fully populated
///         <see cref="RssFeed"/>, <see cref="AtomFeed"/> or <see cref="OpmlDocument"/> that was parsed;
///         cast it and every field, extension and attribute is there. OPML is a third format this type
///         accepts, and for it <see cref="Resource"/> is the <i>only</i> useful output — an OPML document
///         has no items to project, so <see cref="Items"/> and the rest stay empty.
///     </para>
///     <para>
///         Any other format — APML, BlogML, RSD, a sitemap, an Atom Publishing document — raises
///         <see cref="FormatException"/> rather than yielding an empty feed and a <see cref="Loaded"/>
///         event claiming success.
///     </para>
/// </remarks>
/// <seealso cref="AtomFeed"/>
/// <seealso cref="RssFeed"/>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\GenericSyndicationFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the GenericSyndicationFeed class." />
/// </example>
public class GenericSyndicationFeed
{

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
    public event EventHandler<SyndicationResourceLoadedEventArgs>? Loaded;

    /// <summary>
    /// Raises the <see cref="GenericSyndicationFeed.Loaded"/> event.
    /// </summary>
    /// <param name="e">A <see cref="SyndicationResourceLoadedEventArgs"/> that contains the event data.</param>
    protected virtual void OnFeedLoaded(SyndicationResourceLoadedEventArgs e) => this.Loaded?.Invoke(this, e);

    /// <summary>
    /// Gets the categories associated with this feed.
    /// </summary>
    /// <value>
    ///     The feed-level categories: an RSS channel's <c>category</c> elements, or an Atom feed's
    ///     <c>category</c> elements. The default value is an <i>empty</i> collection. Categories on
    ///     individual items are on the items, not here, and an OPML document contributes none.
    /// </value>
    public IList<GenericSyndicationCategory> Categories { get; } = [];

    /// <summary>
    /// Gets character data that provides a human-readable characterization or summary of this feed.
    /// </summary>
    /// <value>
    ///     An RSS channel's <c>description</c>, or an Atom feed's <c>subtitle</c>. The default value is
    ///     an <i>empty</i> string, which indicates that none was specified. The text is whatever the
    ///     publisher wrote, which for RSS may be HTML.
    /// </value>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the <see cref="SyndicationContentFormat"/> that this syndication feed implements.
    /// </summary>
    /// <value>
    ///     The format the loaded document turned out to be. The default value is
    ///     <see cref="SyndicationContentFormat.None"/>, and after a successful load it is one of
    ///     <see cref="SyndicationContentFormat.Rss"/>, <see cref="SyndicationContentFormat.Atom"/> or
    ///     <see cref="SyndicationContentFormat.Opml"/> — no other value is reachable, because a load of
    ///     anything else throws.
    /// </value>
    /// <remarks>
    ///     Test this before casting <see cref="Resource"/>.
    /// </remarks>
    public SyndicationContentFormat Format { get; private set; } = SyndicationContentFormat.None;

    /// <summary>
    /// Gets the distinct content published in this feed.
    /// </summary>
    /// <value>
    ///     An RSS channel's <c>item</c> elements, or an Atom feed's <c>entry</c> elements, in document
    ///     order. The default value is an <i>empty</i> collection — which is also what an OPML document
    ///     yields, since it has no items to project.
    /// </value>
    public IList<GenericSyndicationItem> Items { get; } = [];

    /// <summary>
    /// Gets the natural or formal language in which the feed content is written.
    /// </summary>
    /// <value>
    ///     An RSS channel's <c>language</c>, or an Atom feed's <c>xml:lang</c>, as a
    ///     <see cref="CultureInfo"/>. The default value is <see langword="null"/>, which indicates that
    ///     none was specified — or that what was specified was not a language tag the runtime knows.
    /// </value>
    public CultureInfo? Language { get; private set; }

    /// <summary>
    /// Gets a date-time indicating the most recent instant in time when this feed was modified in a way the publisher considers significant.
    /// </summary>
    /// <value>
    ///     An RSS channel's <c>lastBuildDate</c>, or an Atom feed's <c>updated</c>. The default value is
    ///     <see cref="DateTime.MinValue"/>, which indicates that no such date was specified.
    /// </value>
    /// <remarks>
    ///     RSS <c>pubDate</c> is <i>not</i> consulted, and many feeds carry that and not
    ///     <c>lastBuildDate</c>. A feed reporting <see cref="DateTime.MinValue"/> here may well be dated
    ///     on the wire; read <see cref="Resource"/> as an <see cref="RssFeed"/> if the date matters.
    /// </remarks>
    public DateTime LastUpdatedOn { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Gets the syndication resource that is being abstracted by this generic feed.
    /// </summary>
    /// <value>
    ///     The fully populated <see cref="RssFeed"/>, <see cref="AtomFeed"/> or
    ///     <see cref="OpmlDocument"/> this view was projected from — extensions, links, authors and all.
    ///     The default value is <see langword="null"/>, which indicates that nothing has been loaded.
    /// </value>
    /// <remarks>
    ///     This is the escape hatch, and the reason the flattening above is safe: nothing the projection
    ///     drops is actually gone. Switch on <see cref="Format"/>, then cast.
    /// </remarks>
    public ISyndicationResource? Resource { get; private set; }

    /// <summary>
    /// Gets character data that provides the name of this feed.
    /// </summary>
    /// <value>
    ///     An RSS channel's <c>title</c>, or an Atom feed's <c>title</c>. The default value is an
    ///     <i>empty</i> string, which indicates that none was specified.
    /// </value>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// Asynchronously creates a new <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <remarks>
    ///     Fetches with <see cref="SyndicationEncodingUtility.SharedHttpClient"/>, the process-wide
    ///     singleton. Use the overload taking an <see cref="HttpClient"/> to supply a proxy, credentials
    ///     or a factory-built client.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
    /// <example>
    ///     <code source="..\..\Argotic.Examples\Core\GenericSyndicationFeedExample.cs" language="cs" title="The following code example demonstrates the usage of the CreateAsync method." />
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
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="GenericSyndicationFeed"/> object loaded using the <paramref name="source"/> data.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
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
    /// <param name="str">The XML of the syndication resource, as a string.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="str"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="str"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
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
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(Stream stream) => this.Load(stream, null);

    /// <summary>
    /// Loads the syndication resource from the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> used to load the syndication resource.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <remarks>
    ///     After the load operation has successfully completed, the <see cref="GenericSyndicationFeed.Loaded"/> event will be raised.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="stream"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="stream"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
    /// <exception cref="XmlException">There is a load or parse error in the XML. In this case, the feed remains empty.</exception>
    public void Load(Stream stream, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(stream);
        XPathNavigator navigator = SyndicationEncodingUtility.CreateSafeNavigator(stream, settings);
        this.Load(navigator, settings ?? new SyndicationResourceLoadSettings(), new SyndicationResourceLoadedEventArgs(navigator));
    }

    /// <summary>
    /// Initializes the generic syndication feed using the supplied <see cref="AtomFeed"/>.
    /// </summary>
    /// <param name="feed">The <see cref="AtomFeed"/> to build an abstraction against.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is <see langword="null"/>.</exception>
    public void Parse(AtomFeed feed)
    {
        ArgumentNullException.ThrowIfNull(feed);
        Resource = feed;
        Format = SyndicationContentFormat.Atom;

        if (feed.Title?.Content is { Length: > 0 } title)
        {
            Title = title;
        }

        if (feed.Subtitle?.Content is { Length: > 0 } subtitle)
        {
            Description = subtitle;
        }

        if (feed.UpdatedOn != DateTime.MinValue)
        {
            LastUpdatedOn = feed.UpdatedOn;
        }

        if (feed.Language is not null)
        {
            Language = feed.Language;
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
    /// <exception cref="ArgumentNullException">The <paramref name="feed"/> is <see langword="null"/>.</exception>
    public void Parse(RssFeed feed)
    {
        ArgumentNullException.ThrowIfNull(feed);
        Resource = feed;
        Format = SyndicationContentFormat.Rss;

        if (!string.IsNullOrEmpty(feed.Channel.Title))
        {
            Title = feed.Channel.Title;
        }

        if (!string.IsNullOrEmpty(feed.Channel.Description))
        {
            Description = feed.Channel.Description;
        }

        if (feed.Channel.LastBuildDate != DateTime.MinValue)
        {
            LastUpdatedOn = feed.Channel.LastBuildDate;
        }

        if (feed.Channel.Language is not null)
        {
            Language = feed.Channel.Language;
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
    /// </summary>
    /// <param name="opmlDocument">The <see cref="OpmlDocument"/> to build an abstraction against.</param>
    /// <remarks>
    ///     Sets <see cref="Resource"/> and <see cref="Format"/>, and nothing else — <see cref="Title"/>,
    ///     <see cref="Items"/> and the rest are left at their defaults. An OPML document is an outline of
    ///     subscriptions rather than published content, and its outlines are references to feeds, not
    ///     entries; there is nothing that would map onto <see cref="GenericSyndicationItem"/> without
    ///     inventing a meaning. Read <see cref="Resource"/> as an <see cref="OpmlDocument"/> instead.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="opmlDocument"/> is <see langword="null"/>.</exception>
    public void Parse(OpmlDocument opmlDocument)
    {
        ArgumentNullException.ThrowIfNull(opmlDocument);
        Resource = opmlDocument;
        Format = SyndicationContentFormat.Opml;
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
    public Task LoadAsync(Uri source, CancellationToken cancellationToken = default) => LoadAsync(source, SyndicationEncodingUtility.SharedHttpClient, null, null, cancellationToken);

    /// <summary>
    /// Asynchronously loads this <see cref="GenericSyndicationFeed"/> instance using the specified <see cref="Uri"/> and <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="source">A <see cref="Uri"/> that represents the URL of the syndication resource XML data.</param>
    /// <param name="httpClient">The <see cref="HttpClient"/> to use for the request. The caller is responsible for managing the client's lifecycle.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> object used to configure the <see cref="GenericSyndicationFeed"/> instance. This value can be <see langword="null"/>.</param>
    /// <param name="requestOptions">A <see cref="SyndicationRequestOptions"/> that holds request-level options (headers). This value can be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous load operation.</returns>
    /// <remarks>
    ///     <para>
    ///         <list type="bullet">
    ///             <item>
    ///                 <description>
    ///                     If <paramref name="settings"/> names no <see cref="SyndicationResourceLoadSettings.CharacterEncoding">character encoding</see> — which is
    ///                     the default — the encoding of the <paramref name="source"/> is determined from its byte-order mark or XML declaration, falling
    ///                     back to <see cref="System.Text.Encoding.UTF8"/> if it declares neither. Naming one overrides what the document declares.
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
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="httpClient"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="source"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
    public async Task LoadAsync(Uri source, HttpClient httpClient, SyndicationResourceLoadSettings? settings = null, SyndicationRequestOptions? requestOptions = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(httpClient);
        settings ??= new SyndicationResourceLoadSettings();

        XPathNavigator navigator = await SyndicationEncodingUtility.CreateSafeNavigatorAsync(
            source, httpClient, settings, SyndicationContentLengthLimits.Feed, requestOptions, cancellationToken).ConfigureAwait(false);

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
    /// <exception cref="ArgumentNullException">The <paramref name="navigator"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="eventData"/> is <see langword="null"/>.</exception>
    /// <exception cref="FormatException">The <paramref name="navigator"/> is RSS, Atom or OPML but malformed, or is a syndication format this type does not abstract over. In this case, the feed remains empty.</exception>
    private void Load(XPathNavigator navigator, SyndicationResourceLoadSettings? settings, SyndicationResourceLoadedEventArgs eventData)
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
        else
        {
            // There was no final arm. Every other format -- APML, BlogML, RSD, a sitemap, an Atom
            // Publishing document, a stand-alone Atom entry document -- fell straight through to the
            // Loaded event below, leaving a default-constructed instance and raising an event that
            // said a load had succeeded. This type wraps the three formats it can Parse, and the other
            // nine are not among them.
            throw new FormatException(
                $"The supplied syndication resource has a content format of {metadata.Format}, which {nameof(GenericSyndicationFeed)} does not represent. "
                + "It abstracts over Atom feed, RSS and OPML documents; load any other format through its own type.");
        }

        this.OnFeedLoaded(eventData);
    }
}