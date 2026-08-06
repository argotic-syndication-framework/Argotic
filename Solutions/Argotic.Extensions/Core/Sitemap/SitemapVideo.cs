using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a video in a sitemap video extension.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapVideo"/> class represents video information that can be included in a sitemap
///         to help search engines discover and understand video content on your site. This conforms to the
///         Google Video Sitemap extension specification version 1.1.
///     </para>
///     <para>
///         <b>Deprecated Elements (May 2022):</b><br/>
///         The following elements were deprecated by Google and are intentionally not implemented:
///         <list type="bullet">
///             <item><c>video:price</c> - Video purchase/rental pricing</item>
///             <item><c>video:category</c> - Video category (max 256 chars)</item>
///             <item><c>video:gallery_loc</c> - Gallery URL with title attribute</item>
///             <item><c>video:tvshow</c> - TV show metadata</item>
///             <item><c>player_loc/@allow_embed</c> - Embed permission attribute</item>
///             <item><c>player_loc/@autoplay</c> - Autoplay parameter attribute</item>
///         </list>
///         See <see href="https://developers.google.com/search/blog/2022/05/spring-cleaning-sitemap-extensions">Google's announcement</see>.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
public class SitemapVideo : IComparable<SitemapVideo>, IEquatable<SitemapVideo>, IComparisonOperators
{
    /// <summary>
    /// Maximum length for video title per Google Video Sitemap 1.1 specification.
    /// </summary>
    /// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd"/>
    public const int MaxTitleLength = 100;

    /// <summary>
    /// The maximum length allowed for the description field.
    /// </summary>
    public const int MaxDescriptionLength = 2048;

    /// <summary>
    /// The maximum length allowed for the uploader field.
    /// </summary>
    public const int MaxUploaderLength = 255;

    /// <summary>
    /// The maximum number of tags allowed.
    /// </summary>
    public const int MaxTagCount = 32;

    /// <summary>
    /// The minimum allowed duration in seconds.
    /// </summary>
    public const int MinDuration = 1;

    /// <summary>
    /// The maximum allowed duration in seconds.
    /// </summary>
    public const int MaxDuration = 28_800;

    /// <summary>
    /// The minimum allowed rating value.
    /// </summary>
    public const decimal MinRating = 0.0m;

    /// <summary>
    /// The maximum allowed rating value.
    /// </summary>
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
    /// <exception cref="ArgumentNullException">The <paramref name="thumbnailLocation"/> is a null reference.</exception>
    /// <exception cref="ArgumentException">The <paramref name="title"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">The <paramref name="description"/> is null or empty.</exception>
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
    /// <value>A <see cref="Uri"/> that represents the URL of the video thumbnail. This is a required property.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
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
    /// <value>The title of the video, limited to 100 characters. This is a required property.</value>
    /// <remarks>
    ///     The title must be limited to 100 characters per the Google Video Sitemap 1.1 specification.
    ///     If a longer value is provided, it will be truncated to the maximum allowed length.
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
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
    /// <value>A description of the video, limited to 2048 characters. This is a required property.</value>
    /// <remarks>
    ///     The description must be limited to 2048 characters. If a longer value is provided,
    ///     it will be truncated to the maximum allowed length.
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
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
    /// <value>A <see cref="Uri"/> that represents the URL of the video content. Optional.</value>
    /// <remarks>
    ///     Either <see cref="ContentLocation"/> or <see cref="PlayerLocation"/> must be specified.
    /// </remarks>
    public Uri? ContentLocation { get; set; }

    /// <summary>
    /// Gets or sets the URL pointing to a player for the video.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the video player. Optional.</value>
    /// <remarks>
    ///     Either <see cref="ContentLocation"/> or <see cref="PlayerLocation"/> must be specified.
    /// </remarks>
    public Uri? PlayerLocation { get; set; }

    /// <summary>
    /// Gets or sets the duration of the video in seconds.
    /// </summary>
    /// <value>The duration of the video in seconds, between 1 and 28800 (8 hours). Optional.</value>
    /// <remarks>
    ///     The duration must be between 1 and 28800 seconds (8 hours).
    /// </remarks>
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the date after which the video will no longer be available.
    /// </summary>
    /// <value>A <see cref="DateTime"/> representing when the video expires. Optional.</value>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the rating of the video.
    /// </summary>
    /// <value>A rating value between 0.0 and 5.0. Optional.</value>
    public decimal? Rating { get; set; }

