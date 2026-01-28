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
///     <para>
///         This class represents a single URL entry in a Sitemap as defined in the
///         <a href="https://www.sitemaps.org/protocol.html">Sitemaps Protocol</a>.
///     </para>
///     <para>
///         A URL entry encapsulates all information about a specific URL, including its location,
///         the date it was last modified, how frequently it changes, and its relative priority within the site.
///     </para>
/// </remarks>
[Serializable]
public class SitemapUrl : IComparable<SitemapUrl>, IEquatable<SitemapUrl>, IExtensibleSyndicationObject
{
    /// <summary>
    /// Private member to hold the URL of the page.
    /// </summary>
    private Uri urlLocation;

    /// <summary>
    /// Private member to hold the date of last modification of the page.
    /// </summary>
    private DateTime? urlLastModified;

    /// <summary>
    /// Private member to hold how frequently the page is likely to change.
    /// </summary>
    private SitemapChangeFrequency? urlChangeFrequency;

    /// <summary>
    /// Private member to hold the priority of this URL relative to other URLs on the site.
    /// </summary>
    private decimal? urlPriority;

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
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapUrl(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.urlLocation = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapUrl"/> class using the specified URL location and last modified date.
    /// </summary>
    /// <param name="location">A <see cref="Uri"/> that represents the URL of the page.</param>
    /// <param name="lastModified">A <see cref="DateTime"/> that indicates when the page was last modified.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapUrl(Uri location, DateTime lastModified)
        : this(location)
    {
        this.urlLastModified = lastModified;
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
    /// Gets or sets the URL of the page.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the page. This URL must begin with the protocol (such as http) and end with a trailing slash, if your web server requires it.</value>
    /// <remarks>
    ///     <para>This value must be less than 2,048 characters.</para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Location
    {
        get
        {
            return this.urlLocation;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this.urlLocation = value;
        }
    }

    /// <summary>
    /// Gets or sets the date of last modification of the page.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates when the page was last modified.
    ///     The default value is <b>null</b>, which indicates that no last modified date was specified.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This date should be in W3C Datetime format. This format allows you to omit the time portion, if desired, and use YYYY-MM-DD.
    ///     </para>
    ///     <para>
    ///         Note that the date must be set to the date the linked page was last modified, not when the sitemap is generated.
    ///     </para>
    ///     <para>
    ///         Note also that this tag is separate from the If-Modified-Since (304) header the server can return,
    ///         and search engines may use the information from both sources differently.
    ///     </para>
    /// </remarks>
    public DateTime? LastModified
    {
        get
        {
            return this.urlLastModified;
        }

        set
        {
            this.urlLastModified = value;
        }
    }

    /// <summary>
    /// Gets or sets how frequently the page is likely to change.
    /// </summary>
    /// <value>
    ///     A <see cref="SitemapChangeFrequency"/> that indicates how frequently the page is likely to change.
    ///     The default value is <b>null</b>, which indicates that no change frequency was specified.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This value provides general information to search engines and may not correlate exactly to how often they crawl the page.
    ///     </para>
    ///     <para>
    ///         The value "always" should be used to describe documents that change each time they are accessed.
    ///         The value "never" should be used to describe archived URLs.
    ///     </para>
    /// </remarks>
    public SitemapChangeFrequency? ChangeFrequency
    {
        get
        {
            return this.urlChangeFrequency;
        }

        set
        {
            this.urlChangeFrequency = value;
        }
    }

    /// <summary>
    /// Gets or sets the priority of this URL relative to other URLs on the site.
    /// </summary>
    /// <value>
    ///     A <see cref="decimal"/> that indicates the priority of this URL relative to other URLs on the site.
    ///     Valid values range from 0.0 to 1.0. The default value is <b>null</b>, which is treated as 0.5.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This priority lets search engines know which pages you deem most important for the crawlers.
    ///     </para>
    ///     <para>
    ///         Please note that the priority you assign to a page is not likely to influence the position of your URLs in a search engine's result pages.
    ///         Search engines may use this information when selecting between URLs on the same site,
    ///         so you can use this tag to increase the likelihood that your most important pages are present in a search index.
    ///     </para>
    ///     <para>
    ///         Also, please note that assigning a high priority to all of the URLs on your site is not likely to help you.
    ///         Since the priority is relative, it is only used to select between URLs on your site.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The <paramref name="value"/> is less than 0.0 or greater than 1.0.</exception>
    public decimal? Priority
    {
        get
        {
            return this.urlPriority;
        }

        set
        {
            if (value.HasValue && (value.Value < 0.0m || value.Value > 1.0m))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Priority must be between 0.0 and 1.0.");
            }

            this.urlPriority = value;
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
    public ISyndicationExtension? FindExtension(Predicate<ISyndicationExtension> match)
    {
        ArgumentNullException.ThrowIfNull(match);
        List<ISyndicationExtension> list = [.. this.Extensions];
        return list.Find(match);
    }

    /// <summary>
    /// Loads this <see cref="SitemapUrl"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="SitemapUrl"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SitemapUrl"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        return this.Load(source, null);
    }

    /// <summary>
    /// Loads this <see cref="SitemapUrl"/> using the supplied <see cref="XPathNavigator"/> and <see cref="SyndicationResourceLoadSettings"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <param name="settings">The <see cref="SyndicationResourceLoadSettings"/> used to configure the load operation.</param>
    /// <returns><b>true</b> if the <see cref="SitemapUrl"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SitemapUrl"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, SyndicationResourceLoadSettings? settings)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(source.NameTable);

        XPathNavigator locNavigator = source.SelectSingleNode("sm:loc", manager);
        XPathNavigator lastmodNavigator = source.SelectSingleNode("sm:lastmod", manager);
        XPathNavigator changefreqNavigator = source.SelectSingleNode("sm:changefreq", manager);
        XPathNavigator priorityNavigator = source.SelectSingleNode("sm:priority", manager);

        if (locNavigator != null)
        {
            if (Uri.TryCreate(locNavigator.Value, UriKind.Absolute, out Uri location))
            {
                this.Location = location;
                wasLoaded = true;
            }
        }

        if (lastmodNavigator != null)
        {
            if (SyndicationDateTimeUtility.TryParseRfc3339DateTime(lastmodNavigator.Value, out DateTime lastModified))
            {
                this.LastModified = lastModified;
                wasLoaded = true;
            }
            else if (DateTime.TryParse(lastmodNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out lastModified))
            {
                this.LastModified = lastModified;
                wasLoaded = true;
            }
        }

        if (changefreqNavigator != null)
        {
            if (SitemapUtility.TryParseChangeFrequency(changefreqNavigator.Value, out SitemapChangeFrequency changeFrequency))
            {
                this.ChangeFrequency = changeFrequency;
                wasLoaded = true;
            }
        }

        if (priorityNavigator != null)
        {
            if (SitemapUtility.TryParsePriority(priorityNavigator.Value, out decimal priority))
            {
                this.Priority = priority;
                wasLoaded = true;
            }
        }

        if (settings != null)
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("url", SitemapUtility.SitemapNamespace);

        if (this.Location != null)
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
    /// Returns a <see cref="String"/> that represents the current <see cref="SitemapUrl"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="SitemapUrl"/>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="SitemapUrl"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SitemapUrl other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Location, this.LastModified, this.ChangeFrequency, this.Priority);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(SitemapUrl? first, SitemapUrl? second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapUrl? first, SitemapUrl? second)
    {
        if (first is null)
        {
            return second is not null;
        }

        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(SitemapUrl? first, SitemapUrl? second)
    {
        if (first is null)
        {
            return false;
        }

        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(SitemapUrl? first, SitemapUrl? second)
    {
        if (first is null)
        {
            return true;
        }

        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(SitemapUrl? first, SitemapUrl? second)
    {
        if (first is null)
        {
            return second is null;
        }

        return first.CompareTo(second) >= 0;
    }
}