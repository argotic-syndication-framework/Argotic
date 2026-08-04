using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing images in sitemaps.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapImageExtension"/> extends sitemap content to include image information
///         that helps search engines discover images on your site. This syndication extension conforms to the
///         Google Image Sitemap extension specification, which can be found at
///         <a href="https://developers.google.com/search/docs/crawling-indexing/sitemaps/image-sitemaps">https://developers.google.com/search/docs/crawling-indexing/sitemaps/image-sitemaps</a>.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-image/1.1/sitemap-image.xsd">Image Sitemap 1.1 Schema</seealso>
[Serializable]
public class SitemapImageExtension : SyndicationExtension, IComparable<SitemapImageExtension>, IEquatable<SitemapImageExtension>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the collection of images.
    /// </summary>
    private readonly List<SitemapImage> extensionImages = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapImageExtension"/> class.
    /// </summary>
    public SitemapImageExtension()
        : base("image", "http://www.google.com/schemas/sitemap-image/1.1", new Version("1.1"), new Uri("https://developers.google.com/search/docs/crawling-indexing/sitemaps/image-sitemaps"), "Google Sitemap Image Extension", "Extends sitemaps to include image information for search engine discovery.")
    {
    }

    /// <summary>
    /// Gets the collection of images associated with this extension.
    /// </summary>
    /// <value>
    ///     An <see cref="IList{T}"/> collection of <see cref="SitemapImage"/> objects that represent images
    ///     associated with the sitemap URL. The default value is an <i>empty</i> collection.
    /// </value>
    public IList<SitemapImage> Images => extensionImages;

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><b>true</b> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is a null reference.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is SitemapImageExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="SitemapImageExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapImageExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator();
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        XPathNodeIterator imageIterator = navigator.Select("image:image", manager);

        if (imageIterator is { Count: > 0 })
        {
            while (imageIterator.MoveNext())
            {
                XPathNavigator? imageNode = imageIterator.Current;
                if (imageNode == null)
                {
                    continue;
                }

                SitemapImage image = new();
                if (image.Load(imageNode, manager))
                {
                    this.extensionImages.Add(image);
                    wasLoaded = true;
                }
            }
        }

        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="SitemapImageExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapImageExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is a null reference.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <b>XmlWriter</b> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        foreach (SitemapImage image in this.Images)
        {
            image.WriteTo(writer, this.XmlNamespace);
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapImageExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapImageExtension"/>.</returns>
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
    public int CompareTo(SitemapImageExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Images, other.Images);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapImageExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapImageExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapImageExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SitemapImageExtension? other)
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
        return obj is SitemapImageExtension other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (SitemapImage image in this.Images)
        {
            hash.Add(HashCodeUtility.Component(image));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapImageExtension? first, SitemapImageExtension? second)
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
    public static bool operator !=(SitemapImageExtension? first, SitemapImageExtension? second)
    {
        return !(first == second);
    }

}