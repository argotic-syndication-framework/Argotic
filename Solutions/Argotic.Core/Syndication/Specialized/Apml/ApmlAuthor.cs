using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents the author of a <see cref="ApmlSource"/>.
/// </summary>
/// <seealso cref="ApmlSource.Authors"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlAuthor class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Apml\ApmlAuthorExample.cs"
///             region="ApmlAuthor"
///         />
///     </code>
/// </example>
[Serializable]
public class ApmlAuthor : IComparable<ApmlAuthor>, IEquatable<ApmlAuthor>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlAuthor"/> class.
    /// </summary>
    public ApmlAuthor()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlAuthor"/> class using the supplied parameters.
    /// </summary>
    /// <param name="key">The unique key for this author.</param>
    /// <param name="value">The decimal score of this author.</param>
    /// <remarks>
    ///     This constructor is meant to be used when creating an <b>explicit</b> author. Explicit data is for items that are explicitly added by a user to represent something.
    ///     For example, a user could edit their own APML file and add items they know they're interested in.
    ///     For this reason the <see cref="From"/> and <see cref="UpdatedOn"/> properties are not necessary for explicit data items, because it's a manual process.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    public ApmlAuthor(string key, decimal value)
    {
        this.Key = key;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlAuthor"/> class using the supplied parameters.
    /// </summary>
    /// <param name="key">The unique key for this author.</param>
    /// <param name="value">The decimal score of this author.</param>
    /// <param name="from">The name of the entity that contributed this author.</param>
    /// <param name="utcUpdatedOn">A <see cref="DateTime"/> object that indicates the last time this author was updated.</param>
    /// <remarks>
    ///     This constructor is meant to be used when creating an <b>implicit</b> author. Implicit data is added by machines/computers that try to make
    ///     some informed guesses about the things that you are interested in. This stuff will change over time and are added with a certain degree of confidence
    ///     that may have a decay in certain applications. For this reason it is important to keep a track of when things were added/modified.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/> is an empty string.</exception>
    public ApmlAuthor(string key, decimal value, string from, DateTime utcUpdatedOn) : this(key, value)
    {
        ArgumentException.ThrowIfNullOrEmpty(from);
        this.From = from;
        this.UpdatedOn = utcUpdatedOn;
    }
    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ISyndicationExtension"/> objects that represent syndication extensions applied to this syndication entity.</value>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><b>true</b> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects, Otherwise, returns <b>false</b>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;
    /// <summary>
    /// Gets or sets the name of the entity that contributed this author.
    /// </summary>
    /// <value>The name of the entity that contributed this author. The default value is an empty string, which indicates no contributor was specified.</value>
    public string From
    {
        get => field;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the unique key for this author.
    /// </summary>
    /// <value>The unique key for this author.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Key
    {
        get => field;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating the last time this author was updated.
    /// </summary>
    /// <value>A <see cref="DateTime"/> object that indicates the last time this author was updated. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update date was specified.</value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the decimal score of this author.
    /// </summary>
    /// <value>The decimal score of this author.</value>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    public decimal Value
    {
        get => field;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, decimal.MinusOne);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, decimal.One);
            field = value;
        }
    } = decimal.MinValue;
    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <summary>
    /// Loads this <see cref="ApmlAuthor"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="ApmlAuthor"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlAuthor"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string keyAttribute = source.GetAttribute("key", string.Empty);
            string valueAttribute = source.GetAttribute("value", string.Empty);
            string fromAttribute = source.GetAttribute("from", string.Empty);
            string updatedAttribute = source.GetAttribute("updated", string.Empty);

            if (!string.IsNullOrEmpty(keyAttribute))
            {
                this.Key = keyAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(valueAttribute))
            {
                if (decimal.TryParse(valueAttribute, System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo, out decimal value))
                {
                    if (value is >= decimal.MinusOne and <= decimal.One)
                    {
                        this.Value = value;
                        wasLoaded = true;
                    }
                }
            }

            if (!string.IsNullOrEmpty(fromAttribute))
            {
                this.From = fromAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(updatedAttribute))
            {
                if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(updatedAttribute, out DateTime updatedOn))
                {
                    this.UpdatedOn = updatedOn;
                    wasLoaded = true;
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlAuthor"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="ApmlAuthor"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlAuthor"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="ApmlAuthor"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("Author", ApmlUtility.ApmlNamespace);

        writer.WriteAttributeString("key", this.Key);
        writer.WriteAttributeString("value", this.Value.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo));

        if (!string.IsNullOrEmpty(this.From))
        {
            writer.WriteAttributeString("from", this.From);
        }

        if (this.UpdatedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("updated", SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="ApmlAuthor"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="ApmlAuthor"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();
    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(ApmlAuthor? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.From, other.From, StringComparison.OrdinalIgnoreCase);
        result |= string.Compare(this.Key, other.Key, StringComparison.OrdinalIgnoreCase);
        result |= this.UpdatedOn.CompareTo(other.UpdatedOn);
        result |= this.Value.CompareTo(other.Value);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApmlAuthor"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ApmlAuthor"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="ApmlAuthor"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(ApmlAuthor? other)
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
        return obj is ApmlAuthor other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.From ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Key ?? string.Empty),
            this.UpdatedOn,
            this.Value);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(ApmlAuthor first, ApmlAuthor second)
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
    public static bool operator !=(ApmlAuthor first, ApmlAuthor second)
    {
        return !(first == second);
    }
}
