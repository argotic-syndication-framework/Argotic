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
[Serializable]
public class RssItem : IComparable<RssItem>, IEquatable<RssItem>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Private member to hold the URL of a web page that contains comments received in response to the item.
    /// </summary>
    private Uri itemComments;
    /// <summary>
    /// Private member to hold the unique identifier for the item.
    /// </summary>
    private RssGuid itemGuid;
    /// <summary>
    /// Private member to hold the URL of a web page associated with the item.
    /// </summary>
    private Uri itemLink;
    /// <summary>
    /// Private member to hold the publication date and time of the item.
    /// </summary>
    private DateTime itemPublicationDate = DateTime.MinValue;
    /// <summary>
    /// Private member to hold information about the source feed that the item was republished from.
    /// </summary>
    private RssSource itemSource;
    /// <summary>
    /// Private member to hold character data that provides the item's headline.
    /// </summary>
    private string itemTitle = string.Empty;
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
        get => field;
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
    public Uri Comments
    {
        get => itemComments;
        set => itemComments = value;
    }

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
        get => field;
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
#pragma warning disable CA1720
    public RssGuid Guid
#pragma warning restore CA1720
    {
        get => itemGuid;
        set => itemGuid = value;
    }

    /// <summary>
    /// Gets or sets the URL of a web page associated with this item.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of a web page associated with this item.</value>
    public Uri Link
    {
        get => itemLink;
        set => itemLink = value;
    }

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
    public DateTime PublicationDate
    {
        get => itemPublicationDate;
        set => itemPublicationDate = value;
    }

    /// <summary>
    /// Gets or sets the source feed that this item was republished from.
    /// </summary>
    /// <value>
    ///     A <see cref="RssSource"/> object that represents the source feed that this item was republished from. The default value is a <b>null</b> reference.
    /// </value>
    public RssSource Source
    {
        get => itemSource;
        set => itemSource = value;
    }

    /// <summary>
    /// Gets or sets character data that provides this item's headline.
    /// </summary>
    /// <value>Character data that provides this item's headline.</value>
    /// <remarks>
    ///     This property is optional if the item contains a <see cref="RssItem.Description"/>.
    /// </remarks>
    public string Title
    {
        get => itemTitle;
        set => itemTitle = value?.Trim() ?? string.Empty;
    }
    public TExtension? FindExtension<TExtension>() where TExtension : ISyndicationExtension
    {
        return this.Extensions.OfType<TExtension>().FirstOrDefault();
    }

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
        XPathNavigator authorNavigator = source.SelectSingleNode("author", manager);
        XPathNavigator commentsNavigator = source.SelectSingleNode("comments", manager);
        XPathNavigator descriptionNavigator = source.SelectSingleNode("description", manager);
        XPathNavigator guidNavigator = source.SelectSingleNode("guid", manager);
        XPathNavigator linkNavigator = source.SelectSingleNode("link", manager);
        XPathNavigator publicationNavigator = source.SelectSingleNode("pubDate", manager);
        XPathNavigator sourceNavigator = source.SelectSingleNode("source", manager);
        XPathNavigator titleNavigator = source.SelectSingleNode("title", manager);

        XPathNodeIterator categoryIterator = source.Select("category", manager);
        XPathNodeIterator enclosureIterator = source.Select("enclosure", manager);
        if (titleNavigator != null)
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        if (descriptionNavigator != null)
        {
            this.Description = descriptionNavigator.Value;
            wasLoaded = true;
        }

        if (linkNavigator != null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri link))
            {
                this.Link = link;
                wasLoaded = true;
            }
        }

        if (authorNavigator != null)
        {
            this.Author = authorNavigator.Value;
            wasLoaded = true;
        }

        if (commentsNavigator != null)
        {
            if (Uri.TryCreate(commentsNavigator.Value, UriKind.RelativeOrAbsolute, out Uri comments))
            {
                this.Comments = comments;
                wasLoaded = true;
            }
        }

        if (guidNavigator != null)
        {
            RssGuid guid = new();
            if (guid.Load(guidNavigator))
            {
                this.Guid = guid;
                wasLoaded = true;
            }
        }

        if (publicationNavigator != null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out DateTime publicationDate))
            {
                this.PublicationDate = publicationDate;
                wasLoaded = true;
            }
        }

        if (sourceNavigator != null)
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
                RssCategory category = new();
                if (category.Load(categoryIterator.Current))
                {
                    this.Categories.Add(category);
                }
            }
        }

        if (enclosureIterator is { Count: > 0 })
        {
            while (enclosureIterator.MoveNext())
            {
                RssEnclosure enclosure = new();
                if (enclosure.Load(enclosureIterator.Current))
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
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = new(source.NameTable);
        XPathNavigator authorNavigator = source.SelectSingleNode("author", manager);
        XPathNavigator commentsNavigator = source.SelectSingleNode("comments", manager);
        XPathNavigator descriptionNavigator = source.SelectSingleNode("description", manager);
        XPathNavigator guidNavigator = source.SelectSingleNode("guid", manager);
        XPathNavigator linkNavigator = source.SelectSingleNode("link", manager);
        XPathNavigator publicationNavigator = source.SelectSingleNode("pubDate", manager);
        XPathNavigator sourceNavigator = source.SelectSingleNode("source", manager);
        XPathNavigator titleNavigator = source.SelectSingleNode("title", manager);

        XPathNodeIterator categoryIterator = source.Select("category", manager);
        XPathNodeIterator enclosureIterator = source.Select("enclosure", manager);
        if (titleNavigator != null)
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        if (descriptionNavigator != null)
        {
            this.Description = descriptionNavigator.Value;
            wasLoaded = true;
        }

        if (linkNavigator != null)
        {
            if (Uri.TryCreate(linkNavigator.Value, UriKind.RelativeOrAbsolute, out Uri link))
            {
                this.Link = link;
                wasLoaded = true;
            }
        }
        if (authorNavigator != null)
        {
            this.Author = authorNavigator.Value;
            wasLoaded = true;
        }

        if (commentsNavigator != null)
        {
            if (Uri.TryCreate(commentsNavigator.Value, UriKind.RelativeOrAbsolute, out Uri comments))
            {
                this.Comments = comments;
                wasLoaded = true;
            }
        }

        if (guidNavigator != null)
        {
            RssGuid guid = new();
            if (guid.Load(guidNavigator, settings))
            {
                this.Guid = guid;
                wasLoaded = true;
            }
        }

        if (publicationNavigator != null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc822DateTime(publicationNavigator.Value, out DateTime publicationDate))
            {
                this.PublicationDate = publicationDate;
                wasLoaded = true;
            }
        }

        if (sourceNavigator != null)
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
                RssCategory category = new();
                if (category.Load(categoryIterator.Current, settings))
                {
                    this.Categories.Add(category);
                }
            }
        }

        if (enclosureIterator is { Count: > 0 })
        {
            while (enclosureIterator.MoveNext())
            {
                RssEnclosure enclosure = new();
                if (enclosure.Load(enclosureIterator.Current, settings))
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

        if (this.Link != null)
        {
            writer.WriteElementString("link", this.Link.ToString());
        }

        if (!string.IsNullOrEmpty(this.Author))
        {
            writer.WriteElementString("author", this.Author);
        }

        if (this.Comments != null)
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
    /// Returns a <see cref="String"/> that represents the current <see cref="RssItem"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="RssItem"/>.</returns>
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
        result |= Uri.Compare(this.Comments, other.Comments, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Link, other.Link, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        result |= this.PublicationDate.CompareTo(other.PublicationDate);
        result |= string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        if (this.Guid != null)
        {
            result |= this.Guid.CompareTo(other.Guid);
        }
        else if (this.Guid == null && other.Guid != null)
        {
            result |= -1;
        }

        if (this.Source != null)
        {
            result |= this.Source.CompareTo(other.Source);
        }
        else if (this.Source == null && other.Source != null)
        {
            result |= -1;
        }cref="string"

        result |= Comparcref="string"ompareSequence(this.Categories, other.Categories);
        result |= ComparisonUtility.CompareSequence(this.Enclosures, other.Enclosures);

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
    public override bool Equals(object? obj)
    {
        return obj is RssItem other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(this.Author, StringComparer.OrdinalIgnoreCase);
        hash.Add(this.Comments);
        hash.Add(this.Description, StringComparer.OrdinalIgnoreCase);
        hash.Add(this.Link);
        hash.Add(this.PublicationDate);
        hash.Add(this.Title, StringComparer.OrdinalIgnoreCase);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(RssItem first, RssItem second)
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
    public static bool operator !=(RssItem first, RssItem second)
    {
        return !(first == second);
    }
}