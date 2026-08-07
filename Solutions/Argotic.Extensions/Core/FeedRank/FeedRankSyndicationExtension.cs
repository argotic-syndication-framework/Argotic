using System.Xml;
using System.Xml.XPath;

using Argotic.Common;

namespace Argotic.Extensions.Core;

/// <summary>
/// Extends syndication specifications to provide a means feed publishers to convey one or more numeric rankings for entries contained within feeds, 
/// each of which can be used, independently or in conjunction with the others, to establish a sorting order.
/// </summary>
/// <remarks>
///     <para>
///         The Atom Ranking Extensions, <c>re:rank</c> in the
///         <c>http://purl.org/atompub/rank/1.0</c> namespace. The specification is the tenth revision of
///         an Internet-Draft,
///         <a href="https://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt">draft-snell-atompub-feed-index-10</a>;
///         it never became an RFC, and this is a niche extension rather than a deployed one.
///     </para>
///     <para>
///         One element carries a decimal <see cref="FeedRankSyndicationExtensionContext.Value"/> and
///         names the ranking it belongs to with
///         <see cref="FeedRankSyndicationExtensionContext.Scheme"/> — a URI identifying the scale, so
///         that a consumer knows whether two feeds' numbers are comparable. Nothing constrains the
///         range or the direction: a scheme may count upwards, downwards, or between zero and one, and
///         a number read without its scheme means nothing.
///     </para>
/// </remarks>
/// <example>
///     <code source="..\..\Argotic.Examples\Extensions\Core\FeedRankSyndicationExtensionExample.cs" language="cs" title="The following code example demonstrates the usage of the FeedRankSyndicationExtension class." />
/// </example>
public class FeedRankSyndicationExtension : SyndicationExtension, IComparable<FeedRankSyndicationExtension>, IEquatable<FeedRankSyndicationExtension>, IComparisonOperators
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeedRankSyndicationExtension"/> class.
    /// </summary>
    public FeedRankSyndicationExtension()
        : base("re", "http://purl.org/atompub/rank/1.0", new Version("1.0"), new Uri("http://xml.coverpages.org/draft-snell-atompub-feed-index-10.txt"), "Feed Ranking", "Extends syndication feeds to provide a means feed publishers to convey one or more numeric rankings for entries contained within feeds, each of which can be used, independently or in conjunction with the others, to establish a sorting order.")
    {
    }

    /// <summary>
    /// Gets or sets the <see cref="FeedRankSyndicationExtensionContext"/> object associated with this extension.
    /// </summary>
    /// <value>The context. Never <see langword="null"/>: one is created with the extension, and the setter rejects <see langword="null"/>.</value>
    /// <remarks>
    ///     The <c>Context</c> encapsulates all the syndication extension information that can be retrieved or written to an extended syndication entity. 
    ///     Its purpose is to prevent property naming collisions between the base <see cref="SyndicationExtension"/> class and any custom properties that 
    ///     are defined for the custom syndication extension.
    /// </remarks>
    /// <exception cref="ArgumentNullException">The value specified for a set operation is <see langword="null"/>.</exception>
    public FeedRankSyndicationExtensionContext Context
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
        return extension is FeedRankSyndicationExtension;
    }

    /// <summary>
    /// Initializes the syndication extension using the supplied <see cref="IXPathNavigable"/>.
    /// </summary>
    /// <param name="source">The <see cref="IXPathNavigable"/> used to load this <see cref="FeedRankSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedRankSyndicationExtension"/> was able to be initialized using the supplied <paramref name="source"/>; otherwise, <see langword="false"/>.</returns>
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
    /// <param name="reader">The <see cref="XmlReader"/> used to load this <see cref="FeedRankSyndicationExtension"/>.</param>
    /// <returns><see langword="true"/> if the <see cref="FeedRankSyndicationExtension"/> was able to be initialized using the supplied <paramref name="reader"/>; otherwise, <see langword="false"/>.</returns>
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
    /// Returns a <see cref="string"/> that represents the current <see cref="FeedRankSyndicationExtension"/>.
    /// </summary>
    /// <returns>A <see cref="string"/> that represents the current <see cref="FeedRankSyndicationExtension"/>.</returns>
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
    public int CompareTo(FeedRankSyndicationExtension? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = string.Compare(this.Description, other.Description, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Uri.Compare(this.Documentation, other.Documentation, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = Comparer<Version>.Default.Compare(this.Version, other.Version);
        if (result == 0) result = string.Compare(this.XmlNamespace, other.XmlNamespace, StringComparison.Ordinal);
        if (result == 0) result = string.Compare(this.XmlPrefix, other.XmlPrefix, StringComparison.Ordinal);

        if (result == 0) result = Uri.Compare(this.Context.Domain, other.Context.Domain, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
        if (result == 0) result = string.Compare(this.Context.Label, other.Context.Label, StringComparison.Ordinal);
        if (result == 0) result = Uri.Compare(this.Context.Scheme, other.Context.Scheme, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped, StringComparison.Ordinal);
        if (result == 0) result = this.Context.Value.CompareTo(other.Context.Value);

        return result;
    }

    /// <summary>
    /// Determines whether the specified <see cref="FeedRankSyndicationExtension"/> is equal to the current instance.
    /// </summary>
    /// <param name="other">The <see cref="FeedRankSyndicationExtension"/> to compare with the current instance.</param>
    /// <returns><see langword="true"/> if the specified <see cref="FeedRankSyndicationExtension"/> is equal to the current instance; otherwise, <see langword="false"/>.</returns>
    public bool Equals(FeedRankSyndicationExtension? other)
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
    public override bool Equals(object? obj) => obj is FeedRankSyndicationExtension other && this.Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode() => HashCode.Combine(HashCodeUtility.Component(this.Description), HashCodeUtility.Component(this.Documentation), HashCodeUtility.Component(this.Name), HashCodeUtility.Component(this.Version), HashCodeUtility.Component(this.XmlNamespace), HashCodeUtility.Component(this.XmlPrefix), HashCodeUtility.Component(HashCode.Combine(HashCodeUtility.Component(this.Context.Domain), HashCodeUtility.Component(this.Context.Label), HashCodeUtility.Component(this.Context.Scheme), HashCodeUtility.Component(this.Context.Value))));

    /// <summary>
    /// Determines if operands are equal.
    /// </summary>
    /// <param name="first">Operand to be compared.</param>
    /// <param name="second">Operand to compare to.</param>
    /// <returns><see langword="true"/> if the values of its operands are equal, otherwise; <see langword="false"/>.</returns>
    public static bool operator ==(FeedRankSyndicationExtension? first, FeedRankSyndicationExtension? second)
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
    public static bool operator !=(FeedRankSyndicationExtension? first, FeedRankSyndicationExtension? second) => !(first == second);

}