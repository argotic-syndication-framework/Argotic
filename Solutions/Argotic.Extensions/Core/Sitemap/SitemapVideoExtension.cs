using System.Xml;
using System.Xml.XPath;

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
[Serializable]
public class SitemapVideoExtension : SyndicationExtension, IComparable
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
    /// Compares two specified <see cref="IList{SitemapVideo}"/> collections.
    /// </summary>
    /// <param name="source">The first collection.</param>
    /// <param name="target">The second collection.</param>
    /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
    /// <remarks>
    ///     <para>
    ///         If the collections contain the same number of elements, determines the lexical relationship between the two sequences of comparands.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>greater than</i> the <paramref name="target"/> element count, returns <b>1</b>.
    ///     </para>
    ///     <para>
    ///         If the <paramref name="source"/> has an element count that is <i>less than</i> the <paramref name="target"/> element count, returns <b>-1</b>.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="target"/> is a null reference.</exception>
    public static int CompareSequence(IList<SitemapVideo> source, IList<SitemapVideo> target)
    {
        int result = 0;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);

        if (source.Count == target.Count)
        {
            for (int i = 0; i < source.Count; i++)
            {
                result |= source[i].CompareTo(target[i]);
            }
        }
        else if (source.Count > target.Count)
        {
            return 1;
        }
        else if (source.Count < target.Count)
        {
            return -1;
        }

        return result;
    }

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
        return extension.GetType() == typeof(SitemapVideoExtension);
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

        XPathNavigator navigator = source.CreateNavigator();
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        XPathNodeIterator videoIterator = navigator.Select("//video:video", manager);

        if (videoIterator is { Count: > 0 })
        {
            while (videoIterator.MoveNext())
            {
                SitemapVideo video = new();
                if (video.Load(videoIterator.Current, manager))
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
    /// Returns a <see cref="String"/> that represents the current <see cref="SitemapVideoExtension"/>.
    /// </summary>
    /// <returns>A <see cref="String"/> that represents the current <see cref="SitemapVideoExtension"/>.</returns>
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

        SitemapVideoExtension other = obj as SitemapVideoExtension;

        if (other != null)
        {
            int result = SitemapVideoExtension.CompareSequence(this.Videos, other.Videos);
            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="object"/> is equal to the current instance.
    /// </summary>
    /// <param name="obj">The <see cref="object"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not SitemapVideoExtension)
        {
            return false;
        }

        return this.CompareTo(obj) == 0;
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(this.ToString());
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapVideoExtension first, SitemapVideoExtension second)
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
    public static bool operator !=(SitemapVideoExtension first, SitemapVideoExtension second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapVideoExtension first, SitemapVideoExtension second)
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
    public static bool operator >(SitemapVideoExtension first, SitemapVideoExtension second)
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
    public static bool operator <=(SitemapVideoExtension first, SitemapVideoExtension second)
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
    public static bool operator >=(SitemapVideoExtension first, SitemapVideoExtension second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}