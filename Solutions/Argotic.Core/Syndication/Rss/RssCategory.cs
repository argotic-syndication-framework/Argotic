using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a category or tag to which a <see cref="RssFeed"/> or <see cref="RssItem"/> belongs.
/// </summary>
/// <seealso cref="RssChannel.Categories"/>
/// <seealso cref="RssItem.Categories"/>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssCategory class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssCategoryExample.cs" 
///             region="RssCategory" 
///         />
///     </code>
/// </example>
public class RssCategory : IComparable<RssCategory>, IEquatable<RssCategory>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="RssCategory"/> class.
    /// </summary>
    public RssCategory()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssCategory"/> class using the supplied hierarchical position in the taxonomy.
    /// </summary>
    /// <param name="value">A slash-delimited string that identifies a hierarchical position in the taxonomy.</param>
    public RssCategory(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssCategory"/> class using the supplied hierarchical position in the taxonomy and taxonomy identifier.
    /// </summary>
    /// <param name="value">A slash-delimited string that identifies a hierarchical position in the taxonomy.</param>
    /// <param name="domain">A string that identifies the taxonomy in which the category is placed.</param>
    public RssCategory(string value, string domain) : this(value)
    {
        this.Domain = domain;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssCategory"/> class using the supplied <see cref="IList{T}"/>.
    /// </summary>
    /// <param name="value">A collection of strings that describe the hierarchical position in the taxonomy. The order of collection elements determines the taxonomy hierarchy.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public RssCategory(IList<string> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Count > 0)
        {
            this.Value = string.Join("/", value);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssCategory"/> class using the supplied <see cref="IList{T}"/> and taxonomy identifier.
    /// </summary>
    /// <param name="value">A collection of strings that describe the hierarchical position in the taxonomy. The order of collection elements determines the taxonomy hierarchy.</param>
    /// <param name="domain">A string that identifies the taxonomy in which the category is placed.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public RssCategory(IList<string> value, string domain) : this(value)
    {
        this.Domain = domain;
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
    /// Gets or sets a string that identifies the taxonomy in which the category is placed.
    /// </summary>
    /// <value>A string that identifies the taxonomy in which the category is placed. The default value is an empty string.</value>
    public string Domain
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Gets or sets a slash-delimited string that identifies a hierarchical position in the taxonomy.
    /// </summary>
    /// <value>A slash-delimited string that identifies a hierarchical position in the taxonomy. The default value is an empty string.</value>
    /// <remarks>
    ///     If the category represents a tag or is the root hierarchical position in the taxonomy, no slash-delimiter is necessary.
    /// </remarks>
    public string Value
    {
        get;
        set => field = value?.Trim() ?? string.Empty;
    } = string.Empty;

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <b>true</b> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is a null reference.</exception>
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        foreach (ISyndicationExtension extension in this.Extensions)
        {
            if (match(extension))
            {
                return extension;
            }
        }

        return null;
    }

    /// <summary>
    /// Loads this <see cref="RssCategory"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="RssCategory"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Value = source.Value;
            wasLoaded = true;
        }

        if (source.HasAttributes)
        {
            string domain = source.GetAttribute("domain", string.Empty);
            if (!string.IsNullOrEmpty(domain))
            {
                this.Domain = domain;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssCategory"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssCategory"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssCategory"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(settings);
        bool wasLoaded = this.Load(source);
        SyndicationExtensionAdapter adapter = new(source, settings);
        adapter.Fill(this);

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="RssCategory"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("category");

        if (!string.IsNullOrEmpty(this.Domain))
        {
            writer.WriteAttributeString("domain", this.Domain);
        }

        writer.WriteString(this.Value);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="RssCategory"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssCategory"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssCategory"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssCategory? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Domain, other.Domain, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssCategory"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssCategory"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="RssCategory"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(RssCategory? other)
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
    public override bool Equals(object? obj) => obj is RssCategory other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Domain ?? string.Empty),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(RssCategory? first, RssCategory? second)
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
    public static bool operator !=(RssCategory? first, RssCategory? second) => !(first == second);
}