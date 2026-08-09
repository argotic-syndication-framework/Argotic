using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents several renditions of one piece of content.
/// </summary>
/// <remarks>
///     <para>
///         <b>A <c>media:group</c> holds one piece of content, expressed several ways.</b> Its
///         <see cref="YahooMediaContent"/> objects are alternatives — the specification's own example is one song
///         published as both WAV and MP3 — differing in format, bitrate or language. The specification says the
///         element must be used for nothing else.
///     </para>
///     <para>
///         <b>The failure mode is duplication, and it is silent.</b> A consumer that flattens
///         <see cref="Contents"/> into its list of media, or counts a group's contents as separate items, shows
///         the same video once per rendition. Pick one: the rendition whose
///         <see cref="YahooMediaContent.IsDefault"/> is <see langword="true"/>, of which the specification
///         permits one per group, or the first, since document order is the publisher's order of presentation.
///     </para>
///     <para>
///         A group carries its own copy of the shared metadata — thumbnails, ratings, credits and the rest — and
///         those apply to every rendition in it. They are <i>not</i> copied down onto the contents;
///         see <see cref="IYahooMediaCommonObjectEntities"/>.
///     </para>
/// </remarks>
/// <seealso cref="YahooMediaContent"/>
/// <seealso cref="IYahooMediaCommonObjectEntities"/>
public class YahooMediaGroup : IComparable<YahooMediaGroup>, IEquatable<YahooMediaGroup>, IYahooMediaCommonObjectEntities, IComparisonOperators
{

    /// <summary>
    /// Private member to hold a collection of media objects that are effectively the same content, yet different representations.
    /// </summary>
    private List<YahooMediaContent>? groupContents;

    /// <summary>
    /// Private member to hold the permissible audiences for the media group.
    /// </summary>
    private List<YahooMediaRating>? mediaObjectRatings;

    /// <summary>
    /// Private member to hold the relevant keywords that describe the media group.
    /// </summary>
    private List<string>? mediaObjectKeywords;

    /// <summary>
    /// Private member to hold the representative images for the media group.
    /// </summary>
    private List<YahooMediaThumbnail>? mediaObjectThumbnails;

    /// <summary>
    /// Private member to hold a taxonomy that gives an indication of the type of content for the media group.
    /// </summary>
    private List<YahooMediaCategory>? mediaObjectCategories;

    /// <summary>
    /// Private member to hold the hash digests for the media group.
    /// </summary>
    private List<YahooMediaHash>? mediaObjectHashes;

    /// <summary>
    /// Private member to hold the entities that contributed to the creation of the media group.
    /// </summary>
    private List<YahooMediaCredit>? mediaObjectCredits;

    /// <summary>
    /// Private member to hold the text transcript, closed captioning, or lyrics for the media group.
    /// </summary>
    private List<YahooMediaText>? mediaObjectTextSeries;

    /// <summary>
    /// Private member to hold the restrictions to be placed on aggregators that are rendering the media group.
    /// </summary>
    private List<YahooMediaRestriction>? mediaObjectRestrictions;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaGroup"/> class.
    /// </summary>
    public YahooMediaGroup()
    {
    }

    /// <summary>
    /// Gets a collection of media objects that are effectively the same content, yet different representations.
    /// </summary>
    /// <value>The renditions, in the publisher's order of presentation. The default value is an <i>empty</i> collection.</value>
    public IList<YahooMediaContent> Contents
    {
        get
        {
            groupContents ??= [];
            return groupContents;
        }
    }

    /// <summary>
    /// Gets a taxonomy that gives an indication of the type of content for this media group.
    /// </summary>
    /// <value>The categories declared on this <c>media:group</c>. The default value is an <i>empty</i> collection.</value>
    public IList<YahooMediaCategory> Categories
    {
        get
        {
            mediaObjectCategories ??= [];
            return mediaObjectCategories;
        }
    }

    /// <summary>
    /// Gets or sets the copyright information for this media group.
    /// </summary>
    /// <value>The copyright information, or <see langword="null"/> if none was declared on this <c>media:group</c>.</value>
    /// <remarks>
    ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
    /// </remarks>
    public YahooMediaCopyright? Copyright { get; set; }

    /// <summary>
    /// Gets the entities that contributed to the creation of this media group.
    /// </summary>
    /// <value>The contributing entities. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     An entity may be a person, a company or a place. One entity may hold several roles and one role may be
    ///     held by several entities; each combination is a separate <see cref="YahooMediaCredit"/>.
    /// </remarks>
    public IList<YahooMediaCredit> Credits
    {
        get
        {
            mediaObjectCredits ??= [];
            return mediaObjectCredits;
        }
    }

    /// <summary>
    /// Gets or sets the description of this media group.
    /// </summary>
    /// <value>A sentence or so of description, or <see langword="null"/> if none was declared on this <c>media:group</c>.</value>
    public YahooMediaTextConstruct? Description { get; set; }

