using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a means of allowing restrictions to be placed on the aggregator rendering the media in the feed.
/// </summary>
/// <remarks>
///     <para>
///         Currently, restrictions are based on <b>distributor</b> or <b>country code</b>. 
///         A <see cref="YahooMediaRestriction"/> is purely informational and no obligation can be assumed or implied. 
///         Only one <see cref="YahooMediaRestriction"/> object of the same type can be applied to a media object, all others will be ignored.
///     </para>
/// </remarks>
public class YahooMediaRestriction : IComparable<YahooMediaRestriction>, IEquatable<YahooMediaRestriction>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaRestriction"/> class.
    /// </summary>
    public YahooMediaRestriction()
    {
    }

    /// <summary>
    /// Gets the entities this restriction applies to.
    /// </summary>
    /// <value>
    ///     A <see cref="IList{T}"/> collection of <see cref="string"/> objects that represent the entities this restriction applies to.
    ///     The default value is an <i>empty</i> collection.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         To allow a producer to explicitly declare their intentions, two literal entity names are reserved: <b>all</b> and <b>none</b>. These literals can <u>only</u> be used once.
    ///     </para>
    ///     <para>
    ///         When the restriction <see cref="EntityType"/> is <see cref="YahooMediaRestrictionType.Uri"/> the elements in this collection should represent distributor <see cref="Uri">Uri's</see>.
    ///     </para>
    ///     <para>
    ///         When the restriction <see cref="EntityType"/> is <see cref="YahooMediaRestrictionType.Country"/> the elements in this collection should represent country codes.
    ///         See <a href="http://www.iso.org/iso/country_codes/iso_3166_code_lists/english_country_names_and_code_elements.htm">ISO 3166</a> for a listing of the permissible country codes.
    ///     </para>
    /// </remarks>
    public IList<string> Entities { get; } = [];

    /// <summary>
    /// Gets or sets the type of media that this restriction applies to.
    /// </summary>
    /// <value>A <see cref="YahooMediaRestrictionType"/> enumeration value that indicates the type of media that this restriction applies to.</value>
    public YahooMediaRestrictionType EntityType { get; set; } = YahooMediaRestrictionType.None;

    /// <summary>
    /// Gets or sets the type of relationship that this restriction represents.
    /// </summary>
    /// <value>A <see cref="YahooMediaRestrictionRelationship"/> enumeration value that indicates the type of relationship that this restriction represents.</value>
    public YahooMediaRestrictionRelationship Relationship { get; set; } = YahooMediaRestrictionRelationship.None;

    /// <summary>
    /// Returns the relationship identifier for the supplied <see cref="YahooMediaRestrictionRelationship"/>.
    /// </summary>
    /// <param name="relationship">The <see cref="YahooMediaRestrictionRelationship"/> to get the relationship identifier for.</param>
    /// <returns>The relationship identifier for the supplied <paramref name="relationship"/>, Otherwise, returns an empty string.</returns>
    public static string RelationshipAsString(YahooMediaRestrictionRelationship relationship) =>
        EnumerationMetadataAttribute.GetAlternateValue(relationship);

    /// <summary>
    /// Returns the <see cref="YahooMediaRestrictionRelationship"/> enumeration value that corresponds to the specified relationship name.
    /// </summary>
    /// <param name="name">The name of the relationship.</param>
    /// <returns>A <see cref="YahooMediaRestrictionRelationship"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>YahooMediaRestrictionRelationship.None</b>.</returns>
    /// <remarks>This method disregards case of specified relationship name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static YahooMediaRestrictionRelationship RelationshipByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaRestrictionRelationship.None);

    /// <summary>
    /// Returns the restriction type identifier for the supplied <see cref="YahooMediaRestrictionType"/>.
    /// </summary>
    /// <param name="type">The <see cref="YahooMediaRestrictionType"/> to get the restriction type identifier for.</param>
    /// <returns>The restriction type identifier for the supplied <paramref name="type"/>, Otherwise, returns an empty string.</returns>
    public static string RestrictionTypeAsString(YahooMediaRestrictionType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="YahooMediaRestrictionType"/> enumeration value that corresponds to the specified restriction type name.
    /// </summary>
    /// <param name="name">The name of the restriction type.</param>
    /// <returns>A <see cref="YahooMediaRestrictionType"/> enumeration value that corresponds to the specified string, Otherwise, returns <b>YahooMediaRestrictionType.None</b>.</returns>
    /// <remarks>This method disregards case of specified restriction type name.</remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    public static YahooMediaRestrictionType RestrictionTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaRestrictionType.None);

    /// <summary>
    /// Loads this <see cref="YahooMediaRestriction"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaRestriction"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaRestriction"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string relationshipAttribute = source.GetAttribute("relationship", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);

            if (!string.IsNullOrEmpty(relationshipAttribute))
            {
                YahooMediaRestrictionRelationship relationship = YahooMediaRestriction.RelationshipByName(relationshipAttribute);
                if (relationship != YahooMediaRestrictionRelationship.None)
                {
                    this.Relationship = relationship;
                    wasLoaded = true;
                }
            }

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                YahooMediaRestrictionType type = YahooMediaRestriction.RestrictionTypeByName(typeAttribute);
                if (type != YahooMediaRestrictionType.None)
                {
                    this.EntityType = type;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            if (source.Value.Contains(' ', StringComparison.Ordinal))
            {
                string[] entities = source.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (entities.Length > 0)
                {
                    foreach (string entity in entities)
                    {
                        this.Entities.Add(entity);
                    }
                    wasLoaded = true;
                }
            }
            else
            {
                this.Entities.Add(source.Value);
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaRestriction"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("restriction", extension.XmlNamespace);

        if (this.Relationship != YahooMediaRestrictionRelationship.None)
        {
            writer.WriteAttributeString("relationship", YahooMediaRestriction.RelationshipAsString(this.Relationship));
        }

        if (this.EntityType != YahooMediaRestrictionType.None)
        {
            writer.WriteAttributeString("type", YahooMediaRestriction.RestrictionTypeAsString(this.EntityType));
        }

        if (this.Entities.Count > 0)
        {
            writer.WriteString(string.Join(" ", this.Entities));
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaRestriction"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="YahooMediaRestriction"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    public int CompareTo(YahooMediaRestriction? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Entities, other.Entities, StringComparison.Ordinal);
        if (result == 0) result = this.EntityType.CompareTo(other.EntityType);
        if (result == 0) result = this.Relationship.CompareTo(other.Relationship);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaRestriction"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaRestriction"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="YahooMediaRestriction"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(YahooMediaRestriction? other)
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj) => obj is YahooMediaRestriction other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     The collection members are folded in element by element. Passing the collection itself to
    ///     <see cref="HashCodeUtility.Component{T}(T)"/> would hash the list reference, so two instances
    ///     that <see cref="CompareTo"/> reports as equal hashed differently.
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.EntityType));
        hash.Add(HashCodeUtility.Component(this.Relationship));
        foreach (string entity in this.Entities)
        {
            hash.Add(HashCodeUtility.Component(entity));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(YahooMediaRestriction? first, YahooMediaRestriction? second)
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
    public static bool operator !=(YahooMediaRestriction? first, YahooMediaRestriction? second) => !(first == second);
}