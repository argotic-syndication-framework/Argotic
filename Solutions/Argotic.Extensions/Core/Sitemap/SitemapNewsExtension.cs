using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing news content in sitemaps.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapNewsExtension"/> extends sitemap content to include news article information
///         that helps search engines discover and understand news content on your site. This syndication extension
///         conforms to the Google News Sitemap extension specification, which can be found at
///         <a href="https://developers.google.com/search/docs/crawling-indexing/sitemaps/news-sitemap">https://developers.google.com/search/docs/crawling-indexing/sitemaps/news-sitemap</a>.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd">News Sitemap 0.9 Schema</seealso>
[Serializable]
public class SitemapNewsExtension : SyndicationExtension, IComparable<SitemapNewsExtension>, IEquatable<SitemapNewsExtension>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the publication information.
    /// </summary>
    private SitemapNewsPublication? extensionPublication;

    /// <summary>
    /// Private member to hold the publication date.
    /// </summary>
    private DateTime extensionPublicationDate = DateTime.MinValue;

    /// <summary>
    /// Private member to hold the title.
    /// </summary>
    private string extensionTitle = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapNewsExtension"/> class.
    /// </summary>
    public SitemapNewsExtension()
        : base("news", "http://www.google.com/schemas/sitemap-news/0.9", new Version("0.9"), new Uri("https://developers.google.com/search/docs/crawling-indexing/sitemaps/news-sitemap"), "Google Sitemap News Extension", "Extends sitemaps to include news article information for search engine discovery.")
    {
    }

    /// <summary>
    /// Gets or sets the publication information for this news article.
    /// </summary>
    /// <value>
    ///     A <see cref="SitemapNewsPublication"/> object that contains information about the publication
    ///     that originally published the news article. The default value is <b>null</b>.
    /// </value>
    public SitemapNewsPublication? Publication
    {
        get => extensionPublication;

        set => extensionPublication = value;
    }

    /// <summary>
    /// Gets or sets the date and time the article was published.
    /// </summary>
    /// <value>
    ///     A <see cref="DateTime"/> representing when the article was published.
    ///     The default value is <see cref="DateTime.MinValue"/>, which indicates no date was specified.
    /// </value>
    /// <remarks>
    ///     The publication date should be the date and time the article was originally published,
    ///     not the date it was added to the sitemap.
    /// </remarks>
    public DateTime PublicationDate
    {
        get => extensionPublicationDate;

        set => extensionPublicationDate = value;
    }

    /// <summary>
    /// Gets or sets the title of the news article.
    /// </summary>
    /// <value>The title of the news article.</value>
    /// <remarks>
    ///     The title should match the article's headline as it appears on your site.
    /// </remarks>
    public string Title
    {
        get => extensionTitle;

        set
        {
            if (string.IsNullOrEmpty(value))
            {
                extensionTitle = string.Empty;
            }
            else
            {
                extensionTitle = value.Trim();
            }
        }
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
        return extension is SitemapNewsExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="SitemapNewsExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapNewsExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        XPathNavigator? newsNavigator = navigator.SelectSingleNode("news:news", manager);

        if (newsNavigator is not null)
        {
            XPathNavigator? publicationNavigator = newsNavigator.SelectSingleNode("news:publication", manager);
            XPathNavigator? publicationDateNavigator = newsNavigator.SelectSingleNode("news:publication_date", manager);
            XPathNavigator? titleNavigator = newsNavigator.SelectSingleNode("news:title", manager);

            if (publicationNavigator is not null)
            {
                SitemapNewsPublication publication = new();
                if (publication.Load(publicationNavigator, manager))
                {
                    this.extensionPublication = publication;
                    wasLoaded = true;
                }
            }

            if (publicationDateNavigator is not null && !string.IsNullOrEmpty(publicationDateNavigator.Value))
            {
                if (DateTime.TryParse(publicationDateNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime publicationDate))
                {
                    this.extensionPublicationDate = publicationDate;
                    wasLoaded = true;
                }
            }

            if (titleNavigator is not null && !string.IsNullOrEmpty(titleNavigator.Value))
            {
                this.extensionTitle = titleNavigator.Value.Trim();
                wasLoaded = true;
            }
        }

        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="SitemapNewsExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapNewsExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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

        writer.WriteStartElement("news", this.XmlNamespace);

        this.Publication?.WriteTo(writer, this.XmlNamespace);

        if (this.PublicationDate != DateTime.MinValue)
        {
            writer.WriteElementString("publication_date", this.XmlNamespace, this.PublicationDate.ToString("yyyy-MM-ddTHH:mm:sszzz", CultureInfo.InvariantCulture));
        }

        if (!string.IsNullOrEmpty(this.Title))
        {
            writer.WriteElementString("title", this.XmlNamespace, this.Title);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapNewsExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapNewsExtension"/>.</returns>
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
    public int CompareTo(SitemapNewsExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = (this.Publication, other.Publication) switch
        {
            (SitemapNewsPublication publication, SitemapNewsPublication otherPublication) => publication.CompareTo(otherPublication),
            (not null, null) => 1,
            (null, not null) => -1,
            _ => 0,
        };

        if (result == 0) result = this.PublicationDate.CompareTo(other.PublicationDate);
        if (result == 0) result = string.Compare(this.Title, other.Title, StringComparison.OrdinalIgnoreCase);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapNewsExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapNewsExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapNewsExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SitemapNewsExtension? other)
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
    public override bool Equals(object? obj) => obj is SitemapNewsExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Publication), HashCodeUtility.Component(this.PublicationDate), HashCodeUtility.Component(this.Title));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapNewsExtension? first, SitemapNewsExtension? second)
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
    public static bool operator !=(SitemapNewsExtension? first, SitemapNewsExtension? second) => !(first == second);

}