    /// <summary>
    /// Gets the hash digests for this media group.
    /// </summary>
    /// <value>The hash digests. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     The specification allows several only if each carries a different <see cref="YahooMediaHash.Algorithm"/>.
    ///     Nothing here enforces that.
    /// </remarks>
    public IList<YahooMediaHash> Hashes
    {
        get
        {
            mediaObjectHashes ??= [];
            return mediaObjectHashes;
        }
    }

    /// <summary>
    /// Gets the relevant keywords that describe this media group.
    /// </summary>
    /// <value>The keywords. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     One <c>media:keywords</c> element carries the lot, comma-separated; this collection is that list split
    ///     apart, and is rejoined with commas on write. The specification suggests a maximum of ten.
    /// </remarks>
    public IList<string> Keywords
    {
        get
        {
            mediaObjectKeywords ??= [];
            return mediaObjectKeywords;
        }
    }

    /// <summary>
    /// Gets or sets a web browser media player console this media group can be accessed through.
    /// </summary>
    /// <value>The player console, or <see langword="null"/> if none was declared on this <c>media:group</c>.</value>
    public YahooMediaPlayer? Player { get; set; }

    /// <summary>
    /// Gets the permissible audiences for this media group.
    /// </summary>
    /// <value>The ratings. The default value is an <i>empty</i> collection, which means no audience restriction.</value>
    public IList<YahooMediaRating> Ratings
    {
        get
        {
            mediaObjectRatings ??= [];
            return mediaObjectRatings;
        }
    }

    /// <summary>
    /// Gets the restrictions to be placed on aggregators that are rendering this media group.
    /// </summary>
    /// <value>The restrictions. The default value is an <i>empty</i> collection.</value>
    public IList<YahooMediaRestriction> Restrictions
    {
        get
        {
            mediaObjectRestrictions ??= [];
            return mediaObjectRestrictions;
        }
    }

    /// <summary>
    /// Gets the text transcript, closed captioning, or lyrics for this media group.
    /// </summary>
    /// <value>The text fragments. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Several of these together form a time series — captions, say. Grouping them by language and ordering
    ///     them by start time is encouraged rather than required, and their time ranges are allowed to overlap,
    ///     so a consumer must not assume either.
    /// </remarks>
    public IList<YahooMediaText> TextSeries
    {
        get
        {
            mediaObjectTextSeries ??= [];
            return mediaObjectTextSeries;
        }
    }

    /// <summary>
    /// Gets the representative images for this media group.
    /// </summary>
    /// <value>The thumbnails. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Where several are given and none carries a <see cref="YahooMediaThumbnail.Time"/>, they are in order of
    ///     importance, so the first is the one to show. These belong to the group, not to any one rendition in it.
    /// </remarks>
    public IList<YahooMediaThumbnail> Thumbnails
    {
        get
        {
            mediaObjectThumbnails ??= [];
            return mediaObjectThumbnails;
        }
    }

    /// <summary>
    /// Gets or sets the title of this media group.
    /// </summary>
    /// <value>The title, or <see langword="null"/> if none was declared on this <c>media:group</c>.</value>
    public YahooMediaTextConstruct? Title { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaGroup"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaGroup"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaGroup"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        YahooMediaSyndicationExtension extension = new();
        XmlNamespaceManager manager = extension.CreateNamespaceManager(source);
        if (source.HasChildren)
        {
            XPathNodeIterator contentIterator = source.SelectChildElements("media", "content", manager);

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
        }

        if (YahooMediaUtility.FillCommonObjectEntities(this, source))
        {
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaGroup"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("group", extension.XmlNamespace);

        foreach (YahooMediaContent content in this.Contents)
        {
            content.WriteTo(writer);
        }

        YahooMediaUtility.WriteCommonObjectEntities(this, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaGroup"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(YahooMediaGroup? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = YahooMediaUtility.CompareSequence(this.Contents, other.Contents);

        if (result == 0) result = YahooMediaUtility.CompareCommonObjectEntities(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaGroup"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaGroup"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaGroup"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaGroup? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaGroup other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     <para>
    ///         <see cref="Contents"/> is folded in element by element. Passing the collection itself to
    ///         <see cref="HashCodeUtility.Component{T}(T)"/> would hash the list reference, so two instances
    ///         that <see cref="CompareTo"/> reports as equal hashed differently.
    ///     </para>
    ///     <para>
    ///         <b>The twelve shared metadata members that <see cref="CompareTo"/> also walks are deliberately
    ///         omitted.</b> The contract runs one way only — equal objects must hash equally — so a hash
    ///         coarser than the comparison is legal: two groups differing only in, say, <c>Title</c> collide,
    ///         which costs a probe and never yields a wrong answer. Widening it is an optimisation, not a fix,
    ///         and the sibling <see cref="YahooMediaContent"/> omits the same twelve.
    ///         <c>YahooMediaHashCodeContractTests</c> pins the omission so that widening it becomes a
    ///         deliberate act rather than an accident.
    ///     </para>
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (YahooMediaContent content in this.Contents)
        {
            hash.Add(HashCodeUtility.Component(content));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaGroup? first, YahooMediaGroup? second)
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
    public static bool operator !=(YahooMediaGroup? first, YahooMediaGroup? second) => !(first == second);
}