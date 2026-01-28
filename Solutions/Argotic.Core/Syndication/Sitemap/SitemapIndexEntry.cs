using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Syndication;

/// <summary>
/// Represents an entry in a Sitemap index file.
/// </summary>
/// <remarks>
///     <para>
///         This class represents a single sitemap entry in a Sitemap index file as defined in the
///         <a href="https://www.sitemaps.org/protocol.html">Sitemaps Protocol</a>.
///     </para>
///     <para>
///         A Sitemap index entry encapsulates the location of a sitemap and optionally when it was last modified.
///     </para>
/// </remarks>
[Serializable]
public class SitemapIndexEntry : IComparable, IEquatable<SitemapIndexEntry>
{
    /// <summary>
    /// Private member to hold the location of the sitemap.
    /// </summary>
    private Uri entryLocation;

    /// <summary>
    /// Private member to hold the date of last modification of the sitemap.
    /// </summary>
    private DateTime? entryLastModified;

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
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapIndexEntry(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.entryLocation = location;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapIndexEntry"/> class using the specified location and last modified date.
    /// </summary>
    /// <param name="location">A <see cref="Uri"/> that represents the location of the sitemap.</param>
    /// <param name="lastModified">A <see cref="DateTime"/> that indicates when the sitemap was last modified.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapIndexEntry(Uri location, DateTime lastModified)
        : this(location)
    {
        this.entryLastModified = lastModified;
    }

    /// <summary>
    /// Gets or sets the location of the sitemap.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the location of the sitemap.</value>
    /// <remarks>
    ///     <para>
    ///         This URL is where the sitemap can be found. It must be a valid URL that conforms to RFC 2396.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Location
    {
        get
        {
            return this.entryLocation;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            this.entryLocation = value;
        }
    }

    /// <summary>
    /// Gets or sets the date of last modification of the sitemap.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> that indicates when the sitemap was last modified.
    ///     The default value is <b>null</b>, which indicates that no last modified date was specified.
    /// </value>
    /// <remarks>
    ///     <para>
    ///         This date should be in W3C Datetime format. This format allows you to omit the time portion, if desired, and use YYYY-MM-DD.
    ///     </para>
    /// </remarks>
    public DateTime? LastModified
    {
        get
        {
            return this.entryLastModified;
        }

        set
        {
            this.entryLastModified = value;
        }
    }

    /// <summary>
    /// Loads this <see cref="SitemapIndexEntry"/> using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> to extract information from.</param>
    /// <returns><b>true</b> if the <see cref="SitemapIndexEntry"/> was initialized using the supplied <paramref name="source"/>, Otherwise, <b>false</b>.</returns>
    /// <remarks>
    ///     This method expects the supplied <paramref name="source"/> to be positioned on the XML element that represents a <see cref="SitemapIndexEntry"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public bool Load(XPathNavigator source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XmlNamespaceManager manager = SitemapUtility.CreateNamespaceManager(source.NameTable);

        XPathNavigator locNavigator = source.SelectSingleNode("sm:loc", manager);
        XPathNavigator lastmodNavigator = source.SelectSingleNode("sm:lastmod", manager);

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

        return wasLoaded;
    }

    /// <summary>
    /// Saves the current <see cref="SitemapIndexEntry"/> to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to save.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteStartElement("sitemap", SitemapUtility.SitemapNamespace);

        if (this.Location != null)
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
    /// Returns a <see cref="String"/> that represents the current <see cref="SitemapIndexEntry"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="SitemapIndexEntry"/>.</returns>
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
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }

        if (obj is SitemapIndexEntry other)
        {
            int result = Uri.Compare(this.Location, other.Location, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);

            if (result != 0)
            {
                return result;
            }

            result = Nullable.Compare(this.LastModified, other.LastModified);
            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapIndexEntry"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapIndexEntry"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapIndexEntry"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SitemapIndexEntry other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Location, this.LastModified);
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(SitemapIndexEntry? first, SitemapIndexEntry? second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapIndexEntry? first, SitemapIndexEntry? second)
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
    public static bool operator >(SitemapIndexEntry? first, SitemapIndexEntry? second)
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
    public static bool operator <=(SitemapIndexEntry? first, SitemapIndexEntry? second)
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
    public static bool operator >=(SitemapIndexEntry? first, SitemapIndexEntry? second)
    {
        if (first is null)
        {
            return second is null;
        }

        return first.CompareTo(second) >= 0;
    }
}