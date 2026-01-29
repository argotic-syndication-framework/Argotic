using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a means of uniquely identifying a <see cref="RssItem"/>.
/// </summary>
/// <seealso cref="RssItem.Guid"/>
/// <remarks>
///     <para>A publisher <i>should</i> provide a guid with each item.</para>
///     <para>
///         A <see cref="RssGuid"/> enables an aggregator to detect when an item has been received previously and does not need to be presented to a user again. 
///         If the guid's <see cref="RssGuid.IsPermanentLink"/> property has a value of <b>true</b>, the guid's value <b>must</b> be 
///         the permanent URL of the web page associated with the item. Otherwise, the guid may employ any syntax the feed's publisher 
///         has devised for ensuring the uniqueness of the string.
///     </para>
/// </remarks>
/// <example>
///     <code lang="cs" title="The following code example demonstrates the usage of the RssGuid class.">
///         <code 
///             source="..\..\Argotic.Examples\Core\Rss\RssGuidExample.cs" 
///             region="RssGuid" 
///         />
///     </code>
/// </example>
[Serializable]
public class RssGuid : IComparable<RssGuid>, IEquatable<RssGuid>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Private member to hold a string value that uniquely identifies the item.
    /// </summary>
    private string guidIdentifier = string.Empty;
    /// <summary>
    /// Initializes a new instance of the <see cref="RssGuid"/> class.
    /// </summary>
    public RssGuid()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssGuid"/> class using the supplied value.
    /// </summary>
    /// <param name="value">A string value that uniquely identifies the item.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public RssGuid(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssGuid"/> class using the supplied value.
    /// </summary>
    /// <param name="value">A string value that uniquely identifies the item.</param>
    /// <param name="isPermanentUrl"><b>true</b> if the <paramref name="value"/> represents a permanent URL of a web page; Otherwise, <b>false</b>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public RssGuid(string value, bool isPermanentUrl) : this(value)
    {
        this.IsPermanentLink = isPermanentUrl;
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
    /// Gets or sets a value indicating if the guid represents a permanent URL of a web page associated with this item.
    /// </summary>
    /// <value><b>true</b> if the guid <see cref="RssGuid.Value">value</see> represents a permanent URL of a web page; Otherwise, <b>false</b>.</value>
    /// <remarks>
    ///     If set to <b>false</b>, the guid may employ any syntax the feed's publisher has devised for ensuring the uniqueness of the string,
    ///     such as the <a href="http://www.faqs.org/rfcs/rfc4151.html">Tag URI scheme</a> described in RFC 4151.
    /// </remarks>
    public bool IsPermanentLink { get; set; } = true;

    /// <summary>
    /// Gets or sets a string value that uniquely identifies this item.
    /// </summary>
    /// <value>A string value that uniquely identifies this item.</value>
    /// <remarks>
    ///     <para>
    ///         If the guid's <see cref="RssGuid.IsPermanentLink"/> property has a value of <b>true</b>, the <see cref="RssGuid.Value"/> property <b>must</b> be 
    ///         the permanent URL of the web page associated with this item. Otherwise, the <see cref="RssGuid.Value"/> property may employ any syntax the feed's publisher 
    ///         has devised for ensuring the uniqueness of the string.
    ///     </para>
    ///     <para>
    ///         When choosing to employ a syntax for ensuring the uniqueness of the string, the <a href="http://www.faqs.org/rfcs/rfc4151.html">Tag URI scheme</a> 
    ///         described in RFC 4151 is recommended.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is an empty string.</exception>
    public string Value
    {
        get => guidIdentifier;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            guidIdentifier = value.Trim();
        }
    }
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
    public ISyndicationExtension FindExtension(Predicate<ISyndicationExtension> match)
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
    /// Loads this <see cref="RssGuid"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="RssGuid"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssGuid"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        if (source.HasAttributes)
        {
            string permalinkAttribute = source.GetAttribute("isPermaLink", string.Empty);

            if (!string.IsNullOrEmpty(permalinkAttribute))
            {
                if (bool.TryParse(permalinkAttribute, out bool isPermaLink))
                {
                    this.IsPermanentLink = isPermaLink;
                    wasLoaded = true;
                }
            }
        }

        if (!string.IsNullOrEmpty(source.Value))
        {
            this.Value = source.Value;
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Loads this <see cref="RssGuid"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="RssGuid"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssGuid"/>.
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
    /// Saves the current <see cref="RssGuid"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStartElement("guid");

        writer.WriteAttributeString("isPermaLink", this.IsPermanentLink ? "true" : "false");
        writer.WriteValue(this.Value);
        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }
    /// <summary>
    /// Returns a <see cref="String"/> that represents the current <see cref="RssGuid"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="RssGuid"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();
    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">The <see cref="RssGuid"/> to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(RssGuid? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.IsPermanentLink.CompareTo(other.IsPermanentLink);
        result |= string.Compare(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssGuid"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssGuid"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="RssGuid"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(RssGuid? other)
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
        return obj is RssGuid other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            this.IsPermanentLink,
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(RssGuid first, RssGuid second)
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
    public static bool operator !=(RssGuid first, RssGuid second)
    {
        return !(first == second);
    }
}