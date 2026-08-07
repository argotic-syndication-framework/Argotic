using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to mark a feed as a list, and to say how it may be sorted and grouped.
/// </summary>
/// <remarks>
///     <para>
///     Microsoft's Simple List Extensions, version 1.0a, published in 2005 alongside the RSS support in
///     Internet Explorer 7 and Windows Vista. The idea was that a feed could be more than a stream of
///     news — a wishlist, a top-ten, a product catalogue — and could tell a reader which of its fields
///     were worth offering as sort and filter controls.
///     </para>
///     <para>
///     <b>It is dead.</b> The reader it was built for is gone, and effectively nothing consumes these
///     elements today. It is implemented here so that archived feeds parse and round-trip; do not reach
///     for it when designing a new feed. The specification link below points at
///     <c>msdn2.microsoft.com</c>, a hostname retired over a decade ago, which is itself a fair summary
///     of the extension's standing.
///     </para>
///     <para>
///     The prefix is <c>cf</c> — for "common feed", after the Windows Common Feed List — bound to
///     <c>http://www.microsoft.com/schemas/rss/core/2005</c>. Neither string mentions lists, which makes
///     this extension harder to recognise in a raw feed than most.
///     </para>
///     <para>
///     For more information, see
///     <a href="https://learn.microsoft.com/en-us/previous-versions/bb190612(v=msdn.10)">https://learn.microsoft.com/en-us/previous-versions/bb190612(v=msdn.10)</a>.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\SimpleListSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the SimpleListSyndicationExtension class." />
/// </example>
public class SimpleListSyndicationExtension : SyndicationExtension, IComparable<SimpleListSyndicationExtension>, IEquatable<SimpleListSyndicationExtension>, IComparisonOperators
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleListSyndicationExtension"/> class.
    /// </summary>
    public SimpleListSyndicationExtension()
        : base("cf", "http://www.microsoft.com/schemas/rss/core/2005", new Version("1.0"), new Uri("http://msdn2.microsoft.com/en-us/xml/bb190612.aspx"), "Simple List", "Extends syndication feeds to provide a means of exposing ordered lists of items easier and more accessible to users.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="SimpleListSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>A <see cref="SimpleListSyndicationExtensionContext"/> object that contains information associated with the current syndication extension.</value>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public SimpleListSyndicationExtensionContext Context
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
        return extension is SimpleListSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="SimpleListSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SimpleListSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="SimpleListSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="SimpleListSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="SimpleListSyndicationExtension"/>.
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
    public int CompareTo(SimpleListSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = this.Context.TreatAsList.CompareTo(other.Context.TreatAsList);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Grouping, other.Context.Grouping);
        if (result == 0) result = ComparisonUtility.CompareSequence(this.Context.Sorting, other.Context.Sorting);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="SimpleListSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="SimpleListSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="SimpleListSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(SimpleListSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is SimpleListSyndicationExtension other && this.Equals(other);

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
        hash.Add(HashCodeUtility.Component(this.Context.TreatAsList));
        foreach (SimpleListGroup group in this.Context.Grouping)
        {
            hash.Add(HashCodeUtility.Component(group));
        }

        foreach (SimpleListSort sort in this.Context.Sorting)
        {
            hash.Add(HashCodeUtility.Component(sort));
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(SimpleListSyndicationExtension? first, SimpleListSyndicationExtension? second)
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
    public static bool operator !=(SimpleListSyndicationExtension? first, SimpleListSyndicationExtension? second) => !(first == second);

}