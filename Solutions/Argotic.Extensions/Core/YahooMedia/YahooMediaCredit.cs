using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents an entity that contributed to the creation of a media object.
/// </summary>
/// <remarks>
///     <para>
///         Current entities can include people, companies, locations, etc. Specific entities can have multiple roles,
///         and several entities can have the same role. These should appear as distinct <see cref="YahooMediaCredit"/> entities.
///     </para>
/// </remarks>
[Serializable]
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
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="entity"/> is an empty string.</exception>
    public YahooMediaCredit(string entity)
    {
        this.Entity = entity;
    }

    /// <summary>
    /// Gets the European Broadcasting Union Roles scheme.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the European Broadcasting Union Roles scheme, which has a value of <b>urn:ebu</b>.</value>
    /// <remarks>
    ///     This scheme can be assumed to be the default scheme for <see cref="YahooMediaCredit"/> when no scheme is provided.
    /// </remarks>
    public static Uri EuropeanBroadcastingUnionRoleScheme
    {
        get
        {
            return new Uri("urn:ebu");
        }
    }

    /// <summary>
    /// Gets or sets the name of the entity that contributed to this media object.
    /// </summary>
    /// <value>The name of the entity that contributed to the creation of this media object.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Entity
    {
        get => field;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the role the entity played in the creation of the media object.
    /// </summary>
    /// <value>The role the entity played in the creation of the media object.</value>
    /// <remarks>
    ///     All roles are converted to their lowercase equivalent. See <a href="http://www.ebu.ch/en/technical/metadata/specifications/role_codes.php">European Broadcasting Union Role Codes</a>
    ///     for a listing of the default entity roles.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
    public string Role
    {
        get => field;
        set => field = value?.ToLowerInvariant().Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a URI that identifies this role scheme.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents this role scheme. The default value is <b>null</b>.</value>
    /// <remarks>
    ///     If no rating scheme is provided, the default scheme is <b>urn:ebu</b>.
    /// </remarks>
    /// <seealso cref="EuropeanBroadcastingUnionRoleScheme"/>
    public Uri Scheme { get; set; }

    /// <summary>
    /// Loads this <see cref="YahooMediaCredit"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="YahooMediaCredit"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="YahooMediaCredit"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
                if (Uri.TryCreate(schemeAttribute, UriKind.RelativeOrAbsolute, out Uri scheme))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        YahooMediaSyndicationExtension extension = new();
        writer.WriteStartElement("credit", extension.XmlNamespace);

        if (!string.IsNullOrEmpty(this.Role))
        {
            writer.WriteAttributeString("role", this.Role);
        }

        if (this.Scheme != null)
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
    /// Returns a <see cref="String"/> that represents the current <see cref="YahooMediaCredit"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="YahooMediaCredit"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = new()
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            Indent = true,
            OmitXmlDeclaration = true
        };

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
        result |= string.Compare(this.Role, other.Role, StringComparison.OrdinalIgnoreCase);
        result |= Uri.Compare(this.Scheme, other.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="YahooMediaCredit"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="YahooMediaCredit"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="YahooMediaCredit"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is YahooMediaCredit other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Entity, this.Role, this.Scheme);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(YahooMediaCredit first, YahooMediaCredit second)
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
    public static bool operator !=(YahooMediaCredit first, YahooMediaCredit second)
    {
        return !(first == second);
    }
}