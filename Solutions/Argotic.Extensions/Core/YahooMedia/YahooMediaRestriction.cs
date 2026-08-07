using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a means of allowing restrictions to be placed on the aggregator rendering the media in the feed.
/// </summary>
/// <remarks>
///     <para>
///         A restriction is three things together: a <see cref="Relationship"/> saying whether the list allows
///         or denies, an <see cref="EntityType"/> saying what the list is a list of, and the
///         <see cref="Entities"/> themselves. Read any one alone and you have the opposite of the publisher's
///         meaning half the time.
///     </para>
///     <para>
///         It is informational. Nothing in the format obliges an aggregator to honour it, and only one
///         restriction of a given <see cref="EntityType"/> applies to a media object — a second is ignored
///         rather than combined.
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
    /// <value>The entity list, whose meaning is fixed by <see cref="EntityType"/>. The default value is an <i>empty</i> collection.</value>
    /// <remarks>
    ///     <para>
    ///         Under <see cref="YahooMediaRestrictionType.Country"/> these are ISO 3166 country codes;
    ///         under <see cref="YahooMediaRestrictionType.Uri"/>, distributor URIs. Nothing here
    ///         validates either.
    ///     </para>
    ///     <para>
    ///         Two names are reserved and stand alone: <c>all</c> and <c>none</c>, each usable once and with no
    ///         <see cref="EntityType"/>.
    ///     </para>
    ///     <para>
    ///         The element's content is one space-separated list, and this collection is that list split apart —
    ///         so an entity containing a space cannot survive a round trip, and would be read back as two.
    ///     </para>
    /// </remarks>
    public IList<string> Entities { get; } = [];

    /// <summary>
    /// Gets or sets the type of media that this restriction applies to.
    /// </summary>
    /// <value>What <see cref="Entities"/> is a list of. The default value is <see cref="YahooMediaRestrictionType.None"/>, which is correct only for <c>all</c> and <c>none</c>.</value>
    public YahooMediaRestrictionType EntityType { get; set; } = YahooMediaRestrictionType.None;

    /// <summary>
    /// Gets or sets whether the entity list is the permitted set or the denied set.
    /// </summary>
    /// <value>The relationship. The default value is <see cref="YahooMediaRestrictionRelationship.None"/>, which leaves the sense of <see cref="Entities"/> undetermined.</value>
    public YahooMediaRestrictionRelationship Relationship { get; set; } = YahooMediaRestrictionRelationship.None;

    /// <summary>
    /// Returns the relationship identifier for the supplied <see cref="YahooMediaRestrictionRelationship"/>.
    /// </summary>
    /// <param name="relationship">The <see cref="YahooMediaRestrictionRelationship"/> to get the relationship identifier for.</param>
    /// <returns>
    ///     The <c>relationship</c> attribute value, <c>allow</c> or <c>deny</c>.
    ///     <see cref="YahooMediaRestrictionRelationship.None"/> maps to an empty string, which is what keeps it
    ///     out of the written feed.
    /// </returns>
    public static string RelationshipAsString(YahooMediaRestrictionRelationship relationship) =>
        EnumerationMetadataAttribute.GetAlternateValue(relationship);

    /// <summary>
    /// Returns the <see cref="YahooMediaRestrictionRelationship"/> enumeration value that corresponds to the specified relationship name.
    /// </summary>
    /// <param name="name">The name of the relationship. Matched without regard to case.</param>
    /// <returns>
    ///     The matching <see cref="YahooMediaRestrictionRelationship"/>, or
    ///     <see cref="YahooMediaRestrictionRelationship.None"/> when <paramref name="name"/> is empty,
    ///     <see langword="null"/>, or neither <c>allow</c> nor <c>deny</c>. This method throws nothing.
    /// </returns>
    public static YahooMediaRestrictionRelationship RelationshipByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaRestrictionRelationship.None);

    /// <summary>
    /// Returns the restriction type identifier for the supplied <see cref="YahooMediaRestrictionType"/>.
    /// </summary>
    /// <param name="type">The <see cref="YahooMediaRestrictionType"/> to get the restriction type identifier for.</param>
    /// <returns>
    ///     The <c>type</c> attribute value, <c>country</c> or <c>uri</c>.
    ///     <see cref="YahooMediaRestrictionType.None"/> maps to an empty string, which is what keeps it out of
    ///     the written feed. The specification permits omitting <c>type</c> only when the entity is one of the
    ///     reserved literals <c>all</c> or <c>none</c>.
    /// </returns>
    public static string RestrictionTypeAsString(YahooMediaRestrictionType type) =>
        EnumerationMetadataAttribute.GetAlternateValue(type);

    /// <summary>
    /// Returns the <see cref="YahooMediaRestrictionType"/> enumeration value that corresponds to the specified restriction type name.
    /// </summary>
    /// <param name="name">The name of the restriction type. Matched without regard to case.</param>
    /// <returns>
    ///     The matching <see cref="YahooMediaRestrictionType"/>, or <see cref="YahooMediaRestrictionType.None"/>
    ///     when <paramref name="name"/> is empty, <see langword="null"/>, or neither <c>country</c> nor
    ///     <c>uri</c>. This method throws nothing.
    /// </returns>
    /// <remarks>
    ///     The maintained specification also defines <c>sharing</c>, which this enumeration does not model, so a
    ///     sharing restriction reads as <see cref="YahooMediaRestrictionType.None"/> and its type is lost on
    ///     save. The entity list itself survives.
    /// </remarks>
    public static YahooMediaRestrictionType RestrictionTypeByName(string name) =>
        EnumerationMetadataAttribute.GetEnumByAlternateValue(name, YahooMediaRestrictionType.None);

    /// <summary>
    /// Loads this <see cref="YahooMediaRestriction"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaRestriction"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaRestriction"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaRestriction"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(YahooMediaRestriction? first, YahooMediaRestriction? second) => !(first == second);
}