using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a publishable media object — one rendition of one piece of content.
/// </summary>
/// <remarks>
///     <para>
///         <c>media:content</c> is what Media RSS supplies in place of RSS's single <c>&lt;enclosure&gt;</c>. It
///         may sit directly under the item, or inside a <see cref="YahooMediaGroup"/> alongside other renditions
///         of the same thing.
///     </para>
///     <para>
///         <b>Renditions in a group are alternatives, not a playlist.</b> Content that is not the same content
///         does not belong in one group, and a consumer that treats a group as a list of distinct media objects
///         will show the same item several times. Where several are offered, <see cref="IsDefault"/> marks the
///         one to prefer; failing that, the document order is the publisher's order of presentation.
///     </para>
///     <para>
///         Every attribute is optional, including <see cref="Url"/> — a media object reachable only through a
///         <see cref="Player"/> console carries no URL at all. Absent numeric attributes read back as the
///         <c>MinValue</c> of their type rather than as zero, because zero is a legal bitrate, height and
///         duration; see each property.
///     </para>
/// </remarks>
/// <seealso cref="IYahooMediaCommonObjectEntities"/>
public class YahooMediaContent : IComparable<YahooMediaContent>, IEquatable<YahooMediaContent>, IYahooMediaCommonObjectEntities, IComparisonOperators
{

    /// <summary>
    /// Private member to hold the permissible audiences for the media object.
    /// </summary>
    private List<YahooMediaRating>? mediaObjectRatings;

    /// <summary>
    /// Private member to hold the relevant keywords that describe the media object.
    /// </summary>
    private List<string>? mediaObjectKeywords;

    /// <summary>
    /// Private member to hold the representative images for the media object.
    /// </summary>
    private List<YahooMediaThumbnail>? mediaObjectThumbnails;

    /// <summary>
    /// Private member to hold a taxonomy that gives an indication of the type of content for the media object.
    /// </summary>
    private List<YahooMediaCategory>? mediaObjectCategories;

    /// <summary>
    /// Private member to hold the hash digests for the media object.
    /// </summary>
    private List<YahooMediaHash>? mediaObjectHashes;

    /// <summary>
    /// Private member to hold the entities that contributed to the creation of the media object.
    /// </summary>
    private List<YahooMediaCredit>? mediaObjectCredits;

    /// <summary>
    /// Private member to hold the text transcript, closed captioning, or lyrics for the media object.
    /// </summary>
    private List<YahooMediaText>? mediaObjectTextSeries;

    /// <summary>
    /// Private member to hold the restrictions to be placed on aggregators that are rendering the media object.
    /// </summary>
    private List<YahooMediaRestriction>? mediaObjectRestrictions;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaContent"/> class.
    /// </summary>
    public YahooMediaContent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaContent"/> class using the supplied <see cref="Uri"/>.
    /// </summary>
    /// <param name="url">A <see cref="Uri"/> that represents the direct URL to this media object.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="url"/> is <see langword="null"/>.</exception>
    public YahooMediaContent(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);

        this.Url = url;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaContent"/> class using the supplied <see cref="YahooMediaPlayer"/>.
    /// </summary>
    /// <param name="player">A <see cref="YahooMediaPlayer"/> that represents a web browser media player console this media object can be accessed through.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="player"/> is <see langword="null"/>.</exception>
    public YahooMediaContent(YahooMediaPlayer player)
    {
        ArgumentNullException.ThrowIfNull(player);

        this.Player = player;
    }

