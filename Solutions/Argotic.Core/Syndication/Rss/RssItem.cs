using System.Xml;
using System.Xml.XPath;
using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents distinct content published in an <see cref="RssFeed"/> such as a news article, weblog entry or some other form of discrete update.
/// </summary>
/// <seealso cref="RssFeed"/>
/// <remarks>
///     Every element of an item is optional bar one constraint: at least one of <see cref="Title"/> and
///     <see cref="Description"/> must be present. Nothing here enforces that, so it is possible to build and
///     save an item that no conforming reader will display.
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rss\RssItemExample.cs" language="cs" title="The following code example demonstrates the usage of the RssItem class." />
/// </example>
public class RssItem : IComparable<RssItem>, IEquatable<RssItem>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RssItem"/> class.
    /// </summary>
    public RssItem()
    {

    }

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
    /// Gets or sets the e-mail address of the person who wrote this item.
    /// </summary>
    /// <value>An e-mail address. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     <para>
    ///         RSS pins no format here. Publishers use the addr-spec of
    ///         <a href="https://www.rfc-editor.org/rfc/rfc2822.html">RFC 2822</a> (now RFC 5322 §3.4.1), the
    ///         mailto conventions of <a href="https://www.rfc-editor.org/rfc/rfc2368.html">RFC 2368</a> (now
    ///         RFC 6068), or something of their own. The recommended shape is
    ///         <c>username@hostname.tld (Real Name)</c> — an address with the display name in parentheses
    ///         after it, not before.
    ///     </para>
    ///     <para>
    ///         A feed written by one person <i>should</i> leave this empty and identify its author once, at
    ///         the channel, through <see cref="RssChannel.ManagingEditor"/> or <see cref="RssChannel.Webmaster"/>.
    ///     </para>
    /// </remarks>
    public string Author
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets the categories or tags to which this item belongs.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> of <see cref="RssCategory"/> objects that represent the categories to which this item belongs. The default value is an <i>empty</i> collection.
    /// </value>
    public IList<RssCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets the URL of a web page that contains comments received in response to this item.
    /// </summary>
    /// <value>The comments page for this item, or <see langword="null"/> if none was specified.</value>
    public Uri? Comments { get; set; }

    /// <summary>
    /// Gets or sets character data that contains this item's full content or a summary of its contents.
    /// </summary>
    /// <value>The item's content, or a summary of it. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     <para>
    ///         This is HTML, and the specification requires the markup to be escaped — either with entities
    ///         (<c>&amp;lt;</c>, <c>&amp;gt;</c>) or inside a <c>CDATA</c> section. The distinction matters on
    ///         the way in as well as out: <c>&amp;lt;p&amp;gt;</c> and <c>&lt;![CDATA[&lt;p&gt;]]&gt;</c> both
    ///         yield the four characters <c>&lt;p&gt;</c> here, and both are correct. Unescaped markup is not
    ///         a description containing HTML, it is a malformed item.
    ///     </para>
    ///     <para>
    ///         Relative URLs do not belong here. RSS has no way to declare a base URL, so a reader has nothing
    ///         to resolve them against beyond guessing at <see cref="RssChannel.Link"/> — which is a guess, and
    ///         one many readers do not make. Absolute URLs, always.
    ///     </para>
    /// </remarks>
    public string Description
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets the media objects associated with this item.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> of <see cref="RssEnclosure"/> objects that represent the media objects such as an audio, video, or executable file that are associated with this item.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     A list because the XML permits repetition, but publish only one. Whether the specification allows
    ///     several is genuinely disputed — the original author intended a single enclosure and never wrote the
    ///     limit down — and readers split accordingly: some present every enclosure, others fetch the first and
    ///     ignore the rest. Podcasting relies on that first-enclosure behaviour.
    /// </remarks>
    public IList<RssEnclosure> Enclosures { get; } = [];

    /// <summary>
    /// Gets or sets the unique identifier for this item.
    /// </summary>
    /// <value>
    ///     A <see cref="RssGuid"/> object that represents the unique identifier for this item. The default value is a <see langword="null"/> reference.
    /// </value>
    /// <remarks>
    ///     A publisher should provide one for every item. It is what lets a reader tell a revised item from a
    ///     new one; without it, an aggregator falls back to comparing links or titles, and an edited headline
    ///     reappears as an unread article. See <see cref="RssGuid.IsPermanentLink"/> for the default that
    ///     catches publishers out.
    /// </remarks>
#pragma warning disable CA1720 // RSS 2.0 names this element <guid>; the property matches the specification
    public RssGuid? Guid { get; set; }
