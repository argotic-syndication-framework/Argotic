namespace Argotic.Extensions.Core;

/// <summary>
/// The Media RSS metadata elements that may hang off any of the three levels a media object can be described at.
/// </summary>
/// <remarks>
///     <para>
///         Media RSS lets the same optional elements — title, description, thumbnails, ratings, credits and the
///         rest — appear on a <c>media:content</c>, on the <c>media:group</c> that encloses it, or on the item
///         itself. This interface is how that sharing is expressed: <see cref="YahooMediaContent"/>,
///         <see cref="YahooMediaGroup"/> and <see cref="YahooMediaSyndicationExtensionContext"/> all implement
///         it, and one pair of helpers in <c>YahooMediaUtility</c> reads and writes all three.
///     </para>
///     <para>
///         The specification's priority order, strongest to weakest, is <see cref="YahooMediaContent"/>,
///         <see cref="YahooMediaGroup"/>, the item, then the feed: an element at a deeper level overrides the
///         same element higher up, and an element that appears only higher up applies to every media object
///         beneath it.
///     </para>
///     <para>
///         <b>This library does not apply that order.</b> Each object is filled from the direct <c>media:</c>
///         children of its own element and from nothing else, so every level holds exactly what the publisher
///         wrote there — a group-level thumbnail does not appear on the contents inside it, and an item-level
///         one does not appear on either. Resolving the override is the caller's job, because merging on read
///         would put values in the object model the feed never carried, and a subsequent save would write them
///         back out as though the publisher had.
///         The cost of not knowing this is measured: <c>media:thumbnail</c> is the most frequent extension
///         element of any family in the 136-document corpus, at <b>4,009</b> occurrences, and nearly all of it
///         arrives inside a <c>media:group</c> in a YouTube channel feed — for which
///         <see cref="YahooMediaSyndicationExtensionContext.Thumbnails"/> reads empty and nothing looks like a
///         failure.
///     </para>
///     <para>
///         <see cref="Title"/>, <see cref="Description"/>, <see cref="Copyright"/>, <see cref="Player"/> and
///         <see cref="Keywords"/> are read from the <i>first</i> matching element at a level; a second is
///         dropped. Every other member is a collection and takes all of them, in document order.
///     </para>
/// </remarks>
/// <seealso cref="YahooMediaContent"/>
/// <seealso cref="YahooMediaGroup"/>
/// <seealso cref="YahooMediaSyndicationExtensionContext"/>
interface IYahooMediaCommonObjectEntities
{
    /// <summary>
    /// Gets a taxonomy that gives an indication of the type of content for the media object.
    /// </summary>
    /// <value>The categories declared at this level. The default value is an <i>empty</i> collection.</value>
    IList<YahooMediaCategory> Categories { get; }

    /// <summary>
    /// Gets or sets the copyright information for the media object.
    /// </summary>
    /// <value>The copyright information, or <see langword="null"/> if none was declared at this level.</value>
    /// <remarks>
    ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
    /// </remarks>
    YahooMediaCopyright? Copyright { get; set; }

    /// <summary>
    /// Gets the entities that contributed to the creation of the media object.
    /// </summary>
    /// <value>The contributing entities. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     An entity may be a person, a company or a place. One entity may hold several roles and one role may be
    ///     held by several entities; each combination is a separate <see cref="YahooMediaCredit"/>.
    /// </remarks>
    IList<YahooMediaCredit> Credits { get; }

    /// <summary>
    /// Gets or sets the description of the media object.
    /// </summary>
    /// <value>A sentence or so of description, or <see langword="null"/> if none was declared at this level.</value>
    YahooMediaTextConstruct? Description { get; set; }

    /// <summary>
    /// Gets the hash digests for the media object.
    /// </summary>
    /// <value>The hash digests. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     The specification allows several only if each carries a different <see cref="YahooMediaHash.Algorithm"/>.
    ///     Nothing here enforces that.
    /// </remarks>
    IList<YahooMediaHash> Hashes { get; }

    /// <summary>
    /// Gets the relevant keywords that describe the media object.
    /// </summary>
    /// <value>The keywords. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     One <c>media:keywords</c> element carries the lot, comma-separated; this collection is that list
    ///     split apart, and is rejoined with commas on write. The specification suggests a maximum of ten.
    /// </remarks>
    IList<string> Keywords { get; }

    /// <summary>
    /// Gets or sets a web browser media player console the media object can be accessed through.
    /// </summary>
    /// <value>The player console, or <see langword="null"/> if none was declared at this level.</value>
    YahooMediaPlayer? Player { get; set; }

    /// <summary>
    /// Gets the permissible audiences for the media object.
    /// </summary>
    /// <value>The ratings. The default value is an <i>empty</i> collection, which means no audience restriction.</value>
    IList<YahooMediaRating> Ratings { get; }

    /// <summary>
    /// Gets the restrictions to be placed on aggregators that are rendering the media object.
    /// </summary>
    /// <value>The restrictions. The default value is an <i>empty</i> collection.</value>
    IList<YahooMediaRestriction> Restrictions { get; }

    /// <summary>
    /// Gets the text transcript, closed captioning, or lyrics for the media object.
    /// </summary>
    /// <value>The text fragments. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Several of these together form a time series — captions, say. Grouping them by language and ordering
    ///     them by start time is encouraged rather than required, and their time ranges are allowed to overlap,
    ///     so a consumer must not assume either.
    /// </remarks>
    IList<YahooMediaText> TextSeries { get; }

    /// <summary>
    /// Gets the representative images for the media object.
    /// </summary>
    /// <value>The thumbnails. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Where several are given and none carries a <see cref="YahooMediaThumbnail.Time"/>, they are in order
    ///     of importance, so the first is the one to show.
    /// </remarks>
    IList<YahooMediaThumbnail> Thumbnails { get; }

    /// <summary>
    /// Gets or sets the title of the media object.
    /// </summary>
    /// <value>The title, or <see langword="null"/> if none was declared at this level.</value>
    YahooMediaTextConstruct? Title { get; set; }
}