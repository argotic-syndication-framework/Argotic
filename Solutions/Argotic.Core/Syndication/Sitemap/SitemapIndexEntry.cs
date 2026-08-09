using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents an entry in a Sitemap index file.
/// </summary>
/// <remarks>
///     A <c>&lt;sitemap&gt;</c> entry in a <c>&lt;sitemapindex&gt;</c>, as defined by the
///     <a href="https://www.sitemaps.org/protocol.html">Sitemaps Protocol</a>. It points at another sitemap
///     document rather than at a page; <see cref="SitemapUrl"/> is the entry type that points at pages.
/// </remarks>
/// <seealso cref="SitemapIndex.Sitemaps"/>
public class SitemapIndexEntry : IComparable<SitemapIndexEntry>, IEquatable<SitemapIndexEntry>, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapIndexEntry"/> class.
    /// </summary>
    public SitemapIndexEntry()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapIndexEntry"/> class using the specified location.
    /// </summary>
    /// <param name="location">A <see cref="Uri"/> that represents the location of the sitemap.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
    public SitemapIndexEntry(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.Location = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapIndexEntry"/> class using the specified location and last modified date.
    /// </summary>
    /// <param name="location">A <see cref="Uri"/> that represents the location of the sitemap.</param>
    /// <param name="lastModified">A <see cref="DateTime"/> that indicates when the sitemap was last modified.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
    public SitemapIndexEntry(Uri location, DateTime lastModified)
        : this(location)
    {
        this.LastModified = lastModified;
    }

    /// <summary>
    /// Gets or sets the location of the sitemap.
    /// </summary>
    /// <value>The absolute URL of the referenced sitemap document, or <see langword="null"/> if none was specified. Required by the protocol.</value>
    /// <remarks>
    ///     The protocol asks that URLs follow <a href="https://www.rfc-editor.org/rfc/rfc3986.html">RFC 3986</a>
    ///     for URIs and <a href="https://www.rfc-editor.org/rfc/rfc3987.html">RFC 3987</a> for IRIs. It also
    ///     confines an index to its own site: "A Sitemap index file can only specify Sitemaps that are found
    ///     on the same site as the Sitemap index file." Nothing here checks that, because the check needs the
    ///     address the index will itself be served from, which this object does not know.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Location
    {
        get;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>
    /// Gets or sets the date of last modification of the sitemap.
    /// </summary>
    /// <value>The default value is <see langword="null"/>, meaning the entry carried no <c>lastmod</c>.</value>
    /// <remarks>
    ///     When the <i>referenced sitemap</i> last changed — not this index. The protocol asks for W3C
    ///     Datetime, so <c>YYYY</c>, <c>YYYY-MM</c> and <c>YYYY-MM-DD</c> are all legal on the wire; saving
    ///     always writes a full RFC 3339 timestamp.
    /// </remarks>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Loads this <see cref="SitemapIndexEntry"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapIndexEntry"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SitemapIndexEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(source.NameTable);

        XPathNavigator? locNavigator = source.SelectChildElement("sm", "loc", manager);
        XPathNavigator? lastmodNavigator = source.SelectChildElement("sm", "lastmod", manager);

        if (locNavigator is not null)
        {
            if (Uri.TryCreate(locNavigator.Value, UriKind.Absolute, out Uri? location))
            {
                this.Location = location;
                wasLoaded = true;
            }
        }

        if (lastmodNavigator is not null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(lastmodNavigator.Value, out DateTime lastModified))
            {
                this.LastModified = lastModified;
                wasLoaded = true;
            }
            else if (DateTime.TryParse(lastmodNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out lastModified))
            {
                this.LastModified = lastModified;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="SitemapIndexEntry"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("sitemap", SitemapUtility.SitemapNamespace);

        if (this.Location is not null)
        {
            writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, this.Location.ToString());
        }

        if (this.LastModified.HasValue)
        {
            writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.LastModified.Value));
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapIndexEntry"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapIndexEntry"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapIndexEntry? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Uri.Compare(this.Location, other.Location, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

        if (result != 0)
        {
            return result;
        }

        result = Nullable.Compare(this.LastModified, other.LastModified);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapIndexEntry"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapIndexEntry"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapIndexEntry"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapIndexEntry? other)
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
    public override bool Equals(object? obj) => obj is SitemapIndexEntry other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Location), HashCodeUtility.Component(this.LastModified));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapIndexEntry? first, SitemapIndexEntry? second)
    {
        if (first is null)
        {
            return second is null;
        }

        return first.Equals(second);
    }

    /// <summary>
    /// Determines if operands are not equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(SitemapIndexEntry? first, SitemapIndexEntry? second) => !(first == second);
}