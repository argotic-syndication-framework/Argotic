using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a free-form text value carried in a feed for a named purpose.
/// </summary>
/// <remarks>
///     <para>
///     Podcasting 2.0's <c>podcast:txt</c> — the namespace's equivalent of a DNS <c>TXT</c> record, and
///     deliberately unstructured: a purpose string and a value, whose meaning is agreed between
///     publisher and whoever reads it.
///     </para>
///     <para>
///     <b>This is the element that carries Apple Podcasts ownership verification for roughly half the
///     publishers who set one.</b> §2.49 found the token written as
///     <c>&lt;podcast:txt purpose="applepodcastsverify"&gt;</c> in 21 of 1,934 live feeds, against 26
///     using <c>itunes:applepodcastsverify</c>. Argotic read the iTunes form and not this one, so a
///     publisher mid-claim on Omny, Captivate or Buzzsprout lost their token on a round-trip. That gap
///     is what this element closes.
///     </para>
/// </remarks>
/// <seealso cref="PodcastSyndicationExtensionContext.TextEntries"/>
public class PodcastText : IComparable<PodcastText>, IEquatable<PodcastText>, IComparisonOperators
{
    /// <summary>
    /// The <c>purpose</c> value Apple Podcasts uses to carry a feed ownership verification token.
    /// </summary>
    /// <remarks>
    ///     Entirely lower case, established by survey rather than documentation — see §2.49 and
    ///     <see cref="ITunesSyndicationExtensionContext.VerificationToken"/>, which is the same token
    ///     delivered by the other route.
    /// </remarks>
    public const string ApplePodcastsVerifyPurpose = "applepodcastsverify";

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastText"/> class.
    /// </summary>
    public PodcastText()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PodcastText"/> class using the supplied purpose and value.
    /// </summary>
    /// <param name="purpose">What this text is for.</param>
    /// <param name="value">The text itself.</param>
    public PodcastText(string purpose, string value)
    {
        this.Purpose = purpose;
        this.Value = value;
    }

    /// <summary>
    /// Gets or sets what this text is for.
    /// </summary>
    /// <value>The purpose, or an <i>empty</i> string if none was specified.</value>
    /// <remarks>
    ///     Free-form by design — it may be a domain name, or a word such as <c>verify</c> or
    ///     <c>release</c>. The specification asks that it not exceed 128 characters.
    /// </remarks>
    public string Purpose
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the text itself.
    /// </summary>
    /// <value>The text. The default value is an <i>empty</i> string.</value>
    public string Value
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this entry carries an Apple Podcasts ownership verification token.
    /// </summary>
    /// <value><see langword="true"/> if <see cref="Purpose"/> is <c>applepodcastsverify</c>; otherwise, <see langword="false"/>.</value>
    public bool IsApplePodcastsVerification =>
        string.Equals(this.Purpose, ApplePodcastsVerifyPurpose, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Loads this <see cref="PodcastText"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="PodcastText"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        ArgumentNullException.ThrowIfNull(source);
        bool wasLoaded = false;

        string purposeAttribute = source.GetAttribute("purpose", string.Empty);
        if (!string.IsNullOrEmpty(purposeAttribute))
        {
            this.Purpose = purposeAttribute;
            wasLoaded = true;
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Value = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="PodcastText"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("txt", PodcastSyndicationExtension.NamespaceUri);
        PodcastExtensionUtility.WriteOptionalAttribute(writer, "purpose", this.Purpose);
        writer.WriteString(this.Value);
        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="PodcastText"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString() => PodcastExtensionUtility.ToXmlString(this.WriteTo);

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(PodcastText? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Purpose, other.Purpose, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Value, other.Value, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="PodcastText"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="PodcastText"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public bool Equals(PodcastText? other) => other is not null && this.CompareTo(other) == 0;

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if equal; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is PodcastText other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(
        HashCodeUtility.Component(this.Purpose),
        HashCodeUtility.Component(this.Value));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(PodcastText? first, PodcastText? second)
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
    public static bool operator !=(PodcastText? first, PodcastText? second) => !(first == second);
}