using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing videos in sitemaps.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapVideoExtension"/> extends sitemap content to include video information
///         that helps search engines discover and understand video content on your site. This syndication extension
///         conforms to the Google Video Sitemap extension specification, which can be found at
///         <a href="https://developers.google.com/search/docs/crawling-indexing/sitemaps/video-sitemaps">https://developers.google.com/search/docs/crawling-indexing/sitemaps/video-sitemaps</a>.
///     </para>
///     <para>
///         <b>Deprecated Elements (May 2022):</b><br/>
///         The following elements were deprecated by Google and are intentionally not implemented:
///         <list type="bullet">
///             <item><c>video:price</c> - Video purchase/rental pricing</item>
///             <item><c>video:category</c> - Video category (max 256 chars)</item>
///             <item><c>video:gallery_loc</c> - Gallery URL with title attribute</item>
///             <item><c>video:tvshow</c> - TV show metadata</item>
///             <item><c>player_loc/@allow_embed</c> - Embed permission attribute</item>
///             <item><c>player_loc/@autoplay</c> - Autoplay parameter attribute</item>
///         </list>
///         See <see href="https://developers.google.com/search/blog/2022/05/spring-cleaning-sitemap-extensions">Google's announcement</see>.
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
    ///     An <see cref="IList{T}"/> collection of <see cref="SitemapVideo"/> objects that represent videos
    ///     associated with the sitemap URL. The default value is an <i>empty</i> collection.
    /// </value>
    public IList<SitemapVideo> Videos => extensionVideos;

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
        return extension is SitemapVideoExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="SitemapVideoExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapVideoExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="SitemapVideoExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapVideoExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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

        foreach (SitemapVideo video in this.Videos)
        {
            video.WriteTo(writer, this.XmlNamespace);
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapVideoExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapVideoExtension"/>.</returns>
    /// <remarks>
    ///     This method returns the XML representation for the current instance.
    /// </remarks>
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
    /// <returns><b>true</b> if the specified <see cref="SitemapVideoExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
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
    /// <returns><b>false</b> if its operands are equal, otherwise; <b>true</b>.</returns>
    public static bool operator !=(SitemapVideoExtension? first, SitemapVideoExtension? second) => !(first == second);

}