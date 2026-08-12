using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents an entity that contributed to the creation of a media object.
/// </summary>
/// <remarks>
///     An entity may be a person, a company or a place. One entity may hold several roles and one role may be held
///     by several entities, and each combination is a separate credit — so the collection is a list of pairings,
///     not a list of contributors, and grouping by <see cref="Entity"/> is the caller's job.
/// </remarks>
public class YahooMediaCredit : IComparable<YahooMediaCredit>, IEquatable<YahooMediaCredit>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaCredit"/> class.
    /// </summary>
    public YahooMediaCredit()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooMediaCredit"/> class using the supplied entity name.
    /// </summary>
    /// <param name="entity">The name of the entity that contributed to the creation of the media object.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="entity"/> is an empty string.</exception>
    public YahooMediaCredit(string entity)
    {
        this.Entity = entity;
    }

    /// <summary>
    /// Gets the European Broadcasting Union Roles scheme.
    /// </summary>
    /// <value>The scheme a credit with no <see cref="Scheme"/> belongs to: <c>urn:ebu</c>.</value>
    public static Uri EuropeanBroadcastingUnionRoleScheme => new("urn:ebu");

    /// <summary>
    /// Gets or sets the name of the entity that contributed to this media object.
    /// </summary>
    /// <value>The entity name, trimmed. The default value is an <i>empty</i> string, which is the one value a set operation cannot produce.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Entity
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the role the entity played in the creation of the media object.
    /// </summary>
    /// <value>The role, lower-cased. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     The setter lower-cases invariantly, so a round-trip does not preserve a publisher's <c>Director</c> —
    ///     it writes back <c>director</c>. Under <see cref="EuropeanBroadcastingUnionRoleScheme"/> the roles are
    ///     drawn from
    ///     <a href="https://tech-metadata.ebu-it-tools.ch/ontologies/skos/ebu_RoleCodeCS.htm">European Broadcasting Union Role Codes</a>;
    ///     under any other <see cref="Scheme"/> the value is whatever that scheme says.
    /// </remarks>
    public string Role
    {
        get;
        set => field = value?.ToLowerInvariant().Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a URI that identifies this role scheme.
    /// </summary>
    /// <value>The vocabulary <see cref="Role"/> is drawn from. The default value is <see langword="null"/>, which means <c>urn:ebu</c>.</value>
    /// <remarks>
    ///     The inference is the caller's to make. <see langword="null"/> is kept distinct from
    ///     <see cref="EuropeanBroadcastingUnionRoleScheme"/> so that saving does not write a <c>scheme</c>
    ///     attribute the publisher omitted.
    /// </remarks>
    /// <seealso cref="EuropeanBroadcastingUnionRoleScheme"/>
    public Uri? Scheme { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaCredit"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="YahooMediaCredit"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaCredit"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string roleAttribute = source.GetAttribute("role", string.Empty);
            string schemeAttribute = source.GetAttribute("scheme", string.Empty);

            if (!string.IsNullOrEmpty(roleAttribute))
            {
                this.Role = roleAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(schemeAttribute))
            {
                if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri? scheme))
                {
                    this.Scheme = scheme;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Entity = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="YahooMediaCredit"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("credit", YahooMediaSyndicationExtension.NamespaceUri);

        if (!string.IsNullOrEmpty(this.Role))
        {
            writer.WriteAttributeString("role", this.Role);
        }

        if (this.Scheme is not null)
        {
            writer.WriteAttributeString("scheme", this.Scheme.ToString());
        }

        if (!string.IsNullOrEmpty(this.Entity))
        {
            writer.WriteString(this.Entity);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="YahooMediaCredit"/>.
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
    public int CompareTo(YahooMediaCredit? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Entity, other.Entity, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Role, other.Role, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Scheme, other.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaCredit"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaCredit"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="YahooMediaCredit"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(YahooMediaCredit? other)
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
    public override bool Equals(object? obj) => obj is YahooMediaCredit other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Entity), HashCodeUtility.Component(this.Role), HashCodeUtility.Component(this.Scheme));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(YahooMediaCredit? first, YahooMediaCredit? second)
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
    public static bool operator !=(YahooMediaCredit? first, YahooMediaCredit? second) => !(first == second);
}