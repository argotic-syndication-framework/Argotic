using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents a publication source in a sitemap news extension.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapNewsPublication"/> class contains information about the publication
///         that originally published the news article, including the publication name and language.
///     </para>
/// </remarks>
/// <seealso href="https://www.google.com/schemas/sitemap-news/0.9/sitemap-news.xsd">News Sitemap 0.9 Schema</seealso>
[Serializable]
public class SitemapNewsPublication : IComparable, IEquatable<SitemapNewsPublication>
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
    /// <param name="name">The name of the news publication.</param>
    /// <param name="language">The language of the publication in ISO 639 format.</param>
    /// <exception cref="ArgumentException">The <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">The <paramref name="language"/> is null or empty.</exception>
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
    /// <value>The name of the publication. This is a required property.</value>
    /// <remarks>
    ///     The name must exactly match the name as it appears on your articles on news.google.com.
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
    public string Name
    {
        get
        {
            return publicationName;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            publicationName = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the language of the publication.
    /// </summary>
    /// <value>The language of the publication in ISO 639 format. This is a required property.</value>
    /// <remarks>
    ///     This should be an ISO 639 language code, optionally followed by a region suffix using ISO 3166-1 alpha-2.
    ///     Examples: "en", "en-US", "zh-cn".
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
    public string Language
    {
        get
        {
            return publicationLanguage;
        }

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
    /// <returns><b>true</b> if the <see cref="SitemapNewsPublication"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        XPathNavigator nameNavigator = source.SelectSingleNode("news:name", manager);
        XPathNavigator languageNavigator = source.SelectSingleNode("news:language", manager);

        if (nameNavigator != null && !string.IsNullOrEmpty(nameNavigator.Value))
        {
            this.publicationName = nameNavigator.Value.Trim();
            wasLoaded = true;
        }

        if (languageNavigator != null && !string.IsNullOrEmpty(languageNavigator.Value))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference or empty string.</exception>
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
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">The <paramref name="obj"/> is not the expected <see cref="Type"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return 1;
        }

        SitemapNewsPublication other = obj as SitemapNewsPublication;

        if (other != null)
        {
            int result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
            result |= string.Compare(this.Language, other.Language, StringComparison.OrdinalIgnoreCase);
            return result;
        }
        else
        {
            throw new ArgumentException(string.Format(null, "obj is not of type {0}, type was found to be '{1}'.", this.GetType().FullName, obj.GetType().FullName), nameof(obj));
        }
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapNewsPublication"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapNewsPublication"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapNewsPublication"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
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
    /// <returns><b>true</b> if the specified <see cref="object"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public override bool Equals(object? obj)
    {
        return obj is SitemapNewsPublication other && this.Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Name, this.Language);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapNewsPublication"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapNewsPublication"/>.</returns>
    public override string ToString()
    {
        return $"{this.Name} ({this.Language})";
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapNewsPublication first, SitemapNewsPublication second)
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
    public static bool operator !=(SitemapNewsPublication first, SitemapNewsPublication second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapNewsPublication first, SitemapNewsPublication second)
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
    public static bool operator >(SitemapNewsPublication first, SitemapNewsPublication second)
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
    public static bool operator <=(SitemapNewsPublication first, SitemapNewsPublication second)
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
    public static bool operator >=(SitemapNewsPublication first, SitemapNewsPublication second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}