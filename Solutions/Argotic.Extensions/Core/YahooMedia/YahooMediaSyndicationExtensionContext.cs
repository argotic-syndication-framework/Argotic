using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="YahooMediaSyndicationExtension"/>.
/// </summary>
[Serializable]
public class YahooMediaSyndicationExtensionContext : IYahooMediaCommonObjectEntities
{
    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaSyndicationExtensionContext"/> class.
    /// </summary>
    public YahooMediaSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets the publishable media objects.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="YahooMediaContent"/> objects that represent publishable media objects.</value>
    /// <remarks>
    ///     The sequence of <see cref="YahooMediaContent"/> objects within a syndication entity implies the order of presentation.
    /// </remarks>
    public IList<YahooMediaContent> Contents { get; } = [];

    /// <summary>
    /// Gets the media objects that are effectively the same content, yet different representations.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="YahooMediaGroup"/> objects that represent effectively the same content, yet different representations.</value>
    /// <remarks>
    ///     Media objects that are not the same content should not be included in the same <see cref="YahooMediaGroup"/>.
    ///     The sequence of <see cref="YahooMediaContent"/> objects within a <see cref="YahooMediaGroup"/> implies the order of presentation.
    /// </remarks>
    public IList<YahooMediaGroup> Groups { get; } = [];

    /// <summary>
    /// Gets a taxonomy that gives an indication of the type of content for this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaCategory"/> objects that represent a taxonomy that gives an indication to the type of content for this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<YahooMediaCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets the copyright information for this syndication entity.
    /// </summary>
    /// <value>A <see cref="YahooMediaCopyright"/> that represents the copyright information for this syndication entity.</value>
    /// <remarks>
    ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
    /// </remarks>
    public YahooMediaCopyright? Copyright { get; set; }

    /// <summary>
    /// Gets the entities that contributed to the creation of this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaCredit"/> objects that represent the entities that contributed to the creation of this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     Current entities can include people, companies, locations, etc. Specific entities can have multiple roles,
    ///     and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
    /// </remarks>
    public IList<YahooMediaCredit> Credits { get; } = [];

    /// <summary>
    /// Gets or sets the description of this syndication entity.
    /// </summary>
    /// <value>A <see cref="YahooMediaTextConstruct"/> that represents a short description of this syndication entity.</value>
    /// <remarks>
    ///     Media object descriptions are typically a sentence in length.
    /// </remarks>
    public YahooMediaTextConstruct? Description { get; set; }

    /// <summary>
    /// Gets the hash digests for this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaHash"/> objects that represent the hash digests for this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     When assigning multiple hashes, each <see cref="YahooMediaHash"/> <b>must</b> have a different <see cref="YahooMediaHash.Algorithm"/>.
    /// </remarks>
    public IList<YahooMediaHash> Hashes { get; } = [];

    /// <summary>
    /// Gets the relevant keywords that describe this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="string"/> objects that represent the relevant keywords that describe this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     Media objects are typically assigned maximum of ten keywords or phrases.
    /// </remarks>
    public IList<string> Keywords { get; } = [];

    /// <summary>
    /// Gets or sets a web browser media player console this syndication entity can be accessed through.
    /// </summary>
    /// <value>A <see cref="YahooMediaPlayer"/> that represents a web browser media player console this syndication entity can be accessed through.</value>
    public YahooMediaPlayer? Player { get; set; }

    /// <summary>
    /// Gets the permissible audiences for this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaRating"/> objects that represent the permissible audiences for this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     If there are no ratings specified, it can be assumed that no restrictions are necessary.
    /// </remarks>
    public IList<YahooMediaRating> Ratings { get; } = [];

    /// <summary>
    /// Gets the restrictions to be placed on aggregators that are rendering this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaRestriction"/> objects that represent restrictions to be placed on aggregators that are rendering this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    public IList<YahooMediaRestriction> Restrictions { get; } = [];

    /// <summary>
    /// Gets the text transcript, closed captioning, or lyrics for this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaText"/> objects that represent text transcript, closed captioning, or lyrics for this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     Many of these <see cref="YahooMediaText"/> objects are permitted to provide a time series of text.
    ///     In such cases, it is encouraged, but not required, that the <see cref="YahooMediaText"/> objects be grouped by language and appear in time sequence order based on the start time.
    ///     <see cref="YahooMediaText"/> objects can have overlapping start and end times.
    /// </remarks>
    public IList<YahooMediaText> TextSeries { get; } = [];

    /// <summary>
    /// Gets the representative images for this syndication entity.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="YahooMediaThumbnail"/> objects that represent images that are representative of this syndication entity.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     If multiple thumbnails are included, and time coding is not at play, it is assumed that the images are in order of importance.
    /// </remarks>
    public IList<YahooMediaThumbnail> Thumbnails { get; } = [];

    /// <summary>
    /// Gets or sets the title of this syndication entity.
    /// </summary>
    /// <value>A <see cref="YahooMediaTextConstruct"/> that represents the title of this syndication entity.</value>
    public YahooMediaTextConstruct? Title { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="YahooMediaSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNodeIterator contentIterator = source.Select("media:content", manager);
            XPathNodeIterator groupIterator = source.Select("media:group", manager);

            if (contentIterator is { Count: > 0 })
            {
                while (contentIterator.MoveNext())
                {
                    XPathNavigator? contentNode = contentIterator.Current;
                    if (contentNode == null)
                    {
                        continue;
                    }

                    YahooMediaContent content = new();
                    if (content.Load(contentNode))
                    {
                        this.Contents.Add(content);
                        wasLoaded = true;
                    }
                }
            }

            if (groupIterator is { Count: > 0 })
            {
                while (groupIterator.MoveNext())
                {
                    XPathNavigator? groupNode = groupIterator.Current;
                    if (groupNode == null)
                    {
                        continue;
                    }

                    YahooMediaGroup group = new();
                    if (group.Load(groupNode))
                    {
                        this.Groups.Add(group);
                        wasLoaded = true;
                    }
                }
            }
        }

        if (YahooMediaUtility.FillCommonObjectEntities(this, source))
        {
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        foreach (YahooMediaContent content in this.Contents)
        {
            content.WriteTo(writer);
        }

        foreach (YahooMediaGroup group in this.Groups)
        {
            group.WriteTo(writer);
        }

        YahooMediaUtility.WriteCommonObjectEntities(this, writer);
    }
}