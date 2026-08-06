using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a link to a chapters file for a podcast episode.
/// </summary>
/// <remarks>
///     Podcasting 2.0's <c>podcast:chapters</c>, present in <b>1.7%</b> of 1,934 live feeds surveyed. An
///     episode has at most one, and the file it points at is normally
///     <c>application/json+chapters</c>.
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.Chapters"/>
public class PodcastChapters : IComparable<PodcastChapters>, IEquatable<PodcastChapters>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastChapters"/> class.
    /// </summary>
    public PodcastChapters()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastChapters"/> class using the supplied location and media type.
    /// </summary>
    /// <param name="url">The location of the chapters file.</param>
    /// <param name="mediaType">The media type of the chapters file.</param>
    public PodcastChapters(Uri url, string mediaType)
    {
        this.Url = url;
        this.MediaType = mediaType;
    }

    /// <summary>
    /// Gets or sets the location of the chapters file.
    /// </summary>
    /// <value>A <see cref="Uri"/>, or <see langword="null"/> if none was specified.</value>
    /// <remarks>Required by the specification.</remarks>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the media type of the chapters file.
    /// </summary>
    /// <value>The media type. The default value is an <i>empty</i> string.</value>
    /// <remarks>Required by the specification, which prefers <c>application/json+chapters</c>.</remarks>
    public string MediaType
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="PodcastChapters"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="PodcastChapters"/> was initialized using the supplied <paramref name="source"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);
        bool wasLoaded = false;

        Uri? url = PodcastExtensionUtility.ReadUriAttribute(source, "url");
        if (url is not null)
        {
            this.Url = url;
            wasLoaded = true;
        }

        string typeAttribute = source.GetAttribute("type", string.Empty);
        if (!string.IsNullOrEmpty(typeAttribute))
        {
            this.MediaType = typeAttribute;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="PodcastChapters"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("chapters", PodcastSyndicationExtension.NamespaceUri);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "url", this.Url);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "type", this.MediaType);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastChapters"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(PodcastChapters? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.MediaType, other.MediaType, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastChapters"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastChapters"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public bool Equals(PodcastChapters? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is PodcastChapters other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Url),
        HashCodeUtility.Component(this.MediaType));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(PodcastChapters? first, PodcastChapters? second)
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
    public static bool operator !=(PodcastChapters? first, PodcastChapters? second) => !(first == second);
}