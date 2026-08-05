using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means of describing alternate language versions in sitemaps.
/// </summary>
/// <remarks>
///     <para>
///         The <see cref="SitemapHreflangExtension"/> extends sitemap content to include hreflang annotations
///         that help search engines understand which pages are intended for which languages and regions.
///         This syndication extension uses the XHTML namespace to specify alternate language versions of a page.
///     </para>
///     <para>
///         For more information, see <a href="https://developers.google.com/search/docs/specialty/international/localized-versions">https://developers.google.com/search/docs/specialty/international/localized-versions</a>.
///     </para>
/// </remarks>
public class SitemapHreflangExtension : SyndicationExtension, IComparable<SitemapHreflangExtension>, IEquatable<SitemapHreflangExtension>, IComparisonOperators
{
    /// <summary>
    /// Private member to hold the collection of hreflang links.
    /// </summary>
    private readonly List<SitemapHreflangLink> extensionLinks = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="SitemapHreflangExtension"/> class.
    /// </summary>
    public SitemapHreflangExtension()
        : base("xhtml", "http://www.w3.org/1999/xhtml", new Version("1.0"), new Uri("https://developers.google.com/search/docs/specialty/international/localized-versions"), "XHTML Hreflang Extension", "Extends sitemaps to include alternate language version annotations for international SEO.")
    {
    }

    /// <summary>
    /// Gets the collection of hreflang links associated with this extension.
    /// </summary>
    /// <value>
    ///     An <see cref="IList{T}"/> collection of <see cref="SitemapHreflangLink"/> objects that represent alternate
    ///     language versions of the page. The default value is an <i>empty</i> collection.
    /// </value>
    public IList<SitemapHreflangLink> Links => extensionLinks;

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
        return extension is SitemapHreflangExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <b>IXPathNavigable</b> used to load this <see cref="SitemapHreflangExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapHreflangExtension"/> was able to be initialized using the supplied <paramref name="source"/>; Otherwise, <b>false</b>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is a null reference.</exception>
    public override bool Load(IXPathNavigable source)
    {
        bool wasLoaded = false;
        ArgumentNullException.ThrowIfNull(source);

        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        XmlNamespaceManager manager = this.CreateNamespaceManager(navigator);

        XPathNodeIterator linkIterator = navigator.Select("xhtml:link", manager);

        if (linkIterator is { Count: > 0 })
        {
            while (linkIterator.MoveNext())
            {
                XPathNavigator? linkNode = linkIterator.Current;
                if (linkNode is null)
                {
                    continue;
                }

                SitemapHreflangLink link = new();
                if (link.Load(linkNode, manager))
                {
                    this.extensionLinks.Add(link);
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
    /// <param name="reader">The <b>XmlReader</b> used to load this <see cref="SitemapHreflangExtension"/>.</param>
    /// <returns><b>true</b> if the <see cref="SitemapHreflangExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; Otherwise, <b>false</b>.</returns>
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

        foreach (SitemapHreflangLink link in this.Links)
        {
            link.WriteTo(writer, this.XmlNamespace);
        }
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SitemapHreflangExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="SitemapHreflangExtension"/>.</returns>
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
    public int CompareTo(SitemapHreflangExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = ComparisonUtility.CompareSequence(this.Links, other.Links);
        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SitemapHreflangExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SitemapHreflangExtension"/> to compare with the current instance.</param>
    /// <returns><b>true</b> if the specified <see cref="SitemapHreflangExtension"/> is equal to the current instance; otherwise, <b>false</b>.</returns>
    public bool Equals(SitemapHreflangExtension? other)
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
    public override bool Equals(object? obj) => obj is SitemapHreflangExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        foreach (SitemapHreflangLink link in this.Links)
        {
            hash.Add(HashCodeUtility.Component(link));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><b>true</b> if the values of its operands are equal, otherwise; <b>false</b>.</returns>
    public static bool operator ==(SitemapHreflangExtension? first, SitemapHreflangExtension? second)
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
    public static bool operator !=(SitemapHreflangExtension? first, SitemapHreflangExtension? second) => !(first == second);

}