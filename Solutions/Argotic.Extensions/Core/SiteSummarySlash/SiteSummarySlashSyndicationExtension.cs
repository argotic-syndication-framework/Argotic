using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications with the section, department and comment count of a Slash-style post.
/// </summary>
/// <remarks>
///     <para>
///     The RDF Site Summary 1.0 Slash module, specified at
///     <a href="https://web.resource.org/rss/1.0/modules/slash/">https://web.resource.org/rss/1.0/modules/slash/</a>.
///     It is named for the Slash engine behind Slashdot, and it is dormant: written for a family of
///     sites that has largely gone, and not adopted outside it. Of the three RSS 1.0 modules here, this
///     is the one you are least likely to meet — contrast
///     <see cref="SiteSummaryContentSyndicationExtension"/>, which is everywhere.
///     </para>
///     <para>
///     The one element with a life beyond Slashdot is <c>slash:comments</c>, an integer comment count
///     that some publishers still emit because RSS itself has no field for one. If that is what you are
///     after, read <see cref="SiteSummarySlashSyndicationExtensionContext.Comments"/> and expect it to be
///     absent far more often than not.
///     </para>
///     <para>
///     As with the other RSS 1.0 modules, the elements are qualified with
///     <c>http://purl.org/rss/1.0/modules/slash/</c>, not with the <c>web.resource.org</c> address the
///     module is documented at.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\SiteSummarySlashSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the SiteSummarySlashSyndicationExtension class." />
/// </example>
public class SiteSummarySlashSyndicationExtension : SyndicationExtension, IComparable<SiteSummarySlashSyndicationExtension>, IEquatable<SiteSummarySlashSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SiteSummarySlashSyndicationExtension"/> class.
    /// </summary>
    public SiteSummarySlashSyndicationExtension()
        : base("slash", "http://purl.org/rss/1.0/modules/slash/", new Version("1.0"), new Uri("http://web.resource.org/rss/1.0/modules/slash/"), "RDF Site Summary (Slash)", "Extends syndication feeds to provide a means of describing Slash-based site meta-data.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="SiteSummarySlashSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="SiteSummarySlashSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public SiteSummarySlashSyndicationExtensionContext Context
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
        return extension is SiteSummarySlashSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="SiteSummarySlashSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummarySlashSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="SiteSummarySlashSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SiteSummarySlashSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="SiteSummarySlashSyndicationExtension"/>.
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
    public int CompareTo(SiteSummarySlashSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Context.Comments.CompareTo(other.Context.Comments);
        if (result == 0) result = string.Compare(this.Context.Department, other.Context.Department, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Section, other.Context.Section, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.HitParade, other.Context.HitParade);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SiteSummarySlashSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SiteSummarySlashSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SiteSummarySlashSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SiteSummarySlashSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is SiteSummarySlashSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    /// <remarks>
    ///     The collection members are folded in element by element. Passing the collection itself to
    ///     <see cref="HashCodeUtility.Component{T}(T)"/> would hash the list reference, so two instances
    ///     that <see cref="CompareTo"/> reports as equal hashed differently.
    /// </remarks>
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(HashCodeUtility.Component(this.Context.Comments));
        hash.Add(HashCodeUtility.Component(this.Context.Department));
        hash.Add(HashCodeUtility.Component(this.Context.Section));
        foreach (int hit in this.Context.HitParade)
        {
            hash.Add(HashCodeUtility.Component(hit));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SiteSummarySlashSyndicationExtension? first, SiteSummarySlashSyndicationExtension? second)
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
    public static bool operator !=(SiteSummarySlashSyndicationExtension? first, SiteSummarySlashSyndicationExtension? second) => !(first == second);

}