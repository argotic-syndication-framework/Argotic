using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="PodcastSyndicationExtension"/>.
/// </summary>
/// <remarks>
///     <para>
///     One context serves both the channel and the item, as every extension in this library does. The
///     namespace places some elements only on a channel (<c>guid</c>, <c>locked</c>, <c>medium</c>,
///     <c>podping</c>), some only on an item (<c>transcript</c>, <c>chapters</c>, <c>season</c>,
///     <c>episode</c>), and some on either (<c>funding</c>, <c>txt</c>, <c>person</c>,
///     <c>license</c>). Which members are populated therefore tells you which level you are looking at.
///     </para>
/// </remarks>
public class PodcastSyndicationExtensionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastSyndicationExtensionContext"/> class.
    /// </summary>
    public PodcastSyndicationExtensionContext()
    {
    }

    /// <summary>
    /// Gets or sets a value indicating whether this feed may be imported by another hosting platform.
    /// </summary>
    /// <value>
    ///     <see langword="true"/> if any attempt to import this feed elsewhere should be rejected, <see langword="false"/> if
    ///     importing is permitted, and <see langword="null"/> if the feed said nothing.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     Podcasting 2.0's <c>podcast:locked</c>, and the namespace's most widely deployed element —
    ///     <b>16.6%</b> of 1,934 live feeds surveyed carry it.
    ///     </para>
    ///     <para>
    ///     It is <see cref="Nullable{T}"/> because saying nothing is not the same as saying no. A feed
    ///     with no <c>locked</c> element has expressed no view on being moved; reporting that as
    ///     <see langword="false"/> would put a decision in the publisher's mouth, and would make an absent element
    ///     appear on the next save.
    ///     </para>
    /// </remarks>
    public bool? IsLocked { get; set; }

    /// <summary>
    /// Gets or sets the address used to verify ownership when this feed is moved or imported.
    /// </summary>
    /// <value>The owner's email address, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>The <c>owner</c> attribute of <c>podcast:locked</c>.</remarks>
    public string LockOwner
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the globally unique identifier for this podcast.
    /// </summary>
    /// <value>The identifier, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     <para>
    ///     Podcasting 2.0's <c>podcast:guid</c>, present in <b>12.9%</b> of 1,934 live feeds surveyed. It
    ///     is assigned once from the feed's URL and then follows the podcast for life — including across
    ///     a change of address — which is how a podcast keeps one identity across the open RSS ecosystem
    ///     with no central authority.
    ///     </para>
    ///     <para>
    ///     A <see cref="string"/> rather than a <see cref="System.Guid"/>. The specification says the
    ///     value is a UUIDv5, but a feed is free to contain something else, and refusing to read a
    ///     malformed identifier would lose the only thing tying that feed to its own history. §2.49
    ///     records the same decision being forced by real data on the Apple verification token, where a
    ///     fifth of live values were not UUIDs at all.
    ///     </para>
    /// </remarks>
    public string Identifier
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets what the content of this feed is.
    /// </summary>
    /// <value>A <see cref="PodcastMedium"/> value. The default is <see cref="PodcastMedium.None"/>.</value>
    /// <remarks>Podcasting 2.0's <c>podcast:medium</c>, present in <b>3.5%</b> of 1,934 live feeds surveyed.</remarks>
    /// <seealso cref="MediumIsList"/>
    public PodcastMedium Medium { get; set; } = PodcastMedium.None;

    /// <summary>
    /// Gets or sets a value indicating whether the medium is the "list" variant of <see cref="Medium"/>.
    /// </summary>
    /// <value><see langword="true"/> for a list feed such as <c>musicL</c>; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     The specification spells a list feed by suffixing the medium name with the letter <c>L</c>.
    ///     Modelling that as a flag rather than as ten more enumeration members keeps the two halves of
    ///     the value independent, which is what the specification says they are.
    /// </remarks>
    public bool MediumIsList { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this feed sends Podping notifications when it changes.
    /// </summary>
    /// <value><see langword="true"/> if the feed signals its changes by Podping; otherwise, <see langword="false"/>.</value>
    /// <remarks>
    ///     Podcasting 2.0's <c>podcast:podping</c>, present in <b>3.5%</b> of 1,934 live feeds surveyed.
    ///     A consumer that trusts it can poll far less often.
    /// </remarks>
    public bool UsesPodping { get; set; }

    /// <summary>
    /// Gets the transcripts and closed-captions files for this episode.
    /// </summary>
    /// <value>A collection of <see cref="PodcastTranscript"/> objects. The default is an <i>empty</i> collection.</value>
    public IList<PodcastTranscript> Transcripts { get; } = [];

    /// <summary>
    /// Gets the ways listeners can fund this podcast.
    /// </summary>
    /// <value>A collection of <see cref="PodcastFunding"/> objects. The default is an <i>empty</i> collection.</value>
    public IList<PodcastFunding> FundingLinks { get; } = [];

    /// <summary>
    /// Gets the free-form text values carried by this feed or episode.
    /// </summary>
    /// <value>A collection of <see cref="PodcastText"/> objects. The default is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     This is where an Apple Podcasts ownership verification token arrives for roughly half the
    ///     publishers who set one — see <see cref="PodcastText.ApplePodcastsVerifyPurpose"/>.
    /// </remarks>
    public IList<PodcastText> TextEntries { get; } = [];

    /// <summary>
    /// Gets the people involved in this podcast or episode.
    /// </summary>
    /// <value>A collection of <see cref="PodcastPerson"/> objects. The default is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     People declared on an item <b>replace</b> those declared on the channel for that episode,
    ///     rather than adding to them. That merge is left to the caller; see <see cref="PodcastPerson"/>.
    /// </remarks>
    public IList<PodcastPerson> People { get; } = [];

    /// <summary>
    /// Gets or sets the season this episode belongs to.
    /// </summary>
    /// <value>The season number, or <see langword="null"/> if none was specified.</value>
    /// <remarks>Podcasting 2.0's <c>podcast:season</c>. Distinct from <c>itunes:season</c>, which a feed may also carry.</remarks>
    public int? Season { get; set; }

    /// <summary>
    /// Gets or sets the name of the season this episode belongs to.
    /// </summary>
    /// <value>The season name, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     The <c>name</c> attribute of <c>podcast:season</c>. When present, an application may show it
    ///     instead of the number and use the number only for ordering.
    /// </remarks>
    public string SeasonName
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the episode number within its season.
    /// </summary>
    /// <value>The episode number, or <see langword="null"/> if none was specified.</value>
    /// <remarks>
    ///     A <see cref="decimal"/>, not an <see cref="int"/>. The specification defines the node value as
    ///     a decimal number precisely so that a publisher can insert an episode between two others as
    ///     <c>3.5</c> — which is exactly the case an integer would silently mangle.
    /// </remarks>
    public decimal? Episode { get; set; }

    /// <summary>
    /// Gets or sets the text an application should show in place of the episode number.
    /// </summary>
    /// <value>The display value, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>The <c>display</c> attribute of <c>podcast:episode</c>.</remarks>
    public string EpisodeDisplay
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the chapters file for this episode.
    /// </summary>
    /// <value>A <see cref="PodcastChapters"/>, or <see langword="null"/> if none was specified.</value>
    public PodcastChapters? Chapters { get; set; }

    /// <summary>
    /// Gets or sets the license this podcast or episode is released under.
    /// </summary>
    /// <value>A <see cref="PodcastLicense"/>, or <see langword="null"/> if none was specified.</value>
    public PodcastLicense? License { get; set; }

    /// <summary>
    /// Initializes the syndication extension context using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="PodcastSyndicationExtensionContext"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> used to resolve prefixed elements and attributes.</param>
    /// <returns><see langword="true"/> if the context was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        if (!source.HasChildren)
        {
            return false;
        }

        bool wasLoaded = this.LoadChannelScalars(source, manager);
        wasLoaded |= this.LoadItemScalars(source, manager);
        wasLoaded |= this.LoadCollections(source, manager);

        return wasLoaded;
    }

    /// <summary>
    /// Writes the current context to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the current context.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements and attributes.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is <see langword="null"/> or an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        if (this.IsLocked.HasValue)
        {
            writer.WriteStartElement("locked", xmlNamespace);
            PodcastExtensionUtility.WriteOptionalAttribute(writer, "owner", this.LockOwner);
            writer.WriteString(PodcastExtensionUtility.YesNo(this.IsLocked.Value));
            writer.WriteEndElement();
        }

        if (!string.IsNullOrEmpty(this.Identifier))
        {
            writer.WriteElementString("guid", xmlNamespace, this.Identifier);
        }

        if (this.Medium != PodcastMedium.None)
        {
            writer.WriteElementString("medium", xmlNamespace, this.MediumAsString());
        }

        if (this.UsesPodping)
        {
            writer.WriteStartElement("podping", xmlNamespace);
            writer.WriteAttributeString("usesPodping", "true");
            writer.WriteEndElement();
        }

        if (this.Season.HasValue)
        {
            writer.WriteStartElement("season", xmlNamespace);
            PodcastExtensionUtility.WriteOptionalAttribute(writer, "name", this.SeasonName);
            writer.WriteString(this.Season.Value.ToString(NumberFormatInfo.InvariantInfo));
            writer.WriteEndElement();
        }

        if (this.Episode.HasValue)
        {
            writer.WriteStartElement("episode", xmlNamespace);
            PodcastExtensionUtility.WriteOptionalAttribute(writer, "display", this.EpisodeDisplay);
            writer.WriteString(this.Episode.Value.ToString(NumberFormatInfo.InvariantInfo));
            writer.WriteEndElement();
        }

        this.Chapters?.WriteTo(writer);
        this.License?.WriteTo(writer);

        foreach (PodcastTranscript transcript in this.Transcripts)
        {
            transcript.WriteTo(writer);
        }

        foreach (PodcastFunding funding in this.FundingLinks)
        {
            funding.WriteTo(writer);
        }

        foreach (PodcastPerson person in this.People)
        {
            person.WriteTo(writer);
        }

        foreach (PodcastText text in this.TextEntries)
        {
            text.WriteTo(writer);
        }
    }

    /// <summary>
    /// Returns the specification's spelling of the current <see cref="Medium"/>, including the list suffix.
    /// </summary>
    /// <returns>The medium name, suffixed with <c>L</c> when <see cref="MediumIsList"/> is set.</returns>
    private string MediumAsString()
    {
        string name = EnumerationMetadataAttribute.GetAlternateValue(this.Medium);

        return this.MediumIsList ? string.Concat(name, "L") : name;
    }

    /// <summary>
    /// Loads the elements the specification places only on a channel.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><see langword="true"/> if any element was present; otherwise, <see langword="false"/>.</returns>
    private bool LoadChannelScalars(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? lockedNavigator = source.SelectChildElement("podcast", "locked", manager);
        if (lockedNavigator is not null)
        {
            bool? locked = PodcastExtensionUtility.ParseYesNo(lockedNavigator.Value);
            if (locked.HasValue)
            {
                this.IsLocked = locked;
                wasLoaded = true;
            }

            string ownerAttribute = lockedNavigator.GetAttribute("owner", string.Empty);
            if (!string.IsNullOrEmpty(ownerAttribute))
            {
                this.LockOwner = ownerAttribute;
                wasLoaded = true;
            }
        }

        XPathNavigator? guidNavigator = source.SelectChildElement("podcast", "guid", manager);
        if (guidNavigator is not null && !string.IsNullOrEmpty(guidNavigator.Value))
        {
            this.Identifier = guidNavigator.Value;
            wasLoaded = true;
        }

        XPathNavigator? mediumNavigator = source.SelectChildElement("podcast", "medium", manager);
        if (mediumNavigator is not null && !string.IsNullOrEmpty(mediumNavigator.Value))
        {
            wasLoaded |= this.LoadMedium(mediumNavigator.Value.Trim());
        }

        XPathNavigator? podpingNavigator = source.SelectChildElement("podcast", "podping", manager);
        if (podpingNavigator is not null)
        {
            // The element exists solely to carry this attribute, so a bare <podcast:podping/> asserts
            // nothing. Only an explicit "true" turns the flag on.
            string usesPodping = podpingNavigator.GetAttribute("usesPodping", string.Empty);
            if (string.Equals(usesPodping.Trim(), "true", StringComparison.OrdinalIgnoreCase))
            {
                this.UsesPodping = true;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Interprets a medium value, separating the list suffix from the medium name.
    /// </summary>
    /// <param name="value">The raw node value.</param>
    /// <returns><see langword="true"/> if the value named a medium; otherwise, <see langword="false"/>.</returns>
    private bool LoadMedium(string value)
    {
        PodcastMedium medium = EnumerationMetadataAttribute.GetEnumByAlternateValue(value, PodcastMedium.None);
        if (medium != PodcastMedium.None)
        {
            this.Medium = medium;
            this.MediumIsList = false;
            return true;
        }

        // Not a plain medium name. The specification's list variants are the same names suffixed with
        // "L", so strip one and try again -- but only if something is left, or an input of "L" alone
        // would be matched against the empty AlternateValue that PodcastMedium.None carries.
        if (value.EndsWith('L') && value.Length > 1)
        {
            medium = EnumerationMetadataAttribute.GetEnumByAlternateValue(value[..^1], PodcastMedium.None);
            if (medium != PodcastMedium.None)
            {
                this.Medium = medium;
                this.MediumIsList = true;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Loads the single-valued elements the specification places on an item.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><see langword="true"/> if any element was present; otherwise, <see langword="false"/>.</returns>
    private bool LoadItemScalars(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;

        XPathNavigator? seasonNavigator = source.SelectChildElement("podcast", "season", manager);
        if (seasonNavigator is not null)
        {
            if (int.TryParse(seasonNavigator.Value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out int season))
            {
                this.Season = season;
                wasLoaded = true;
            }

            string nameAttribute = seasonNavigator.GetAttribute("name", string.Empty);
            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.SeasonName = nameAttribute;
                wasLoaded = true;
            }
        }

        XPathNavigator? episodeNavigator = source.SelectChildElement("podcast", "episode", manager);
        if (episodeNavigator is not null)
        {
            if (decimal.TryParse(episodeNavigator.Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out decimal episode))
            {
                this.Episode = episode;
                wasLoaded = true;
            }

            string displayAttribute = episodeNavigator.GetAttribute("display", string.Empty);
            if (!string.IsNullOrEmpty(displayAttribute))
            {
                this.EpisodeDisplay = displayAttribute;
                wasLoaded = true;
            }
        }

        XPathNavigator? chaptersNavigator = source.SelectChildElement("podcast", "chapters", manager);
        if (chaptersNavigator is not null)
        {
            PodcastChapters chapters = new();
            if (chapters.Load(chaptersNavigator))
            {
                this.Chapters = chapters;
                wasLoaded = true;
            }
        }

        XPathNavigator? licenseNavigator = source.SelectChildElement("podcast", "license", manager);
        if (licenseNavigator is not null)
        {
            PodcastLicense license = new();
            if (license.Load(licenseNavigator))
            {
                this.License = license;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads the elements that may appear more than once.
    /// </summary>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <returns><see langword="true"/> if any element was present; otherwise, <see langword="false"/>.</returns>
    private bool LoadCollections(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = LoadInto(source, manager, "transcript", this.Transcripts, static () => new PodcastTranscript(), static (t, n) => t.Load(n));
        wasLoaded |= LoadInto(source, manager, "funding", this.FundingLinks, static () => new PodcastFunding(), static (f, n) => f.Load(n));
        wasLoaded |= LoadInto(source, manager, "person", this.People, static () => new PodcastPerson(), static (p, n) => p.Load(n));
        wasLoaded |= LoadInto(source, manager, "txt", this.TextEntries, static () => new PodcastText(), static (t, n) => t.Load(n));

        return wasLoaded;
    }

    /// <summary>
    /// Reads every occurrence of a repeatable element into the supplied collection.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The navigator to read from.</param>
    /// <param name="manager">The namespace manager.</param>
    /// <param name="localName">The local name of the element to read.</param>
    /// <param name="target">The collection to fill.</param>
    /// <param name="create">Creates an instance to load into.</param>
    /// <param name="load">Loads an instance from a navigator.</param>
    /// <returns><see langword="true"/> if at least one element loaded; otherwise, <see langword="false"/>.</returns>
    private static bool LoadInto<T>(
        XPathNavigator source,
        XmlNamespaceManager manager,
        string localName,
        IList<T> target,
        Func<T> create,
        Func<T, XPathNavigator, bool> load)
    {
        bool wasLoaded = false;
        XPathNodeIterator iterator = source.SelectChildElements("podcast", localName, manager);

        while (iterator.MoveNext())
        {
            XPathNavigator? node = iterator.Current;
            if (node is null)
            {
                continue;
            }

            T instance = create();
            if (load(instance, node))
            {
                target.Add(instance);
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }
}