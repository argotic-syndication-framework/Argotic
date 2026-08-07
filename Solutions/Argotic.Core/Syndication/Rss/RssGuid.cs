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
///     <para>
///         RSS 2.0 lays down no syntax at all for a guid: "There are no rules for the syntax of a guid.
///         Aggregators must view them as a string." Its one job is to let an aggregator recognise an item
///         it has already shown, so the only property that matters is that the publisher never reuses one.
///     </para>
///     <para>
///         <see cref="IsPermanentLink"/> defaults to <see langword="true"/>, and that default is the trap.
///         A publisher who writes a bare <c>&lt;guid&gt;</c> containing an opaque token — a database key, a
///         UUID — has, by the letter of the specification, told every reader that the token is a URL it may
///         open in a browser. Set <see cref="IsPermanentLink"/> to <see langword="false"/> whenever the
///         value is not a resolvable permalink.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Core\Rss\RssGuidExample.cs" language="cs" title="The following code example demonstrates the usage of the RssGuid class." />
/// </example>
public class RssGuid : IComparable<RssGuid>, IEquatable<RssGuid>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

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
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is an empty string.</exception>
    public RssGuid(string value)
    {
        this.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RssGuid"/> class using the supplied value.
    /// </summary>
    /// <param name="value">A string value that uniquely identifies the item.</param>
    /// <param name="isPermanentUrl"><see langword="true"/> if <paramref name="value"/> is a permanent URL that can be opened in a browser; otherwise, <see langword="false"/>.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is an empty string.</exception>
    public RssGuid(string value, bool isPermanentUrl) : this(value)
    {
        this.IsPermanentLink = isPermanentUrl;
    }

    /// <summary>
    /// Gets the syndication extensions applied to this syndication entity.
    /// </summary>
    public IList<ISyndicationExtension> Extensions { get; } = [];

    /// <summary>
    /// Gets a value indicating if this syndication entity has one or more syndication extensions applied to it.
    /// </summary>
    /// <value><see langword="true"/> if the <see cref="Extensions"/> collection for this entity contains one or more <see cref="ISyndicationExtension"/> objects; otherwise, <see langword="false"/>.</value>
    public bool HasExtensions => this.Extensions.Count > 0;

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Value"/> is a permanent URL that can be opened in a browser.
    /// </summary>
    /// <value>The default value is <see langword="true"/> — the default the specification assigns to an absent <c>isPermaLink</c> attribute.</value>
    /// <remarks>
    ///     When <see langword="false"/>, "the guid may not be assumed to be a url, or a url to anything in
    ///     particular", and the publisher is free to use any scheme that guarantees uniqueness — the tag URI
    ///     scheme of <a href="https://www.rfc-editor.org/rfc/rfc4151.html">RFC 4151</a> is the usual choice,
    ///     because it embeds a domain and a date and so needs no central registry.
    /// </remarks>
    public bool IsPermanentLink { get; set; } = true;

    /// <summary>
    /// Gets or sets a string value that uniquely identifies this item.
    /// </summary>
    /// <value>An opaque identifier. The default value is an <i>empty</i> string.</value>
    /// <remarks>
    ///     Uniqueness is the publisher's responsibility; nothing here enforces it. If
    ///     <see cref="IsPermanentLink"/> is <see langword="true"/> this must be the permanent URL of the page
    ///     for the item, since that is what a reader is entitled to assume.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Value
    {
        get;
        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            field = value.Trim();
        }
    } = string.Empty;

    /// <summary>
    /// Searches for a syndication extension that matches the conditions defined by the specified predicate, and returns the first occurrence within the <see cref="Extensions"/> collection.
    /// </summary>
    /// <param name="match">The <see cref="Predicate{ISyndicationExtension}"/> delegate that defines the conditions of the <see cref="ISyndicationExtension"/> to search for.</param>
    /// <returns>
    ///     The first syndication extension that matches the conditions defined by the specified predicate, if found; otherwise, the default value for <see cref="ISyndicationExtension"/>.
    /// </returns>
    /// <remarks>
    ///     The <see cref="Predicate{ISyndicationExtension}"/> is a delegate to a method that returns <see langword="true"/> if the object passed to it matches the conditions defined in the delegate.
    ///     The elements of the current <see cref="Extensions"/> are individually passed to the <see cref="Predicate{ISyndicationExtension}"/> delegate, moving forward in
    ///     the <see cref="Extensions"/>, starting with the first element and ending with the last element. Processing is stopped when a match is found.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="match"/> is <see langword="null"/>.</exception>
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
    /// Loads this <see cref="RssGuid"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="RssGuid"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssGuid"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
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
    /// <returns><see langword="true"/> if the <see cref="RssGuid"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="RssGuid"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="settings"/> is <see langword="null"/>.</exception>
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
    /// Saves the current <see cref="RssGuid"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="RssGuid"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="RssGuid"/>.</returns>
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
        if (result == 0) result = string.Compare(this.Value, other.Value, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="RssGuid"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="RssGuid"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="RssGuid"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is RssGuid other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(
            HashCodeUtility.Component(this.IsPermanentLink),
            StringComparer.OrdinalIgnoreCase.GetHashCode(this.Value ?? string.Empty));
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(RssGuid? first, RssGuid? second)
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
    public static bool operator !=(RssGuid? first, RssGuid? second) => !(first == second);
}