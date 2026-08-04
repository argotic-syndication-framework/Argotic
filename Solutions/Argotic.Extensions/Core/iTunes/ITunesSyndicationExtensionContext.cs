using System.Globalization;
using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Encapsulates specific information about an individual <see cref="ITunesSyndicationExtension"/>.
/// </summary>
[Serializable]
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
        get => field;
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
    /// Gets or sets a URL that points to the album artwork for this podcast.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents a URL that points to the album artwork for this podcast.</value>
    /// <remarks>
    ///     iTunes recommends the use of square images that are at least 600 by 600 pixels.
    ///     iTunes supports images in <i>JPEG</i> and <i>PNG</i> formats.
    ///     The URL <b>must</b> end in ".jpg" or ".png".
    /// </remarks>
    public Uri Image { get; set; }

    /// <summary>
    /// Gets or sets a value indicating if this podcast is blocked from appearing in the iTunes Podcast directory.
    /// </summary>
    /// <value><b>true</b> if this podcast is blocked from appearing in the iTunes Podcast directory; Otherwise, <b>false</b>. The default value is <b>false</b>.</value>
    public bool IsBlocked { get; set; }

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
    public Uri NewFeedUrl { get; set; }

    /// <summary>
    /// Gets or sets information that can be used to contact the owner of this podcast.
    /// </summary>
    /// <value>
    ///     A <see cref="ITunesOwner"/> object that represents information that can be used to contact the owner of this podcast.
    ///     The default value is a <b>null</b> reference.
    /// </value>
    public ITunesOwner Owner { get; set; }

    /// <summary>
    /// Gets or sets a brief synopsis of this podcast.
    /// </summary>
    /// <value>A brief synopsis of this podcast.</value>
    /// <remarks>
    ///     It is recommended that the subtitle is only a few words long.
    /// </remarks>
    public string Subtitle
    {
        get => field;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the full description of this podcast.
    /// </summary>
    /// <value>The full description of this podcast.</value>
    public string Summary
    {
        get => field;
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
        if (this.NewFeedUrl != null)
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

        if (this.Image != null)
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
            string[] keywords = new string[this.Keywords.Count];
            this.Keywords.CopyTo(keywords, 0);

            writer.WriteElementString("keywords", xmlNamespace, string.Join(",", keywords));
        }

        if (this.ExplicitMaterial != ITunesExplicitMaterial.None)
        {
            writer.WriteElementString("explicit", xmlNamespace, ITunesSyndicationExtension.ExplicitMaterialAsString(this.ExplicitMaterial));
        }

        if (this.IsBlocked)
        {
            writer.WriteElementString("block", xmlNamespace, "yes");
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
            XPathNavigator? authorNavigator = source.SelectSingleNode("itunes:author", manager);
            XPathNavigator? keywordsNavigator = source.SelectSingleNode("itunes:keywords", manager);
            XPathNavigator? newFeedUrlNavigator = source.SelectSingleNode("itunes:new-feed-url", manager);
            XPathNavigator? ownerNavigator = source.SelectSingleNode("itunes:owner", manager);
            XPathNavigator? subtitleNavigator = source.SelectSingleNode("itunes:subtitle", manager);
            XPathNavigator? summaryNavigator = source.SelectSingleNode("itunes:summary", manager);

            XPathNodeIterator categoryIterator = source.Select("itunes:category", manager);

            if (authorNavigator != null && !string.IsNullOrEmpty(authorNavigator.Value))
            {
                this.Author = authorNavigator.Value;
                wasLoaded = true;
            }

            if (keywordsNavigator != null && !string.IsNullOrEmpty(keywordsNavigator.Value))
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

            if (newFeedUrlNavigator != null)
            {
                if (Uri.TryCreate(newFeedUrlNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? newFeedUrl))
                {
                    this.NewFeedUrl = newFeedUrl;
                    wasLoaded = true;
                }
            }

            if (ownerNavigator != null)
            {
                ITunesOwner owner = new();
                if (owner.Load(ownerNavigator))
                {
                    this.Owner = owner;
                    wasLoaded = true;
                }
            }

            if (subtitleNavigator != null && !string.IsNullOrEmpty(subtitleNavigator.Value))
            {
                this.Subtitle = subtitleNavigator.Value;
                wasLoaded = true;
            }

            if (summaryNavigator != null && !string.IsNullOrEmpty(summaryNavigator.Value))
            {
                this.Summary = summaryNavigator.Value;
                wasLoaded = true;
            }

            if (categoryIterator is { Count: > 0 })
            {
                while (categoryIterator.MoveNext())
                {
                    XPathNavigator? categoryNode = categoryIterator.Current;
                    if (categoryNode == null)
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
            XPathNavigator? blockNavigator = source.SelectSingleNode("itunes:block", manager);
            XPathNavigator? imageNavigator = source.SelectSingleNode("itunes:image", manager);
            XPathNavigator? durationNavigator = source.SelectSingleNode("itunes:duration", manager);
            XPathNavigator? explicitNavigator = source.SelectSingleNode("itunes:explicit", manager);

            if (blockNavigator != null && !string.IsNullOrEmpty(blockNavigator.Value))
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

            if (durationNavigator != null && !string.IsNullOrEmpty(durationNavigator.Value))
            {
                TimeSpan duration = ITunesSyndicationExtensionContext.ParseDuration(durationNavigator.Value);
                if (duration != TimeSpan.MinValue)
                {
                    this.Duration = duration;
                    wasLoaded = true;
                }
            }

            if (explicitNavigator != null && !string.IsNullOrEmpty(explicitNavigator.Value))
            {
                ITunesExplicitMaterial explicitMaterial = ITunesSyndicationExtension.ExplicitMaterialByName(explicitNavigator.Value.Trim());
                if (explicitMaterial != ITunesExplicitMaterial.None)
                {
                    this.ExplicitMaterial = explicitMaterial;
                    wasLoaded = true;
                }
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

            if (durationParts.Length == 2)
            {
                if (int.TryParse(durationParts[0], out int minutes) && int.TryParse(durationParts[1], out int seconds))
                {
                    timeSpan = new TimeSpan(0, minutes, seconds);
                }
            }
            else if (durationParts.Length >= 3)
            {
                string hoursValue = durationParts[0];
                string minutesValue = durationParts[1];
                string secondsValue = durationParts[2];

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