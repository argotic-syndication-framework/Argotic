using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents the publication that originally carried a news article.
/// </summary>
/// <remarks>
///     Both members are required by the specification, and both are load-bearing for matching: the
///     <see cref="Name"/> identifies the publication to Google and the <see cref="Language"/> tells it
///     which edition it is reading.
/// </remarks>
/// <seealso cref="SitemapNewsExtension.Publication"/>
/// <seealso href="https://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd">News Sitemap 0.9 Schema</seealso>
public class SitemapNewsPublication : IComparable<SitemapNewsPublication>, IEquatable<SitemapNewsPublication>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the name of the publication.
    /// </summary>
    private string publicationName = string.Empty;

    /// <summary>
    /// Private member to hold the language of the publication.
    /// </summary>
    private string publicationLanguage = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapNewsPublication"/> class.
    /// </summary>
    public SitemapNewsPublication()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapNewsPublication"/> class with the specified name and language.
    /// </summary>
    /// <param name="name">The name of the news publication, as Google already shows it.</param>
    /// <param name="language">An ISO 639 language code, optionally with an ISO 3166-1 alpha-2 region.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="language"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="language"/> is an empty string.</exception>
    public SitemapNewsPublication(string name, string language)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentException.ThrowIfNullOrEmpty(language);

        this.publicationName = name.Trim();
        this.publicationLanguage = language.Trim();
    }

    /// <summary>
    /// Gets or sets the name of the news publication.
    /// </summary>
    /// <value>
    ///     The publication name, trimmed. The default value is an <i>empty</i> string. Required by the
    ///     specification.
    /// </value>
    /// <remarks>
    ///     It must match the name Google already shows against the publication's articles, exactly. A near
    ///     miss is not corrected or fuzzily matched; it simply fails to associate.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Name
    {
        get => publicationName;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            publicationName = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the language of the publication.
    /// </summary>
    /// <value>
    ///     An ISO 639 language code, optionally followed by an ISO 3166-1 alpha-2 region — <c>en</c>,
    ///     <c>en-US</c>, <c>zh-cn</c>. The default value is an <i>empty</i> string. Required by the
    ///     specification.
    /// </value>
    /// <remarks>The value is trimmed and stored as written; it is not validated or case-normalised.</remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Language
    {
        get => publicationLanguage;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            publicationLanguage = value.Trim();
        }
    }

    /// <summary>
    /// Initializes the publication using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapNewsPublication"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed XML namespaces.</param>
    /// <returns><see langword="true"/> if the <see cref="SitemapNewsPublication"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        XPathNavigator? nameNavigator = source.SelectChildElement("news", "name", manager);
        XPathNavigator? languageNavigator = source.SelectChildElement("news", "language", manager);

        if (nameNavigator is not null && !string.IsNullOrEmpty(nameNavigator.Value))
        {
            this.publicationName = nameNavigator.Value.Trim();
            wasLoaded = true;
        }

        if (languageNavigator is not null && !string.IsNullOrEmpty(languageNavigator.Value))
        {
            this.publicationLanguage = languageNavigator.Value.Trim();
            wasLoaded = true;
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the publication to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the publication will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("publication", xmlNamespace);

        if (!string.IsNullOrEmpty(this.Name))
        {
            writer.WriteElementString("name", xmlNamespace, this.Name);
        }

        if (!string.IsNullOrEmpty(this.Language))
        {
            writer.WriteElementString("language", xmlNamespace, this.Language);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapNewsPublication? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Language, other.Language, StringComparison.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapNewsPublication"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapNewsPublication"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapNewsPublication"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapNewsPublication? other)
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
    public override bool Equals(object? obj) => obj is SitemapNewsPublication other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Language));

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapNewsPublication"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapNewsPublication"/>.</returns>
    public override string ToString() => $"{this.Name} ({this.Language})";

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapNewsPublication? first, SitemapNewsPublication? second)
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
    public static bool operator !=(SitemapNewsPublication? first, SitemapNewsPublication? second) => !(first == second);

}