using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents an image in a sitemap image extension.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapImage"/> class represents image information that can be included in a sitemap
///         to help search engines discover images on your site. This conforms to the Google Image Sitemap extension.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-image/1.1/sitemap-image.xsd">Image Sitemap 1.1 Schema</seealso>
[Serializable]
public class SitemapImage : IComparable<SitemapImage>, IEquatable<SitemapImage>
{
    /// <summary>
    /// Private member to hold the URL of the image.
    /// </summary>
    private Uri imageLocation;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapImage"/> class.
    /// </summary>
    public SitemapImage()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapImage"/> class with the specified location.
    /// </summary>
    /// <param name="location">The URL of the image.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is a null reference.</exception>
    public SitemapImage(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.imageLocation = location;
    }

    /// <summary>
    /// Gets or sets the URL of the image.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the image. This is a required property.</value>
    /// <remarks>
    ///     The URL must be from the same domain as the page containing the sitemap, or from an allowed CDN domain.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Location
    {
        get
        {
            return imageLocation;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            imageLocation = value;
        }
    }

    /// <summary>
    /// Initializes the image using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapImage"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed XML namespaces.</param>
    /// <returns><b>true</b> if the <see cref="SitemapImage"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        // Try to find loc element - handle both cases:
        // 1. Navigator positioned at the <image> element (look for child)
        // 2. Navigator positioned at document root (look for descendant)
        XPathNavigator locNavigator = source.SelectSingleNode("image:loc", manager);
        locNavigator ??= source.SelectSingleNode("descendant::image:loc", manager);

        if (locNavigator != null && !string.IsNullOrEmpty(locNavigator.Value))
        {
            if (Uri.TryCreate(locNavigator.Value, UriKind.RelativeOrAbsolute, out Uri location))
            {
                this.imageLocation = location;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the image to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the image will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference or empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("image", xmlNamespace);

        if (this.Location != null)
        {
            writer.WriteElementString("loc", xmlNamespace, this.Location.ToString());
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapImage? other)
    {
        if (other is null)
        {
            return 1;
        }

        return Uri.Compare(this.Location, other.Location, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapImage"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapImage"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapImage"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SitemapImage? other)
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
        return obj is SitemapImage other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return this.Location?.GetHashCode() ?? 0;
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapImage"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapImage"/>.</returns>
    public override string ToString()
    {
        return this.Location?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapImage first, SitemapImage second)
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
    public static bool operator !=(SitemapImage first, SitemapImage second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapImage first, SitemapImage second)
    {
        if (first is null) return second is not null;
        return first.CompareTo(second) < 0;
    }

    /// <summary>
    /// Determines if first operand is greater than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than the second, otherwise; <b>false</b>.</returns>
    public static bool operator >(SitemapImage first, SitemapImage second)
    {
        if (first is null) return false;
        return first.CompareTo(second) > 0;
    }

    /// <summary>
    /// Determines if first operand is less than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator <=(SitemapImage first, SitemapImage second)
    {
        if (first is null) return true;
        return first.CompareTo(second) <= 0;
    }

    /// <summary>
    /// Determines if first operand is greater than or equal to the second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is greater than or equal to the second, otherwise; <b>false</b>.</returns>
    public static bool operator >=(SitemapImage first, SitemapImage second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}