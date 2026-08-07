using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="YahooMediaSyndicationExtension"/>.
/// </summary>
/// <remarks>
///     This is the item level, the outermost of the three at which Media RSS metadata may appear. It holds what
///     the item itself declared and nothing that came from a <see cref="YahooMediaGroup"/> or a
///     <see cref="YahooMediaContent"/> inside it — which is why a YouTube channel feed, whose media all lives in
///     a group, leaves <see cref="Contents"/> and <see cref="Thumbnails"/> empty here. See
///     <see cref="IYahooMediaCommonObjectEntities"/>.
/// </remarks>
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
    /// <value>
    ///     The media objects hanging directly off the item, in the publisher's order of presentation. The default
    ///     value is an <i>empty</i> collection — including when the item's media is all inside <see cref="Groups"/>.
    /// </value>
    public IList<YahooMediaContent> Contents { get; } = [];

    /// <summary>
    /// Gets the groups of renditions declared on this syndication entity.
    /// </summary>
    /// <value>The groups. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Each <see cref="YahooMediaGroup"/> is <i>one</i> piece of content offered several ways, not several
    ///     pieces of content. Flattening the groups into <see cref="Contents"/> duplicates every item once per
    ///     rendition.
    /// </remarks>
    public IList<YahooMediaGroup> Groups { get; } = [];

    /// <summary>
    /// Gets a taxonomy that gives an indication of the type of content for this syndication entity.
    /// </summary>
    /// <value>The categories declared on the item itself. The default value is an <i>empty</i> collection.</value>
    public IList<YahooMediaCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets the copyright information for this syndication entity.
    /// </summary>
    /// <value>The copyright information, or <see langword="null"/> if the item itself declared none.</value>
    /// <remarks>
    ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
    /// </remarks>
    public YahooMediaCopyright? Copyright { get; set; }

    /// <summary>
    /// Gets the entities that contributed to the creation of this syndication entity.
    /// </summary>
    /// <value>The contributing entities. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     An entity may be a person, a company or a place. One entity may hold several roles and one role may be
    ///     held by several entities; each combination is a separate <see cref="YahooMediaCredit"/>.
    /// </remarks>
    public IList<YahooMediaCredit> Credits { get; } = [];

    /// <summary>
    /// Gets or sets the description of this syndication entity.
    /// </summary>
    /// <value>A sentence or so of description, or <see langword="null"/> if the item itself declared none.</value>
    public YahooMediaTextConstruct? Description { get; set; }

    /// <summary>
    /// Gets the hash digests for this syndication entity.
    /// </summary>
    /// <value>The hash digests. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     The specification allows several only if each carries a different <see cref="YahooMediaHash.Algorithm"/>.
    ///     Nothing here enforces that.
    /// </remarks>
    public IList<YahooMediaHash> Hashes { get; } = [];

    /// <summary>
    /// Gets the relevant keywords that describe this syndication entity.
    /// </summary>
    /// <value>The keywords. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     One <c>media:keywords</c> element carries the lot, comma-separated; this collection is that list split
    ///     apart, and is rejoined with commas on write. The specification suggests a maximum of ten.
    /// </remarks>
    public IList<string> Keywords { get; } = [];

    /// <summary>
    /// Gets or sets a web browser media player console this syndication entity can be accessed through.
    /// </summary>
    /// <value>The player console, or <see langword="null"/> if the item itself declared none.</value>
    public YahooMediaPlayer? Player { get; set; }

    /// <summary>
    /// Gets the permissible audiences for this syndication entity.
    /// </summary>
    /// <value>The ratings. The default value is an <i>empty</i> collection, which means no audience restriction.</value>
    public IList<YahooMediaRating> Ratings { get; } = [];

    /// <summary>
    /// Gets the restrictions to be placed on aggregators that are rendering this syndication entity.
    /// </summary>
    /// <value>The restrictions. The default value is an <i>empty</i> collection.</value>
    public IList<YahooMediaRestriction> Restrictions { get; } = [];

    /// <summary>
    /// Gets the text transcript, closed captioning, or lyrics for this syndication entity.
    /// </summary>
    /// <value>The text fragments. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Several of these together form a time series — captions, say. Grouping them by language and ordering
    ///     them by start time is encouraged rather than required, and their time ranges are allowed to overlap,
    ///     so a consumer must not assume either.
    /// </remarks>
    public IList<YahooMediaText> TextSeries { get; } = [];

    /// <summary>
    /// Gets the representative images for this syndication entity.
    /// </summary>
    /// <value>The thumbnails declared on the item itself. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     <b>A thumbnail inside a <see cref="YahooMediaGroup"/> or a <see cref="YahooMediaContent"/> is not here.</b>
    ///     That is the shape most of the web's thumbnails arrive in: <c>media:thumbnail</c> is the most frequent
    ///     extension element of any family in the 136-document corpus at <b>4,009</b> occurrences, nearly all of
    ///     them inside a group, for which this collection reads empty and nothing looks like a failure
    ///     (<c>.endjin/build-warnings.md</c> §2.46). Where several are given at one level and none carries a
    ///     <see cref="YahooMediaThumbnail.Time"/>, they are in order of importance.
    /// </remarks>
    public IList<YahooMediaThumbnail> Thumbnails { get; } = [];

    /// <summary>
    /// Gets or sets the title of this syndication entity.
    /// </summary>
    /// <value>The title, or <see langword="null"/> if the item itself declared none.</value>
    public YahooMediaTextConstruct? Title { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="YahooMediaSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNodeIterator contentIterator = source.SelectChildElements("media", "content", manager);
            XPathNodeIterator groupIterator = source.SelectChildElements("media", "group", manager);

            if (contentIterator is { Count: > 0 })
            {
                while (contentIterator.MoveNext())
                {
                    XPathNavigator? contentNode = contentIterator.Current;
                    if (contentNode is null)
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
                    if (groupNode is null)
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
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     <para>
    ///         Folds the same members <see cref="YahooMediaSyndicationExtension.CompareTo(YahooMediaSyndicationExtension)"/>
    ///         walks — <see cref="Contents"/>, <see cref="Groups"/> and the shared metadata of
    ///         <see cref="IYahooMediaCommonObjectEntities"/> — with every collection taken element by
    ///         element. Passing a collection itself to <see cref="HashCodeUtility.Component{T}(T)"/> would
    ///         hash the list reference, so two instances carrying equal data would hash differently.
    ///     </para>
    ///     <para>
    ///         This exists because the extension used to hash <i>this whole object</i> through that same
    ///         identity overload, and this class overrode nothing: two extensions built from identical data
    ///         were <see cref="object.Equals(object)"/> and hashed by reference identity, which made the type
    ///         unusable as a dictionary key. It is the tenth instance of the family recorded in
    ///         <c>.endjin/build-warnings.md</c> §4.3, and the sweep there missed it because it looked for
    ///         collections passed to the identity overload rather than for a whole context object.
    ///     </para>
    ///     <para>
    ///         This class declares no <see cref="object.Equals(object)"/> of its own, so its own equality is
    ///         still reference equality — a hash coarser than that is legal, and this one is used as a
    ///         component of the extension's.
    ///     </para>
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (YahooMediaContent content in this.Contents)
        {
            hash.Add(HashCodeUtility.Component(content));
        }

        foreach (YahooMediaGroup group in this.Groups)
        {
            hash.Add(HashCodeUtility.Component(group));
        }

        hash.Add(HashCodeUtility.Component(this.Copyright));
        hash.Add(HashCodeUtility.Component(this.Description));
        hash.Add(HashCodeUtility.Component(this.Player));
        hash.Add(HashCodeUtility.Component(this.Title));

        foreach (YahooMediaCategory category in this.Categories)
        {
            hash.Add(HashCodeUtility.Component(category));
        }

        foreach (YahooMediaCredit credit in this.Credits)
        {
            hash.Add(HashCodeUtility.Component(credit));
        }

        foreach (YahooMediaHash mediaHash in this.Hashes)
        {
            hash.Add(HashCodeUtility.Component(mediaHash));
        }

        foreach (string keyword in this.Keywords)
        {
            hash.Add(HashCodeUtility.Component(keyword));
        }

        foreach (YahooMediaRating rating in this.Ratings)
        {
            hash.Add(HashCodeUtility.Component(rating));
        }

        foreach (YahooMediaRestriction restriction in this.Restrictions)
        {
            hash.Add(HashCodeUtility.Component(restriction));
        }

        foreach (YahooMediaText text in this.TextSeries)
        {
            hash.Add(HashCodeUtility.Component(text));
        }

        foreach (YahooMediaThumbnail thumbnail in this.Thumbnails)
        {
            hash.Add(HashCodeUtility.Component(thumbnail));
        }

        return hash.ToHashCode();
    }
}