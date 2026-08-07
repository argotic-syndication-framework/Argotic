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
    /// Gets or sets the name of the person or group responsible for this podcast.
    /// </summary>
    /// <value>The author name, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     The most widely emitted element in the namespace: <b>99.6%</b> of 1,934 live feeds sampled
    ///     from the Apple directory carry one.
    /// </remarks>
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
    /// <remarks>
    ///     Channel level. The text of each category is drawn from a fixed taxonomy Apple publishes,
    ///     not free prose, and Apple's guidance is that <i>"You can choose up to two categories per
    ///     show — primary and secondary — plus subcategories for each, if available."</i> A
    ///     subcategory is a <see cref="ITunesCategory"/> nested inside its parent's
    ///     <see cref="ITunesCategory.Categories"/>, never a second entry in this collection.
    /// </remarks>
    /// <seealso cref="ITunesCategory"/>
    public IList<ITunesCategory> Categories { get; } = [];

    /// <summary>
    /// Gets or sets the total duration of this podcast.
    /// </summary>
    /// <value>A <see cref="TimeSpan"/> that represents total duration of this podcast. The default value is <see cref="TimeSpan.MinValue"/>, which indicates that no duration was specified.</value>
    /// <remarks>
    ///     Reading this element is not a matter of parsing <c>HH:MM:SS</c>. All six spellings the
    ///     loader accepts are in live use, and <b>the bare-integer forms — a count of seconds with no
    ///     colon at all — total 2,644 of the 5,646 durations in the 136-document corpus</b>, forty
    ///     times more common than <c>MM:SS</c>. Argotic always <i>writes</i> <c>HH:MM:SS</c>, so a
    ///     round-trip normalises the spelling even though it preserves the value.
    /// </remarks>
    public TimeSpan Duration { get; set; } = TimeSpan.MinValue;

    /// <summary>
    /// Gets or sets the explicit language or adult content advisory information for this podcast.
    /// </summary>
    /// <value>
    ///     An <see cref="ITunesExplicitMaterial"/> enumeration value that indicates whether the podcast contains explicit material.
    ///     The default value is <see cref="ITunesExplicitMaterial.None"/>.
    /// </value>
    /// <remarks>
    ///     Apple retired the <c>yes</c> / <c>no</c> / <c>clean</c> vocabulary in favour of
    ///     <c>true</c> / <c>false</c>, and 62% of the values in the 136-document corpus use the newer
    ///     spelling. Both are read; only the legacy spelling is written back. See
    ///     <see cref="ITunesSyndicationExtension.ExplicitMaterialByName"/> for why that asymmetry
    ///     exists.
    /// </remarks>
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
    /// <remarks>
    ///     Apple defines this as a positive integer, so the loader refuses a zero or negative value
    ///     rather than storing one: a loader that can produce a value the setter would reject is the
    ///     defect §2.45 of <c>docs/build-warnings.md</c> records.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>1</c>.</exception>
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
    ///     Defined by the same paragraph of Apple's specification as <see cref="Episode"/>, and added
    ///     on that reasoning alone: it has <b>zero</b> occurrences in the 136-document corpus, which is
    ///     the one place in this family where a member is spec-derived rather than corpus-derived.
    ///     A later sample of <b>1,934</b> live feeds found it in <b>23.5%</b> of them, so the smaller
    ///     corpus was simply too small to see a situational element — which is why the spec-derived
    ///     call was the right one.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>1</c>.</exception>
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
    /// <value>A <see cref="Uri"/> that represents the location of the artwork, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     Carried on the <c>href</c> attribute of <c>itunes:image</c>, not as element content — an
    ///     <c>itunes:image</c> with a text body and no attribute reads as absent. Apple's stated
    ///     dimensions are <i>"3000 x 3000 pixels. If submitting a Show Cover via RSS feed, Apple
    ///     Podcasts accepts Show Cover artwork ranging from 1400 x 1400 to 3000 x 3000 pixels."</i>,
    ///     the format is PNG or JPG, and covers <i>"cannot contain transparency and should not contain
    ///     an alpha channel"</i>. Nothing here validates any of that; the URL is stored as given.
    /// </remarks>
    public Uri? Image { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this podcast is blocked from appearing in the iTunes Podcast directory.
    /// </summary>
    /// <value><see langword="true"/> if this podcast is blocked from appearing in the iTunes Podcast directory; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.</value>
    /// <remarks>
    ///     Unlike <see cref="IsComplete"/>, <c>itunes:block</c> has both a <c>yes</c> and a <c>no</c>
    ///     spelling and both are read. Only <c>yes</c> is written: a feed that said
    ///     <c>&lt;itunes:block&gt;no&lt;/itunes:block&gt;</c> loses the element on a round-trip, which
    ///     changes the document but not its meaning, since absence and <c>no</c> say the same thing.
    /// </remarks>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this podcast has finished and will publish no further episodes.
    /// </summary>
    /// <value><see langword="true"/> if no episode will ever be added to this podcast again; otherwise, <see langword="false"/>. The default value is <see langword="false"/>.</value>
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
    /// <value>A <see cref="IList{T}"/> collection of search keywords. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     The whole collection is one element on the wire — a single <c>itunes:keywords</c> whose
    ///     content is the members joined with commas — so a keyword containing a comma will come back
    ///     as two. <c>podcast-standard.org</c> lists the element as deprecated; <b>35.0%</b> of 1,934
    ///     live feeds still emit it.
    /// </remarks>
    public IList<string> Keywords { get; } = [];

    /// <summary>
    /// Gets or sets the URL where this podcast feed has been relocated to.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the new location, or <see langword="null"/> if the feed has not moved.</value>
    /// <remarks>
    ///     Channel level, and the only way a feed can tell Apple it has moved without losing its
    ///     subscribers. It has to be served from the <i>old</i> address to be seen at all, which is
    ///     why the old feed has to stay up after the move rather than being retired with it.
    ///     <b>34.4%</b> of 1,934 live feeds carry one.
    /// </remarks>
    public Uri? NewFeedUrl { get; set; }

    /// <summary>
    /// Gets or sets the token Apple Podcasts reads to verify who owns this feed.
    /// </summary>
    /// <value>The verification token, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     <para>
    ///     Apple's <c>itunes:applepodcastsverify</c>. When somebody claims a show in Apple Podcasts
    ///     Connect, Apple issues a token that has to appear in the feed before the claim will complete.
    ///     Losing it on a round-trip fails the claim, which is why it is modelled: it is the same silent
    ///     round-trip loss as §2.47, on an element with a deadline attached.
    ///     </para>
    ///     <para>
    ///     <b>The element name is entirely lower case</b> — not <c>applePodcastsVerify</c>, which is how
    ///     it is commonly written in prose. XML element names are case-sensitive, so the difference
    ///     decides whether this ever matches. Of 1,934 live feeds sampled from the Apple directory, 26
    ///     carry the element and <b>all 26 spell it lower case</b>; the camel-cased spelling appears
    ///     zero times.
    ///     </para>
    ///     <para>
    ///     It is a <see cref="string"/> rather than a <see cref="Guid"/> deliberately. Twenty-one of
    ///     those 26 tokens are UUIDs, but four are six-digit codes and one is ten digits — Apple's older
    ///     numeric authorization codes. A <see cref="Guid"/> would refuse <b>19%</b> of the tokens
    ///     actually in use.
    ///     </para>
    /// </remarks>
    public string VerificationToken
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets information that can be used to contact the owner of this podcast.
    /// </summary>
    /// <value>
    ///     A <see cref="ITunesOwner"/> object that represents information that can be used to contact the owner of this podcast.
    ///     The default value is <see langword="null"/>.
    /// </value>
    /// <seealso cref="ITunesOwner"/>
    public ITunesOwner? Owner { get; set; }

    /// <summary>
    /// Gets or sets a brief synopsis of this podcast.
    /// </summary>
    /// <value>The subtitle, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     <c>podcast-standard.org</c> lists <c>itunes:subtitle</c> as deprecated. It is still in
    ///     <b>56.8%</b> of 1,934 live feeds, which is what deprecated looks like on a format nobody
    ///     can force a re-release of, so it is read and written unchanged.
    /// </remarks>
    public string Subtitle
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the full description of this podcast.
    /// </summary>
    /// <value>The summary, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     <c>podcast-standard.org</c> lists <c>itunes:summary</c> as deprecated, and it is
    ///     nonetheless in <b>91.8%</b> of 1,934 live feeds — the most widely emitted deprecated
    ///     element in the namespace. It is <i>not</i> tied to the enclosing entity's own
    ///     <c>description</c>: neither is derived from the other here, so a publisher who sets one and
    ///     expects the other to follow will ship a feed with a blank half.
    /// </remarks>
    public string Summary
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="ITunesSyndicationExtensionContext"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
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
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed syndication extension elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);
        if (this.NewFeedUrl is not null)
        {
            writer.WriteElementString("new-feed-url", xmlNamespace, this.NewFeedUrl.ToString());
        }

        if (!string.IsNullOrEmpty(this.VerificationToken))
        {
            writer.WriteElementString("applepodcastsverify", xmlNamespace, this.VerificationToken);
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
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="ITunesSyndicationExtensionContext"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
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
            XPathNavigator? verificationNavigator = source.SelectChildElement("itunes", "applepodcastsverify", manager);
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

            if (verificationNavigator is not null && !string.IsNullOrEmpty(verificationNavigator.Value))
            {
                this.VerificationToken = verificationNavigator.Value;
                wasLoaded = true;
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
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if the <see cref="ITunesSyndicationExtensionContext"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
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
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="ITunesSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed syndication extension elements and attributes.</param>
    /// <returns><see langword="true"/> if any of the elements were present; otherwise, <see langword="false"/>.</returns>
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