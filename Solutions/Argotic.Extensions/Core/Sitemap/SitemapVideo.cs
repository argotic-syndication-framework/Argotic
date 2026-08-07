using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a single video described by a <c>video:video</c> element in a Google video sitemap.
/// </summary>
/// <remarks>
///     <para>
///     Four things are required and the fifth is a choice: <see cref="ThumbnailLocation"/>,
///     <see cref="Title"/> and <see cref="Description"/> must all be present, and at least one of
///     <see cref="ContentLocation"/> or <see cref="PlayerLocation"/>. Everything else on this class is
///     optional. Unlike its image counterpart, the video extension survived Google's 2022 cull largely
///     intact — <c>expiration_date</c>, <c>rating</c>, <c>view_count</c>, <c>family_friendly</c>,
///     <c>restriction</c>, <c>platform</c>, <c>requires_subscription</c>, <c>uploader</c>, <c>live</c>
///     and <c>tag</c> are all still read.
///     </para>
///     <para>
///     What went, on <b>6 August 2022</b>, is modelled nowhere here and deliberately so:
///     <list type="bullet">
///         <item><c>video:category</c></item>
///         <item><c>video:gallery_loc</c></item>
///         <item><c>video:price</c>, and its attributes</item>
///         <item><c>video:tvshow</c>, and its attributes</item>
///         <item>the <c>allow_embed</c> and <c>autoplay</c> attributes of <c>video:player_loc</c></item>
///     </list>
///     A sitemap that still carries them is not invalid, merely ignored; this class will discard them on
///     the way in and never write them back.
///     </para>
///     <para>
///     <b>Every length limit on this class truncates rather than throws.</b> Assigning a 300-character
///     <see cref="Title"/> leaves you holding a 100-character one, and the same is true when loading:
///     an over-long title in the source document is cut without complaint. That is silent data loss on a
///     round-trip, and it is the shape most likely to surprise.
///     </para>
/// </remarks>
/// <seealso cref="SitemapVideoExtension.Videos"/>
/// <seealso href="https://developers.google.com/search/blog/2022/05/spring-cleaning-sitemap-extensions">Spring cleaning: some sitemap extension tags are going away</seealso>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
public class SitemapVideo : IComparable<SitemapVideo>, IEquatable<SitemapVideo>, IComparisonOperators
{
    /// <summary>
    /// The length at which <see cref="Title"/> truncates, in characters.
    /// </summary>
    /// <remarks>
    ///     The <c>maxLength</c> facet the 1.1 XSD puts on the title. Google's prose documentation states no
    ///     title limit at all, so this is the stricter of the two sources.
    /// </remarks>
    /// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd"/>
    public const int MaxTitleLength = 100;

    /// <summary>
    /// The length at which <see cref="Description"/> truncates, in characters.
    /// </summary>
    /// <remarks>The XSD and Google's documentation agree: "Maximum 2048 characters."</remarks>
    public const int MaxDescriptionLength = 2048;

    /// <summary>
    /// The length at which <see cref="Uploader"/> truncates, in characters.
    /// </summary>
    /// <remarks>Google: "The string value can be a maximum of 255 characters."</remarks>
    public const int MaxUploaderLength = 255;

    /// <summary>
    /// The number of <see cref="Tags"/> read and written; the rest are dropped.
    /// </summary>
    /// <remarks>Google: "A maximum of 32 tags is permitted per video."</remarks>
    public const int MaxTagCount = 32;

    /// <summary>
    /// The shortest duration Google accepts, in seconds.
    /// </summary>
    /// <remarks>
    ///     The <c>minInclusive</c> facet the 1.1 XSD puts on the duration, and it is enforced: the
    ///     <see cref="Duration"/> setter throws below it, while <see cref="Load"/> skips a source value below
    ///     it rather than rejecting the document.
    /// </remarks>
    public const int MinDuration = 1;

    /// <summary>
    /// The longest duration Google accepts, in seconds — eight hours.
    /// </summary>
    /// <remarks>
    ///     The matching <c>maxInclusive</c> facet, enforced the same way as <see cref="MinDuration"/>.
    ///     Google: "Value must be from <c>1</c> to <c>28800</c> (8 hours)."
    /// </remarks>
    public const int MaxDuration = 28_800;

    /// <summary>
    /// The lowest rating Google accepts.
    /// </summary>
    /// <remarks>
    ///     Enforced: the <see cref="Rating"/> setter throws below it and <see cref="Load"/> skips a source
    ///     value below it.
    /// </remarks>
    public const decimal MinRating = 0.0m;

    /// <summary>
    /// The highest rating Google accepts.
    /// </summary>
    /// <remarks>
    ///     Enforced as <see cref="MinRating"/> is. Google: "Supported values are float numbers in the range
    ///     <c>0.0</c> (low) to <c>5.0</c> (high)."
    /// </remarks>
    public const decimal MaxRating = 5.0m;

