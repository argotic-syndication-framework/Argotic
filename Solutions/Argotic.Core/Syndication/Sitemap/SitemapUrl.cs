using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;
using Argotic.Extensions;

namespace Argotic.Syndication;

/// <summary>
/// Represents a URL entry in a Sitemap.
/// </summary>
/// <remarks>
///     A <c>&lt;url&gt;</c> entry as defined by the
///     <a href="https://www.sitemaps.org/protocol.html">Sitemaps Protocol</a>. Only
///     <see cref="Location"/> is required; <see cref="LastModified"/>, <see cref="ChangeFrequency"/> and
///     <see cref="Priority"/> are optional, and "support for these optional tags may vary among search
///     engines".
/// </remarks>
public class SitemapUrl : IComparable<SitemapUrl>, IEquatable<SitemapUrl>, IExtensibleSyndicationObject, IComparisonOperators, IXmlWritable
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapUrl"/> class.
    /// </summary>
    public SitemapUrl()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapUrl"/> class using the specified URL location.
    /// </summary>
    /// <param name="location">A <see cref="Uri"/> that represents the URL of the page.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
    public SitemapUrl(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.Location = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapUrl"/> class using the specified URL location and last modified date.
    /// </summary>
    /// <param name="location">A <see cref="Uri"/> that represents the URL of the page.</param>
    /// <param name="lastModified">A <see cref="DateTime"/> that indicates when the page was last modified.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
    public SitemapUrl(Uri location, DateTime lastModified)
        : this(location)
    {
        this.LastModified = lastModified;
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
    /// Gets or sets the URL of the page.
    /// </summary>
    /// <value>An absolute URL, under 2,048 characters, or <see langword="null"/> if none was specified. Required by the protocol.</value>
    /// <remarks>
    ///     Every URL in a sitemap must use the same scheme and reside on the same host as the sitemap
    ///     document itself, and the sitemap's own directory bounds what it may list: a sitemap at
    ///     <c>http://example.com/catalog/sitemap.xml</c> may list <c>http://example.com/catalog/…</c> but not
    ///     <c>http://example.com/images/…</c>, and not <c>https://</c> anything. Out-of-scope URLs are not an
    ///     error — they "are dropped from further consideration", silently. Nothing here enforces the rule;
    ///     it is a property of the document as a whole, and only the publisher knows where it will be served.
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
    /// Gets or sets the date of last modification of the page.
    /// </summary>
    /// <value>The default value is <see langword="null"/>, meaning the entry carried no <c>lastmod</c>.</value>
    /// <remarks>
    ///     <para>
    ///         This is when the <i>linked page</i> last changed, not when the sitemap was generated. Setting
    ///         it to the generation time on every write is the commonest way to make a sitemap useless: it
    ///         tells a crawler the whole site changed, every time.
    ///     </para>
    ///     <para>
    ///         The protocol asks for W3C Datetime, <i>not</i> RFC 3339: the shorter forms <c>YYYY</c>,
    ///         <c>YYYY-MM</c> and <c>YYYY-MM-DD</c> are all legal <c>lastmod</c> values, and RFC 3339 permits
    ///         none of them. Saving always writes a complete RFC 3339 date and time. That conforms, but it is
    ///         narrower than the protocol allows, so a document that arrived carrying <c>2004-12-23</c> is
    ///         written back as a full timestamp.
    ///     </para>
    ///     <para>
    ///         It is also independent of the <c>If-Modified-Since</c> / 304 exchange, and search engines may
    ///         weigh the two sources differently.
    ///     </para>
    /// </remarks>
    public DateTime? LastModified { get; set; }

    /// <summary>
    /// Gets or sets how frequently the page is likely to change.
    /// </summary>
    /// <value>The default value is <see langword="null"/>, meaning the entry carried no <c>changefreq</c>.</value>
    /// <remarks>
    ///     A hint that crawlers are free to disregard in either direction. See
    ///     <see cref="SitemapChangeFrequency"/>.
    /// </remarks>
    public SitemapChangeFrequency? ChangeFrequency { get; set; }

    /// <summary>
    /// Gets or sets the priority of this URL relative to other URLs on the site.
    /// </summary>
    /// <value><c>0.0</c> to <c>1.0</c> inclusive, or <see langword="null"/> if none was specified. An absent value is treated by crawlers as <c>0.5</c>.</value>
    /// <remarks>
    ///     <para>
    ///         Priority is <i>relative to other URLs on your own site</i>. It "does not affect how your pages
    ///         are compared to pages on other sites", and is "not likely to influence the position of your
    ///         URLs in a search engine's result pages". It only helps a crawler choose among your pages.
    ///     </para>
    ///     <para>
    ///         Because it is relative, marking every page <c>1.0</c> conveys exactly as much as marking every
    ///         page <c>0.5</c>: nothing.
    ///     </para>
    ///     <para>
    ///         Saving formats the value to one decimal place, so a priority carrying more precision than
    ///         that — <c>0.12</c> is written as <c>0.1</c> — does not survive a round-trip.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The value specified for a set operation is less than <c>0.0</c> or greater than <c>1.0</c>.</exception>
    public decimal? Priority
    {
        get;
        set
        {
            if (value is < 0.0m or > 1.0m)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Priority must be between 0.0 and 1.0.");
            }

            field = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="SitemapUrl"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapUrl"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SitemapUrl"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source) => this.Load(source, null);

    /// <summary>
    /// Loads this <see cref="SitemapUrl"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapUrl"/> was initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SitemapUrl"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(source.NameTable);

        XPathNavigator? locNavigator = source.SelectChildElement("sm", "loc", manager);
        XPathNavigator? lastmodNavigator = source.SelectChildElement("sm", "lastmod", manager);
        XPathNavigator? changefreqNavigator = source.SelectChildElement("sm", "changefreq", manager);
        XPathNavigator? priorityNavigator = source.SelectChildElement("sm", "priority", manager);

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

        if (changefreqNavigator is not null)
        {
            if (SitemapUtility.TryParseChangeFrequency(changefreqNavigator.Value, out SitemapChangeFrequency changeFrequency))
            {
                this.ChangeFrequency = changeFrequency;
                wasLoaded = true;
            }
        }

        if (priorityNavigator is not null)
        {
            if (SitemapUtility.TryParsePriority(priorityNavigator.Value, out decimal priority))
            {
                this.Priority = priority;
                wasLoaded = true;
            }
        }

        if (settings is not null)
        {
            SyndicationExtensionAdapter adapter = new(source, settings);
            adapter.Fill(this);
        }

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="SitemapUrl"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("url", SitemapUtility.SitemapNamespace);

        if (this.Location is not null)
        {
            writer.WriteElementString("loc", SitemapUtility.SitemapNamespace, this.Location.ToString());
        }

        if (this.LastModified.HasValue)
        {
            writer.WriteElementString("lastmod", SitemapUtility.SitemapNamespace, SyndicationDateTimeUtility.ToRfc3339DateTime(this.LastModified.Value));
        }

        if (this.ChangeFrequency.HasValue)
        {
            writer.WriteElementString("changefreq", SitemapUtility.SitemapNamespace, SitemapUtility.ChangeFrequencyAsString(this.ChangeFrequency.Value));
        }

        if (this.Priority.HasValue)
        {
            writer.WriteElementString("priority", SitemapUtility.SitemapNamespace, this.Priority.Value.ToString("F1", CultureInfo.InvariantCulture));
        }

        SyndicationExtensionAdapter.WriteExtensionsTo(this.Extensions, writer);

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapUrl"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapUrl"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
    public override string ToString() => this.ToXmlString();

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapUrl? other)
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
        if (result != 0)
        {
            return result;
        }

        result = Nullable.Compare(this.ChangeFrequency, other.ChangeFrequency);
        if (result != 0)
        {
            return result;
        }

        result = Nullable.Compare(this.Priority, other.Priority);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapUrl"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapUrl"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapUrl"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapUrl? other)
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
    public override bool Equals(object? obj) => obj is SitemapUrl other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Location), HashCodeUtility.Component(this.LastModified), HashCodeUtility.Component(this.ChangeFrequency), HashCodeUtility.Component(this.Priority));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapUrl? first, SitemapUrl? second)
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
    public static bool operator !=(SitemapUrl? first, SitemapUrl? second) => !(first == second);
}