using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a single image attached to a URL in a Google image sitemap.
/// </summary>
/// <remarks>
///     <para>
///     One property, because one child element survives. An <c>image:image</c> once carried
///     <c>image:caption</c>, <c>image:geo_location</c>, <c>image:title</c> and <c>image:license</c>
///     alongside <c>image:loc</c>. Google stopped supporting all four on <b>6 August 2022</b> and struck
///     them from its documentation, which now lists <c>image:image</c> and <c>image:loc</c> as the whole
///     of the format. Leaving the retired elements in a sitemap costs nothing and buys nothing, so this
///     class does not model them.
///     </para>
///     <para>
///     A reader that encounters them will therefore drop them silently: <see cref="Load"/> looks for
///     <c>image:loc</c> and nothing else, and an <c>image:image</c> without one does not load at all.
///     </para>
/// </remarks>
/// <seealso cref="SitemapImageExtension.Images"/>
/// <seealso href="https://www.google.com/schemas/sitemap-image/1.1/sitemap-image.xsd">Image Sitemap 1.1 Schema</seealso>
public class SitemapImage : IComparable<SitemapImage>, IEquatable<SitemapImage>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the URL of the image.
    /// </summary>
    private Uri? imageLocation;

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
    /// <exception cref="ArgumentNullException">The <paramref name="location"/> is <see langword="null"/>.</exception>
    public SitemapImage(Uri location)
    {
        ArgumentNullException.ThrowIfNull(location);
        this.imageLocation = location;
    }

    /// <summary>
    /// Gets or sets the URL of the image.
    /// </summary>
    /// <value>
    ///     A <see cref="Uri"/> that represents the URL of the image, or <see langword="null"/> if none was
    ///     specified. Required by the specification; a <c>url</c> entry may carry up to 1,000 images.
    /// </value>
    /// <remarks>
    ///     The image need not be hosted on the site the sitemap describes. Google's requirement for a
    ///     cross-domain image — a CDN, typically — is that both domains be verified in Search Console and
    ///     that <c>robots.txt</c> on the hosting domain permit crawling it.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Location
    {
        get => imageLocation;

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
    /// <returns><see langword="true"/> if the <see cref="SitemapImage"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        // Try to find loc element - handle both cases:
        // 1. Navigator positioned at the <image> element (look for child)
        // 2. Navigator positioned at document root (look for descendant)
        XPathNavigator? locNavigator = source.SelectChildElement("image", "loc", manager);
        locNavigator ??= source.SelectSingleNode("descendant::image:loc", manager);

        if (locNavigator is not null && !string.IsNullOrEmpty(locNavigator.Value))
        {
            if (Uri.TryCreate(locNavigator.Value, UriKind.RelativeOrAbsolute, out Uri? location))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("image", xmlNamespace);

        if (this.Location is not null)
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
    /// <returns><see langword="true"/> if the specified <see cref="SitemapImage"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object? obj) => obj is SitemapImage other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     Routed through <see cref="HashCodeUtility.Component(Uri)"/> rather than calling
    ///     <see cref="Uri.GetHashCode"/> directly. <see cref="CompareTo(SitemapImage)"/> compares the
    ///     location with <see cref="StringComparison.OrdinalIgnoreCase"/>, but <see cref="Uri.GetHashCode"/>
    ///     is case-sensitive over the path, so two images differing only in path case compared equal
    ///     yet hashed differently.
    /// </remarks>
    public override int GetHashCode() => HashCodeUtility.Component(this.Location);

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapImage"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapImage"/>.</returns>
    public override string ToString() => this.Location?.ToString() ?? string.Empty;

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapImage? first, SitemapImage? second)
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
    public static bool operator !=(SitemapImage? first, SitemapImage? second) => !(first == second);

}