    /// <summary>
    /// Private member to hold the URL of the video thumbnail.
    /// </summary>
    private Uri? videoThumbnailLocation;

    /// <summary>
    /// Private member to hold the title of the video.
    /// </summary>
    private string videoTitle = string.Empty;

    /// <summary>
    /// Private member to hold the tags for the video.
    /// </summary>
    private readonly List<string> videoTags = [];

    /// <summary>
    /// Private member to hold the identifiers for the video.
    /// </summary>
    private readonly List<SitemapVideoId> videoIdentifiers = [];

    /// <summary>
    /// Private member to hold the content segments for the video.
    /// </summary>
    private readonly List<SitemapVideoSegment> videoContentSegments = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideo"/> class.
    /// </summary>
    public SitemapVideo()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideo"/> class with required properties.
    /// </summary>
    /// <param name="thumbnailLocation">The URL of the video thumbnail image.</param>
    /// <param name="title">The title of the video.</param>
    /// <param name="description">A description of the video.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="thumbnailLocation"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="title"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="title"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="description"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="description"/> is an empty string.</exception>
    public SitemapVideo(Uri thumbnailLocation, string title, string description)
    {
        ArgumentNullException.ThrowIfNull(thumbnailLocation);
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentException.ThrowIfNullOrEmpty(description);

        this.videoThumbnailLocation = thumbnailLocation;
        this.Title = title;
        this.Description = description;
    }

