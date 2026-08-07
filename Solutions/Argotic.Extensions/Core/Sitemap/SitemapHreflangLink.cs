using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Represents one alternate language or region version of a page, as an XHTML <c>link</c> element.
/// </summary>
/// <remarks>
///     A link is written as <c>&lt;xhtml:link rel="alternate" hreflang="…" href="…"/&gt;</c>. The
///     <c>rel</c> is always <c>alternate</c> — it is not a property, because no other value is meaningful
///     here, and <see cref="Load"/> ignores any <c>link</c> that says otherwise.
/// </remarks>
/// <seealso cref="SitemapHreflangExtension.Links"/>
public class SitemapHreflangLink : IComparable<SitemapHreflangLink>, IEquatable<SitemapHreflangLink>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the language/region code.
    /// </summary>
    private string linkHreflang = string.Empty;

    /// <summary>
    /// Private member to hold the URL of the alternate version.
    /// </summary>
    private Uri? linkHref;

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapHreflangLink"/> class.
    /// </summary>
    public SitemapHreflangLink()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapHreflangLink"/> class with the specified hreflang and href.
    /// </summary>
    /// <param name="hreflang">An ISO 639-1 language code with an optional region, or <c>x-default</c>.</param>
    /// <param name="href">The fully-qualified URL of the alternate version.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="hreflang"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="hreflang"/> is an empty string.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="href"/> is <see langword="null"/>.</exception>
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
    ///     An ISO 639-1 language code, optionally followed by an ISO 3166-1 Alpha 2 region — <c>en</c>,
    ///     <c>en-US</c>, <c>de-AT</c> — or <c>x-default</c>. The default value is an <i>empty</i> string.
    ///     Required by the specification.
    /// </value>
    /// <remarks>
    ///     <para>
    ///     <b>The language comes first and cannot be omitted.</b> A bare country code is not a valid value:
    ///     Google does not infer a language from a region, so <c>hreflang="de"</c> targets German speakers
    ///     everywhere while a lone <c>AT</c> targets nobody. Region without language is the common mistake.
    ///     </para>
    ///     <para>
    ///     <c>x-default</c> is the reserved fallback for a visitor whose browser settings match none of the
    ///     alternates. It names no language, by design.
    ///     </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The value specified for a set operation is an empty string.</exception>
    public string Hreflang
    {
        get => linkHreflang;

        set
        {
            ArgumentException.ThrowIfNullOrEmpty(value);
            linkHreflang = value.Trim();
        }
    }

    /// <summary>
    /// Gets or sets the fully-qualified URL of the alternate language/region version of the page.
    /// </summary>
    /// <value>
    ///     A fully-qualified <see cref="Uri"/> for the alternate version, or <see langword="null"/> if none
    ///     was specified. Required by the specification.
    /// </value>
    /// <remarks>
    ///     This is the URL the alternate page must point back to for the pair to count as reciprocal. See
    ///     <see cref="SitemapHreflangExtension"/> for what happens when it does not.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public Uri? Href
    {
        get => linkHref;

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
    /// <returns><see langword="true"/> if the <see cref="SitemapHreflangLink"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="manager"/> is <see langword="null"/>.</exception>
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

            if (!string.IsNullOrEmpty(hrefAttr) && Uri.TryCreate(hrefAttr, UriKind.RelativeOrAbsolute, out Uri? href))
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
    /// <exception cref="ArgumentNullException">The <paramref name="writer"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="xmlNamespace"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">The <paramref name="xmlNamespace"/> is an empty string.</exception>
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

        if (this.Href is not null)
        {
            writer.WriteAttributeString("href", this.Href.ToString());
        }

        writer.WriteEndElement();
    }

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="other">An object to compare with this instance.</param>
    /// <returns>A 32-bit signed integer that indicates the relative order of the objects being compared.</returns>
    public int CompareTo(SitemapHreflangLink? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Hreflang, other.Hreflang, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Href, other.Href, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapHreflangLink"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapHreflangLink"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SitemapHreflangLink"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SitemapHreflangLink? other)
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
    public override bool Equals(object? obj) => obj is SitemapHreflangLink other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Hreflang), HashCodeUtility.Component(this.Href));

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapHreflangLink"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapHreflangLink"/>.</returns>
    public override string ToString() => $"{this.Hreflang}: {this.Href}";

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SitemapHreflangLink? first, SitemapHreflangLink? second)
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
    public static bool operator !=(SitemapHreflangLink? first, SitemapHreflangLink? second) => !(first == second);

}