using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication.Specialized;

/// <summary>
/// Represents a specific source of information that an entity is interested in.
/// </summary>
/// <seealso cref="ApmlProfile.ExplicitSources"/>
/// <seealso cref="ApmlProfile.ImplicitSources"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the ApmlSource class.">
///         <code
///             source="..\..\Argotic.Examples\Core\Apml\ApmlSourceExample.cs"
///             region="ApmlSource"
///         />
///     </code>
/// </example>
public class ApmlSource : IComparable<ApmlSource>, IEquatable<ApmlSource>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlSource"/> class.
    /// </summary>
    public ApmlSource()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlSource"/> class using the supplied parameters.
    /// </summary>
    /// <param name="key">The unique key for this source.</param>
    /// <param name="name">The friendly name of this source.</param>
    /// <param name="type">the MIME content type for this source.</param>
    /// <param name="value">The decimal score of this source.</param>
    /// <remarks>
    ///     This constructor is meant to be used when creating an <b>explicit</b> source. Explicit data is for items that are explicitly added by a user to represent something.
    ///     For example, a user could edit their own APML file and add items they know they're interested in.
    ///     For this reason the <see cref="From"/> and <see cref="UpdatedOn"/> properties are not necessary for explicit data items, because it's a manual process.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    public ApmlSource(string key, string name, string type, decimal value)
    {
        this.Key = key;
        this.Name = name;
        this.MimeType = type;
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApmlSource"/> class using the supplied parameters.
    /// </summary>
    /// <param name="key">The unique key for this source.</param>
    /// <param name="name">The friendly name of this source.</param>
    /// <param name="type">the MIME content type for this source.</param>
    /// <param name="value">The decimal score of this source.</param>
    /// <param name="from">The name of the entity that contributed this concept.</param>
    /// <remarks>
    ///     This constructor is meant to be used when creating an <b>implicit</b> source. Implicit data is added by machines/computers that try to make
    ///     some informed guesses about the things that you are interested in. This stuff will change over time and are added with a certain degree of confidence
    ///     that may have a decay in certain applications. For this reason it is important to keep a track of when things were added/modified.
    /// </remarks>
    /// <param name="utcUpdatedOn">A <see cref="DateTime"/> object that indicates the last time this concept was updated.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="key"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is an empty string.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="from"/> is an empty string.</exception>
    public ApmlSource(string key, string name, string type, decimal value, string from, DateTime utcUpdatedOn) : this(key, name, type, value)
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
    /// Gets the authors of this source.
    /// </summary>
    /// <value>A <see cref="IList{T}"/> collection of <see cref="ApmlAuthor"/> objects that represent the authors of this source.</value>
    public IList<ApmlAuthor> Authors { get; } = [];

    /// <summary>
    /// Gets or sets the name of the entity that contributed this source.
    /// </summary>
    /// <value>The name of the entity that contributed this source. The default value is an empty string, which indicates no contributor was specified.</value>
    public string From
    {
        get;
        set => field = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
    } = string.Empty;

    /// <summary>
    /// Gets or sets the unique key for this source.
    /// </summary>
    /// <value>The unique key for this source.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Key
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME content type for this source.
    /// </summary>
    /// <value>the MIME content type for this source.</value>
    /// <remarks>
    ///     See <a href="http://www.iana.org/assignments/media-types/">http://www.iana.org/assignments/media-types/</a> for a listing of registered MIME content types.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string MimeType
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets the friendly name of this source.
    /// </summary>
    /// <value>The friendly name of this source.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Name
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Gets or sets a date-time indicating the last time this source was updated.
    /// </summary>
    /// <value>A <see cref="DateTime"/> object that indicates the last time this source was updated. The default value is <see cref="DateTime.MinValue"/>, which indicates that no update date was specified.</value>
    /// <remarks>
    ///     The <see cref="DateTime"/> should be provided in Coordinated Universal Time (UTC).
    /// </remarks>
    public DateTime UpdatedOn { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the decimal score of this source.
    /// </summary>
    /// <value>The decimal score of this source.</value>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than -1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is greater than 1.</exception>
    public decimal Value
    {
        get;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, decimal.MinusOne);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, decimal.One);
            field = value;
        }
    } = decimal.MinValue;

    /// <summary>
    /// Loads this <see cref="ApmlSource"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="ApmlSource"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string keyAttribute = source.GetAttribute("key", string.Empty);
            string nameAttribute = source.GetAttribute("name", string.Empty);
            string valueAttribute = source.GetAttribute("value", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string fromAttribute = source.GetAttribute("from", string.Empty);
            string updatedAttribute = source.GetAttribute("updated", string.Empty);

            if (!string.IsNullOrEmpty(keyAttribute))
            {
                this.Key = keyAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.Name = nameAttribute;
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

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                this.MimeType = typeAttribute;
                wasLoaded = true;
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

        if (source.HasChildren)
        {
            XPathNodeIterator authorIterator = source.Select("apml:Author", manager);

            if (authorIterator is { Count: > 0 })
            {
                while (authorIterator.MoveNext())
                {
                    XPathNavigator? authorNode = authorIterator.Current;
                    if (authorNode is null)
                    {
                        continue;
                    }

                    ApmlAuthor author = new();
                    if (author.Load(authorNode))
                    {
                        this.Authors.Add(author);
                        wasLoaded = true;
                    }
                }
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="ApmlSource"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="ApmlSource"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="ApmlSource"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        XmlNamespaceManager manager = ApmlUtility.CreateNamespaceManager(source.NameTable);
        if (source.HasAttributes)
        {
            string keyAttribute = source.GetAttribute("key", string.Empty);
            string nameAttribute = source.GetAttribute("name", string.Empty);
            string valueAttribute = source.GetAttribute("value", string.Empty);
            string typeAttribute = source.GetAttribute("type", string.Empty);
            string fromAttribute = source.GetAttribute("from", string.Empty);
            string updatedAttribute = source.GetAttribute("updated", string.Empty);

            if (!string.IsNullOrEmpty(keyAttribute))
            {
                this.Key = keyAttribute;
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(nameAttribute))
            {
                this.Name = nameAttribute;
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

            if (!string.IsNullOrEmpty(typeAttribute))
            {
                this.MimeType = typeAttribute;
                wasLoaded = true;
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

        if (source.HasChildren)
        {
            XPathNodeIterator authorIterator = source.Select("apml:Author", manager);

            if (authorIterator is { Count: > 0 })
            {
                while (authorIterator.MoveNext())
                {
                    XPathNavigator? authorNode = authorIterator.Current;
                    if (authorNode is null)
                    {
                        continue;
                    }

                    ApmlAuthor author = new();
                    if (author.Load(authorNode, settings))
                    {
                        this.Authors.Add(author);
                        wasLoaded = true;
                    }
                }
            }
        }
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="ApmlSource"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("Source", ApmlUtility.ApmlNamespace);

        writer.WriteAttributeString("key", this.Key);
        writer.WriteAttributeString("name", this.Name);
        writer.WriteAttributeString("value", this.Value.ToString("0.00", System.Globalization.NumberFormatInfo.InvariantInfo));
        writer.WriteAttributeString("type", this.MimeType);

        if (!string.IsNullOrEmpty(this.From))
        {
            writer.WriteAttributeString("from", this.From);
        }

        if (this.UpdatedOn != DateTime.MinValue)
        {
            writer.WriteAttributeString("updated", SyndicationDateTimeUtility.ToRfc3339DateTime(this.UpdatedOn));
        }

        foreach (ApmlAuthor author in this.Authors)
        {
            author.WriteTo(writer);
        }
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="ApmlSource"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="ApmlSource"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(ApmlSource? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Authors, other.Authors);
        if (result == 0) result = string.Compare(this.From, other.From, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Key, other.Key, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.MimeType, other.MimeType, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = this.UpdatedOn.CompareTo(other.UpdatedOn);
        if (result == 0) result = this.Value.CompareTo(other.Value);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="ApmlSource"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="ApmlSource"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="ApmlSource"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(ApmlSource? other)
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
    public override bool Equals(object? obj) => obj is ApmlSource other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(this.Authors.Count),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.From ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Key ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.MimeType ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Name ?? string.Empty),
            HashCodeUtility.Component(this.UpdatedOn),
            HashCodeUtility.Component(this.Value));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(ApmlSource? first, ApmlSource? second)
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
    public static bool operator !=(ApmlSource? first, ApmlSource? second) => !(first == second);
}