using System.Globalization;
using System.Xml;
using System.Xml.XPath;
using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="ITunesSyndicationExtension"/>.
/// </summary>
public class ITunesSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ITunesSyndicationExtensionContext"/> class.
    /// </summary>
    public ITunesSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets the name of the artist of this podcast.
    /// </summary>
    /// <value>The name of the artist of this podcast.</value>
    public string Author
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets the categories to which this podcast belongs.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="ITunesCategory"/> objects that represent the categories to which this podcast belongs. The default value is an <i>empty</i> collection.
    /// </value>
    public IList<ITunesCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets the total duration of this podcast.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> that represents total duration of this podcast. The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no duration was specified.</value>
    public TimeSpan Duration { get; set; } = TimeSpan.MinValue;

    /// <summary>
    /// Gets or sets the explicit language or adult content advisory information for this podcast.
    /// </summary>
    /// <value>
    ///     An <see cref="ITunesExplicitMaterial"/> enumeration value that indicates whether the podcast contains explicit material.
    ///     The default value is <see cref="ITunesExplicitMaterial.None"/>.
    /// </value>
    public ITunesExplicitMaterial ExplicitMaterial { get; set; } = ITunesExplicitMaterial.None;

    /// <summary>
    /// Gets or sets the title Apple Podcasts shows for this item, in place of its ordinary title.
    /// </summary>
    /// <value>The iTunes-specific title, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     Distinct from the item's own <c>title</c>, and usually shorter: a publisher writes the full
    ///     headline in <c>title</c> and a clean episode name here. <b>4,544</b> occurrences in the
    ///     real-world corpus, which makes it one of the most common extension elements there is.
    /// </remarks>
    public string Title
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the episode number within its season.
    /// </summary>
    /// <value>The episode number, or <see langword="null"/> if none was specified.</value>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <i>one</i>.</exception>
    public int? Episode
    {
        get;
        set
        {
            if (value.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.Value, 1);
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the season this episode belongs to.
    /// </summary>
    /// <value>The season number, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     Defined by the same paragraph of Apple's specification as <see cref="Episode"/> and included
    ///     for that reason. Unlike every other member added alongside it, this one has <b>no</b>
    ///     occurrences in the real-world corpus — a fact recorded rather than hidden, because an
    ///     <see cref="Episode"/> without a <see cref="Season"/> would be a lopsided API.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than <i>one</i>.</exception>
    public int? Season
    {
        get;
        set
        {
            if (value.HasValue)
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value.Value, 1);
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the kind of episode this item is.
    /// </summary>
    /// <value>
    ///     An <see cref="ITunesEpisodeType"/> enumeration value that indicates the kind of episode.
    ///     The default value is <see cref="ITunesEpisodeType.None"/>.
    /// </value>
    public ITunesEpisodeType EpisodeType { get; set; } = ITunesEpisodeType.None;

    /// <summary>
    /// Gets or sets how this podcast's episodes are meant to be consumed.
    /// </summary>
    /// <value>
    ///     An <see cref="ITunesPodcastType"/> enumeration value that indicates the presentation order.
    ///     The default value is <see cref="ITunesPodcastType.None"/>.
    /// </value>
    public ITunesPodcastType PodcastType { get; set; } = ITunesPodcastType.None;

    /// <summary>
    /// Gets or sets a URL that points to the album artwork for this podcast.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a URL that points to the album artwork for this podcast.</value>
    /// <remarks>
    ///     iTunes recommends the use of square images that are at least 600 by 600 pixels.
    ///     iTunes supports images in <i>JPEG</i> and <i>PNG</i> formats.
    ///     The URL <b>must</b> end in ".jpg" or ".png".
    /// </remarks>
    public Uri? Image { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this podcast is blocked from appearing in the iTunes Podcast directory.
    /// </summary>
    /// <value><b>true</b> if this podcast is blocked from appearing in the iTunes Podcast directory; Otherwise, <b>false</b>. The default value is <b>false</b>.</value>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this podcast has finished and will publish no further episodes.
    /// </summary>
    /// <value><b>true</b> if no episode will ever be added to this podcast again; otherwise, <b>false</b>. The default value is <b>false</b>.</value>
    /// <remarks>
    ///     <para>
    ///     Apple's <c>itunes:complete</c>. Setting it tells Apple Podcasts to stop polling the feed,
    ///     and Apple documents the effect as potentially <b>irreversible</b> — a feed marked complete
    ///     may never be able to publish another episode at that URL. It is written only when
    ///     <see langword="true"/> for that reason: the element has no "no" spelling, and emitting one
    ///     unasked would be a change of meaning rather than a round-trip.
    ///     </para>
    ///     <para>
    ///     Channel level, and situational — it appears in none of the 136 documents of the real-world
    ///     corpus, which is what being situational looks like. It is implemented because it is the one
    ///     element of Apple's current specification this library did not model.
    ///     </para>
    /// </remarks>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Gets the search keywords for this podcast.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of strings that allows users to search on a maximum of 12 text keywords.</value>
    public IList<string> Keywords { get; } = [];

    /// <summary>
    /// Gets or sets the URL where this podcast feed has been relocated to.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL where this podcast feed has been relocated to.</value>
    /// <remarks>
    ///     It is recommended that you should maintain the old feed for 48 hours before retiring it. At that point, iTunes will have updated the directory with the new feed URL.
    /// </remarks>
    public Uri? NewFeedUrl { get; set; }

    /// <summary>
    /// Gets or sets information that can be used to contact the owner of this podcast.
    /// </summary>
    /// <value>
    ///     A <see cref="ITunesOwner"/> object that represents information that can be used to contact the owner of this podcast.
    ///     The default value is a <b>null</b> reference.
    /// </value>
    public ITunesOwner? Owner { get; set; }

    /// <summary>
    /// Gets or sets a brief synopsis of this podcast.
    /// </summary>
    /// <value>A brief synopsis of this podcast.</value>
    /// <remarks>
    ///     It is recommended that the subtitle is only a few words long.
    /// </remarks>
    public string Subtitle
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the full description of this podcast.
    /// </summary>
    /// <value>The full description of this podcast.</value>
    public string Summary
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (this.LoadCommon(source, manager))
        {
            wasLoaded = true;
        }

        if (this.LoadOptionals(source, manager))
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
        if (this.NewFeedUrl is not null)
        {
            writer.WriteElementString("new-feed-url", xmlNamespace, this.NewFeedUrl.ToString());
        }

        if (!string.IsNullOrEmpty(this.Subtitle))
        {
            writer.WriteElementString("subtitle", xmlNamespace, this.Subtitle);
        }

        if (!string.IsNullOrEmpty(this.Author))
        {
            writer.WriteElementString("author", xmlNamespace, this.Author);
        }

        if (!string.IsNullOrEmpty(this.Summary))
        {
            writer.WriteElementString("summary", xmlNamespace, this.Summary);
        }

        this.Owner?.WriteTo(writer);

        if (this.Image is not null)
        {
            writer.WriteStartElement("image", xmlNamespace);
            writer.WriteAttributeString("href", this.Image.ToString());
            writer.WriteEndElement();
        }

        if (this.Duration != TimeSpan.MinValue)
        {
            string hours = this.Duration.Hours < 10 ? string.Concat("0", this.Duration.Hours.ToString(NumberFormatInfo.InvariantInfo)) : this.Duration.Hours.ToString(NumberFormatInfo.InvariantInfo);
            string minutes = this.Duration.Minutes < 10 ? string.Concat("0", this.Duration.Minutes.ToString(NumberFormatInfo.InvariantInfo)) : this.Duration.Minutes.ToString(NumberFormatInfo.InvariantInfo);
            string seconds = this.Duration.Seconds < 10 ? string.Concat("0", this.Duration.Seconds.ToString(NumberFormatInfo.InvariantInfo)) : this.Duration.Seconds.ToString(NumberFormatInfo.InvariantInfo);
            string duration = $"{hours}:{minutes}:{seconds}";

            writer.WriteElementString("duration", xmlNamespace, duration);
        }

        if (this.Keywords.Count > 0)
        {
            writer.WriteElementString("keywords", xmlNamespace, string.Join(",", this.Keywords));
        }

        if (this.ExplicitMaterial != ITunesExplicitMaterial.None)
        {
            writer.WriteElementString("explicit", xmlNamespace, ITunesSyndicationExtension.ExplicitMaterialAsString(this.ExplicitMaterial));
        }

        if (this.IsBlocked)
        {
            writer.WriteElementString("block", xmlNamespace, "yes");
        }

        if (this.IsComplete)
        {
            writer.WriteElementString("complete", xmlNamespace, "yes");
        }

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteElementString("title", xmlNamespace, this.Title);
        }

        if (this.Episode.HasValue)
        {
            writer.WriteElementString("episode", xmlNamespace, this.Episode.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.Season.HasValue)
        {
            writer.WriteElementString("season", xmlNamespace, this.Season.Value.ToString(NumberFormatInfo.InvariantInfo));
        }

        if (this.EpisodeType != ITunesEpisodeType.None)
        {
            writer.WriteElementString("episodeType", xmlNamespace, ITunesSyndicationExtension.EpisodeTypeAsString(this.EpisodeType));
        }

        if (this.PodcastType != ITunesPodcastType.None)
        {
            writer.WriteElementString("type", xmlNamespace, ITunesSyndicationExtension.PodcastTypeAsString(this.PodcastType));
        }

        if (this.Categories.Count > 0)
        {
            foreach (ITunesCategory category in this.Categories)
            {
                category.WriteTo(writer);
            }
        }
    }

    /// <summary>
    /// Initializes the common syndication extension information using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private bool LoadCommon(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? authorNavigator = source.SelectChildElement("itunes", "author", manager);
            XPathNavigator? keywordsNavigator = source.SelectChildElement("itunes", "keywords", manager);
            XPathNavigator? newFeedUrlNavigator = source.SelectChildElement("itunes", "new-feed-url", manager);
            XPathNavigator? ownerNavigator = source.SelectChildElement("itunes", "owner", manager);
            XPathNavigator? subtitleNavigator = source.SelectChildElement("itunes", "subtitle", manager);
            XPathNavigator? summaryNavigator = source.SelectChildElement("itunes", "summary", manager);

            XPathNodeIterator categoryIterator = source.SelectChildElements("itunes", "category", manager);

            if (authorNavigator is not null && !string.IsNullOrEmpty(authorNavigator.Value))
            {
                this.Author = authorNavigator.Value;
                wasLoaded = true;
            }

            if (keywordsNavigator is not null && !string.IsNullOrEmpty(keywordsNavigator.Value))
            {
                if (keywordsNavigator.Value.Contains(',', StringComparison.Ordinal))
                {
                    string[] keywords = keywordsNavigator.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (string keyword in keywords)
                    {
                        this.Keywords.Add(keyword);
                        wasLoaded = true;
                    }
                }
                else
                {
                    this.Keywords.Add(keywordsNavigator.Value);
                    wasLoaded = true;
                }
            }

            if (newFeedUrlNavigator is not null)
            {
                if (Uri.TryCreate(newFeedUrlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? newFeedUrl))
                {
                    this.NewFeedUrl = newFeedUrl;
                    wasLoaded = true;
                }
            }

            if (ownerNavigator is not null)
            {
                ITunesOwner owner = new();
                if (owner.Load(ownerNavigator))
                {
                    this.Owner = owner;
                    wasLoaded = true;
                }
            }

            if (subtitleNavigator is not null && !string.IsNullOrEmpty(subtitleNavigator.Value))
            {
                this.Subtitle = subtitleNavigator.Value;
                wasLoaded = true;
            }

            if (summaryNavigator is not null && !string.IsNullOrEmpty(summaryNavigator.Value))
            {
                this.Summary = summaryNavigator.Value;
                wasLoaded = true;
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

                    ITunesCategory category = new();
                    if (category.Load(categoryNode))
                    {
                        this.Categories.Add(category);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the optional syndication extension information using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if the <see cref="ITunesSyndicationExtensionContext"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    private bool LoadOptionals(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);
        if (source.HasChildren)
        {
            XPathNavigator? blockNavigator = source.SelectChildElement("itunes", "block", manager);
            XPathNavigator? completeNavigator = source.SelectChildElement("itunes", "complete", manager);
            XPathNavigator? imageNavigator = source.SelectChildElement("itunes", "image", manager);
            XPathNavigator? durationNavigator = source.SelectChildElement("itunes", "duration", manager);
            XPathNavigator? explicitNavigator = source.SelectChildElement("itunes", "explicit", manager);

            if (blockNavigator is not null && !string.IsNullOrEmpty(blockNavigator.Value))
            {
                if (string.Equals(blockNavigator.Value, "yes", StringComparison.OrdinalIgnoreCase))
                {
                    this.IsBlocked = true;
                    wasLoaded = true;
                }
                else if (string.Equals(blockNavigator.Value, "no", StringComparison.OrdinalIgnoreCase))
                {
                    this.IsBlocked = false;
                    wasLoaded = true;
                }
            }

            // Apple defines one value for this element: "yes". Unlike itunes:block, which documents both
            // spellings, "no" has no meaning here -- the absence of the element is how a podcast says it
            // is still running. So only "yes" is recognised, and only "yes" is ever written back.
            if (completeNavigator is not null
                && string.Equals(completeNavigator.Value.Trim(), "yes", StringComparison.OrdinalIgnoreCase))
            {
                this.IsComplete = true;
                wasLoaded = true;
            }

            if (imageNavigator is { HasAttributes: true })
            {
                string hrefAttribute = imageNavigator.GetAttribute("href", string.Empty);
                if (!string.IsNullOrEmpty(hrefAttribute))
                {
                    if (Uri.TryCreate(hrefAttribute, UriKind.RelativeOrAbsolute, out Uri? image))
                    {
                        this.Image = image;
                        wasLoaded = true;
                    }
                }
            }

            if (durationNavigator is not null && !string.IsNullOrEmpty(durationNavigator.Value))
            {
                TimeSpan duration = ITunesSyndicationExtensionContext.ParseDuration(durationNavigator.Value);
                if (duration != TimeSpan.MinValue)
                {
                    this.Duration = duration;
                    wasLoaded = true;
                }
            }

            if (explicitNavigator is not null && !string.IsNullOrEmpty(explicitNavigator.Value))
            {
                ITunesExplicitMaterial explicitMaterial = ITunesSyndicationExtension.ExplicitMaterialByName(explicitNavigator.Value.Trim());
                if (explicitMaterial != ITunesExplicitMaterial.None)
                {
                    this.ExplicitMaterial = explicitMaterial;
                    wasLoaded = true;
                }
            }

            wasLoaded |= this.LoadEpisodeMetadata(source, manager);
        }

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the episode metadata Apple added in 2017 using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <b>XPathNavigator</b> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><b>true</b> if any of the elements were present; otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     Separated from <c>LoadOptionals</c> only to keep either method a readable length. These are
    ///     the four elements a real podcast feed emits in quantity and this library previously dropped:
    ///     across the 136-document corpus, <c>episodeType</c> appears 4,695 times, <c>title</c> 4,544,
    ///     <c>episode</c> 1,003 and <c>type</c> 6 — <b>10,248 occurrences</b> in all. Because nothing
    ///     read them, a load followed by a save discarded every one. <c>season</c> joins them because
    ///     Apple defines it in the same breath as <c>episode</c>, though it appears in the corpus zero
    ///     times.
    /// </remarks>
    private bool LoadEpisodeMetadata(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? titleNavigator = source.SelectChildElement("itunes", "title", manager);
        XPathNavigator? episodeNavigator = source.SelectChildElement("itunes", "episode", manager);
        XPathNavigator? seasonNavigator = source.SelectChildElement("itunes", "season", manager);
        XPathNavigator? episodeTypeNavigator = source.SelectChildElement("itunes", "episodeType", manager);
        XPathNavigator? podcastTypeNavigator = source.SelectChildElement("itunes", "type", manager);

        if (titleNavigator is not null && !string.IsNullOrEmpty(titleNavigator.Value))
        {
            this.Title = titleNavigator.Value;
            wasLoaded = true;
        }

        // Apple defines both as positive integers. A zero or negative value is refused rather than
        // stored, because the property's own setter would refuse it and a loader must not be able to
        // produce a value a caller cannot write back -- the defect §2.45 records in RssEnclosure.
        if (episodeNavigator is not null
            && int.TryParse(episodeNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int episode)
            && episode >= 1)
        {
            this.Episode = episode;
            wasLoaded = true;
        }

        if (seasonNavigator is not null
            && int.TryParse(seasonNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int season)
            && season >= 1)
        {
            this.Season = season;
            wasLoaded = true;
        }

        if (episodeTypeNavigator is not null && !string.IsNullOrEmpty(episodeTypeNavigator.Value))
        {
            ITunesEpisodeType episodeType = ITunesSyndicationExtension.EpisodeTypeByName(episodeTypeNavigator.Value.Trim());
            if (episodeType != ITunesEpisodeType.None)
            {
                this.EpisodeType = episodeType;
                wasLoaded = true;
            }
        }

        if (podcastTypeNavigator is not null && !string.IsNullOrEmpty(podcastTypeNavigator.Value))
        {
            ITunesPodcastType podcastType = ITunesSyndicationExtension.PodcastTypeByName(podcastTypeNavigator.Value.Trim());
            if (podcastType != ITunesPodcastType.None)
            {
                this.PodcastType = podcastType;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Returns the <see cref="TimeSpan"/> represented by the supplied duration value.
    /// </summary>
    /// <param name="value">The ITunes duration representation of the podcast duration.</param>
    /// <returns>A <see cref="TimeSpan"/> that represents the duration. If unable to determine duration, returns <see cref="TimeSpan.MinValue"/>.</returns>
    /// <remarks>Value can be formatted as an integer, HH:MM:SS, H:MM:SS, MM:SS, or M:SS.</remarks>
    private static TimeSpan ParseDuration(string value)
    {
        TimeSpan timeSpan = TimeSpan.MinValue;

        if (!value.Contains(':', StringComparison.Ordinal))
        {
            if (int.TryParse(value, out int totalSeconds))
            {
                timeSpan = new TimeSpan(0, 0, totalSeconds);
            }
            else
            {
                if (TimeSpan.TryParse(value, out TimeSpan duration))
                {
                    timeSpan = duration;
                }
            }
        }
        else
        {
            string[] durationParts = value.Split(':', StringSplitOptions.RemoveEmptyEntries);

            // mm:ss and hh:mm:ss are matched separately. The two patterns cannot reuse names -
            // a pattern variable declared in an `if` condition is scoped to the whole enclosing
            // block, not to its own branch - so each is named for the format it matches.
            if (durationParts is [string mmOnlyValue, string ssOnlyValue])
            {
                if (int.TryParse(mmOnlyValue, out int minutes) && int.TryParse(ssOnlyValue, out int seconds))
                {
                    timeSpan = new TimeSpan(0, minutes, seconds);
                }
            }
            else if (durationParts is [string hoursValue, string minutesValue, string secondsValue, ..])
            {
                if (int.TryParse(hoursValue, out int hours) && int.TryParse(minutesValue, out int minutes) && int.TryParse(secondsValue, out int seconds))
                {
                    timeSpan = new TimeSpan(hours, minutes, seconds);
                }
                else
                {
                    if (TimeSpan.TryParse(value, out TimeSpan duration))
                    {
                        timeSpan = duration;
                    }
                }
            }
        }

        return timeSpan;
    }
}