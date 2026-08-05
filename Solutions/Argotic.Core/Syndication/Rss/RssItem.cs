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
///     A <see cref="RssItem"/> <b>must</b> contain either a <see cref="RssItem.Title"/> <i>or</i> <see cref="RssItem.Description"/>.
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssItem class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssItemExample.cs" 
///             region="RssItem" 
///         />
///     </code>
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
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets the e-mail address of the person who wrote this item.
    /// </summary>
    /// <value>The e-mail address of the person who wrote this item.</value>
    /// <remarks>
    ///     <para>
    ///         There is no requirement to follow a specific format for email addresses. Publishers can format addresses according to the RFC 2822 Address Specification,
    ///         the RFC 2368 guidelines for mailto links, or some other scheme. The recommended format for e-mail addresses is <i>username@hostname.tld (Real Name)</i>.
    ///     </para>
    ///     <para>
    ///         A feed published by an individual <i>should</i> omit the item <see cref="RssItem.Author">author</see>
    ///         and use the <see cref="RssChannel.ManagingEditor"/> or <see cref="RssChannel.Webmaster"/> channel properties to provide contact information.
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
    /// <value>A <see cref="Uri"/> that represents the URL of a web page that contains comments received in response to this item.</value>
    public Uri? Comments { get; set; }

    /// <summary>
    /// Gets or sets character data that contains this item's full content or a summary of its contents.
    /// </summary>
    /// <value>Character data that contains this item's full content or a summary of its contents.</value>
    /// <remarks>
    ///     <para>The description <i>may</i> be empty if the item specifies a <see cref="RssItem.Title"/>.</para>
    ///     <para>
    ///         The description <b>must</b> be suitable for presentation as HTML.
    ///         HTML markup must be encoded as character data either by employing the <b>HTML entities</b> (&lt; and &gt;) <i>or</i> a <b>CDATA</b> section.
    ///     </para>
    ///     <para>
    ///         The description <i>should not</i> contain relative URLs, because the RSS format does not provide a means to identify the base URL of a document.
    ///         When a relative URL is present, an aggregator <i>may</i> attempt to resolve it to a full URL using the channel's <see cref="RssChannel.Link">link</see> as the base.
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
    ///     <para>
    ///         Support for the enclosure element in RSS software varies significantly because of disagreement over whether the specification permits more than one enclosure per item.
    ///         Although the original author intended to permit no more than one enclosure in each item, this limit is not explicit in the specification.
    ///         For best support in the widest number of aggregators, an item <i>should not</i> contain more than one enclosure.
    ///     </para>
    /// </remarks>
    public IList<RssEnclosure> Enclosures { get; } = [];

    /// <summary>
    /// Gets or sets the unique identifier for this item.
    /// </summary>
    /// <value>
    ///     A <see cref="RssGuid"/> object that represents the unique identifier for this item. The default value is a <b>null</b> reference.
    /// </value>
    /// <remarks>
    ///     A publisher <i>should</i> provide a guid for each item.
    /// </remarks>
#pragma warning disable CA1720 // RSS 2.0 names this element <guid>; the property matches the specification
    public RssGuid? Guid { get; set; }
#pragma warning restore CA1720

    /// <summary>
    /// Gets or sets the URL of a web page associated with this item.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of a web page associated with this item.</value>
    public Uri? Link { get; set; }

    /// <summary>
    /// Gets or sets the publication date and time of this item.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> object that represents the publication date and time of this item. 
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates that no publication date was specified.
    /// </value>
    /// <remarks>
    ///     The specification recommends that aggregators <i>should</i> ignore items with a publication date that occurs in the future, 
    ///     providing a means for publishers to embargo an item until that date. However, it is recommended that publishers <i>should not</i> 
    ///     include items in a feed until they are ready for publication.
    /// </remarks>
    public DateTime PublicationDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the source feed that this item was republished from.
    /// </summary>
    /// <value>
    ///     A <see cref="RssSource"/> object that represents the source feed that this item was republished from. The default value is a <b>null</b> reference.
    /// </value>
    public RssSource? Source { get; set; }

    /// <summary>
    /// Gets or sets character data that provides this item's headline.
    /// </summary>
    /// <value>Character data that provides this item's headline.</value>
    /// <remarks>
    ///     This property is optional if the item contains a <see cref="RssItem.Description"/>.
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
    /// <returns>
    ///     The first extension in <see cref="RssItem.Extensions"/> that is assignable to <typeparamref name="TExtension"/>,
    ///     Otherwise, a <b>null</b> reference if this item has no extension of that type.
    /// </returns>
    public TExtension? FindExtension<TExtension>() where TExtension : ISyndicationExtension => this.Extensions.OfType<TExtension>().FirstOrDefault();

    /// <summary>
    /// Loads this <see cref="RssItem"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="RssItem"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssItem"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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

        XPathNodeIterator categoryIterator = source.Select("category", manager);
        XPathNodeIterator enclosureIterator = source.Select("enclosure", manager);
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
    /// <returns><b>true</b> if the <see cref="RssItem"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssItem"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
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

        XPathNodeIterator categoryIterator = source.Select("category", manager);
        XPathNodeIterator enclosureIterator = source.Select("enclosure", manager);
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
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
    /// <returns><b>true</b> if the specified <see cref="RssItem"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(RssItem? first, RssItem? second) => !(first == second);
}