    /// <summary>
    /// Gets or sets the number of times the video has been viewed.
    /// </summary>
    /// <value>The view count of the video. Optional.</value>
    public int? ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the date the video was first published.
    /// </summary>
    /// <value>A <see cref="DateTime"/> representing when the video was published. Optional.</value>
    public DateTime? PublicationDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is appropriate for all audiences.
    /// </summary>
    /// <value><b>true</b> if the video is family friendly; otherwise, <b>false</b>. Default is <b>true</b>.</value>
    public bool FamilyFriendly { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether a subscription is required to view the video.
    /// </summary>
    /// <value><b>true</b> if a subscription is required; otherwise, <b>false</b>. Default is <b>false</b>.</value>
    public bool RequiresSubscription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the video is a live stream.
    /// </summary>
    /// <value><b>true</b> if the video is a live stream; otherwise, <b>false</b>. Default is <b>false</b>.</value>
    public bool Live { get; set; }

    /// <summary>
    /// Gets or sets the name of the video uploader.
    /// </summary>
    /// <value>The name of the uploader, limited to 255 characters. Optional.</value>
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
    /// <value>A <see cref="Uri"/> pointing to information about the uploader. Optional.</value>
    public Uri? UploaderInfo { get; set; }

    /// <summary>
    /// Gets or sets the platforms on which the video can be played.
    /// </summary>
    /// <value>A <see cref="SitemapVideoPlatform"/> value indicating allowed platforms. Optional.</value>
    public SitemapVideoPlatform? Platform { get; set; }

    /// <summary>
    /// Gets or sets the relationship type for platform restrictions.
    /// </summary>
    /// <value>A <see cref="SitemapVideoRelationship"/> indicating whether platforms are allowed or denied. Optional.</value>
    public SitemapVideoRelationship? PlatformRelationship { get; set; }

    /// <summary>
    /// Gets or sets the country restriction as a space-delimited list of ISO 3166 country codes.
    /// </summary>
    /// <value>A space-delimited string of ISO 3166 country codes. Optional.</value>
    public string? Restriction { get; set; }

    /// <summary>
    /// Gets or sets the relationship type for country restrictions.
    /// </summary>
    /// <value>A <see cref="SitemapVideoRelationship"/> indicating whether countries are allowed or denied. Optional.</value>
    public SitemapVideoRelationship? RestrictionRelationship { get; set; }

    /// <summary>
    /// Gets the tags associated with the video.
    /// </summary>
    /// <value>A list of tags for the video, limited to 32 tags. Optional.</value>
    public IList<string> Tags => videoTags;

    /// <summary>
    /// Gets the identifiers associated with the video.
    /// </summary>
    /// <value>A list of video identifiers. Optional.</value>
    public IList<SitemapVideoId> Identifiers => videoIdentifiers;

    /// <summary>
    /// Gets the content segment locations for the video.
    /// </summary>
    /// <value>A list of content segment locations. Optional.</value>
    public IList<SitemapVideoSegment> ContentSegments => videoContentSegments;

    /// <summary>
    /// Initializes the video using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapVideo"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed XML namespaces.</param>
    /// <returns><b>true</b> if the <see cref="SitemapVideo"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
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

        if (durationNavigator is not null && int.TryParse(durationNavigator.Value, out int duration))
        {
            this.Duration = duration;
            wasLoaded = true;
        }

        if (expirationNavigator is not null && DateTime.TryParse(expirationNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime expiration))
        {
            this.ExpirationDate = expiration;
            wasLoaded = true;
        }

        if (ratingNavigator is not null && decimal.TryParse(ratingNavigator.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal rating))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference or empty string.</exception>
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
    public int CompareTo(SitemapVideo? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.ThumbnailLocation, other.ThumbnailLocation, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapVideo"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapVideo"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapVideo"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is SitemapVideo other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Title), HashCodeUtility.Component(this.ThumbnailLocation), HashCodeUtility.Component(this.Description));

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideo"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapVideo"/>.</returns>
    public override string ToString() => this.Title ?? string.Empty;

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(SitemapVideo? first, SitemapVideo? second) => !(first == second);

}