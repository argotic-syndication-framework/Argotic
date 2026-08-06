using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a way for listeners to fund a podcast.
/// </summary>
/// <remarks>
///     Podcasting 2.0's <c>podcast:funding</c>, present in <b>3.9%</b> of 1,934 live feeds surveyed. The
///     node value is the wording the publisher wants shown beside the link, not a description of it.
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.FundingLinks"/>
public class PodcastFunding : IComparable<PodcastFunding>, IEquatable<PodcastFunding>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastFunding"/> class.
    /// </summary>
    public PodcastFunding()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastFunding"/> class using the supplied location and message.
    /// </summary>
    /// <param name="url">The location listeners are sent to.</param>
    /// <param name="message">The wording shown beside the link.</param>
    public PodcastFunding(Uri url, string message)
    {
        this.Url = url;
        this.Message = message;
    }

    /// <summary>
    /// Gets or sets the location listeners are sent to in order to fund the podcast.
    /// </summary>
    /// <value>A <see cref="Uri"/>, or <see langword="null"/> if none was specified.</value>
    /// <remarks>Required by the specification.</remarks>
    public Uri? Url { get; set; }

    /// <summary>
    /// Gets or sets the wording the publisher wants shown beside the link.
    /// </summary>
    /// <value>The message. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     The specification asks publishers to keep this under 128 characters or aggregators may
    ///     truncate it. That is a request to publishers rather than a constraint on the format, so it is
    ///     documented here and not enforced — refusing a longer value would lose data the feed contains.
    /// </remarks>
    public string Message
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Loads this <see cref="PodcastFunding"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="PodcastFunding"/> was initialized using the supplied <paramref name="source"/>; otherwise, <b>false</b>.</returns>
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

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Message = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="PodcastFunding"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("funding", PodcastSyndicationExtension.NamespaceUri);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "url", this.Url);
        writer.WriteString(this.Message);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastFunding"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(PodcastFunding? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Message, other.Message, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastFunding"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastFunding"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public bool Equals(PodcastFunding? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is PodcastFunding other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Url),
        HashCodeUtility.Component(this.Message));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(PodcastFunding? first, PodcastFunding? second)
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
    public static bool operator !=(PodcastFunding? first, PodcastFunding? second) => !(first == second);
}