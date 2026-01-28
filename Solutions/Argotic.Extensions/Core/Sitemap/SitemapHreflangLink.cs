using System.Xml;
using System.Xml.XPath;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents an hreflang link in a sitemap for international and multilingual content.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapHreflangLink"/> class represents an alternate language version of a page.
///         It is used to indicate to search engines that different URLs serve the same content in different languages
///         or for different regions.
///     </para>
/// </remarks>
[Serializable]
public class SitemapHreflangLink : IComparable
{
    /// <summary>
    /// Private member to hold the language/region code.
    /// </summary>
    private string linkHreflang = string.Empty;

    /// <summary>
    /// Private member to hold the URL of the alternate version.
    /// </summary>
    private Uri linkHref;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapHreflangLink"/> class.
    /// </summary>
    public SitemapHreflangLink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapHreflangLink"/> class with the specified hreflang and href.
    /// </summary>
    /// <param name="hreflang">The language/region code or "x-default" for the default version.</param>
    /// <param name="href">The URL of the alternate version.</param>
    /// <exception cref="ArgumentException">The <paramref name="hreflang"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is a null reference.</exception>
    public SitemapHreflangLink(string hreflang, Uri href)
    {
        ArgumentException.ThrowIfNullOrEmpty(hreflang);
        ArgumentNullException.ThrowIfNull(href);

        this.linkHreflang = hreflang.Trim();
        this.linkHref = href;
    }

    /// <summary>
    /// Gets or sets the language and optional regional code for the alternate page.
    /// </summary>
    /// <value>
    ///     A language code in ISO 639-1 format, optionally followed by a region in ISO 3166-1 Alpha 2 format,
    ///     or "x-default" for the default/fallback version. This is a required property.
    /// </value>
    /// <remarks>
    ///     <para>Examples: "en", "en-US", "en-GB", "de", "de-AT", "x-default".</para>
    ///     <para>Use "x-default" to specify the page that should be shown when no other language matches the user's browser settings.</para>
    /// </remarks>
    /// <exception cref="ArgumentException">The <paramref name="value"/> is null or empty.</exception>
    public string Hreflang
    {
        get
        {
            return linkHreflang;
        }

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            linkHreflang = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the fully-qualified URL of the alternate language/region version of the page.
    /// </summary>
    /// <value>A <see cref="Uri"/> that represents the URL of the alternate version. This is a required property.</value>
    /// <exception cref="ArgumentNullException">The <paramref name="value"/> is a null reference.</exception>
    public Uri Href
    {
        get
        {
            return linkHref;
        }

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            linkHref = value;
        }
    }

    /// <summary>
    /// Initializes the hreflang link using the supplied <see cref="XPathNavigator"/>.
    /// </summary>
    /// <param name="source">The <see cref="XPathNavigator"/> used to load this <see cref="SitemapHreflangLink"/>.</param>
    /// <param name="manager">The <see cref="XmlNamespaceManager"/> object used to resolve prefixed XML namespaces.</param>
    /// <returns><b>true</b> if the <see cref="SitemapHreflangLink"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is a null reference.</exception>
    public bool Load(XPathNavigator source, XmlNamespaceManager manager)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(manager);

        // The hreflang link element has attributes rel, hreflang, and href
        string relAttr = source.GetAttribute("rel", string.Empty);
        string hreflangAttr = source.GetAttribute("hreflang", string.Empty);
        string hrefAttr = source.GetAttribute("href", string.Empty);

        // Only process if rel="alternate"
        if (string.Equals(relAttr, "alternate", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.IsNullOrEmpty(hreflangAttr))
            {
                this.linkHreflang = hreflangAttr.Trim();
                wasLoaded = true;
            }

            if (!string.IsNullOrEmpty(hrefAttr) && Uri.TryCreate(hrefAttr, UriKind.RelativeOrAbsolute, out Uri href))
            {
                this.linkHref = href;
                wasLoaded = true;
            }
        }

        return wasLoaded;
    }

    /// <summary>
    /// Writes the hreflang link to the specified <see cref="XmlWriter"/>.
    /// </summary>
    /// <param name="writer">The <see cref="XmlWriter"/> to which the hreflang link will be written.</param>
    /// <param name="xmlNamespace">The XML namespace used to qualify prefixed elements.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is a null reference.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is a null reference or empty string.</exception>
    public void WriteTo(XmlWriter writer, string xmlNamespace)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentException.ThrowIfNullOrEmpty(xmlNamespace);

        writer.WriteStartElement("link", xmlNamespace);
        writer.WriteAttributeString("rel", "alternate");

        if (!string.IsNullOrEmpty(this.Hreflang))
        {
            writer.WriteAttributeString("hreflang", this.Hreflang);
        }

        if (this.Href != null)
        {
            writer.WriteAttributeString("href", this.Href.ToString());
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

        SitemapHreflangLink other = obj as SitemapHreflangLink;

        if (other != null)
        {
            int result = string.Compare(this.Hreflang, other.Hreflang, StringComparison.OrdinalIgnoreCase);
            result |= Uri.Compare(this.Href, other.Href, UriComponents.AbsoluteUri, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase);
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
        if (obj is not SitemapHreflangLink)
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
        return HashCode.Combine(this.Hreflang, this.Href);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapHreflangLink"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapHreflangLink"/>.</returns>
    public override string ToString()
    {
        return $"{this.Hreflang}: {this.Href}";
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapHreflangLink first, SitemapHreflangLink second)
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
    public static bool operator !=(SitemapHreflangLink first, SitemapHreflangLink second)
    {
        return !(first == second);
    }

    /// <summary>
    /// Determines if first operand is less than second operand.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the first operand is less than the second, otherwise; <b>false</b>.</returns>
    public static bool operator <(SitemapHreflangLink first, SitemapHreflangLink second)
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
    public static bool operator >(SitemapHreflangLink first, SitemapHreflangLink second)
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
    public static bool operator <=(SitemapHreflangLink first, SitemapHreflangLink second)
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
    public static bool operator >=(SitemapHreflangLink first, SitemapHreflangLink second)
    {
        if (first is null) return second is null;
        return first.CompareTo(second) >= 0;
    }
}