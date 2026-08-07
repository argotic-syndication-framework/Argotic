using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a transcript or closed-captions file that accompanies a podcast episode.
/// </summary>
/// <remarks>
///     Podcasting 2.0's <c>podcast:transcript</c>, and the second most widely deployed element in the
///     namespace — <b>13.4%</b> of 1,934 live feeds surveyed carry one. An episode may carry several,
///     one per format.
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.Transcripts"/>
public class PodcastTranscript : IComparable<PodcastTranscript>, IEquatable<PodcastTranscript>, IComparisonOperators
{
    /// <summary>
    /// The <c>rel</c> value that marks a transcript as a closed-captions file.
    /// </summary>
    public const string CaptionsRelationship = "captions";

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastTranscript"/> class.
    /// </summary>
    public PodcastTranscript()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastTranscript"/> class using the supplied location and media type.
    /// </summary>
    /// <param name="url">The location of the transcript.</param>
    /// <param name="mediaType">The media type of the transcript.</param>
    public PodcastTranscript(Uri url, string mediaType)
    {
        this.Url = url;
        this.MediaType = mediaType;
    }

    /// <summary>
    /// Gets or sets the location of the transcript.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the location of the transcript, or <see langword="null"/> if none was specified.</value>
    /// <remarks>Required by the specification.</remarks>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the media type of the transcript.
    /// </summary>
    /// <value>The media type, such as <c>text/vtt</c> or <c>application/json</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>Required by the specification.</remarks>
    public string MediaType
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the language of the transcript.
    /// </summary>
    /// <value>An IETF language tag, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     When absent, the specification says the transcript is in the language the feed's own
    ///     <c>language</c> element declares. That inference is left to the caller rather than filled in
    ///     here, because writing it back would put a value in the feed the publisher did not.
    /// </remarks>
    public string Language
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the relationship this file has to the episode.
    /// </summary>
    /// <value>The <c>rel</c> value, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     The specification defines one value, <see cref="CaptionsRelationship"/>, which marks the file
    ///     as closed captions <i>whatever</i> its media type. It is a free string rather than an
    ///     enumeration so an unrecognised future value survives a round-trip instead of being dropped.
    /// </remarks>
    /// <seealso cref="IsCaptions"/>
    public string Relationship
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this file is a closed-captions file.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Relationship"/> is <c>captions</c>; otherwise, <see langword="false"/>.</value>
    public bool IsCaptions => string.Equals(this.Relationship, CaptionsRelationship, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Loads this <see cref="PodcastTranscript"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="PodcastTranscript"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);
        bool wasLoaded = false;

        if (!source.HasAttributes)
        {
            return false;
        }

        string urlAttribute = source.GetAttribute("url", string.Empty);
        string typeAttribute = source.GetAttribute("type", string.Empty);
        string languageAttribute = source.GetAttribute("language", string.Empty);
        string relAttribute = source.GetAttribute("rel", string.Empty);

        if (!string.IsNullOrEmpty(urlAttribute)
            && Uri.TryCreate(urlAttribute, UriKind.RelativeOrAbsolute, out Uri? url))
        {
            this.Url = url;
            wasLoaded = true;
        }

        if (!string.IsNullOrEmpty(typeAttribute))
        {
            this.MediaType = typeAttribute;
            wasLoaded = true;
        }

        if (!string.IsNullOrEmpty(languageAttribute))
        {
            this.Language = languageAttribute;
            wasLoaded = true;
        }

        if (!string.IsNullOrEmpty(relAttribute))
        {
            this.Relationship = relAttribute;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="PodcastTranscript"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("transcript", PodcastSyndicationExtension.NamespaceUri);

        if (this.Url is not null)
        {
            writer.WriteAttributeString("url", this.Url.ToString());
        }

        if (!string.IsNullOrEmpty(this.MediaType))
        {
            writer.WriteAttributeString("type", this.MediaType);
        }

        if (!string.IsNullOrEmpty(this.Language))
        {
            writer.WriteAttributeString("language", this.Language);
        }

        if (!string.IsNullOrEmpty(this.Relationship))
        {
            writer.WriteAttributeString("rel", this.Relationship);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastTranscript"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(PodcastTranscript? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.MediaType, other.MediaType, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Language, other.Language, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Relationship, other.Relationship, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastTranscript"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastTranscript"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(PodcastTranscript? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is PodcastTranscript other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Url),
        HashCodeUtility.Component(this.MediaType),
        HashCodeUtility.Component(this.Language),
        HashCodeUtility.Component(this.Relationship));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(PodcastTranscript? first, PodcastTranscript? second)
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
    public static bool operator !=(PodcastTranscript? first, PodcastTranscript? second) => !(first == second);
}