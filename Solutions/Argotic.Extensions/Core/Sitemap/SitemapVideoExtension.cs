using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends a sitemap entry to describe the videos hosted on the page.
/// </summary>
/// <remarks>
///     <para>
///     Google's video sitemap extension, version 1.1, specified at
///     <a href="https://developers.google.com/search/docs/crawling-indexing/sitemaps/video-sitemaps">https://developers.google.com/search/docs/crawling-indexing/sitemaps/video-sitemaps</a>.
///     A page may hold several videos, and each <c>video:video</c> becomes one <see cref="SitemapVideo"/>.
///     </para>
///     <para>
///     Google withdrew a handful of the 1.1 elements on 6 August 2022; <see cref="SitemapVideo"/> names
///     them and explains why none of them appear on it. The rest of the format is intact and current.
///     </para>
///     <para>
///     The prefix is bound to <c>http://www.google.com/schemas/sitemap-video/1.1</c>, an identifier
///     rather than an address. It stays <c>http</c>, and a sitemap declaring the <c>https</c> spelling
///     names a different namespace that will not match.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-video/1.1/sitemap-video.xsd">Video Sitemap 1.1 Schema</seealso>
public class SitemapVideoExtension : SyndicationExtension, IComparable<SitemapVideoExtension>, IEquatable<SitemapVideoExtension>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the collection of videos.
    /// </summary>
    private readonly List<SitemapVideo> extensionVideos = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapVideoExtension"/> class.
    /// </summary>
    public SitemapVideoExtension()
        : base("video", "http://www.google.com/schemas/sitemap-video/1.1", new Version("1.1"), new Uri("https://developers.google.com/search/docs/crawling-indexing/sitemaps/video-sitemaps"), "Google Sitemap Video Extension", "Extends sitemaps to include video information for search engine discovery.")
    {
    }

    /// <summary>
    /// Gets the collection of videos associated with this extension.
    /// </summary>
    /// <value>
    ///     A collection of <see cref="SitemapVideo"/> objects, one per video on the page. The default value
    ///     is an <i>empty</i> collection.
    /// </value>
    public IList<SitemapVideo> Videos => extensionVideos;

    /// <summary>
    /// Predicate delegate that returns a value indicating if the supplied <see cref="ISyndicationExtension"/>
    /// represents the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>.
    /// </summary>
    /// <param name="extension">The <see cref="ISyndicationExtension"/> to be compared.</param>
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is SitemapVideoExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="SitemapVideoExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapVideoExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        XPathNodeIterator videoIterator = navigator.SelectChildElements("video", "video", manager);

        if (videoIterator is { Count: > 0 })
        {
            while (videoIterator.MoveNext())
            {
                XPathNavigator? videoNode = videoIterator.Current;
                if (videoNode is null)
                {
                    continue;
                }

                SitemapVideo video = new();
                if (video.Load(videoNode, manager))
                {
                    this.extensionVideos.Add(video);
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="SitemapVideoExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapVideoExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="reader"/> is <see langword="null"/>.</exception>
    public override bool Load(XmlReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        XPathDocument document = new(reader);

        return this.Load(document.CreateNavigator());
    }

    /// <summary>
    /// Writes the syndication extension to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which you want to write the syndication extension.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    public override void WriteTo(XmlWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);

        foreach (SitemapVideo video in this.Videos)
        {
            video.WriteTo(writer, this.XmlNamespace);
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideoExtension"/>.
    /// </summary>
    /// <returns>The XML representation for the current instance.</returns>
    public override string ToString()
    {
        using MemoryStream stream = new();
        XmlWriterSettings settings = SyndicationEncodingUtility.CreateFragmentXmlWriterSettings();

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
    public int CompareTo(SitemapVideoExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Videos, other.Videos);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapVideoExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapVideoExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapVideoExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapVideoExtension? other)
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
    public override bool Equals(object? obj) => obj is SitemapVideoExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (SitemapVideo video in this.Videos)
        {
            hash.Add(HashCodeUtility.Component(video));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapVideoExtension? first, SitemapVideoExtension? second)
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
    public static bool operator !=(SitemapVideoExtension? first, SitemapVideoExtension? second) => !(first == second);

}