    /// <summary>
    /// Gets or sets the kilobits per second rate of this media object.
    /// </summary>
    /// <value>The <i>kilobits</i> per second rate of this media object. The default value is <see cref="Int32.MinValue"/>, which indicates that no bit-rate was specified.</value>
    public int Bitrate { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the number of audio channels in this media object.
    /// </summary>
    /// <value>The number of audio channels in this media object. The default value is <see cref="Int32.MinValue"/>, which indicates that no audio channels were specified.</value>
    public int Channels { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the content type of this media object.
    /// </summary>
    /// <value>A registered media type, such as <c>video/mp4</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     See <a href="https://www.iana.org/assignments/media-types/media-types.xhtml">IANA MIME Media Types</a> for a listing of registered MIME types.
    ///     This is the finer-grained partner of <see cref="Medium"/>, which names only the broad kind.
    /// </remarks>
    public string ContentType
    {
        get;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                field = string.Empty;
            }
            else
            {
                field = value.Trim();
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the total play time for this media object.
    /// </summary>
    /// <value>The total playing time. The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no duration was specified.</value>
    /// <remarks>
    ///     The attribute is a whole number of seconds. That is what is written, and it is tried first on read; a
    ///     value that is not an integer is then tried as a <see cref="TimeSpan"/>, so a publisher's
    ///     <c>duration="00:04:31"</c> survives being read even though it is not what the specification asks for.
    ///     A saved feed always carries the seconds form, so that round-trip is not byte-identical.
    /// </remarks>
    public TimeSpan Duration { get; set; } = TimeSpan.MinValue;

    /// <summary>
    /// Gets or sets the expressed version of this media object.
    /// </summary>
    /// <value>
    ///     Whether this is the full version, a sample, or a continuous stream. The default value is
    ///     <see cref="YahooMediaExpression.None"/>, which indicates that no expression was specified.
    /// </value>
    /// <remarks>
    ///     The specification's default when the attribute is absent is <c>full</c>. That inference is left to
    ///     the caller: <see cref="YahooMediaExpression.None"/> is kept distinct so that saving a feed does not
    ///     write an <c>expression</c> the publisher never wrote.
    /// </remarks>
    public YahooMediaExpression Expression { get; set; } = YahooMediaExpression.None;

    /// <summary>
    /// Gets or sets the file size of this media object.
    /// </summary>
    /// <value>The number of bytes this media object represents on disk. The default value is <see cref="Int64.MinValue"/>, which indicates that no file size was specified.</value>
    public long FileSize { get; set; } = long.MinValue;

    /// <summary>
    /// Gets or sets the number of frames per second for this media object.
    /// </summary>
    /// <value>The number of frames per second for this media object. The default value is <see cref="Int32.MinValue"/>, which indicates that no frame-rate was specified.</value>
    public int FrameRate { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets the height of this media object.
    /// </summary>
    /// <value>The height of this media object, typically in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates that no height was specified.</value>
    public int Height { get; set; } = int.MinValue;

    /// <summary>
    /// Gets or sets a value indicating if this media object is the default object in a group.
    /// </summary>
    /// <value><see langword="true"/> if this is the rendition a consumer should prefer; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.</value>
    /// <remarks>
    ///     The specification permits one default per <see cref="YahooMediaGroup"/>. Nothing here enforces that,
    ///     on read or on write, so a consumer should take the first it finds rather than assume uniqueness.
    /// </remarks>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Gets or sets the primary language encapsulated in this media object.
    /// </summary>
    /// <value>The primary language, or <see langword="null"/> if no language was specified.</value>
    /// <remarks>
    ///     Media RSS pins the <c>lang</c> attribute to
    ///     <a href="https://www.rfc-editor.org/rfc/rfc3066.html">RFC 3066</a> (BCP 47; now RFC 5646). A tag
    ///     <see cref="CultureInfo"/> cannot construct is traced and dropped rather than throwing, so a feed with
    ///     one unparseable <c>lang</c> still loads — and this property still reads <see langword="null"/>,
    ///     indistinguishably from the attribute being absent.
    /// </remarks>
    public CultureInfo? Language { get; set; }

    /// <summary>
    /// Gets or sets the content medium of this media object.
    /// </summary>
    /// <value>
    ///     The broad kind of object — image, audio, video, document or executable. The default value is
    ///     <see cref="YahooMediaMedium.None"/>, which indicates that no medium was specified.
    /// </value>
    /// <remarks>
    ///     A <c>medium</c> this library does not recognise also arrives as <see cref="YahooMediaMedium.None"/>
    ///     and is dropped on save. <see cref="ContentType"/> survives whatever it says and is the safer thing to
    ///     branch on.
    /// </remarks>
    public YahooMediaMedium Medium { get; set; } = YahooMediaMedium.None;

    /// <summary>
    /// Gets or sets the number of samples per second taken to create this media object.
    /// </summary>
    /// <value>
    ///     The sampling rate in <i>thousands</i> of samples per second (kHz) — <c>44.1</c>, not <c>44100</c>.
    ///     The default value is <see cref="Decimal.MinValue"/>, which indicates that no sampling rate was specified.
    /// </value>
    public decimal SamplingRate { get; set; } = decimal.MinValue;

    /// <summary>
    /// Gets or sets the location of this media object.
    /// </summary>
    /// <value>The direct URL to the media, or <see langword="null"/> if it is reachable only through <see cref="Player"/>.</value>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the width of this media object.
    /// </summary>
    /// <value>The width of this media object, typically in pixels. The default value is <see cref="Int32.MinValue"/>, which indicates that no width was specified.</value>
    public int Width { get; set; } = int.MinValue;

    /// <summary>
    /// Gets a taxonomy that gives an indication of the type of content for this media object.
    /// </summary>
    /// <value>The categories declared on this <c>media:content</c>. The default value is an <i>empty</i> collection.</value>
    /// <seealso cref="IYahooMediaCommonObjectEntities"/>
    public IList<YahooMediaCategory> Categories
    {
        get
        {
            mediaObjectCategories ??= [];
            return mediaObjectCategories;
        }
    }

    /// <summary>
    /// Gets or sets the copyright information for this media object.
    /// </summary>
    /// <value>The copyright information, or <see langword="null"/> if none was declared on this <c>media:content</c>.</value>
    /// <remarks>
    ///     If the media is operating under a <i>Creative Commons license</i>, a <see cref="CreativeCommonsSyndicationExtension">Creative Commons extension</see> should be used instead.
    /// </remarks>
    public YahooMediaCopyright? Copyright { get; set; }

    /// <summary>
    /// Gets the entities that contributed to the creation of this media object.
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
    /// Gets or sets the description of this media object.
    /// </summary>
    /// <value>A sentence or so of description, or <see langword="null"/> if none was declared on this <c>media:content</c>.</value>
    public YahooMediaTextConstruct? Description { get; set; }

    /// <summary>
    /// Gets the hash digests for this media object.
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
    /// Gets the relevant keywords that describe this media object.
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
    /// Gets or sets a web browser media player console this media object can be accessed through.
    /// </summary>
    /// <value>The player console, or <see langword="null"/> if none was declared. Required when <see cref="Url"/> is <see langword="null"/>.</value>
    public YahooMediaPlayer? Player { get; set; }

    /// <summary>
    /// Gets the permissible audiences for this media object.
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
    /// Gets the restrictions to be placed on aggregators that are rendering this media object.
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
    /// Gets the text transcript, closed captioning, or lyrics for this media object.
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
    /// Gets the representative images for this media object.
    /// </summary>
    /// <value>The thumbnails. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     Where several are given and none carries a <see cref="YahooMediaThumbnail.Time"/>, they are in order of
    ///     importance, so the first is the one to show.
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
    /// Gets or sets the title of this media object.
    /// </summary>
    /// <value>The title, or <see langword="null"/> if none was declared on this <c>media:content</c>.</value>
    public YahooMediaTextConstruct? Title { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaContent"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaContent"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaContent"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);
        bool wasLoaded = this.LoadPrimary(source);

        if (this.LoadSecondary(source))
        {
            wasLoaded = true;
        }

        if (YahooMediaUtility.FillCommonObjectEntities(this, source))
        {
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaContent"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("content", YahooMediaSyndicationExtension.NamespaceUri);

        if (this.Url is not null)
        {
            writer.WriteAttributeString("url", this.Url.ToString());
        }

        if (this.FileSize != long.MinValue)
        {
            writer.WriteAttributeString("fileSize", this.FileSize.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (!string.IsNullOrEmpty(this.ContentType))
        {
            writer.WriteAttributeString("type", this.ContentType);
        }

        if (this.Medium != YahooMediaMedium.None)
        {
            writer.WriteAttributeString("medium", YahooMediaSyndicationExtension.MediumAsString(this.Medium));
        }

        if (this.IsDefault)
        {
            writer.WriteAttributeString("isDefault", "true");
        }

        if (this.Expression != YahooMediaExpression.None)
        {
            writer.WriteAttributeString("expression", YahooMediaSyndicationExtension.ExpressionAsString(this.Expression));
        }

        if (this.Bitrate != int.MinValue)
        {
            writer.WriteAttributeString("bitrate", this.Bitrate.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.FrameRate != int.MinValue)
        {
            writer.WriteAttributeString("framerate", this.FrameRate.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.SamplingRate != decimal.MinValue)
        {
            writer.WriteAttributeString("samplingrate", this.SamplingRate.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Channels != int.MinValue)
        {
            writer.WriteAttributeString("channels", this.Channels.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Duration != TimeSpan.MinValue)
        {
            writer.WriteAttributeString("duration", this.Duration.TotalSeconds.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Height != int.MinValue)
        {
            writer.WriteAttributeString("height", this.Height.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Width != int.MinValue)
        {
            writer.WriteAttributeString("width", this.Width.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Language is not null)
        {
            writer.WriteAttributeString("lang", this.Language.Name);
        }

        YahooMediaUtility.WriteCommonObjectEntities(this, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaContent"/>.
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
    public int CompareTo(YahooMediaContent? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Bitrate.CompareTo(other.Bitrate);
        if (result == 0) result = this.Channels.CompareTo(other.Channels);
        if (result == 0) result = string.Compare(this.ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Duration.CompareTo(other.Duration);
        if (result == 0) result = this.Expression.CompareTo(other.Expression);
        if (result == 0) result = this.FileSize.CompareTo(other.FileSize);
        if (result == 0) result = this.FrameRate.CompareTo(other.FrameRate);
        if (result == 0) result = this.Height.CompareTo(other.Height);
        if (result == 0) result = this.IsDefault.CompareTo(other.IsDefault);

        string sourceLanguageName = this.Language?.Name ?? string.Empty;
        string targetLanguageName = other.Language?.Name ?? string.Empty;
        if (result == 0) result = string.Compare(sourceLanguageName, targetLanguageName, StringComparison.OrdinalIgnoreCase);

        if (result == 0) result = this.Medium.CompareTo(other.Medium);
        if (result == 0) result = this.SamplingRate.CompareTo(other.SamplingRate);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.Width.CompareTo(other.Width);

        if (result == 0) result = YahooMediaUtility.CompareCommonObjectEntities(this, other);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaContent"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaContent"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaContent"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaContent? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaContent other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Bitrate));
        hash.Add(HashCodeUtility.Component(this.Channels));
        hash.Add(HashCodeUtility.Component(this.ContentType));
        hash.Add(HashCodeUtility.Component(this.Duration));
        hash.Add(HashCodeUtility.Component(this.Expression));
        hash.Add(HashCodeUtility.Component(this.FileSize));
        hash.Add(HashCodeUtility.Component(this.FrameRate));
        hash.Add(HashCodeUtility.Component(this.Height));
        hash.Add(HashCodeUtility.Component(this.IsDefault));
        hash.Add(HashCodeUtility.Component(this.Language));
        hash.Add(HashCodeUtility.Component(this.Medium));
        hash.Add(HashCodeUtility.Component(this.SamplingRate));
        hash.Add(HashCodeUtility.Component(this.Url));
        hash.Add(HashCodeUtility.Component(this.Width));
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaContent? first, YahooMediaContent? second)
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
    public static bool operator !=(YahooMediaContent? first, YahooMediaContent? second) => !(first == second);

    /// <summary>
    /// Loads the primary properties of this <see cref="YahooMediaContent"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaContent"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaContent"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    private bool LoadPrimary(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string urlAttribute = source.GetAttribute("url", string.Empty);
            string fileSizeAttribute = source.GetAttribute("fileSize", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string mediumAttribute = source.GetAttribute("medium", string.Empty);
            string isDefaultAttribute = source.GetAttribute("isDefault", string.Empty);
            string expressionAttribute = source.GetAttribute("expression", string.Empty);
            string bitrateAttribute = source.GetAttribute("bitrate", string.Empty);

            if (!string.IsNullOrEmpty(urlAttribute))
            {
                if (Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
                {
                    this.Url = url;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(fileSizeAttribute))
            {
                if (long.TryParse(fileSizeAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out long fileSize))
                {
                    this.FileSize = fileSize;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                this.ContentType = typeAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(mediumAttribute))
            {
                YahooMediaMedium medium = YahooMediaSyndicationExtension.MediumByName(mediumAttribute);
                if (medium != YahooMediaMedium.None)
                {
                    this.Medium = medium;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(isDefaultAttribute))
            {
                if (string.Equals(isDefaultAttribute, "true", StringComparison.OrdinalIgnoreCase))
                {
                    this.IsDefault = true;
                    wasLoaded = true;
                }
                else if (string.Equals(isDefaultAttribute, "false", StringComparison.OrdinalIgnoreCase))
                {
                    this.IsDefault = false;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(expressionAttribute))
            {
                YahooMediaExpression expression = YahooMediaSyndicationExtension.ExpressionByName(expressionAttribute);
                if (expression != YahooMediaExpression.None)
                {
                    this.Expression = expression;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(bitrateAttribute))
            {
                if (int.TryParse(bitrateAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int bitrate))
                {
                    this.Bitrate = bitrate;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads the secondary properties of this <see cref="YahooMediaContent"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaContent"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaContent"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    private bool LoadSecondary(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string frameRateAttribute = source.GetAttribute("framerate", string.Empty);
            string samplingRateAttribute = source.GetAttribute("samplingrate", string.Empty);
            string channelsAttribute = source.GetAttribute("channels", string.Empty);
            string durationAttribute = source.GetAttribute("duration", string.Empty);
            string heightAttribute = source.GetAttribute("height", string.Empty);
            string widthAttribute = source.GetAttribute("width", string.Empty);
            string languageAttribute = source.GetAttribute("lang", string.Empty);

            if (!string.IsNullOrEmpty(frameRateAttribute))
            {
                if (int.TryParse(frameRateAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int frameRate))
                {
                    this.FrameRate = frameRate;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(samplingRateAttribute))
            {
                if (decimal.TryParse(samplingRateAttribute, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out decimal samplingRate))
                {
                    this.SamplingRate = samplingRate;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(channelsAttribute))
            {
                if (int.TryParse(channelsAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int channels))
                {
                    this.Channels = channels;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(durationAttribute))
            {
                if (int.TryParse(durationAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int seconds))
                {
                    this.Duration = new TimeSpan(0, 0, seconds);
                    wasLoaded = true;
                }
                else if (TimeSpan.TryParse(durationAttribute, out TimeSpan duration))
                {
                    this.Duration = duration;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(heightAttribute))
            {
                if (int.TryParse(heightAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int height))
                {
                    this.Height = height;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(widthAttribute))
            {
                if (int.TryParse(widthAttribute, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int width))
                {
                    this.Width = width;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(languageAttribute))
            {
                try
                {
                    CultureInfo language = new(languageAttribute);
                    this.Language = language;
                    wasLoaded = true;
                }
                catch (ArgumentException)
                {
                    System.Diagnostics.Trace.TraceWarning("Unable to determine CultureInfo with a name of {0}.", languageAttribute);
                }
            }
        }

        return wasLoaded;
    }
}