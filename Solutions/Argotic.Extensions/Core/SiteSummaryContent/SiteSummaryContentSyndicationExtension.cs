using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to carry an item's full content, rather than a summary of it.
/// </summary>
/// <remarks>
///     <para>
///     The RDF Site Summary 1.0 Content module, specified at
///     <a href="https://web.resource.org/rss/1.0/modules/content/">https://web.resource.org/rss/1.0/modules/content/</a>.
///     Of the three RSS 1.0 modules in this library it is by far the most alive: <c>content:encoded</c>
///     is where WordPress and most other publishing systems put the full HTML of a post, and a consumer
///     that reads only RSS's own <c>description</c> will get a truncated excerpt from feeds that have the
///     whole article sitting in this element.
///     </para>
///     <para>
///     <b>The module has two syntaxes and only one of them survived.</b>
///     <see cref="SiteSummaryContentSyndicationExtensionContext.Encoded"/> is the one publishers use; the
///     RDF-flavoured <see cref="SiteSummaryContentSyndicationExtensionContext.Items"/> collection, which
///     modelled several encodings of the same content, is essentially unseen in the wild. Both are read
///     and written here, because a parser has no say in what arrives.
///     </para>
///     <para>
///     Note the split between the documentation URL and the namespace: the module is described at
///     <c>web.resource.org</c> but its elements are qualified with
///     <c>http://purl.org/rss/1.0/modules/content/</c>. Matching on the former finds nothing.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\SiteSummaryContentSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the SiteSummaryContentSyndicationExtension class." />
/// </example>
public class SiteSummaryContentSyndicationExtension : SyndicationExtension, IComparable<SiteSummaryContentSyndicationExtension>, IEquatable<SiteSummaryContentSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummaryContentSyndicationExtension"/> class.
    /// </summary>
    public SiteSummaryContentSyndicationExtension()
        : base("content", "http://purl.org/rss/1.0/modules/content/", new Version("1.0"), new Uri("http://web.resource.org/rss/1.0/modules/content/"), "RDF Site Summary (Content)", "Extends syndication feeds to provide a means of describing content, including its format and encoding.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="SiteSummaryContentSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="SiteSummaryContentSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public SiteSummaryContentSyndicationExtensionContext Context
    {
        get;

        set
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    } = new();

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
        return extension is SiteSummaryContentSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="SiteSummaryContentSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryContentSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="source"/> is <see langword="null"/>.</exception>
    public override bool Load(IXPathNavigable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        XPathNavigator navigator = source.CreateNavigator()
            ?? throw new ArgumentException("The supplied source did not provide a navigator.", nameof(source));
        bool wasLoaded = this.Context.Load(navigator, this.CreateNamespaceManager(navigator));
        SyndicationExtensionLoadedEventArgs args = new(source, this);
        this.OnExtensionLoaded(args);

        return wasLoaded;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="XmlReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="SiteSummaryContentSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummaryContentSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
        this.Context.WriteTo(writer, this.XmlNamespace);
    }

    /// <summary>
    /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummaryContentSyndicationExtension"/>.
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
    public int CompareTo(SiteSummaryContentSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Context.Encoded, other.Context.Encoded, StringComparison.Ordinal);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Items, other.Context.Items);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SiteSummaryContentSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SiteSummaryContentSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SiteSummaryContentSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SiteSummaryContentSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is SiteSummaryContentSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(this.Context.Encoded, StringComparer.Ordinal);
        foreach (SiteSummaryContentItem item in this.Context.Items)
        {
            hash.Add(HashCodeUtility.Component(item));
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SiteSummaryContentSyndicationExtension? first, SiteSummaryContentSyndicationExtension? second)
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
    public static bool operator !=(SiteSummaryContentSyndicationExtension? first, SiteSummaryContentSyndicationExtension? second) => !(first == second);

}