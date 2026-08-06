using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents information about the meta-data and contents associated to an <see cref="RssFeed"/>.
/// </summary>
/// <seealso cref="RssFeed"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssChannel class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssChannelExample.cs" 
///             region="RssChannel" 
///         />
///     </code>
/// </example>
public class RssChannel : IComparable<RssChannel>, IEquatable<RssChannel>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="RssChannel"/> class.
    /// </summary>
    public RssChannel()
    {


    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssChannel"/> class using the supplied link, title, and description.
    /// </summary>
    /// <param name="link">A <see cref="Uri"/> that represents the URL of the website associated with this feed.</param>
    /// <param name="title">Character data that provides the name of this feed.</param>
    /// <param name="description">Character data that provides a human-readable characterization or summary of this feed.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="link"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is an empty string.</exception>
    public RssChannel(Uri link, string title, string description)
    {
        this.Link = link;
        this.Title = title;
        this.Description = description;
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
    /// Gets the categories or tags to which this channel belongs.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="RssCategory"/> objects that represent the categories to which this channel belongs. The default value is an <i>empty</i> collection.
    /// </value>
    public IList<RssCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets the meta-data clients can use to register to be notified of updates to this feed.
    /// </summary>
    /// <value>
    ///     A <see cref="RssCloud"/> object that represents the meta-data clients can use for monitoring 
    ///     updates to this feed using a web service that implements the RssCloud application programming interface. 
    ///     The default value is a <b>null</b> reference.
    /// </value>
    public RssCloud? Cloud { get; set; }

    /// <summary>
    /// Gets or sets the human-readable copyright statement that applies to this feed.
    /// </summary>
    /// <value>The human-readable copyright statement that applies to this feed.</value>
    /// <remarks>
    ///     When a feed lacks a copyright element, aggregators <i>should not</i> assume that is in the public domain and can be republished and redistributed without restriction.
    /// </remarks>
    public string Copyright
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets character data that provides a human-readable characterization or summary of this feed.
    /// </summary>
    /// <value>Character data that provides a human-readable characterization or summary of this feed.</value>
    /// <remarks>
    ///     The description character data <b>must</b> be suitable for presentation as HTML.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Description
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets the URL of the RSS specification implemented by the software that created this feed.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the RSS specification implemented by the software that created this feed.</value>
    public static Uri Documentation { get; } = new("http://www.rssboard.org/rss-specification");

    /// <summary>
    /// Gets or sets a value that credits the software that created this feed.
    /// </summary>
    /// <value>A value that credits the software that created this feed. The default value is an agent that describes this syndication framework.</value>
    public string Generator
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = $"Argotic Syndication Framework {System.Reflection.Assembly.GetAssembly(typeof(RssChannel))?.GetName().Version?.ToString(4) ?? "unknown"}, https://github.com/argotic-syndication-framework/argotic/";

    /// <summary>
    /// Gets or sets the graphical logo for this feed.
    /// </summary>
    /// <value>
    ///     A <see cref="RssImage"/> object that represents the graphical logo for this feed. The default value is a <b>null</b> reference.
    /// </value>
    public RssImage? Image { get; set; }

    /// <summary>
    /// Gets the distinct content published in this feed.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="RssItem"/> objects that represent distinct content published in this feed.</value>
    public IList<RssItem> Items { get; } = [];

    /// <summary>
    /// Gets or sets the natural language employed in this feed.
    /// </summary>
    /// <value>A <see cref="CultureInfo"/> object that represents the natural language employed in this feed. The default value is a <b>null</b> reference.</value>
    /// <remarks>
    ///     The language <b>must</b> be identified using one of the <a href="http://www.rssboard.org/rss-language-codes">RSS language codes</a> 
    ///     or a <a href="http://www.w3.org/TR/REC-html40/struct/dirlang.html#langcodes">W3C language code</a>.
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets or sets the last date and time the content of this feed was updated.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> object that represents the last date and time the content of this feed was updated. 
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no last build date was specified.
    /// </value>
    public DateTime LastBuildDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the URL of the website associated with this feed.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the website associated with this feed.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri? Link
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the e-mail address of the person to contact regarding the editorial content of this feed.
    /// </summary>
    /// <value>The e-mail address of the person to contact regarding the editorial content of this feed.</value>
    /// <remarks>
    ///     <para>
    ///         There is no requirement to follow a specific format for email addresses. Publishers can format addresses according to the RFC 2822 Address Specification,
    ///         the RFC 2368 guidelines for mailto links, or some other scheme. The recommended format for e-mail addresses is <i>username@hostname.tld (Real Name)</i>.
    ///     </para>
    /// </remarks>
    public string ManagingEditor
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the publication date and time of this feed's content.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> object that represents the publication date and time of this feed's content. 
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no publication date was specified.
    /// </value>
    /// <remarks>
    ///     Publishers of daily, weekly or monthly periodicals can use this element to associate feed items with the date they most recently went to press.
    /// </remarks>
    public DateTime PublicationDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets an advisory label for the content in this feed.
    /// </summary>
    /// <value>A string value, formatted according to the specification for the Platform for Internet Content Selection (PICS), that supplies an advisory label for the content in this feed.</value>
    /// <remarks>
    ///     <para>
    ///         For further information on the <b>Platform for Internet Content Selection (PICS)</b> advisory label formatting specification,
    ///         see <a href="http://www.w3.org/TR/REC-PICS-labels#General">http://www.w3.org/TR/REC-PICS-labels#General</a>.
    ///     </para>
    /// </remarks>
    public string Rating
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a URL that describes the feed itself.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a URL that points to where this feed can be retrieved from.</value>
    /// <remarks>
    ///     <para>
    ///         Identifying a feed's URL within the feed makes it more portable, self-contained, and easier to cache. 
    ///         For these reasons, a feed <i>should</i> provide a value for <see cref="SelfLink"/> that is used for this purpose.
    ///     </para>
    ///     <para>
    ///         Identifying a self-referential link is achieved by including an <i>atom:link</i> element within the channel. 
    ///         See <a href="http://www.rssboard.org/rss-profile#namespace-elements-atom-link">RSS Profile</a> for more information.
    ///     </para>
    /// </remarks>
    public Uri? SelfLink { get; set; }

    /// <summary>
    /// Gets the days of the week during which this feed is not updated.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="DayOfWeek"/> enumeration values that indicate the days of the week during which this feed is not updated.</value>
    /// <remarks>
    ///     <see cref="DayOfWeek"/> enumeration values within this collection <b>must not</b> be duplicated.
    /// </remarks>
    public IList<DayOfWeek> SkipDays { get; } = [];

    /// <summary>
    /// Gets the hours of the day during which this feed is not updated.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="Int32"/> objects that indicate the hours of the day during which this feed is not updated.</value>
    /// <remarks>
    ///     Values from 0 to 23 are permitted, with 0 representing midnight. Integer values within this collection <b>must not</b> be duplicated.
    /// </remarks>
    public IList<int> SkipHours { get; } = [];

    /// <summary>
    /// Gets or sets a form to submit a text query to this feed's publisher over the Common Gateway Interface (CGI).
    /// </summary>
    /// <value>
    ///     A <see cref="TextInput"/> object that represents a form to submit a text query to this feed's publisher over the Common Gateway Interface (CGI). 
    ///     The default value is a <b>null</b> reference.
    /// </value>
    public RssTextInput? TextInput { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of minutes to cache the data before a client should request it again.
    /// </summary>
    /// <value>
    ///     The maximum number of minutes to cache the data before an aggregator should request it again. 
    ///     The default value is <see cref="Int32.MinValue"/>, which indicates no time-to-live was specified.
    /// </value>
    /// <remarks>
    ///     Aggregators that support this property <i>should</i> treat it as a publisher's suggestion of a feed's update frequency, not a hard rule.
    /// </remarks>
    public int TimeToLive { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets character data that provides the name of this feed.
    /// </summary>
    /// <value>Character data that provides the name of this feed.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Title
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the e-mail address of the person to contact about technical issues regarding this feed.
    /// </summary>
    /// <value>The e-mail address of the person to contact about technical issues regarding this feed.</value>
    /// <remarks>
    ///     <para>
    ///         There is no requirement to follow a specific format for email addresses. Publishers can format addresses according to the RFC 2822 Address Specification,
    ///         the RFC 2368 guidelines for mailto links, or some other scheme. The recommended format for e-mail addresses is <i>username@hostname.tld (Real Name)</i>.
    ///     </para>
    /// </remarks>
    public string Webmaster
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source) => this.Load(source, new SyndicationResourceLoadSettings());

    /// <summary>
    /// Loads this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = new(source.NameTable);
        manager.AddNamespace("atom", "http://www.w3.org/2005/Atom");
        XPathNavigator? descriptionNavigator = source.SelectChildElement("description");
        XPathNavigator? linkNavigator = source.SelectChildElement("link");
        XPathNavigator? titleNavigator = source.SelectChildElement("title");

        if (descriptionNavigator is not null && !string.IsNullOrEmpty(descriptionNavigator.Value))
        {
            this.Description = descriptionNavigator.Value;
            wasLoaded = true;
        }

        if (linkNavigator is not null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? link))
            {
                this.Link = link;
                wasLoaded = true;
            }
        }

        if (titleNavigator is not null && !string.IsNullOrEmpty(titleNavigator.Value))
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }
        if (this.LoadOptionals(source, manager, settings))
        {
            wasLoaded = true;
        }

        if (this.LoadCollections(source, manager, settings))
        {
            wasLoaded = true;
        }


        if (this.LoadProfile(source, manager, settings))
        {
            wasLoaded = true;
        }

        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="RssChannel"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("channel");
        writer.WriteElementString("title", this.Title);
        writer.WriteElementString("link", this.Link?.ToString() ?? string.Empty);
        writer.WriteElementString("description", this.Description);

        this.Cloud?.WriteTo(writer);

        if (!string.IsNullOrEmpty(this.Copyright))
        {
            writer.WriteElementString("copyright", this.Copyright);
        }

        writer.WriteElementString("docs", RssChannel.Documentation.ToString());

        if (!string.IsNullOrEmpty(this.Generator))
        {
            writer.WriteElementString("generator", this.Generator);
        }

        this.Image?.WriteTo(writer);

        if (this.Language is not null)
        {
            writer.WriteElementString("language", this.Language.Name);
        }

        if (this.LastBuildDate != DateTime.MinValue)
        {
            writer.WriteElementString("lastBuildDate", SyndicationDateTimeUtility.ToRfc822DateTime(this.LastBuildDate));
        }

        if (!string.IsNullOrEmpty(this.ManagingEditor))
        {
            writer.WriteElementString("managingEditor", this.ManagingEditor);
        }

        if (this.PublicationDate != DateTime.MinValue)
        {
            writer.WriteElementString("pubDate", SyndicationDateTimeUtility.ToRfc822DateTime(this.PublicationDate));
        }

        if (!string.IsNullOrEmpty(this.Rating))
        {
            writer.WriteElementString("rating", this.Rating);
        }

        this.TextInput?.WriteTo(writer);

        if (this.TimeToLive != int.MinValue)
        {
            writer.WriteElementString("ttl", this.TimeToLive.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (!string.IsNullOrEmpty(this.Webmaster))
        {
            writer.WriteElementString("webMaster", this.Webmaster);
        }

        if (this.SkipDays.Count > 0)
        {
            writer.WriteStartElement("skipDays");
            foreach (DayOfWeek day in this.SkipDays)
            {
                writer.WriteElementString("day", day.ToString());
            }
            writer.WriteEndElement();
        }

        if (this.SkipHours.Count > 0)
        {
            writer.WriteStartElement("skipHours");
            foreach (int hour in this.SkipHours)
            {
                writer.WriteElementString("hour", hour.ToString(NumberFormatInfo.InvariantInfo));
            }
            writer.WriteEndElement();
        }

        foreach (RssCategory category in this.Categories)
        {
            category.WriteTo(writer);
        }

        if (this.SelfLink is not null)
        {
            writer.WriteStartElement("link", "http://www.w3.org/2005/Atom");
            writer.WriteAttributeString("href", this.SelfLink.ToString());
            writer.WriteAttributeString("rel", "self");
            writer.WriteAttributeString("type", "application/rss+xml");
            writer.WriteEndElement();
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        foreach (RssItem item in this.Items)
        {
            item.WriteTo(writer);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Loads the collection elements of this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     <para>
    ///         This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
    ///     </para>
    ///     <para>
    ///         The number of <see cref="RssChannel.Items"/> that are loaded is limited based on the <see cref="SyndicationResourceLoadSettings.RetrievalLimit"/>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    private bool LoadCollections(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);
        XPathNodeIterator categoryIterator = source.SelectChildElements("category");
        XPathNodeIterator skipDaysIterator = source.Select("skipDays/day", manager);
        XPathNodeIterator skipHoursIterator = source.Select("skipHours/hour", manager);
        XPathNodeIterator itemIterator = source.SelectChildElements("item");

        if (categoryIterator is { Count: > 0 })
        {
            while (categoryIterator.MoveNext())
            {
                XPathNavigator? categoryNode = categoryIterator.Current;
                if (categoryNode is null)
                {
                    continue;
                }

                RssCategory category = new();
                if (category.Load(categoryNode, settings))
                {
                    this.Categories.Add(category);
                    wasLoaded = true;
                }
            }
        }

        if (skipDaysIterator is { Count: > 0 })
        {
            while (skipDaysIterator.MoveNext())
            {
                XPathNavigator? skipDaysNode = skipDaysIterator.Current;
                if (skipDaysNode is null)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(skipDaysNode.Value))
                {
                    try
                    {
                        DayOfWeek day = Enum.Parse<DayOfWeek>(skipDaysNode.Value, true);
                        if (!this.SkipDays.Contains(day))
                        {
                            this.SkipDays.Add(day);
                            wasLoaded = true;
                        }
                    }
                    catch (ArgumentException)
                    {
                        System.Diagnostics.Trace.TraceWarning("RssChannel unable to determine DayOfWeek with a name of {0}.", skipDaysNode.Value);
                    }
                }
            }
        }

        if (skipHoursIterator is { Count: > 0 })
        {
            while (skipHoursIterator.MoveNext())
            {
                XPathNavigator? skipHoursNode = skipHoursIterator.Current;
                if (skipHoursNode is null)
                {
                    continue;
                }

                if (int.TryParse(skipHoursNode.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int hour))
                {
                    if (!this.SkipHours.Contains(hour) && hour is >= 0 and <= 23)
                    {
                        this.SkipHours.Add(hour);
                        wasLoaded = true;
                    }
                    else
                    {
                        System.Diagnostics.Trace.TraceWarning("RssChannel unable to add duplicate or out-of-range skip hour with a value of {0}.", hour);
                    }
                }
            }
        }

        if (itemIterator is { Count: > 0 })
        {
            int counter = 0;
            while (itemIterator.MoveNext())
            {
                XPathNavigator? itemNode = itemIterator.Current;
                if (itemNode is null)
                {
                    continue;
                }

                RssItem item = new();
                counter++;

                if (item.Load(itemNode, settings))
                {
                    if (settings.RetrievalLimit != 0 && counter > settings.RetrievalLimit)
                    {
                        break;
                    }

                    this.Items.Add(item);
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads the optional elements of this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);
        XPathNavigator? cloudNavigator = source.SelectChildElement("cloud");
        XPathNavigator? copyrightNavigator = source.SelectChildElement("copyright");
        XPathNavigator? generatorNavigator = source.SelectChildElement("generator");
        XPathNavigator? imageNavigator = source.SelectChildElement("image");
        XPathNavigator? languageNavigator = source.SelectChildElement("language");
        XPathNavigator? lastBuildDateNavigator = source.SelectChildElement("lastBuildDate");
        XPathNavigator? managingEditorNavigator = source.SelectChildElement("managingEditor");
        XPathNavigator? publicationNavigator = source.SelectChildElement("pubDate");
        XPathNavigator? ratingNavigator = source.SelectChildElement("rating");
        XPathNavigator? textInputNavigator = source.SelectChildElement("textInput");
        XPathNavigator? timeToLiveNavigator = source.SelectChildElement("ttl");
        XPathNavigator? webMasterNavigator = source.SelectChildElement("webMaster");

        if (cloudNavigator is not null)
        {
            RssCloud cloud = new();
            if (cloud.Load(cloudNavigator, settings))
            {
                this.Cloud = cloud;
                wasLoaded = true;
            }
        }

        if (copyrightNavigator is not null)
        {
            this.Copyright = copyrightNavigator.Value;
            wasLoaded = true;
        }

        if (generatorNavigator is not null)
        {
            this.Generator = generatorNavigator.Value;
            wasLoaded = true;
        }

        if (imageNavigator is not null)
        {
            RssImage image = new();
            if (image.Load(imageNavigator, settings))
            {
                this.Image = image;
                wasLoaded = true;
            }
        }

        if (languageNavigator is not null && !string.IsNullOrEmpty(languageNavigator.Value))
        {
            try
            {
                CultureInfo language = new(languageNavigator.Value);
                this.Language = language;
                wasLoaded = true;
            }
            catch (ArgumentException)
            {
                System.Diagnostics.Trace.TraceWarning("RssChannel unable to determine CultureInfo with a name of {0}.", languageNavigator.Value);
            }
        }

        if (lastBuildDateNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(lastBuildDateNavigator.Value, out DateTime lastBuildDate))
            {
                this.LastBuildDate = lastBuildDate;
                wasLoaded = true;
            }
        }

        if (managingEditorNavigator is not null)
        {
            this.ManagingEditor = managingEditorNavigator.Value;
            wasLoaded = true;
        }

        if (publicationNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out DateTime publicationDate))
            {
                this.PublicationDate = publicationDate;
                wasLoaded = true;
            }
        }

        if (ratingNavigator is not null)
        {
            this.Rating = ratingNavigator.Value;
            wasLoaded = true;
        }

        if (textInputNavigator is not null)
        {
            RssTextInput textInput = new();
            if (textInput.Load(textInputNavigator, settings))
            {
                this.TextInput = textInput;
                wasLoaded = true;
            }
        }

        if (timeToLiveNavigator is not null)
        {
            if (int.TryParse(timeToLiveNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int timeToLive))
            {
                this.TimeToLive = timeToLive;
                wasLoaded = true;
            }
        }

        if (webMasterNavigator is not null)
        {
            this.Webmaster = webMasterNavigator.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads the optional RSS Profile elements of this <see cref="RssChannel"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve namespace prefixes.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssChannel"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssChannel"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private bool LoadProfile(XPathNavigator source, XmlNamespaceManager manager, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(settings);
        XPathNodeIterator atomLinkIterator = source.SelectChildElements("atom", "link", manager);

        if (atomLinkIterator is { Count: > 0 })
        {
            while (atomLinkIterator.MoveNext())
            {
                XPathNavigator? atomLinkNode = atomLinkIterator.Current;
                if (atomLinkNode is null)
                {
                    continue;
                }

                if (atomLinkNode.HasAttributes)
                {
                    string relAttribute = atomLinkNode.GetAttribute("rel", string.Empty);
                    if (string.Equals(relAttribute, "self", StringComparison.OrdinalIgnoreCase))
                    {
                        string hrefAttribute = atomLinkNode.GetAttribute("href", string.Empty);
                        if (!string.IsNullOrEmpty(hrefAttribute))
                        {
                            if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out Uri? atomLink))
                            {
                                this.SelfLink = atomLink;
                                wasLoaded = true;
                            }
                        }
                        break;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RssChannel"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssChannel"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssChannel"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssChannel? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Copyright, other.Copyright, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Generator, other.Generator, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.LastBuildDate.CompareTo(other.LastBuildDate);
        if (result == 0) result = Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.ManagingEditor, other.ManagingEditor, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.PublicationDate.CompareTo(other.PublicationDate);
        if (result == 0) result = string.Compare(this.Rating, other.Rating, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.TimeToLive.CompareTo(other.TimeToLive);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Webmaster, other.Webmaster, StringComparison.OrdinalIgnoreCase);

        if (this.Cloud is not null)
        {
            if (result == 0) result = this.Cloud.CompareTo(other.Cloud);
        }
        else if (other.Cloud is not null)
        {
            if (result == 0) result = -1;
        }

        if (this.Image is not null)
        {
            if (result == 0) result = this.Image.CompareTo(other.Image);
        }
        else if (other.Image is not null)
        {
            if (result == 0) result = -1;
        }

        if (result == 0) result = (this.Language, other.Language) switch
        {
            (CultureInfo language, CultureInfo otherLanguage) => string.Compare(language.Name, otherLanguage.Name, StringComparison.OrdinalIgnoreCase),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (this.TextInput is not null)
        {
            if (result == 0) result = this.TextInput.CompareTo(other.TextInput);
        }
        else if (other.TextInput is not null)
        {
            if (result == 0) result = -1;
        }

        if (result == 0) result = ComparisonUtility.CompareSequence(this.Categories, other.Categories);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Items, other.Items);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.SkipDays, other.SkipDays);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.SkipHours, other.SkipHours);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssChannel"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssChannel"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="RssChannel"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(RssChannel? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.CompareTo(other) == 0;
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is RssChannel other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(this.Copyright, StringComparer.OrdinalIgnoreCase);
        hash.Add(this.Description, StringComparer.OrdinalIgnoreCase);
        hash.Add(this.Generator, StringComparer.OrdinalIgnoreCase);
        hash.Add(HashCodeUtility.Component(this.LastBuildDate));
        hash.Add(HashCodeUtility.Component(this.Link));
        hash.Add(this.ManagingEditor, StringComparer.OrdinalIgnoreCase);
        hash.Add(HashCodeUtility.Component(this.PublicationDate));
        hash.Add(this.Rating, StringComparer.OrdinalIgnoreCase);
        hash.Add(HashCodeUtility.Component(this.TimeToLive));
        hash.Add(this.Title, StringComparer.OrdinalIgnoreCase);
        hash.Add(this.Webmaster, StringComparer.OrdinalIgnoreCase);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(RssChannel? first, RssChannel? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(RssChannel? first, RssChannel? second) => !(first == second);
}