#pragma warning restore CA1720

    /// <summary>
    /// Gets or sets the URL of a web page associated with this item.
    /// </summary>
    /// <value>The page this item points at, or <see langword="null"/> if none was specified.</value>
    public Uri? Link { get; set; }

    /// <summary>
    /// Gets or sets the publication date and time of this item.
    /// </summary>
    /// <value>The default value is <see cref="DateTime.MinValue"/>, the sentinel for "no <c>pubDate</c> was specified".</value>
    /// <remarks>
    ///     <para>
    ///         RSS 2.0 pins this to the date-and-time syntax of
    ///         <a href="https://www.rfc-editor.org/rfc/rfc822.html">RFC 822</a> — "with the exception that the
    ///         year may be expressed with two characters or four characters (four preferred)". That exception
    ///         is why the citation is RFC 822 and not RFC 5322: RFC 5322 forbids two-digit years, so a feed
    ///         written to it would be conforming, and a parser written to it would reject conforming feeds.
    ///     </para>
    ///     <para>
    ///         A future date is an embargo: "If it's a date in the future, aggregators may choose to not
    ///         display the item until that date." Since <i>may</i> is not <i>must</i>, an embargoed item is
    ///         still published — do not use this to hide an item that is not ready.
    ///     </para>
    /// </remarks>
    public DateTime PublicationDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the source feed that this item was republished from.
    /// </summary>
    /// <value>The default value is <see langword="null"/>, meaning the item is original to this feed.</value>
    public RssSource? Source { get; set; }

    /// <summary>
    /// Gets or sets character data that provides this item's headline.
    /// </summary>
    /// <value>The item's headline. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Optional, but only if <see cref="Description"/> carries something; an item with neither is not a
    ///     conforming item.
    /// </remarks>
    public string Title
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Searches for the first syndication extension of the specified type that is attached to this item.
    /// </summary>
    /// <typeparam name="TExtension">The type of <see cref="ISyndicationExtension"/> to search for.</typeparam>
    /// <returns>The first extension in <see cref="Extensions"/> assignable to <typeparamref name="TExtension"/>, or <see langword="null"/> if the item carries none.</returns>
    public TExtension? FindExtension<TExtension>() where TExtension : ISyndicationExtension => this.Extensions.OfType<TExtension>().FirstOrDefault();

    /// <summary>
    /// Loads this <see cref="RssItem"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="RssItem"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssItem"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = new(source.NameTable);
        XPathNavigator? authorNavigator = source.SelectChildElement("author");
        XPathNavigator? commentsNavigator = source.SelectChildElement("comments");
        XPathNavigator? descriptionNavigator = source.SelectChildElement("description");
        XPathNavigator? guidNavigator = source.SelectChildElement("guid");
        XPathNavigator? linkNavigator = source.SelectChildElement("link");
        XPathNavigator? publicationNavigator = source.SelectChildElement("pubDate");
        XPathNavigator? sourceNavigator = source.SelectChildElement("source");
        XPathNavigator? titleNavigator = source.SelectChildElement("title");

        XPathNodeIterator categoryIterator = source.SelectChildElements("category");
        XPathNodeIterator enclosureIterator = source.SelectChildElements("enclosure");
        if (titleNavigator is not null)
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        if (descriptionNavigator is not null)
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

        if (authorNavigator is not null)
        {
            this.Author = authorNavigator.Value;
            wasLoaded = true;
        }

        if (commentsNavigator is not null)
        {
            if (Uri.TryCreate(commentsNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? comments))
            {
                this.Comments = comments;
                wasLoaded = true;
            }
        }

        if (guidNavigator is not null)
        {
            RssGuid guid = new();
            if (guid.Load(guidNavigator))
            {
                this.Guid = guid;
                wasLoaded = true;
            }
        }

        if (publicationNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out DateTime publicationDate))
            {
                this.PublicationDate = publicationDate;
                wasLoaded = true;
            }
        }

        if (sourceNavigator is not null)
        {
            RssSource sourceFeed = new();
            if (sourceFeed.Load(sourceNavigator))
            {
                this.Source = sourceFeed;
                wasLoaded = true;
            }
        }

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
                if (category.Load(categoryNode))
                {
                    this.Categories.Add(category);
                }
            }
        }

        if (enclosureIterator is { Count: > 0 })
        {
            while (enclosureIterator.MoveNext())
            {
                XPathNavigator? enclosureNode = enclosureIterator.Current;
                if (enclosureNode is null)
                {
                    continue;
                }

                RssEnclosure enclosure = new();
                if (enclosure.Load(enclosureNode))
                {
                    this.Enclosures.Add(enclosure);
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssItem"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="RssItem"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssItem"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = new(source.NameTable);
        XPathNavigator? authorNavigator = source.SelectChildElement("author");
        XPathNavigator? commentsNavigator = source.SelectChildElement("comments");
        XPathNavigator? descriptionNavigator = source.SelectChildElement("description");
        XPathNavigator? guidNavigator = source.SelectChildElement("guid");
        XPathNavigator? linkNavigator = source.SelectChildElement("link");
        XPathNavigator? publicationNavigator = source.SelectChildElement("pubDate");
        XPathNavigator? sourceNavigator = source.SelectChildElement("source");
        XPathNavigator? titleNavigator = source.SelectChildElement("title");

        XPathNodeIterator categoryIterator = source.SelectChildElements("category");
        XPathNodeIterator enclosureIterator = source.SelectChildElements("enclosure");
        if (titleNavigator is not null)
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        if (descriptionNavigator is not null)
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
        if (authorNavigator is not null)
        {
            this.Author = authorNavigator.Value;
            wasLoaded = true;
        }

        if (commentsNavigator is not null)
        {
            if (Uri.TryCreate(commentsNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? comments))
            {
                this.Comments = comments;
                wasLoaded = true;
            }
        }

        if (guidNavigator is not null)
        {
            RssGuid guid = new();
            if (guid.Load(guidNavigator, settings))
            {
                this.Guid = guid;
                wasLoaded = true;
            }
        }

        if (publicationNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out DateTime publicationDate))
            {
                this.PublicationDate = publicationDate;
                wasLoaded = true;
            }
        }

        if (sourceNavigator is not null)
        {
            RssSource sourceFeed = new();
            if (sourceFeed.Load(sourceNavigator, settings))
            {
                this.Source = sourceFeed;
                wasLoaded = true;
            }
        }
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
                }
            }
        }

        if (enclosureIterator is { Count: > 0 })
        {
            while (enclosureIterator.MoveNext())
            {
                XPathNavigator? enclosureNode = enclosureIterator.Current;
                if (enclosureNode is null)
                {
                    continue;
                }

                RssEnclosure enclosure = new();
                if (enclosure.Load(enclosureNode, settings))
                {
                    this.Enclosures.Add(enclosure);
                }
            }
        }
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="RssItem"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("item");

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteElementString("title", this.Title);
        }

        if (!string.IsNullOrEmpty(this.Description))
        {
            writer.WriteElementString("description", this.Description);
        }

        if (this.Link is not null)
        {
            writer.WriteElementString("link", this.Link.ToString());
        }

        if (!string.IsNullOrEmpty(this.Author))
        {
            writer.WriteElementString("author", this.Author);
        }

        if (this.Comments is not null)
        {
            writer.WriteElementString("comments", this.Comments.ToString());
        }

        this.Guid?.WriteTo(writer);

        if (this.PublicationDate != DateTime.MinValue)
        {
            writer.WriteElementString("pubDate", SyndicationDateTimeUtility.ToRfc822DateTime(this.PublicationDate));
        }

        this.Source?.WriteTo(writer);

        foreach (RssCategory category in this.Categories)
        {
            category.WriteTo(writer);
        }

        foreach (RssEnclosure enclosure in this.Enclosures)
        {
            enclosure.WriteTo(writer);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RssItem"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssItem"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssItem"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssItem? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Author, other.Author, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Comments, other.Comments, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.PublicationDate.CompareTo(other.PublicationDate);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        if (this.Guid is not null)
        {
            if (result == 0) result = this.Guid.CompareTo(other.Guid);
        }
        else if (other.Guid is not null)
        {
            if (result == 0) result = -1;
        }

        if (this.Source is not null)
        {
            if (result == 0) result = this.Source.CompareTo(other.Source);
        }
        else if (other.Source is not null)
        {
            if (result == 0) result = -1;
        }

        if (result == 0) result = ComparisonUtility.CompareSequence(this.Categories, other.Categories);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Enclosures, other.Enclosures);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssItem"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssItem"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RssItem"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(RssItem? other)
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is RssItem other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(this.Author, StringComparer.OrdinalIgnoreCase);
        hash.Add(HashCodeUtility.Component(this.Comments));
        hash.Add(this.Description, StringComparer.OrdinalIgnoreCase);
        hash.Add(HashCodeUtility.Component(this.Link));
        hash.Add(HashCodeUtility.Component(this.PublicationDate));
        hash.Add(this.Title, StringComparer.OrdinalIgnoreCase);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(RssItem? first, RssItem? second)
    {
        if (first is null) return second is null;
        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(RssItem? first, RssItem? second) => !(first == second);
}