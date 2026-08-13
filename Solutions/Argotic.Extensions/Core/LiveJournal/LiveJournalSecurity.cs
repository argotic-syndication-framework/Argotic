using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the access level of a LiveJournal entry.
/// </summary>
/// <seealso cref="LiveJournalSyndicationExtensionContext.Security"/>
public class LiveJournalSecurity : IComparable<LiveJournalSecurity>, IEquatable<LiveJournalSecurity>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class.
    /// </summary>
    public LiveJournalSecurity()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class using the supplied <see cref="LiveJournalSecurityType"/>.
    /// </summary>
    /// <param name="accessType">A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type.</param>
    public LiveJournalSecurity(LiveJournalSecurityType accessType)
    {
        this.Accessibility = accessType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LiveJournalSecurity"/> class using the supplied <see cref="LiveJournalSecurityType"/>.
    /// </summary>
    /// <param name="accessType">A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type.</param>
    /// <param name="mask">A bit field naming the friend groups the entry is visible to. Meaningful only when <paramref name="accessType"/> is <see cref="LiveJournalSecurityType.Friends"/>.</param>
    public LiveJournalSecurity(LiveJournalSecurityType accessType, int mask)
    {
        this.Accessibility = accessType;
        this.Mask = mask;
    }

    /// <summary>
    /// Gets or sets the accessibility type.
    /// </summary>
    /// <value>A <see cref="LiveJournalSecurityType"/> enumeration value that represents the access type. The default value is <see cref="LiveJournalSecurityType.Public"/>.</value>
    public LiveJournalSecurityType Accessibility { get; set; } = LiveJournalSecurityType.Public;

    /// <summary>
    /// Gets or sets the friend-groups mask.
    /// </summary>
    /// <value>A bit field naming the friend groups the entry is visible to. The default value is <see cref="Int32.MinValue"/>, which indicates that no friend-groups mask was specified.</value>
    /// <remarks>
    ///     Applies only when <see cref="Accessibility"/> is <see cref="LiveJournalSecurityType.Friends">friends</see>,
    ///     and LiveJournal emits it only when the author of the post is the same user who authenticated
    ///     the feed request — so for anyone else it is always absent, whatever the entry's real
    ///     visibility.
    /// </remarks>
    public int Mask { get; set; } = int.MinValue;

    /// <summary>
    /// Returns the access level identifier for the supplied <see cref="LiveJournalSecurityType"/>.
    /// </summary>
    /// <param name="level">The <see cref="LiveJournalSecurityType"/> to get the access level identifier for.</param>
    /// <returns>The access level identifier for the supplied <paramref name="level"/>; otherwise, an empty string.</returns>
    public static string AccessibilityAsString(LiveJournalSecurityType level) =>
        EnumerationMetadataAttribute.GetAlternateValue(level);

    /// <summary>
    /// Returns the <see cref="LiveJournalSecurityType"/> enumeration value that corresponds to the specified access level name.
    /// </summary>
    /// <param name="name">The name of the access level.</param>
    /// <returns>A <see cref="LiveJournalSecurityType"/> enumeration value that corresponds to the specified string; otherwise, <see cref="LiveJournalSecurityType.None"/>.</returns>
    /// <remarks>This method disregards case of specified access level name.</remarks>
    public static LiveJournalSecurityType AccessibilityByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, LiveJournalSecurityType.None);

    /// <summary>
    /// Loads this <see cref="LiveJournalSecurity"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="LiveJournalSecurity"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="LiveJournalSecurity"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string maskAttribute = source.GetAttribute("mask", string.Empty);

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                LiveJournalSecurityType accessLevel = LiveJournalSecurity.AccessibilityByName(typeAttribute);
                if (accessLevel != LiveJournalSecurityType.None)
                {
                    this.Accessibility = accessLevel;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(maskAttribute))
            {
                if (int.TryParse(maskAttribute, System.Globalization.NumberStyles.Integer, System.Globalization.NumberFormatInfo.InvariantInfo, out int mask))
                {
                    this.Mask = mask;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="LiveJournalSecurity"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("security", LiveJournalSyndicationExtension.NamespaceUri);

        writer.WriteAttributeString("type", LiveJournalSecurity.AccessibilityAsString(this.Accessibility));

        if (this.Mask != int.MinValue)
        {
            writer.WriteAttributeString("mask", this.Mask.ToString(System.Globalization.NumberFormatInfo.InvariantInfo));
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="LiveJournalSecurity"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

        using (XmlWriter writer = XmlWriter.Create(stream, settings))
        {
            this.WriteTo(writer);
        }

        stream.Seek(0, SeekOrigin.Begin);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <remarks>
    ///     <see cref="Accessibility"/> is primary and <see cref="Mask"/> secondary, which is the order
    ///     they matter in: the accessibility decides who may read the entry, and the mask only narrows a
    ///     <see cref="LiveJournalSecurityType.Friends">friends</see> entry further. Comparing the mask
    ///     alone made <c>new LiveJournalSecurity(Public)</c> and <c>new LiveJournalSecurity(Private)</c>
    ///     equal, because neither sets a mask.
    /// </remarks>
    public int CompareTo(LiveJournalSecurity? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Accessibility.CompareTo(other.Accessibility);
        if (result == 0) result = this.Mask.CompareTo(other.Mask);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="LiveJournalSecurity"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="LiveJournalSecurity"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="LiveJournalSecurity"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(LiveJournalSecurity? other)
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is LiveJournalSecurity other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>Folds the same two members <see cref="CompareTo"/> compares, in the same order.</remarks>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Accessibility), HashCodeUtility.Component(this.Mask));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(LiveJournalSecurity? first, LiveJournalSecurity? second)
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
    public static bool operator !=(LiveJournalSecurity? first, LiveJournalSecurity? second) => !(first == second);

}