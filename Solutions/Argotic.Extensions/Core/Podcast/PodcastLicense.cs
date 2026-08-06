using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the license a podcast or episode is released under.
/// </summary>
/// <remarks>
///     Podcasting 2.0's <c>podcast:license</c>, present in <b>1.4%</b> of 1,934 live feeds surveyed. The
///     node value is a lower-cased identifier from the specification's companion license list; the
///     <c>url</c> attribute is optional for well-known licenses and required for custom ones.
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.License"/>
public class PodcastLicense : IComparable<PodcastLicense>, IEquatable<PodcastLicense>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastLicense"/> class.
    /// </summary>
    public PodcastLicense()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastLicense"/> class using the supplied identifier.
    /// </summary>
    /// <param name="identifier">The license identifier.</param>
    public PodcastLicense(string identifier)
    {
        this.Identifier = identifier;
    }

    /// <summary>
    /// Gets or sets the license identifier.
    /// </summary>
    /// <value>The identifier, such as <c>cc-by-4.0</c>. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Not lower-cased on the way in. The specification says the value <i>must</i> be lower case, but
    ///     a publisher who writes <c>CC-BY-4.0</c> has still said which license applies, and normalising
    ///     it would report a value the feed does not contain.
    /// </remarks>
    public string Identifier
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the location of the full legal text of the license.
    /// </summary>
    /// <value>A <see cref="Uri"/>, or <see langword="null"/> if none was specified.</value>
    public Uri? Url { get; set; }

    /// <summary>
    /// Loads this <see cref="PodcastLicense"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="PodcastLicense"/> was initialized using the supplied <paramref name="source"/>; otherwise, <b>false</b>.</returns>
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
            this.Identifier = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="PodcastLicense"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("license", PodcastSyndicationExtension.NamespaceUri);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "url", this.Url);
        writer.WriteString(this.Identifier);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastLicense"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(PodcastLicense? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Identifier, other.Identifier, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Url, other.Url, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastLicense"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastLicense"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public bool Equals(PodcastLicense? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if equal; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is PodcastLicense other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Identifier),
        HashCodeUtility.Component(this.Url));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(PodcastLicense? first, PodcastLicense? second)
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
    public static bool operator !=(PodcastLicense? first, PodcastLicense? second) => !(first == second);
}