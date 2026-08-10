using System.Globalization;
using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends a sitemap entry to describe a news article.
/// </summary>
/// <remarks>
///     <para>
///     Google's news sitemap extension, specified at
///     <a href="https://developers.google.com/search/docs/crawling-indexing/sitemaps/news-sitemap">https://developers.google.com/search/docs/crawling-indexing/sitemaps/news-sitemap</a>.
///     All three members are required: a <see cref="Publication"/>, a <see cref="PublicationDate"/> and a
///     <see cref="Title"/>.
///     </para>
///     <para>
///     <b>A news sitemap is a rolling window, not an archive.</b> Google asks for URLs of articles
///     "created in the last two days" and for a file of no more than 1,000 <c>news:news</c> entries.
///     Both are the publisher's job: this class writes whatever it is given, so a sitemap that
///     accumulates months of articles will be generated happily and read as stale.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd">News Sitemap 0.9 Schema</seealso>
public class SitemapNewsExtension : SyndicationExtension, IComparable<SitemapNewsExtension>, IEquatable<SitemapNewsExtension>, IComparisonOperators
{

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
    ///     The publication that originally carried the article, or <see langword="null"/> if none was
    ///     specified. The default value is <see langword="null"/>. Required by the specification.
    /// </value>
    public SitemapNewsPublication? Publication { get; set; }

    /// <summary>
    /// Gets or sets the date and time the article was published.
    /// </summary>
    /// <value>
    ///     When the article first appeared — not when it entered the sitemap. The default value is
    ///     <see cref="DateTime.MinValue"/>, which stands in for "unset" and is the one value
    ///     <see cref="WriteTo(XmlWriter)"/> will not write. Required by the specification.
    /// </value>
    /// <remarks>
    ///     Loading normalises to UTC, so the offset the publisher wrote is lost on a round-trip:
    ///     <c>2024-01-15T10:00:00-05:00</c> is written back as <c>2024-01-15T15:00:00Z</c>. Same
    ///     instant, different text, and Google accepts either.
    /// </remarks>
    public DateTime PublicationDate { get; set; } = DateTime.MinValue;

    /// <summary>
    /// Gets or sets the headline of the news article.
    /// </summary>
    /// <value>
    ///     The headline as it appears on the page. The default value is an <i>empty</i> string. Required by
    ///     the specification.
    /// </value>
    /// <remarks>
    ///     Setting <see langword="null"/> or an empty string clears the value rather than throwing;
    ///     anything else is trimmed.
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
    /// <returns><see langword="true"/> if the <paramref name="extension"/> is the same <see cref="Type"/> as this <see cref="SyndicationExtension"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="extension"/> is <see langword="null"/>.</exception>
    public static bool MatchByType(ISyndicationExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return extension is SitemapNewsExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="SitemapNewsExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapNewsExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        XPathNavigator? newsNavigator = navigator.SelectChildElement("news", "news", manager);

        if (newsNavigator is not null)
        {
            XPathNavigator? publicationNavigator = newsNavigator.SelectChildElement("news", "publication", manager);
            XPathNavigator? publicationDateNavigator = newsNavigator.SelectChildElement("news", "publication_date", manager);
            XPathNavigator? titleNavigator = newsNavigator.SelectChildElement("news", "title", manager);

            if (publicationNavigator is not null)
            {
                SitemapNewsPublication publication = new();
                if (publication.Load(publicationNavigator, manager))
                {
                    this.Publication = publication;
                    wasLoaded = true;
                }
            }

            if (publicationDateNavigator is not null && !string.IsNullOrEmpty(publicationDateNavigator.Value))
            {
                if (DateTime.TryParse(publicationDateNavigator.Value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime publicationDate))
                {
                    this.PublicationDate = publicationDate;
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="SitemapNewsExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapNewsExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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

        writer.WriteStartElement("news", this.XmlNamespace);

        this.Publication?.WriteTo(writer, this.XmlNamespace);

        if (this.PublicationDate != DateTime.MinValue)
        {
            writer.WriteElementString("publication_date", this.XmlNamespace, SitemapExtensionUtility.ToSitemapDateTime(this.PublicationDate));
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
    /// <returns><see langword="true"/> if the specified <see cref="SitemapNewsExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the specified <see cref="object"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
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
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
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
    /// <returns><see langword="false"/> if its operands are equal, otherwise; <see langword="true"/>.</returns>
    public static bool operator !=(SitemapNewsExtension? first, SitemapNewsExtension? second) => !(first == second);

}