    /// <summary>
    /// Gets or sets the URL pointing to the video thumbnail image file.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> that represents the URL of the video thumbnail, or <see langword="null"/> if
    ///     none was specified. Required by the specification.
    /// </value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? ThumbnailLocation
    {
        get => videoThumbnailLocation;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            videoThumbnailLocation = value;
        }
    }

    /// <summary>
    /// Gets or sets the title of the video.
    /// </summary>
    /// <value>
    ///     The title, trimmed and truncated to <see cref="MaxTitleLength"/> characters. The default value is
    ///     an <i>empty</i> string. Required by the specification.
    /// </value>
    /// <remarks>
    ///     Truncation is silent, on assignment and on load alike, and there is no way to detect afterwards
    ///     that it happened. Google asks that the title match the one on the page; check the length before
    ///     assigning if that matters.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Title
    {
        get => videoTitle;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            string trimmedValue = value.Trim();
            if (trimmedValue.Length > MaxTitleLength)
            {
                videoTitle = trimmedValue[..MaxTitleLength];
            }
            else
            {
                videoTitle = trimmedValue;
            }
        }
    }

    /// <summary>
    /// Gets or sets the description of the video.
    /// </summary>
    /// <value>
    ///     The description, trimmed and truncated to <see cref="MaxDescriptionLength"/> characters. The
    ///     default value is an <i>empty</i> string. Required by the specification.
    /// </value>
    /// <remarks>Truncated silently, as <see cref="Title"/> is.</remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Description
    {
        get;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            string trimmedValue = value.Trim();
            if (trimmedValue.Length > MaxDescriptionLength)
            {
                field = trimmedValue[..MaxDescriptionLength];
            }
            else
            {
                field = trimmedValue;
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the URL pointing to the actual video media file.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> that represents the URL of the video file itself, or <see langword="null"/>
    ///     if none was specified.
    /// </value>
    /// <remarks>
    ///     Individually optional, but at least one of <see cref="ContentLocation"/> and
    ///     <see cref="PlayerLocation"/> is required and Google recommends this one. Neither the setter nor
    ///     <see cref="WriteTo(XmlWriter, string)"/> checks that: a <see cref="SitemapVideo"/> with both left
    ///     <see langword="null"/> writes out cleanly and is rejected downstream.
    /// </remarks>
    public Uri? ContentLocation { get; set; }

    /// <summary>
    /// Gets or sets the URL pointing to a player for the video.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> that represents the URL of a player for the video, or <see langword="null"/>
    ///     if none was specified.
    /// </value>
    /// <remarks>
    ///     The alternative to <see cref="ContentLocation"/>; see that property for the requirement the pair
    ///     jointly carries. The <c>allow_embed</c> and <c>autoplay</c> attributes this element used to take
    ///     were withdrawn in 2022 and are neither read nor written.
    /// </remarks>
    public Uri? PlayerLocation { get; set; }

    /// <summary>
    /// Gets or sets the duration of the video in seconds.
    /// </summary>
    /// <value>
    ///     Seconds, between <see cref="MinDuration"/> and <see cref="MaxDuration"/> inclusive, or
    ///     <see langword="null"/> if none was specified.
    /// </value>
    /// <remarks>
    ///     The range is enforced here but not in <see cref="Load"/>: assigning an out-of-range value is a
    ///     programming error and throws, whereas a feed carrying one is untrusted input and has its duration
    ///     skipped rather than costing the whole document.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is outside <see cref="MinDuration"/>..<see cref="MaxDuration"/>.</exception>
    public int? Duration
    {
        get;

        set
        {
            if (value is int duration)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(duration, MinDuration, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(duration, MaxDuration, nameof(value));
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the date after which the video is no longer available.
    /// </summary>
    /// <value>The expiry instant, or <see langword="null"/> if the video does not expire.</value>
    /// <remarks>
    ///     Loading converts to UTC, so the offset a publisher wrote is not what a round-trip writes back —
    ///     <c>2024-01-15T10:00:00-05:00</c> returns as <c>2024-01-15T15:00:00+00:00</c>. The instant is
    ///     preserved; the wall-clock text is not.
    /// </remarks>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the rating of the video.
    /// </summary>
    /// <value>
    ///     A rating between <see cref="MinRating"/> and <see cref="MaxRating"/> inclusive, or
    ///     <see langword="null"/> if none was specified. Written to one decimal place.
    /// </value>
    /// <remarks>Enforced here and skipped on load, exactly as <see cref="Duration"/> is.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is outside <see cref="MinRating"/>..<see cref="MaxRating"/>.</exception>
    public decimal? Rating
    {
        get;

        set
        {
            if (value is decimal rating)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(rating, MinRating, nameof(value));
                ArgumentOutOfRangeException.ThrowIfGreaterThan(rating, MaxRating, nameof(value));
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the number of times the video has been viewed.
    /// </summary>
    /// <value>The view count, or <see langword="null"/> if none was specified.</value>
    public int? ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the date the video was first published.
    /// </summary>
    /// <value>The publication instant, or <see langword="null"/> if none was specified.</value>
    /// <remarks>Normalised to UTC on load, as <see cref="ExpirationDate"/> is.</remarks>
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is appropriate for all audiences.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if the video is suitable for all audiences; otherwise,
    ///     <see langword="false"/>. The default value is <see langword="true"/>.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     <c>video:family_friendly</c> carries <c>yes</c> or <c>no</c>, and this is a <see cref="bool"/>
    ///     rather than a <see cref="Nullable{T}"/>, so an absent element and an explicit <c>yes</c> land on
    ///     the same value. <see cref="WriteTo(XmlWriter, string)"/> writes the element only when the answer
    ///     is <c>no</c>, which means a source document that spelled out <c>yes</c> loses the element on a
    ///     round-trip. The meaning survives — the default is the same — but the bytes do not.
    ///     </para>
    ///     <para>
    ///     Parsing is lenient in one direction: any value that is not <c>yes</c>, including a misspelling,
    ///     reads as <see langword="false"/> and so marks the video as unsuitable for all audiences.
    ///     </para>
    /// </remarks>
    public bool FamilyFriendly { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a subscription is required to view the video.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if a subscription is required; otherwise, <see langword="false"/>. The
    ///     default value is <see langword="false"/>.
    /// </value>
    /// <remarks>Written only when <see langword="true"/>; see <see cref="FamilyFriendly"/>.</remarks>
    public bool RequiresSubscription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is a live stream.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if the video is a live stream; otherwise, <see langword="false"/>. The
    ///     default value is <see langword="false"/>.
    /// </value>
    /// <remarks>Written only when <see langword="true"/>; see <see cref="FamilyFriendly"/>.</remarks>
    public bool Live { get; set; }

    /// <summary>
    /// Gets or sets the name of the video uploader.
    /// </summary>
    /// <value>
    ///     The uploader's name, trimmed and truncated to <see cref="MaxUploaderLength"/> characters. The
    ///     default value is an <i>empty</i> string.
    /// </value>
    /// <remarks>
    ///     Unlike <see cref="Title"/>, this setter accepts <see langword="null"/> and an empty string, both
    ///     of which clear the value rather than throwing.
    /// </remarks>
    public string Uploader
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
                string trimmedValue = value.Trim();
                if (trimmedValue.Length > MaxUploaderLength)
                {
                    field = trimmedValue[..MaxUploaderLength];
                }
                else
                {
                    field = trimmedValue;
                }
            }
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the URL of a page with information about the uploader.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> pointing to information about the uploader, or <see langword="null"/> if none
    ///     was specified.
    /// </value>
    /// <remarks>
    ///     The <c>info</c> attribute of <c>video:uploader</c>. It is written only when
    ///     <see cref="Uploader"/> is non-empty, because it has no element of its own to hang on.
    /// </remarks>
    public Uri? UploaderInfo { get; set; }

    /// <summary>
    /// Gets or sets the platforms on which the video can be played.
    /// </summary>
    /// <value>
    ///     The platforms named by <c>video:platform</c>, or <see langword="null"/> if the element was
    ///     absent. <see cref="SitemapVideoPlatform.None"/> means the element was present but named no
    ///     platform this library recognises.
    /// </value>
    /// <remarks>
    ///     Meaningless without <see cref="PlatformRelationship"/>, which decides whether the set is an
    ///     allow-list or a deny-list. Reading one and ignoring the other inverts the restriction.
    /// </remarks>
    public SitemapVideoPlatform? Platform { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Platform"/> names the platforms permitted or the platforms blocked.
    /// </summary>
    /// <value>
    ///     The <c>relationship</c> attribute of <c>video:platform</c>, or <see langword="null"/> if the
    ///     attribute was absent.
    /// </value>
    /// <remarks>
    ///     Parsing treats anything that is not <c>allow</c> as <see cref="SitemapVideoRelationship.Deny"/>,
    ///     so a typo produces the restrictive reading rather than the permissive one.
    /// </remarks>
    public SitemapVideoRelationship? PlatformRelationship { get; set; }

    /// <summary>
    /// Gets or sets the countries in which the video may or may not be played.
    /// </summary>
    /// <value>
    ///     A space-delimited list of ISO 3166 country codes, or <see langword="null"/> if none was
    ///     specified. Stored as written; the codes are neither split nor validated.
    /// </value>
    /// <remarks>
    ///     Paired with <see cref="RestrictionRelationship"/> in the same way <see cref="Platform"/> is
    ///     paired with <see cref="PlatformRelationship"/>.
    /// </remarks>
    public string? Restriction { get; set; }

    /// <summary>
    /// Gets or sets whether <see cref="Restriction"/> names the countries permitted or the countries blocked.
    /// </summary>
    /// <value>
    ///     The <c>relationship</c> attribute of <c>video:restriction</c>, or <see langword="null"/> if the
    ///     attribute was absent.
    /// </value>
    /// <remarks>Anything that is not <c>allow</c> reads as <see cref="SitemapVideoRelationship.Deny"/>.</remarks>
    public SitemapVideoRelationship? RestrictionRelationship { get; set; }

    /// <summary>
    /// Gets the tags describing the video.
    /// </summary>
    /// <value>
    ///     Up to <see cref="MaxTagCount"/> tags. The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     The cap binds where it is applied, not where the list is held: loading stops after
    ///     <see cref="MaxTagCount"/> tags and writing stops after <see cref="MaxTagCount"/> non-empty ones,
    ///     but the collection itself will hold as many as you add.
    /// </remarks>
    public IList<string> Tags => videoTags;

    /// <summary>
    /// Gets the external identifiers for the video.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="SitemapVideoId"/> objects. The default value is an <i>empty</i>
    ///     collection.
    /// </value>
    public IList<SitemapVideoId> Identifiers => videoIdentifiers;

    /// <summary>
    /// Gets the individual media files the video is split across.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="SitemapVideoSegment"/> objects. The default value is an <i>empty</i>
    ///     collection.
    /// </value>
    public IList<SitemapVideoSegment> ContentSegments => videoContentSegments;

    /// <summary>
    /// Initializes the video using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapVideo"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed XML namespaces.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapVideo"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        wasLoaded |= LoadRequiredProperties(source, manager);
        wasLoaded |= LoadOptionalProperties(source, manager);

        return wasLoaded;
    }

    /// <summary>
    /// Loads the required properties from the source.
    /// </summary>
    private bool LoadRequiredProperties(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? thumbnailNavigator = source.SelectChildElement("video", "thumbnail_loc", manager);
        XPathNavigator? titleNavigator = source.SelectChildElement("video", "title", manager);
        XPathNavigator? descriptionNavigator = source.SelectChildElement("video", "description", manager);

        if (thumbnailNavigator is not null && !string.IsNullOrEmpty(thumbnailNavigator.Value))
        {
            if (Uri.TryCreate(thumbnailNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? thumbnailUrl))
            {
                this.videoThumbnailLocation = thumbnailUrl;
                wasLoaded = true;
            }
        }

        if (titleNavigator is not null && !string.IsNullOrEmpty(titleNavigator.Value))
        {
            string trimmedTitle = titleNavigator.Value.Trim();
            this.videoTitle = trimmedTitle.Length > MaxTitleLength ? trimmedTitle[..MaxTitleLength] : trimmedTitle;
            wasLoaded = true;
        }

        if (descriptionNavigator is not null && !string.IsNullOrEmpty(descriptionNavigator.Value))
        {
            this.Description = descriptionNavigator.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads the optional properties from the source.
    /// </summary>
    private bool LoadOptionalProperties(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? contentLocNavigator = source.SelectChildElement("video", "content_loc", manager);
        XPathNavigator? playerLocNavigator = source.SelectChildElement("video", "player_loc", manager);
        XPathNavigator? durationNavigator = source.SelectChildElement("video", "duration", manager);
        XPathNavigator? expirationNavigator = source.SelectChildElement("video", "expiration_date", manager);
        XPathNavigator? ratingNavigator = source.SelectChildElement("video", "rating", manager);
        XPathNavigator? viewCountNavigator = source.SelectChildElement("video", "view_count", manager);
        XPathNavigator? publicationNavigator = source.SelectChildElement("video", "publication_date", manager);
        XPathNavigator? familyFriendlyNavigator = source.SelectChildElement("video", "family_friendly", manager);
        XPathNavigator? subscriptionNavigator = source.SelectChildElement("video", "requires_subscription", manager);
        XPathNavigator? liveNavigator = source.SelectChildElement("video", "live", manager);
        XPathNavigator? uploaderNavigator = source.SelectChildElement("video", "uploader", manager);
        XPathNavigator? platformNavigator = source.SelectChildElement("video", "platform", manager);
        XPathNavigator? restrictionNavigator = source.SelectChildElement("video", "restriction", manager);
        XPathNodeIterator tagIterator = source.SelectChildElements("video", "tag", manager);

        if (contentLocNavigator is not null && Uri.TryCreate(contentLocNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? contentUrl))
        {
            this.ContentLocation = contentUrl;
            wasLoaded = true;
        }

        if (playerLocNavigator is not null && Uri.TryCreate(playerLocNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? playerUrl))
        {
            this.PlayerLocation = playerUrl;
            wasLoaded = true;
        }

        // A duration outside the schema's facets is skipped rather than rejected: a feed is untrusted input
        // and one bad element must not cost the document.
        if (durationNavigator is not null
            && int.TryParse(durationNavigator.Value, out int duration)
            && duration is >= MinDuration and <= MaxDuration)
        {
            this.Duration = duration;
            wasLoaded = true;
        }

        if (expirationNavigator is not null && DateTime.TryParse(expirationNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime expiration))
        {
            this.ExpirationDate = expiration;
            wasLoaded = true;
        }

        if (ratingNavigator is not null
            && decimal.TryParse(ratingNavigator.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal rating)
            && rating is >= MinRating and <= MaxRating)
        {
            this.Rating = rating;
            wasLoaded = true;
        }

        if (viewCountNavigator is not null && int.TryParse(viewCountNavigator.Value, out int viewCount))
        {
            this.ViewCount = viewCount;
            wasLoaded = true;
        }

        if (publicationNavigator is not null && DateTime.TryParse(publicationNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime publication))
        {
            this.PublicationDate = publication;
            wasLoaded = true;
        }

        if (familyFriendlyNavigator is not null)
        {
            this.FamilyFriendly = string.Equals(familyFriendlyNavigator.Value, "yes", StringComparison.OrdinalIgnoreCase);
            wasLoaded = true;
        }

        if (subscriptionNavigator is not null)
        {
            this.RequiresSubscription = string.Equals(subscriptionNavigator.Value, "yes", StringComparison.OrdinalIgnoreCase);
            wasLoaded = true;
        }

        if (liveNavigator is not null)
        {
            this.Live = string.Equals(liveNavigator.Value, "yes", StringComparison.OrdinalIgnoreCase);
            wasLoaded = true;
        }

        if (uploaderNavigator is not null)
        {
            this.Uploader = uploaderNavigator.Value;
            string uploaderInfoAttr = uploaderNavigator.GetAttribute("info", string.Empty);
            if (!string.IsNullOrEmpty(uploaderInfoAttr) && Uri.TryCreate(uploaderInfoAttr, UriKind.RelativeOrAbsolute, out Uri? uploaderInfo))
            {
                this.UploaderInfo = uploaderInfo;
            }
            wasLoaded = true;
        }

        if (platformNavigator is not null)
        {
            wasLoaded |= LoadPlatform(platformNavigator);
        }

        if (restrictionNavigator is not null)
        {
            wasLoaded |= LoadRestriction(restrictionNavigator);
        }

        if (tagIterator is { Count: > 0 })
        {
            while (tagIterator.MoveNext() && this.videoTags.Count < MaxTagCount)
            {
                XPathNavigator? tagNode = tagIterator.Current;
                if (tagNode is not null && !string.IsNullOrEmpty(tagNode.Value))
                {
                    this.videoTags.Add(tagNode.Value.Trim());
                    wasLoaded = true;
                }
            }
        }

        XPathNodeIterator idIterator = source.SelectChildElements("video", "id", manager);
        if (idIterator is { Count: > 0 })
        {
            while (idIterator.MoveNext())
            {
                XPathNavigator? idNode = idIterator.Current;
                if (idNode is null)
                {
                    continue;
                }

                SitemapVideoId videoId = new();
                if (videoId.Load(idNode))
                {
                    this.videoIdentifiers.Add(videoId);
                    wasLoaded = true;
                }
            }
        }

        XPathNodeIterator segmentIterator = source.SelectChildElements("video", "content_segment_loc", manager);
        if (segmentIterator is { Count: > 0 })
        {
            while (segmentIterator.MoveNext())
            {
                XPathNavigator? segmentNode = segmentIterator.Current;
                if (segmentNode is null)
                {
                    continue;
                }

                SitemapVideoSegment segment = new();
                if (segment.Load(segmentNode))
                {
                    this.videoContentSegments.Add(segment);
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads platform information from the navigator.
    /// </summary>
    private bool LoadPlatform(XPathNavigator platformNavigator)
    {
        string relationshipAttr = platformNavigator.GetAttribute("relationship", string.Empty);
        if (!string.IsNullOrEmpty(relationshipAttr))
        {
            this.PlatformRelationship = string.Equals(relationshipAttr, "allow", StringComparison.OrdinalIgnoreCase)
                ? SitemapVideoRelationship.Allow
                : SitemapVideoRelationship.Deny;
        }

        string platformValue = platformNavigator.Value;
        if (!string.IsNullOrEmpty(platformValue))
        {
            SitemapVideoPlatform platform = SitemapVideoPlatform.None;
            string[] platforms = platformValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (string p in platforms)
            {
                if (string.Equals(p, "web", StringComparison.OrdinalIgnoreCase))
                {
                    platform |= SitemapVideoPlatform.Web;
                }
                else if (string.Equals(p, "mobile", StringComparison.OrdinalIgnoreCase))
                {
                    platform |= SitemapVideoPlatform.Mobile;
                }
                else if (string.Equals(p, "tv", StringComparison.OrdinalIgnoreCase))
                {
                    platform |= SitemapVideoPlatform.Tv;
                }
            }
            this.Platform = platform;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Loads restriction information from the navigator.
    /// </summary>
    private bool LoadRestriction(XPathNavigator restrictionNavigator)
    {
        string relationshipAttr = restrictionNavigator.GetAttribute("relationship", string.Empty);
        if (!string.IsNullOrEmpty(relationshipAttr))
        {
            this.RestrictionRelationship = string.Equals(relationshipAttr, "allow", StringComparison.OrdinalIgnoreCase)
                ? SitemapVideoRelationship.Allow
                : SitemapVideoRelationship.Deny;
        }

        if (!string.IsNullOrEmpty(restrictionNavigator.Value))
        {
            this.Restriction = restrictionNavigator.Value.Trim();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Writes the video to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the video will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("video", xmlNamespace);

        // Required elements
        if (this.ThumbnailLocation is not null)
        {
            writer.WriteElementString("thumbnail_loc", xmlNamespace, this.ThumbnailLocation.ToString());
        }

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteElementString("title", xmlNamespace, this.Title);
        }

        if (!string.IsNullOrEmpty(this.Description))
        {
            writer.WriteElementString("description", xmlNamespace, this.Description);
        }

        // Optional elements
        WriteOptionalElements(writer, xmlNamespace);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Writes optional elements to the writer.
    /// </summary>
    private void WriteOptionalElements(XmlWriter writer, string xmlNamespace)
    {
        if (this.ContentLocation is not null)
        {
            writer.WriteElementString("content_loc", xmlNamespace, this.ContentLocation.ToString());
        }

        if (this.PlayerLocation is not null)
        {
            writer.WriteElementString("player_loc", xmlNamespace, this.PlayerLocation.ToString());
        }

        if (this.Duration.HasValue)
        {
            writer.WriteElementString("duration", xmlNamespace, this.Duration.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (this.ExpirationDate.HasValue)
        {
            writer.WriteElementString("expiration_date", xmlNamespace, this.ExpirationDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }

        if (this.Rating.HasValue)
        {
            writer.WriteElementString("rating", xmlNamespace, this.Rating.Value.ToString("F1", CultureInfo.InvariantCulture));
        }

        if (this.ViewCount.HasValue)
        {
            writer.WriteElementString("view_count", xmlNamespace, this.ViewCount.Value.ToString(CultureInfo.InvariantCulture));
        }

        if (this.PublicationDate.HasValue)
        {
            writer.WriteElementString("publication_date", xmlNamespace, this.PublicationDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }

        if (!this.FamilyFriendly)
        {
            writer.WriteElementString("family_friendly", xmlNamespace, "no");
        }

        if (this.RequiresSubscription)
        {
            writer.WriteElementString("requires_subscription", xmlNamespace, "yes");
        }

        if (this.Live)
        {
            writer.WriteElementString("live", xmlNamespace, "yes");
        }

        WriteUploaderElement(writer, xmlNamespace);
        WritePlatformElement(writer, xmlNamespace);
        WriteRestrictionElement(writer, xmlNamespace);
        WriteTagElements(writer, xmlNamespace);
        WriteIdentifierElements(writer, xmlNamespace);
        WriteContentSegmentElements(writer, xmlNamespace);
    }

    /// <summary>
    /// Writes the uploader element if present.
    /// </summary>
    private void WriteUploaderElement(XmlWriter writer, string xmlNamespace)
    {
        if (!string.IsNullOrEmpty(this.Uploader))
        {
            writer.WriteStartElement("uploader", xmlNamespace);
            if (this.UploaderInfo is not null)
            {
                writer.WriteAttributeString("info", this.UploaderInfo.ToString());
            }
            writer.WriteString(this.Uploader);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Writes the platform element if present.
    /// </summary>
    private void WritePlatformElement(XmlWriter writer, string xmlNamespace)
    {
        if (this.Platform.HasValue && this.Platform.Value != SitemapVideoPlatform.None)
        {
            writer.WriteStartElement("platform", xmlNamespace);
            if (this.PlatformRelationship.HasValue)
            {
                writer.WriteAttributeString("relationship", this.PlatformRelationship.Value == SitemapVideoRelationship.Allow ? "allow" : "deny");
            }

            List<string> platforms = [];
            if ((this.Platform.Value & SitemapVideoPlatform.Web) == SitemapVideoPlatform.Web)
            {
                platforms.Add("web");
            }
            if ((this.Platform.Value & SitemapVideoPlatform.Mobile) == SitemapVideoPlatform.Mobile)
            {
                platforms.Add("mobile");
            }
            if ((this.Platform.Value & SitemapVideoPlatform.Tv) == SitemapVideoPlatform.Tv)
            {
                platforms.Add("tv");
            }

            writer.WriteString(string.Join(" ", platforms));
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Writes the restriction element if present.
    /// </summary>
    private void WriteRestrictionElement(XmlWriter writer, string xmlNamespace)
    {
        if (!string.IsNullOrEmpty(this.Restriction))
        {
            writer.WriteStartElement("restriction", xmlNamespace);
            if (this.RestrictionRelationship.HasValue)
            {
                writer.WriteAttributeString("relationship", this.RestrictionRelationship.Value == SitemapVideoRelationship.Allow ? "allow" : "deny");
            }
            writer.WriteString(this.Restriction);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// Writes the tag elements.
    /// </summary>
    private void WriteTagElements(XmlWriter writer, string xmlNamespace)
    {
        int count = 0;
        foreach (string tag in this.Tags)
        {
            if (count >= MaxTagCount)
            {
                break;
            }
            if (!string.IsNullOrEmpty(tag))
            {
                writer.WriteElementString("tag", xmlNamespace, tag);
                count++;
            }
        }
    }

    /// <summary>
    /// Writes the identifier elements.
    /// </summary>
    private void WriteIdentifierElements(XmlWriter writer, string xmlNamespace)
    {
        foreach (SitemapVideoId identifier in this.Identifiers)
        {
            identifier.WriteTo(writer, xmlNamespace);
        }
    }

    /// <summary>
    /// Writes the content segment elements.
    /// </summary>
    private void WriteContentSegmentElements(XmlWriter writer, string xmlNamespace)
    {
        foreach (SitemapVideoSegment segment in this.ContentSegments)
        {
            segment.WriteTo(writer, xmlNamespace);
        }
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <remarks>
    ///     <para>
    ///     <b>Every member participates.</b> <see cref="Title"/> dominates, then
    ///     <see cref="ThumbnailLocation"/> and <see cref="Description"/> — the three Google requires — and
    ///     then the optional members in declaration order, ending with <see cref="Tags"/>,
    ///     <see cref="Identifiers"/> and <see cref="ContentSegments"/> compared element by element.
    ///     </para>
    ///     <para>
    ///     It used to fold only the first three, which made two videos differing in content location,
    ///     duration or tags report themselves equal — so a <see cref="HashSet{T}"/> or a
    ///     <c>Distinct()</c> silently discarded one of them. <see cref="GetHashCode"/> was widened in the
    ///     same change and must stay in step: a comparison finer than the hash is a lookup miss.
    ///     </para>
    /// </remarks>
    public int CompareTo(SitemapVideo? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = CompareLocation(this.ThumbnailLocation, other.ThumbnailLocation);
        if (result == 0) result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = CompareLocation(this.ContentLocation, other.ContentLocation);
        if (result == 0) result = CompareLocation(this.PlayerLocation, other.PlayerLocation);
        if (result == 0) result = Nullable.Compare(this.Duration, other.Duration);
        if (result == 0) result = Nullable.Compare(this.ExpirationDate, other.ExpirationDate);
        if (result == 0) result = Nullable.Compare(this.Rating, other.Rating);
        if (result == 0) result = Nullable.Compare(this.ViewCount, other.ViewCount);
        if (result == 0) result = Nullable.Compare(this.PublicationDate, other.PublicationDate);
        if (result == 0) result = this.FamilyFriendly.CompareTo(other.FamilyFriendly);
        if (result == 0) result = this.RequiresSubscription.CompareTo(other.RequiresSubscription);
        if (result == 0) result = this.Live.CompareTo(other.Live);
        if (result == 0) result = string.Compare(this.Uploader, other.Uploader, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = CompareLocation(this.UploaderInfo, other.UploaderInfo);
        if (result == 0) result = Nullable.Compare((int?)this.Platform, (int?)other.Platform);
        if (result == 0) result = Nullable.Compare((int?)this.PlatformRelationship, (int?)other.PlatformRelationship);
        if (result == 0) result = string.Compare(this.Restriction, other.Restriction, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Nullable.Compare((int?)this.RestrictionRelationship, (int?)other.RestrictionRelationship);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Tags, other.Tags, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Identifiers, other.Identifiers);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.ContentSegments, other.ContentSegments);

        return result;
    }

    /// <summary>
    /// Orders two optional locations, disregarding case and treating an absent one as the lesser.
    /// </summary>
    /// <param name="first">The first location, which may be <see langword="null"/>.</param>
    /// <param name="second">The second location, which may be <see langword="null"/>.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the two locations.</returns>
    /// <remarks>
    ///     Disregarding case is what keeps this in step with <see cref="HashCodeUtility.Component(Uri)"/>,
    ///     which hashes the URI's text under <see cref="StringComparison.OrdinalIgnoreCase"/>. A relative
    ///     URI — which <see cref="Load"/> accepts — is compared rather than rejected.
    /// </remarks>
    private static int CompareLocation(Uri? first, Uri? second) =>
        Uri.Compare(first, second, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the specified <see cref="SitemapVideo"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapVideo"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapVideo"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapVideo? other)
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
    public override bool Equals(object? obj) => obj is SitemapVideo other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     Folds the same members <see cref="CompareTo(SitemapVideo)"/> walks, with the three collections
    ///     taken element by element. Passing a collection itself to
    ///     <see cref="HashCodeUtility.Component{T}(T)"/> would hash the list reference, so two videos that
    ///     <see cref="CompareTo(SitemapVideo)"/> reports as equal would hash differently — which is the
    ///     defect that widening the comparison alone would have reintroduced.
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Title));
        hash.Add(HashCodeUtility.Component(this.ThumbnailLocation));
        hash.Add(HashCodeUtility.Component(this.Description));
        hash.Add(HashCodeUtility.Component(this.ContentLocation));
        hash.Add(HashCodeUtility.Component(this.PlayerLocation));
        hash.Add(HashCodeUtility.Component(this.Duration));
        hash.Add(HashCodeUtility.Component(this.ExpirationDate));
        hash.Add(HashCodeUtility.Component(this.Rating));
        hash.Add(HashCodeUtility.Component(this.ViewCount));
        hash.Add(HashCodeUtility.Component(this.PublicationDate));
        hash.Add(HashCodeUtility.Component(this.FamilyFriendly));
        hash.Add(HashCodeUtility.Component(this.RequiresSubscription));
        hash.Add(HashCodeUtility.Component(this.Live));
        hash.Add(HashCodeUtility.Component(this.Uploader));
        hash.Add(HashCodeUtility.Component(this.UploaderInfo));
        hash.Add(HashCodeUtility.Component(this.Platform));
        hash.Add(HashCodeUtility.Component(this.PlatformRelationship));
        hash.Add(HashCodeUtility.Component(this.Restriction));
        hash.Add(HashCodeUtility.Component(this.RestrictionRelationship));

        foreach (string tag in this.Tags)
        {
            hash.Add(HashCodeUtility.Component(tag));
        }

        foreach (SitemapVideoId identifier in this.Identifiers)
        {
            hash.Add(HashCodeUtility.Component(identifier));
        }

        foreach (SitemapVideoSegment segment in this.ContentSegments)
        {
            hash.Add(HashCodeUtility.Component(segment));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideo"/>.
    /// </summary>
    /// <returns>The <see cref="Title"/>, which is an <i>empty</i> string until one is set.</returns>
    public override string ToString() => this.Title;

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapVideo? first, SitemapVideo? second)
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
    public static bool operator !=(SitemapVideo? first, SitemapVideo? second) => !(first == second);

}